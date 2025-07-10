using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[HttpContextEnabledTest]
	public sealed class RedirectOnDefaultPathTest : TransactionedTestCase, IHttpContextEnabledTestWithRequestPath
	{
		public void TestDefaultPathAndPages()
		{
			var global = new ZGlobalForDefaultPathTest();
			AssertEquals("precondition", "/webapp/", global.ApplicationRoot);

			AssertEquals("BaseStyleSheet", global.ApplicationRoot + "BaseStyle.css", global.BaseStyleSheet);
			AssertEquals("ErrorPage", global.ApplicationRoot + "Error.aspx", global.ErrorPage);
			AssertEquals("LoginPage", "", global.LoginPage);
			AssertEquals("LogoImage", global.ApplicationRoot + "Images/Logo.gif", global.LogoImage);
			AssertEquals("HomePage", String.Empty, global.HomePage);
			AssertEquals("CompanyName", String.Empty, global.CompanyName);
		}

		public void TestErrorPageOnSessionStartAbandonsSession()
		{
			var global = new ZGlobalForDefaultPathTest();
			TestGlobal.InitZGlobalWithHttpContext(global);

			global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		public string RequestPath => "/";

		public string QueryString => string.Empty;

		class ZGlobalForDefaultPathTest : ZGlobalForTesting
		{
			public override string ApplicationRoot => "/webapp/";
		}
	}
}
