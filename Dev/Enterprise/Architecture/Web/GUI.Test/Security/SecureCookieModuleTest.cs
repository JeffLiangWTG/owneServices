using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public class SecureCookiesWithHttpsTest : SecureCookieModuleTestCase, IHttpContextEnabledTestWithHttps
	{
		public override bool IsSecureCookieExpected => true;
	}

	public class SecureCookiesWithForwardedHttpsTest : SecureCookieModuleTestCase
	{
		public override bool IsSecureCookieExpected => true;

		protected override void SetUp()
		{
			base.SetUp();

			HttpContext.Current.Request.SetRequestHeader("X-Forwarded-Proto", "https");
		}
	}

	public class SecureCookiesNoHttpsTest : SecureCookieModuleTestCase
	{
		public override bool IsSecureCookieExpected => false;

		protected override void SetUp()
		{
			base.SetUp();

			HttpContext.Current.Request.SetRequestHeader("X-Forwarded-Proto", "http");
		}
	}

	[HttpContextEnabledTest]
	public abstract class SecureCookieModuleTestCase : TestCase
	{
		public void TestSecureCookies()
		{
			module.ApplicationEndRequestForTest();

			AssertEquals(IsSecureCookieExpected, HttpContext.Current.Response.Cookies[0].Secure);
			AssertEquals(IsSecureCookieExpected, HttpContext.Current.Response.Cookies[1].Secure);
		}

		public abstract bool IsSecureCookieExpected { get; }

		protected override void SetUp()
		{
			module = new SecureCookieModuleForTest();

			var cookie1 = new HttpCookie("one", "1");
			var cookie2 = new HttpCookie("two", "2");

			HttpContext.Current.Response.Cookies.Add(cookie1);
			HttpContext.Current.Response.Cookies.Add(cookie2);
		}

		SecureCookieModuleForTest module;

		protected DummyHttpApplication ApplicationInstance
		{
			get { return HttpContext.Current.ApplicationInstance as DummyHttpApplication; }
		}
	}

	sealed class SecureCookieModuleForTest : SecureCookieModule
	{
		public void ApplicationEndRequestForTest()
		{
			this.Application_EndRequest(HttpContext.Current.ApplicationInstance, new EventArgs());
		}
	}
}
