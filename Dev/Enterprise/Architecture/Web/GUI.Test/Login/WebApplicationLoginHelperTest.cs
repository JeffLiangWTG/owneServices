using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Authentication.Primitives;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI
{
	[HttpContextEnabledTest]
	public class WebApplicationLoginHelperTest : TestCaseWithFactory
	{
		public void TestConstructors()
		{
			ZPage page = GetNewTestPage();
			WebApplicationLoginHelper helper = GetNewHelper(page);
			AssertSame("reference to page should be saved", page, helper.Page);
			Assert("IsRedirectAfterSignIn should be set to True", helper.IsRedirectAfterSignIn);

			helper = GetNewHelper(page, true);
			AssertSame("reference to page should be saved", page, helper.Page);

			helper = GetNewHelper(page, false);
			AssertSame("reference to page should be saved", page, helper.Page);
		}

		public void TestOnPageLoad_CookieOnlyContainsCompanyCodeAndUserName()
		{
			ZPage page = GetNewPageWithRequest();
			WebApplicationLoginHelper helper = GetNewHelper(page);
			page.AppInstance.ApplicationCookie.WriteUser("EDISYD", "mehmeh@meh.com.au");

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			AssertEquals("Pre-condition", "", helper.LoginMan.CompanyCode);
			AssertEquals("Pre-condition", "", helper.LoginMan.UserName);
			AssertEquals("Pre-condition", true, page.AppInstance.ApplicationCookie.CookieExist());
			AssertEquals("Pre-condition", true, helper.LoginMan.AllowCompanyCodeAndUsernameToBeChanged);

			helper.OnPageLoad();
			AssertEquals("EDISYD", helper.LoginMan.CompanyCode);
			AssertEquals("mehmeh@meh.com.au", helper.LoginMan.UserName);
			TimeSpan timeSpan = DateTime.Now - page.Response.Cookies[page.AppInstance.ApplicationCookie.CookieName].Expires; // datetime in cookie is unrelated to server time
			AssertEquals("Should be removed / made expired", 1, timeSpan.Days);
			AssertEquals(true, helper.LoginMan.AllowCompanyCodeAndUsernameToBeChanged);

			AssertEquals("OnLoginSuccess should not be called because Login was not tried", 0, helper.onLoginSucceed_CallCount);
			AssertEquals("OnLoginFailure should not be called because Login was not tried", 0, helper.onLoginFailure_CallCount);
			AssertEquals("TryToRedirectToCustomLoginPage should be called once", 1, helper.TryToRedirectToCustomLoginPage_CallCount);
		}

		public virtual void TestOnPageLoad_CookieOnlyContainsUserNameAndPassword()
		{
			ZPage page = GetNewPageWithRequest();
			WebApplicationLoginHelper helper = GetNewHelper(page);
			page.AppInstance.ApplicationCookie.WriteUser(string.Empty, "mehmeh@meh.com.au", "password");

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			AssertEquals("Pre-condition", "", helper.LoginMan.CompanyCode);
			AssertEquals("Pre-condition", "", helper.LoginMan.UserName);
			AssertEquals("Pre-condition", true, page.AppInstance.ApplicationCookie.CookieExist());
			AssertEquals("Pre-condition", true, helper.LoginMan.AllowCompanyCodeAndUsernameToBeChanged);
			helper.LoginMan.IsCompanyCodeRequired = false;

			helper.OnPageLoad();
			AssertEquals(string.Empty, helper.LoginMan.CompanyCode);
			AssertEquals("mehmeh@meh.com.au", helper.LoginMan.UserName);
			AssertEquals("password", helper.LoginMan.Password);
			AssertEquals(true, helper.LoginMan.AllowCompanyCodeAndUsernameToBeChanged);

			AssertEquals("OnLoginFailure should be called because Login was tried", 1, helper.onLoginFailure_CallCount);
		}

		public void TestOnPageLoad_DoesNotLogout()
		{
			var page = GetNewPageWithRequest();
			var helper = GetNewHelper(page);
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();
			page.SiteUser.Login("XXXXX", "mehmeh@meh.com.au", "pass");

			helper.OnPageLoad();

			Assert("Currently logged in user should not be logged out when loading login page", page.SiteUser.IsLoggedIn);
		}

		public virtual void TestCompanyCodeCanBeEmptyIfLoginManagerAllows()
		{
			OrgHeader org = CreateNewCompany();
			OrgContact contact = CreateNewContact(org);
			Factory.Save();

			ZPage page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			LoginManager loginMan = page.DataSource as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("CompanyCode is Required by default", true, loginMan.IsCompanyCodeRequired);
			loginMan.IsCompanyCodeRequired = false;

			WebApplicationLoginHelper helper = GetNewHelper(page);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");
			helper.SetParamsValueForTest("UserPassword", "pass");
			helper.SetParamsValueForTest("RememberMe", "on");

			AssertEquals("Pre-condition", false, page.AppInstance.ApplicationCookie.CookieExist());

			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			AssertEquals("OrgCode should be correct", "XXXXX", page.SiteUser.AffiliationCode);
			AssertEquals("Contact Name should be correct", "Meh Meh", page.SiteUser.LoggedInUser.Name);
			AssertEquals("Contact Email should be correct", "mehmeh@meh.com.au", page.SiteUser.LoggedInUser.Email);
			Assert("Cookie should be written", page.AppInstance.ApplicationCookie.CookieExist());

			AssertEquals("OnLoginSuccess should be called once", 1, helper.onLoginSucceed_CallCount);
			AssertEquals("OnLoginFailure should not be called", 0, helper.onLoginFailure_CallCount);
			AssertEquals("TryToRedirectToCustomLoginPage should not be called", 0, helper.TryToRedirectToCustomLoginPage_CallCount);
		}

		public void TestTryToRedirectToCustomLoginPageIfIncompleteLoginData()
		{
			ZPage page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			LoginManager loginMan = page.DataSource as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("CompanyCode is Required by default", true, loginMan.IsCompanyCodeRequired);
			AssertEquals("Pre-condition", false, page.AppInstance.ApplicationCookie.CookieExist());

			WebApplicationLoginHelper helper = GetNewHelper(page);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");

			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			AssertEquals("User should not be logged in", false, page.SiteUser.IsLoggedIn);

			AssertEquals("OnLoginSuccess should not be called", 0, helper.onLoginSucceed_CallCount);
			AssertEquals("OnLoginFailure should not be called", 0, helper.onLoginFailure_CallCount);
			AssertEquals("TryToRedirectToCustomLoginPage should be called", 1, helper.TryToRedirectToCustomLoginPage_CallCount);
		}

		public void TestTryToRedirectToCustomLoginPageIfLoginFails()
		{
			ZPage page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			LoginManager loginMan = page.DataSource as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("Pre-condition", false, page.AppInstance.ApplicationCookie.CookieExist());
			AssertEquals("CompanyCode is Required by default", true, loginMan.IsCompanyCodeRequired);
			loginMan.IsCompanyCodeRequired = false;

			WebApplicationLoginHelper helper = GetNewHelper(page);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");
			helper.SetParamsValueForTest("UserPassword", "wrongpass");

			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			AssertEquals("User should not be logged in", false, page.SiteUser.IsLoggedIn);

			AssertEquals("OnLoginSuccess should not be called", 0, helper.onLoginSucceed_CallCount);
			AssertEquals("OnLoginFailure should be called", 1, helper.onLoginFailure_CallCount);
			AssertEquals("TryToRedirectToCustomLoginPage should be called", 1, helper.TryToRedirectToCustomLoginPage_CallCount);
		}

		public virtual void TestSignInViaRouting()
		{
			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = GetNewHelper(page);
			var loginMan = page.DataSource as LoginManager;
			loginMan.CompanyCode = org.OH_Code;
			loginMan.UserName = "mehmeh@meh.com.au";
			loginMan.Password = "pass";

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("Precondition: User should not be logged in", !page.SiteUser.IsLoggedIn);
			Assert("Precondition: Should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);

			helper.SignInViaRouting();

			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			Assert("Precondition: Should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals(page.AppInstance.DefaultPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestRetrieveLoginHashFromCookie()
		{
			using (var page = GetNewPageWithRequest())
			{
				var companyCode = "TestCompany";
				var userName = "TestUser";

				var loginSuccessCookieHelper = new LoginSuccessCookieHelper();
				loginSuccessCookieHelper.WriteLoginHashCookie(OrgContactLoginAttemptRecorder.CombineLoginNameAndCompanyCode(companyCode, userName));

				var hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(companyCode, userName);
				AssertEquals("Should retrive hash for valid user", 32, hash?.Length);

				hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(companyCode, "others");
				AssertNull("Should return null for invalid user", hash);
			}
		}

		public void TestWriteLoginHashToCookie()
		{
			using (var page = GetNewPageWithRequest())
			{
				var companyCode = "TestCompany";
				var userName = "TestUser";

				WebApplicationLoginHelper.WriteLoginHashToCookie(companyCode, userName);

				var cookie = page.Response.Cookies[DeviceCookieHelper.GetCookieName(OrgContactLoginAttemptRecorder.CombineLoginNameAndCompanyCode(companyCode, userName))];
				AssertNotNull("Hash cookie is written", cookie);
			}
		}

		public void TestReadWriteLoginHash()
		{
			using (var page = GetNewPageWithRequest())
			{
				var companyCode = "TestCompany";
				var userName = "TestUser";

				WebApplicationLoginHelper.WriteLoginHashToCookie(companyCode, userName);
				var hash1 = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(companyCode, userName);
				AssertEquals(32, hash1.Length);

				WebApplicationLoginHelper.WriteLoginHashToCookie(companyCode, userName);
				var hash2 = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(companyCode, userName);
				AssertEquals(32, hash2.Length);

				AssertNotEquals("Should generate different hash", hash1, hash2);
			}
		}

		public void TestRedirect_DefaultPage()
		{
			AssertRedirectLocationAfterLogin(true, null);
		}

		public void TestRedirect_RelativeReturnUrl()
		{
			AssertRedirectLocationAfterLogin(false, "/Shipments/Shipments.aspx");
		}

		public void TestRedirect_AbsoluteReturnUrl()
		{
			AssertRedirectLocationAfterLogin(true, "http://www.untrustedwebsite.com");
		}

		protected virtual void AssertRedirectLocationAfterLogin(bool expectDefaultPage, string returnURL)
		{
			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();

			HttpContext.Current.Request.QueryString["ReturnURL"] = returnURL;

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var loginMan = page.DataSource as LoginManager;
			var helper = GetNewHelper(page);
			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");
			helper.SetParamsValueForTest("UserPassword", "pass");
			helper.SetParamsValueForTest("RememberMe", "on");

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			Assert("Should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals(expectDefaultPage ? page.AppInstance.DefaultPage : returnURL, HttpContext.Current.Response.RedirectLocation);
		}

		#region Implementation

		protected virtual WebApplicationLoginHelper GetNewHelper(ZPage page)
		{
			return new WebApplicationLoginHelper(page);
		}

		protected virtual WebApplicationLoginHelper GetNewHelper(ZPage page, bool isAutoMaticallyHookupOnLoad)
		{
			return new WebApplicationLoginHelper(page, isAutoMaticallyHookupOnLoad);
		}

		protected virtual ZPage GetNewTestPage()
		{
			ZTestPage result = new ZTestPage();
			result.TestDataSource = GetPageNewDataSource();
			return result;
		}

		protected virtual LoginManager GetPageNewDataSource()
		{
			return new LoginManager();
		}

		protected virtual OrgHeader CreateNewCompany()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = "XXXXX";
			return result;
		}

		protected virtual OrgContact CreateNewContact(OrgHeader company)
		{
			OrgContact result = company.Contacts.AddNew();
			result.OC_ContactName = "Meh Meh";
			result.OC_Email = "mehmeh@meh.com.au";
			result.SetHashedPassword("pass");
			result.OC_WebAccessEnabled = true;
			return result;
		}

		protected ZPage GetNewPageWithRequest()
		{
			ZPage result = GetNewTestPage();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(HttpContext) },
					null);
			method.Invoke(result, new object[] { HttpContext.Current });
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new ZWebTestHelper(Factory);

			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}

		protected override void TearDown()
		{
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				Helper.RestoreEnvironmentSettings();
			}
			base.TearDown();
		}

		ZWebTestHelper Helper;

		#endregion
	}
}
