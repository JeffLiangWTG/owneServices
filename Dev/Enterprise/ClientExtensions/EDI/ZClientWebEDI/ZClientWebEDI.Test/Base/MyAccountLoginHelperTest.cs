using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class MyAccountLoginHelperTest : WebApplicationLoginHelperTest
	{
		public void TestUserLoginShouldNotCreateEventLog()
		{
			WebDataRegistry.Instance.WebActivityLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = CreateNewCompany();
			var cwSupportContact = org.Contacts.AddNew();
			cwSupportContact.OC_ContactName = "CWSupport";
			cwSupportContact.OC_Email = "mehmeh@meh.com.au";
			cwSupportContact.SetHashedPassword("pass1");
			cwSupportContact.OC_WebAccessEnabled = true;
			var userContact = org.Contacts.AddNew();
			userContact.OC_ContactName = "User";
			userContact.OC_Email = "user@meh.com.au";
			userContact.SetHashedPassword("pass2");
			userContact.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertLoginShouldNotCreateEventLog(org.OH_Code, User.SupportUserName, CWSupportLoginToken.TokenForTest);
			AssertLoginShouldNotCreateEventLog(org.OH_Code, User.WebUserName, User.WebTransientPassword);
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebUserAlex");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebUserAlexPass");
			AssertLoginShouldNotCreateEventLog(org.OH_Code, WebDataRegistry.Instance.WebServiceUsername.Value, WebDataRegistry.Instance.WebServicePassword.Value);
			AssertLoginShouldNotCreateEventLog(org.OH_Code, userContact.OC_Email, "pass2", isSpecialUser: false);
		}

		void AssertLoginShouldNotCreateEventLog(string orgCode, string userName, string password, bool isSpecialUser = true)
		{
			var page = GetNewBasePage();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			var loginMan = page.DataSource as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("CompanyCode is Required by default", true, loginMan.IsCompanyCodeRequired);
			loginMan.IsCompanyCodeRequired = false;
			if (isSpecialUser)
			{
				var helper = GetNewMyAccountHelper(page);
				helper.SetParamsValueForTest("CompanyCode", orgCode);
				helper.SetParamsValueForTest("UserEmail", userName);
				helper.SetParamsValueForTest("UserPassword", password);
				helper.SetParamsValueForTest("RememberMe", "on");
				helper.OnPageLoad();
			}
			else
			{
				var helper = new MyAccountLoginHelperForTest(page);
				helper.LoginMan_Exposed.CompanyCode = orgCode;
				helper.LoginMan_Exposed.UserName = userName;
				helper.LoginMan_Exposed.Password = password;
				helper.SignIn();
			}

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			AssertEquals("Is special user", isSpecialUser, page.SiteUser.IsSpecialUser);
			var query = new ZQuery(StmALogSchema.SL_Parent, page.SiteUser.LoggedInUserPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Login.Code);
			AssertNull(Factory.LoadTop1<StmALog>(query));
		}

		public void TestSuperUserLoginShouldSetupEnv()
		{
			var org = CreateNewCompany();
			var cwSupportContact = org.Contacts.AddNew();
			cwSupportContact.OC_ContactName = "CWSupport";
			cwSupportContact.OC_Email = "mehmeh@meh.com.au";
			cwSupportContact.SetHashedPassword("pass");
			cwSupportContact.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertLoginShouldSetupEnv(org.OH_Code, User.SupportUserName, CWSupportLoginToken.TokenForTest);
			AssertLoginShouldSetupEnv(org.OH_Code, User.WebUserName, User.WebTransientPassword);
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebUserAlex");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "WebUserAlexPass");
			AssertLoginShouldSetupEnv(org.OH_Code, WebDataRegistry.Instance.WebServiceUsername.Value, WebDataRegistry.Instance.WebServicePassword.Value);
		}

		void AssertLoginShouldSetupEnv(string orgCode, string userName, string password)
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				Env.ClearUserContext();
			}
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			AssertEquals("Precondition: User context should be unset", null, GlbStaff.CurrentUser);
			var page = GetNewBasePage();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			var helper = GetNewMyAccountHelper(page);
			helper.SetParamsValueForTest("CompanyCode", orgCode);
			helper.SetParamsValueForTest("UserEmail", userName);
			helper.SetParamsValueForTest("UserPassword", password);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			Assert("Is special user", page.SiteUser.IsSpecialUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			AssertNotEquals("User context should be set", null, GlbStaff.CurrentUser);
		}

		public void TestWebServiceUserShouldSetupEnv()
		{
			var org = CreateNewCompany();
			var cwSupportContact = org.Contacts.AddNew();
			cwSupportContact.OC_ContactName = "CWSupport";
			cwSupportContact.OC_Email = "mehmeh@meh.com.au";
			cwSupportContact.SetHashedPassword("pass");
			cwSupportContact.OC_WebAccessEnabled = true;
			Factory.Save();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			AssertEquals("Precondition: User context should be unset", null, GlbStaff.CurrentUser);
			var page = GetNewBasePage();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			var helper = GetNewMyAccountHelper(page);
			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", User.SupportUserName);
			helper.SetParamsValueForTest("UserPassword", CWSupportLoginToken.TokenForTest);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should be logged in", page.SiteUser.IsLoggedIn);
			Assert("Is super user", page.SiteUser.IsSuperUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			AssertNotEquals("User context should be set", null, GlbStaff.CurrentUser);
		}

		public void TestRedirectViaLoginRouter()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var helper = new MyAccountLoginHelper(page);
			helper.RedirectViaLoginRouter(contact);
			AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", page.Response.RedirectLocation);
		}

		public void TestRedirectViaLoginRouter_WithRememberMeOptionOn()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var zPage = page as ZPage;
			var helper = new MyAccountLoginHelperForTest(page);
			AssertEquals(false, zPage.AppInstance.ApplicationCookie.CookieExist());
			helper.LoginMan_Exposed.UserName = "hamlet@shakespeare.com";
			helper.LoginMan_Exposed.CompanyCode = "TST";
			helper.LoginMan_Exposed.Password = "test";
			helper.LoginMan_Exposed.RememberMe = true;
			helper.RedirectViaLoginRouter(contact);
			AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", page.Response.RedirectLocation);
			var cookie = zPage.AppInstance.ApplicationCookie;
			Assert("Application cookie should be created", cookie.CookieExist());
			AssertEquals("Should save user email to cookie", "hamlet@shakespeare.com", cookie.GetUserEmail());
			AssertEquals("Should save company code to cookie", "TST", cookie.GetCompanyCode());
			AssertEquals("Should not save password to cookie", string.Empty, cookie.GetUserPassword());
		}

		public void TestRedirectViaLoginRouter_WithRememberMeOptionOnNoCompanyCode()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var zPage = page as ZPage;
			var helper = new MyAccountLoginHelperForTest(page);
			AssertEquals(false, zPage.AppInstance.ApplicationCookie.CookieExist());
			helper.LoginMan_Exposed.UserName = "hamlet@shakespeare.com";
			helper.LoginMan_Exposed.Password = "test";
			helper.LoginMan_Exposed.RememberMe = true;
			helper.RedirectViaLoginRouter(contact);
			AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", page.Response.RedirectLocation);
			var cookie = zPage.AppInstance.ApplicationCookie;
			Assert("Application cookie should be created", cookie.CookieExist());
			AssertEquals("Should save user email to cookie", "hamlet@shakespeare.com", cookie.GetUserEmail());
			AssertEquals("Should save company code to cookie", contact.OrganisationCode, cookie.GetCompanyCode());
			AssertEquals("Should not save password to cookie", string.Empty, cookie.GetUserPassword());
		}

		public void TestRedirectViaLoginRouter_WithRememberMeOptionOff()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var zPage = page as ZPage;
			var helper = new MyAccountLoginHelperForTest(page);
			AssertEquals(false, zPage.AppInstance.ApplicationCookie.CookieExist());
			helper.LoginMan_Exposed.UserName = "hamlet@shakespeare.com";
			helper.LoginMan_Exposed.CompanyCode = "TST";
			helper.LoginMan_Exposed.Password = "test";
			helper.LoginMan_Exposed.RememberMe = false;
			helper.RedirectViaLoginRouter(contact);
			AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", page.Response.RedirectLocation);
			var cookie = zPage.AppInstance.ApplicationCookie;
			Assert("Application cookie should not be created", !cookie.CookieExist());
		}

		public void TestRedirectViaLoginRouter_ShouldSetSwitchCompanyFunc()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var zPage = page as ZPage;
			var helper = new MyAccountLoginHelperForTest(page);
			AssertEquals(false, zPage.AppInstance.ApplicationCookie.CookieExist());
			helper.LoginMan_Exposed.UserName = "hamlet@shakespeare.com";
			helper.LoginMan_Exposed.CompanyCode = "TST";
			helper.LoginMan_Exposed.Password = "test";
			helper.LoginMan_Exposed.RememberMe = false;
			helper.RedirectViaLoginRouter(contact);
			AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", page.Response.RedirectLocation);
			AssertNotNull("Switch company function should be set", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
		}

		public void TestRedirectIfLoggedInShouldNotRedirectIfNotLoggedIn()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			var helper = new MyAccountLoginHelperForTest(page);
			helper.SetIsAuthenticated(true);
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			helper.RedirectIfLoggedIn();
			AssertEquals("Should not redirect since site user is not logged in", null, page.Response.RedirectLocation);
		}

		public void TestRedirectIfLoggedInShouldNotRedirectIfNotAuthenticated()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
			var helper = new MyAccountLoginHelperForTest(page);
			AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
			helper.RedirectIfLoggedIn();
			AssertEquals("Should not redirect since request is not authenticated", null, page.Response.RedirectLocation);
		}

		public void TestRedirectIfLoggedInShouldRedirectIfLoggedInAndAuthenticated()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			var page = GetNewBasePage();
			page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
			var helper = new MyAccountLoginHelperForTest(page);
			helper.SetIsAuthenticated(true);
			AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
			helper.RedirectIfLoggedIn();
			AssertNotEquals("Should redirect since request is authenticated and site user is logged in", null, page.Response.RedirectLocation);
		}

		public void TestSwitchCompany()
		{
			AssertNull("Prerequisite: Switch company function should not be set", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact = CreateNewContact(org1);
			contact.OC_Email = "test@test.com";
			contact.SetHashedPassword("password");
			contact = CreateNewContact(org2);
			contact.OC_Email = "test@test.com";
			contact.SetHashedPassword("password");
			contact = CreateNewContact(org3);
			contact.OC_Email = "other@test.com";
			contact.SetHashedPassword("other");
			Factory.Save();
			var page = GetNewBasePage();
			var helper = GetNewMyAccountHelper(page);
			helper.IsRedirectAfterSignIn = false;
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			var loginMan = (LoginManager)page.DataSource;
			loginMan.CompanyCode = org1.OH_Code;
			loginMan.UserName = "test@test.com";
			loginMan.Password = "password";
			helper.SignIn();
			Assert("Page site user is logged in.", page.SiteUser.IsLoggedIn);
			AssertNotNull("Switch company function should be set", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
			AssertEquals(org1.OH_Code, page.SiteUser.AffiliationCode);
			loginMan.Password = string.Empty;
			loginMan.CompanyCode = org2.OH_Code;
			helper.SignIn();
			Assert("Page site user is logged in.", page.SiteUser.IsLoggedIn);
			AssertNotNull("Switch company function should be set", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
			AssertEquals(org2.OH_Code, page.SiteUser.AffiliationCode);
			loginMan.UserName = "other@test.com";
			loginMan.Password = "other";
			loginMan.CompanyCode = org3.OH_Code;
			helper.SignIn();
			Assert("Page site user is logged in.", page.SiteUser.IsLoggedIn);
			AssertNotNull("Switch company function should be set", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
			AssertEquals(org3.OH_Code, page.SiteUser.AffiliationCode);
			loginMan.Password = "wrong";
			helper.SignIn();
			Assert("Page site user is not logged in.", !page.SiteUser.IsLoggedIn);
			AssertNull("Switch company function should be empty", HttpContext.Current.Session[OrgContactLoginHelper.SwitchCompanyFuncSessionKey]);
		}

		public override void TestOnPageLoad_CookieOnlyContainsUserNameAndPassword()
		{
			var page = GetNewBasePage();
			var helper = new MyAccountLoginHelperForTest(page);
			page.AppInstance.ApplicationCookie.WriteUser(string.Empty, "mehmeh@meh.com.au", "password");
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);
			AssertEquals("Pre-condition", "", helper.LoginMan_Exposed.CompanyCode);
			AssertEquals("Pre-condition", "", helper.LoginMan_Exposed.UserName);
			AssertEquals("Pre-condition", true, page.AppInstance.ApplicationCookie.CookieExist());
			AssertEquals("Pre-condition", true, helper.LoginMan_Exposed.AllowCompanyCodeAndUsernameToBeChanged);
			helper.LoginMan_Exposed.IsCompanyCodeRequired = false;
			helper.OnPageLoad();
			AssertEquals(string.Empty, helper.LoginMan_Exposed.CompanyCode);
			AssertEquals("mehmeh@meh.com.au", helper.LoginMan_Exposed.UserName);
			AssertEquals("password", helper.LoginMan_Exposed.Password);
			TimeSpan timeSpan = DateTime.Now - page.Response.Cookies[page.AppInstance.ApplicationCookie.CookieName].Expires; // datetime in cookie is unrelated to server time
			AssertNotEquals("Should not be removed / made expired", 1, timeSpan.Days);
			AssertEquals(true, helper.LoginMan_Exposed.AllowCompanyCodeAndUsernameToBeChanged);
			AssertEquals("OnLoginFailure should not be called because Login was not tried", 0, helper.OnLoginFailure_CallCount);
			AssertEquals("OnLoginSucceed should not be called because Login was not tried", 0, helper.OnLoginSucceed_CallCount);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		protected virtual MyAccountLoginHelper GetNewMyAccountHelper(BasePage page)
		{
			return new MyAccountLoginHelperForTest(page);
		}

		protected virtual BasePage GetNewBasePage()
		{
			var result = new BasePageForTest();
			result.TestDataSource = new LoginManager();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			method.Invoke(result, new object[] { HttpContext.Current });
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, result, null);
			return result;
		}

		public class BasePageForTest : BasePage
		{
			BusinessObject fTestDataSource;
			public virtual BusinessObject TestDataSource
			{
				get
				{
					return fTestDataSource;
				}

				set
				{
					fTestDataSource = value;
				}
			}

			protected override BusinessObject GetNewDataSource()
			{
				return TestDataSource;
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		public class MyAccountLoginHelperForTest : MyAccountLoginHelper
		{
			public MyAccountLoginHelperForTest(BasePage page) : base(page)
			{
			}

			protected override void RedirectAfterSignIn()
			{
			}

			protected override bool IsAuthenticated => isAuthenticated;
			bool isAuthenticated;
			public void SetIsAuthenticated(bool shouldBeAuthenticated)
			{
				isAuthenticated = shouldBeAuthenticated;
			}

			public LoginManager LoginMan_Exposed => base.LoginMan;
		}
		#endregion
	}
}
