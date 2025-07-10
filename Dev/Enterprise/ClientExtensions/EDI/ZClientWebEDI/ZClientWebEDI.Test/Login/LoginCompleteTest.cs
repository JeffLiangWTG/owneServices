using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LoginCompleteTest : TestCaseWithFactory
	{
		public void TestPageLoad_UserIsLockedOut()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WISGLOTST";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.OC_OH = org.PK;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(string.Empty, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterIdentity, tokenInfo, TimeSpan.FromMinutes(15), 1);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, HttpUtility.UrlEncode(secureQueryString.ToString()));

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				page.SiteUser.Login("WISGLOTST", "user.one@test.com", "12345678");
			}

			page.DoPageLoad();
			AssertEquals(true, page.SiteUser.IsLoggedIn);
		}

		public void TestPageLoad_UserAccountToken()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(user);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals("http://test.org/", page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertNotEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_ContactToken()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals("http://test.org/", page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertEquals(ZGuid.Empty, ((MyAccountWebUser)page.SiteUser).LoggedInUserAccountPK);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertNotEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_ContactIsValidForWebLogin()
		{
			EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.OC_IsActive = false;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(user);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals("Precondition", false, contact.IsValidForWebLogin);
			AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
			AssertEquals(false, page.SiteUser.IsLoggedIn);
			AssertEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_UserAccountRequiringRouting()
		{
			EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			user.EUA_IsContactRelationshipActive = false;
			user.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(user);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
			AssertEquals(false, page.SiteUser.IsLoggedIn);
			AssertEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_ContactRequiringRouting()
		{
			EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
			AssertEquals(false, page.SiteUser.IsLoggedIn);
			AssertEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_AlreadyLoggedIn()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DAJBEF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();
				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				AssertEquals("Precondition", contact.PK, page.SiteUser.LoggedInUserPK);
				page.SetIsAuthenticated(true);
				var originalLoginLogsCount = contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code);
				page.DoPageLoad();
				AssertEquals("Should not have created login log", originalLoginLogsCount, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code));
				AssertEquals("http://test.org/", page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_UserAccountAlreadyLoggedIn()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var db = licence.Database;
				db.LD_TenantID = "SYS0399481";
				db.LD_DatabaseNumber = 8000;
				var org = db.WebAccessOrg;
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var user = Factory.New<EdiCustomerUserAccount>();
				user.EUA_LD = db.PK;
				user.EUA_UserID = "U048173";
				user.EUA_FullName = "User One";
				user.EUA_Email = "user.one@test.com";
				user.EUA_OC_WebAccessContact = contact.PK;
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();
				var token = MyAccountLoginRouterIdentityManager.GenerateToken(user);
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				AssertEquals("Precondition", contact.PK, page.SiteUser.LoggedInUserPK);
				page.SetIsAuthenticated(true);
				var originalLoginLogsCount = contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code);
				page.DoPageLoad();
				AssertEquals("Should not have created login log", originalLoginLogsCount, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code));
				AssertEquals("http://test.org/", page.Response.RedirectLocation);
			}
		}

		public void TestPageLoad_AlreadyLoggedInButNotAuthenticated()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DAJBEF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();
				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				AssertEquals("Precondition", contact.PK, page.SiteUser.LoggedInUserPK);
				var originalLoginLogsCount = contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code);
				page.DoPageLoad();
				AssertEquals("Should have created login log", originalLoginLogsCount + 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code));
				AssertEquals("http://test.org/", page.Response.RedirectLocation);
				AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			}
		}

		public void TestPageLoad_AuthenticatedButNotLoggedIn()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DAJBEF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = LoginRouterIdentityManager.GenerateToken(contact), [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
				var originalLoginLogsCount = contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code);
				page.SetIsAuthenticated(true);
				page.DoPageLoad();
				AssertEquals("Should have created login log", originalLoginLogsCount + 1, contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.Login.Code));
				AssertEquals("http://test.org/", page.Response.RedirectLocation);
			}
		}

		public void TestPageLoad_NoOriginalUrl()
		{
			EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DAJBEF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();
			AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"]?.Value ?? string.Empty);
			AssertNotEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestPageLoad_ShouldSetupSessionIfNull()
		{
			EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DAJBEF";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			AssertEquals("Precondition: User context should be unset", null, GlbStaff.CurrentUser);
			page.DoPageLoad();
			AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
			AssertEquals(true, page.SiteUser.IsLoggedIn);
			AssertEquals(contact.PK, page.SiteUser.LoggedInOrgContact.PK);
			AssertEquals("Token should be consumed", false, ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			AssertNotEquals("User context should be set", null, GlbStaff.CurrentUser);
		}

		[TestDate(2023, 7, 12)]
		[TestUtcOffset(10, 0, 0)]
		public void TestPageLoad_NoCurrentSupport()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			licence.LA_ContractExpiryDate = new ZDateTime(2023, 7, 1);

			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token, [LoginRouter.OriginalUrlQueryStringKey] = "http://test.org/" };
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			page.DoPageLoad();

			AssertStartsWith("Should redirect to login page", "/webapp/Login/LoginLite.aspx?data=", page.Response.RedirectLocation);
			AssertEquals(false, page.SiteUser.IsLoggedIn);
		}

		public void TestEventLog()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DAJBEF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				AssertEquals(false, AutoLoginHelper.CheckPreviousLoginStatus(page.Request.QueryString));
				page.DoPageLoad();
				AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
				AssertEquals(true, page.SiteUser.IsLoggedIn);
				AssertNotNull(contact.Logs.Find(x => x.SL_SE_NKEvent == "LGI").Single());
			}
		}

		public void TestEventLog_NoLogForPreviousLogin()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.MyAccountIndexPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DAJBEF";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "User One";
				contact.OC_Email = "user.one@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				contact.Person.PER_EmailAddress = contact.OC_Email;
				Factory.Save();

				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
				secureQueryString.Add("UDF_MYA_PLI", "1");
				var page = GetPageForTest();
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				AssertEquals(true, AutoLoginHelper.CheckPreviousLoginStatus(page.Request.QueryString));
				page.DoPageLoad();
				AssertEquals(EDIDataRegistry.Instance.MyAccountIndexPage.Value, page.Response.RedirectLocation);
				AssertEquals(true, page.SiteUser.IsLoggedIn);
				AssertEquals(false, contact.Logs.Find(x => x.SL_SE_NKEvent == "LGI").Any());
			}
		}

		#region Implementation
		LoginCompleteForTest GetPageForTest()
		{
			var page = new LoginCompleteForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class LoginCompleteForTest : LoginComplete
		{
			public void DoPageLoad()
			{
				try
				{
					base.Page_Load(null, EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException || ex is NullReferenceException)
					{
						throw;
					}
				}
			}

			protected override bool IsAuthenticated => isAuthenticated;
			bool isAuthenticated;
			public void SetIsAuthenticated(bool shouldBeAuthenticated)
			{
				isAuthenticated = shouldBeAuthenticated;
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

			public string LastReportedMessage;
			protected override void ReportErrorCore(string reportKey, string message)
			{
				LastReportedMessage = message;
			}
		}
		#endregion
	}
}
