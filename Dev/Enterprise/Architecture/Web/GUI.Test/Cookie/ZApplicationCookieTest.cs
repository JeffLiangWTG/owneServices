namespace Enterprise.ZArchitecture.Web.GUI.Cookie
{
	sealed class ZApplicationCookieTest : ZBaseCookieTest
	{
		protected override ZBaseCookie GetNewCookie(string cookieName)
		{
			return new ZApplicationCookie(cookieName);
		}

		new ZApplicationCookie TestCookie
		{
			get { return base.TestCookie as ZApplicationCookie; }
		}

		public void TestWriteReadUser()
		{
			TestCookie.WriteUser("TestCompany", "test@edi.com.au", "testPassword");
			AssertEquals("TestCompany", TestCookie.GetCompanyCode());
			AssertEquals("test@edi.com.au", TestCookie.GetUserEmail());
			AssertEquals("testPassword", TestCookie.GetUserPassword());
		}

		public void TestWriteReadUser_WithoutPassword()
		{
			TestCookie.WriteUser("TestCompany", "test@edi.com.au");
			AssertEquals("TestCompany", TestCookie.GetCompanyCode());
			AssertEquals("test@edi.com.au", TestCookie.GetUserEmail());
			AssertEquals("", TestCookie.GetUserPassword());
		}
	}
}
