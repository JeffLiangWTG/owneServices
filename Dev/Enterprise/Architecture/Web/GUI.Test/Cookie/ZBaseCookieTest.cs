using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Cookie
{
	public abstract class ZBaseCookieTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			TestCookie = GetNewCookie("CookieName");
		}

		protected ZBaseCookie TestCookie;
		protected abstract ZBaseCookie GetNewCookie(string cookieName);

		public void TestWriteReadCookie()
		{
			TestCookie.WriteCookie("testValue");
			AssertEquals(true, TestCookie.CookieExist());
			AssertEquals("testValue", TestCookie.ReadCookie());
			AssertEquals(true, TestCookie.CookieExist());
			TestCookie.Remove();
			AssertEquals("Removed Cookie should not exist", false, TestCookie.CookieExist());
			Assert("Should be expired", TestCookie.ResponseRequestCookies[0].Expires < DateTime.Now);// Don't need a DB hit for setting a cookie
		}

		public void TestWriteReadEmptyCookie()
		{
			TestCookie.WriteCookie("");
			AssertEquals(true, TestCookie.CookieExist());
			AssertEquals("", TestCookie.ReadCookie());
			AssertEquals(true, TestCookie.CookieExist());
			TestCookie.Remove();
			AssertEquals("Removed Cookie should not exist", false, TestCookie.CookieExist());
			Assert("Should be expired", TestCookie.ResponseRequestCookies[0].Expires < DateTime.Now);// Don't need a DB hit for setting a cookie
		}

		public void TestCookieEncryption()
		{
			TestCookie.WriteCookie("test");
			AssertEquals("test", TestCookie.Encoder.Decrypt(TestCookie.ResponseRequestCookies[0].Value));
		}
	}
}
