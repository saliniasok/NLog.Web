﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Web.LayoutRenderers;
using Xunit;

namespace NLog.Web.Tests.LayoutRenderers
{
    public class AspNetSessionValueLayoutRendererTests : IDisposable
    {


        public AspNetSessionValueLayoutRendererTests()
        {
            SetUp();
        }

        public void SetUp()
        {
            SetupFakeSession();
        }

        public void CleanUp()
        {

            Session.Clear();
        }

        private HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        [Fact]
        public void SimpleTest()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "a"
            };

            ExecTest("a", "b", "b", appSettingLayoutRenderer);
        }

        [Fact]
        public void SimpleTest2()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "a.b"
            };

            ExecTest("a.b", "c", "c", appSettingLayoutRenderer);
        }

        [Fact]
        public void NestedProps()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "a.b",
                EvaluateAsNestedProperties = true
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "c", appSettingLayoutRenderer);
        }

        [Fact]
        public void NestedProps2()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "a.b.c",
                EvaluateAsNestedProperties = true
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }

        [Fact]
        public void NestedProps3()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "a.b..c",
                EvaluateAsNestedProperties = true
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }

        [Fact]
        public void EmptyPath()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "",
                EvaluateAsNestedProperties = true
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }

        [Fact]
        public void EmptyVarname()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = "",
                EvaluateAsNestedProperties = false
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }

        [Fact]
        public void NullPath()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = null,
                EvaluateAsNestedProperties = true
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }

        [Fact]
        public void NullVarname()
        {
            var appSettingLayoutRenderer = new AspNetSessionValueLayoutRenderer()
            {
                Variable = null,
                EvaluateAsNestedProperties = false
            };

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "", appSettingLayoutRenderer);
        }


        [Fact]
        public void SessionWithPadding()
        {
            Layout layout = "${aspnet-session:a.b:padding=5:evaluateAsNestedProperties=true}";

            var o = new { b = "c" };
            //set in "a"
            ExecTest("a", o, "    c", layout);
        }


        [Fact]
        public void SessionWithCulture()
        {
            Layout layout = "${aspnet-session:a.b:culture=en-GB:evaluateAsNestedProperties=true}";

            var o = new { b = new DateTime(2015, 11, 24, 2, 30, 23) };
            //set in "a"
            ExecTest("a", o, "11/24/2015 2:30:23 AM", layout);
        }

        /// <summary>
        /// set in Session and test
        /// </summary>
        /// <param name="key">set with this key</param>
        /// <param name="value">set this value</param>
        /// <param name="expected">expected</param>
        /// <param name="appSettingLayoutRenderer"></param>
        ///  <remarks>IRenderable is internal</remarks>
        private void ExecTest(string key, object value, object expected, Layout appSettingLayoutRenderer)
        {
            Session[key] = value;

            var rendered = appSettingLayoutRenderer.Render(LogEventInfo.CreateNullEvent());

            Assert.Equal(expected, rendered);
        }

        /// <summary>
        /// set in Session and test
        /// </summary>
        /// <param name="key">set with this key</param>
        /// <param name="value">set this value</param>
        /// <param name="expected">expected</param>
        /// <param name="appSettingLayoutRenderer"></param>
        /// <remarks>IRenderable is internal</remarks>
        private void ExecTest(string key, object value, object expected, LayoutRenderer appSettingLayoutRenderer)
        {
            Session[key] = value;

            var rendered = appSettingLayoutRenderer.Render(LogEventInfo.CreateNullEvent());

            Assert.Equal(expected, rendered);
        }

        /// <summary>
        /// Create Fake Session http://stackoverflow.com/a/10126711/201303
        /// </summary>
        public static void SetupFakeSession()
        {
            var httpRequest = new HttpRequest("", "http://stackoverflow/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            HttpContext.Current = httpContext;

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            CleanUp();
        }
    }
}
