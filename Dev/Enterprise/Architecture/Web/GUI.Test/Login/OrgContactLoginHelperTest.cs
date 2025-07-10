using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[HttpContextEnabledTest]
	public class OrgContactLoginHelperTest : WebApplicationLoginHelperTest
	{
		public void TestSignIn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals(true, helper.SignIn());
			AssertEquals("Site User should be logged in", true, page.SiteUser.IsLoggedIn);
		}

		public virtual void TestSignInAsSupersededContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Assert(loginContact.OC_WebAccessEnabled);
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var otherActiveContact = CreateNewContact("user0", "user@2.com", "1234", org2);
			otherActiveContact.OC_PER = loginContact.OC_PER;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertEquals("Should not sign in superseded contacts", false, helper.SignIn());
			AssertEquals("Should not sign in superseded contacts", false, page.SiteUser.IsLoggedIn);
		}

		public void TestIsSpecialLogin()
		{
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);
			AssertEquals("Should be a normal login", false, helper.IsSpecialLogin());

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", WebDataRegistry.Instance.WebServiceUsername.Value);
			helper.SetParamsValueForTest("UserPassword", WebDataRegistry.Instance.WebServicePassword.Value);
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Should be a special login", true, helper.IsSpecialLogin());
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

			var page = GetNewPageWithRequest();
			var helper = (OrgContactLoginHelper)GetNewHelper(page);
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

		public void TestSwitchCompany_ShouldNotRecordLoginAttempt()
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

			var page = GetNewPageWithRequest();
			var helper = (OrgContactLoginHelper)GetNewHelper(page);
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

			loginMan.Password = "wrongPassword";
			loginMan.CompanyCode = org2.OH_Code;
			helper.SignIn();

			Assert("Page site user isn't logged in.", !page.SiteUser.IsLoggedIn);
			Assert("Should not log login attempt", !Factory.Load<StmLoginFailureLog>(new ZQuery()).Any());
		}

		#region GetContactFromCredentials

		public void TestGetContactFromCredentials()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Should get contact", loginContact.PK, helper.GetContactFromCredentials().PK);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		#region Superseded Contacts

		public void TestGetContactFromCredentialsSupersededContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Assert(loginContact.OC_WebAccessEnabled);
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var otherActiveContact = CreateNewContact("user0", "user@2.com", "1234", org2);
			otherActiveContact.OC_PER = loginContact.OC_PER;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertEquals("Should get superseded contact", loginContact.PK, helper.GetContactFromCredentials().PK);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		public void TestGetContactFromCredentialsSupersededContactWithNoActiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("not user1", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Assert(loginContact.OC_WebAccessEnabled);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertEquals("Should not get superseded contacts without a related active login contact", null, helper.GetContactFromCredentials());
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		#endregion

		public void TestGetContactFromCredentialsInactiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user1", "user@1.com", "1234", org);
			loginContact.OC_IsActive = false;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			AssertEquals("Should not get inactive contacts", null, helper.GetContactFromCredentials());
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		#endregion

		#region GetContactsFromCredentials

		public void TestGetContactsFromCredentials()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			var (result, contactsFromCredentials) = helper.GetContactsFromCredentials();
			AssertEquals("Should get contact", 1, contactsFromCredentials.Length);
			AssertEquals(LoginContactsResult.Success, result);
			AssertEquals("Should get contact", loginContact.PK, contactsFromCredentials[0].PK);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		#region Superseded Contacts

		public void TestGetContactsFromCredentialsSupersededContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user0", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Assert(loginContact.OC_WebAccessEnabled);
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var otherActiveContact = CreateNewContact("user0", "user@2.com", "1234", org2);
			otherActiveContact.OC_PER = loginContact.OC_PER;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			var (result, contactsFromCredentials) = helper.GetContactsFromCredentials();
			AssertEquals("Should get superseded contact", 1, contactsFromCredentials.Length);
			AssertEquals(LoginContactsResult.Success, result);
			AssertEquals("Should get superseded contact", loginContact.PK, contactsFromCredentials[0].PK);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		public void TestGetContactsFromCredentialsSupersededContactWithNoActiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("not user1", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Assert(loginContact.OC_WebAccessEnabled);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			var (result, contactsFromCredentials) = helper.GetContactsFromCredentials();
			AssertEquals("Should not get superseded contacts without a related active login contact", 0, contactsFromCredentials.Length);
			AssertEquals(LoginContactsResult.Failure, result);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		#endregion

		public void TestGetContactsFromCredentialsInactiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user1", "user@1.com", "1234", org);
			loginContact.OC_IsActive = false;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", loginContact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", "1234");
			helper.SetParamsValueForTest("RememberMe", "on");
			SetupLoginDataFromParamsForTest(helper);

			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);
			var (result, contactsFromCredentials) = helper.GetContactsFromCredentials();
			AssertEquals("Should not get inactive contacts", 0, contactsFromCredentials.Length);
			AssertEquals(LoginContactsResult.Failure, result);
			AssertEquals("Should not set site user", false, page.SiteUser.IsLoggedIn);
		}

		public void TestGetContactsFromCredentialsShouldReturnAllMatchingContacts()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_IsActive = true;
			contactMatch1.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_IsActive = true;
			contactMatch2.SetHashedPassword(commonPassword);
			var contactNonMatch3 = CreateNewContact("user1", commonEmail, string.Empty, org3);
			contactNonMatch3.OC_WebAccessEnabled = true;
			contactNonMatch3.OC_IsActive = true;
			contactNonMatch3.SetHashedPassword("DifferentPass123!");
			var contactNonMatch4 = CreateNewContact("user1", "different@email.com", string.Empty, org4);
			contactNonMatch4.OC_WebAccessEnabled = true;
			contactNonMatch4.OC_IsActive = true;
			contactNonMatch4.SetHashedPassword(commonPassword);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();
			AssertEquals("Precondition", false, page.SiteUser.IsLoggedIn);

			helper.SetParamsValueForTest("UserEmail", commonEmail);
			helper.SetParamsValueForTest("UserPassword", commonPassword);
			helper.SetParamsValueForTest("RememberMe", "on");
			var loginMan = (LoginManager)page.DataSource;
			loginMan.IsCompanyCodeRequired = false;
			SetupLoginDataFromParamsForTest(helper);

			var (result, contactsFromCredentials) = helper.GetContactsFromCredentials();
			AssertEquals("Should return first and second contact", 2, contactsFromCredentials.Length);
			AssertEquals(LoginContactsResult.Success, result);
			var loginContactPKs = contactsFromCredentials.Select(x => x.PK);
			AssertContainsExactElementsInAnyOrder("Should return first and second contact", new[] { contactMatch1.PK, contactMatch2.PK }, loginContactPKs);
		}

		public void TestRedirectToChooseCompanyShouldRememberMe()
		{
			AssertRedirectToChooseCompanyRememberMe(true);
		}

		public void TestRedirectToChooseCompanyShouldNotRememberMe()
		{
			AssertRedirectToChooseCompanyRememberMe(false);
		}

		void AssertRedirectToChooseCompanyRememberMe(ZBool shouldRememberMe)
		{
			var org = CreateNewCompany();
			var contact = CreateNewContact(org);
			var commonEmail = "alex@email.com";
			contact.OC_Email = commonEmail;
			var password = "ChangeMe123!";
			contact.SetHashedPassword(password);
			Factory.Save();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = commonEmail;
			otherContact.SetHashedPassword(password);

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			var loginMan = (LoginManager)page.DataSource;
			helper.LoginContactCandidates.AddRange(new[] { contact, otherContact });
			loginMan.UserName = commonEmail;
			loginMan.Password = password;
			loginMan.RememberMe = shouldRememberMe;

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("Precondition: User should not be logged in", !page.SiteUser.IsLoggedIn);
			Assert("Precondition: Should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);

			helper.RedirectToChooseCompany(new Uri("http://google.com.au/Admin/ChooseCompany.aspx"), new ZGlobalForTesting());

			var redirectURL = HttpContext.Current.Response.RedirectLocation;
			AssertContains("Should redirect to ChooseCompany", "/Admin/ChooseCompany.aspx", redirectURL);
			var uriDeconstructor = new UriDeconstructor(new Uri(redirectURL, UriKind.RelativeOrAbsolute));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[SecureQueryString.QueryStringKey]));
			var identityToken = secureQueryStringDecoded[LoginRouter.IdentityTokenQueryStringKey];
			var redirectUrl = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("/DefaultPage.aspx", redirectUrl);

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Token should be legitimate", true, accessControl.TryPeek(identityToken, AccessTokenTypes.LoginRouterMultiContactIdentity, out var accessToken));
			AssertEquals("Contact should be one of those from LoginContactCandidates", true, accessToken.ParentId.Equals(contact.PK.ToGuid()) || accessToken.ParentId.Equals(otherContact.PK.ToGuid()));
			AssertEquals("Should contain both contact PKs", true,
				accessToken.Scope.Equals(FormattableString.Invariant($"{shouldRememberMe}:{contact.PK},{otherContact.PK}"))
				|| accessToken.Scope.Equals(FormattableString.Invariant($"{shouldRememberMe}:{otherContact.PK},{contact.PK}")));

			var rememberMeCookie = page.AppInstance.ApplicationCookie;
			if (shouldRememberMe)
			{
				AssertEquals("Should save empty company code to cookie", string.Empty, rememberMeCookie.GetCompanyCode());
				AssertEquals("Should save user email to cookie", commonEmail, rememberMeCookie.GetUserEmail());
				AssertEquals("Should not save password to cookie", string.Empty, rememberMeCookie.GetUserPassword());
			}
			else
			{
				AssertEquals(string.Empty, rememberMeCookie.GetCompanyCode());
				AssertEquals(string.Empty, rememberMeCookie.GetUserEmail());
				AssertEquals(string.Empty, rememberMeCookie.GetUserPassword());
			}
		}

		public void TestRedirectToChooseCompanyShouldSetupSession()
		{
			var org = CreateNewCompany();
			var contact = CreateNewContact(org);
			var commonEmail = "alex@email.com";
			contact.OC_Email = commonEmail;
			var password = "ChangeMe123!";
			contact.SetHashedPassword(password);
			Factory.Save();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = commonEmail;
			otherContact.SetHashedPassword(password);

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			var loginMan = (LoginManager)page.DataSource;
			helper.LoginContactCandidates.AddRange(new[] { contact, otherContact });
			loginMan.UserName = commonEmail;
			loginMan.Password = password;

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("Precondition: User should not be logged in", !page.SiteUser.IsLoggedIn);
			Assert("Precondition: Should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);

			helper.RedirectToChooseCompany(new Uri("http://google.com.au/Admin/ChooseCompany.aspx"), new ZGlobalForTesting());
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		#endregion

		public override void TestSignInViaRouting()
		{
			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (OrgContactLoginHelper)GetNewHelper(page);
			var loginMan = page.DataSource as LoginManager;
			loginMan.CompanyCode = org.OH_Code;
			loginMan.UserName = "mehmeh@meh.com.au";
			loginMan.Password = "pass";

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("Precondition: User should not be logged in", !page.SiteUser.IsLoggedIn);
			Assert("Precondition: Should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);

			helper.SignInViaRouting();

			var redirectURL = HttpContext.Current.Response.RedirectLocation;
			AssertContains("Should redirect via login router", "LoginComplete.aspx", redirectURL);
		}

		public override void TestCompanyCodeCanBeEmptyIfLoginManagerAllows()
		{
			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var loginMan = page.DataSource as LoginManager;
			AssertNotNull("LoginManager", loginMan);
			AssertEquals("CompanyCode is Required by default", true, loginMan.IsCompanyCodeRequired);
			loginMan.IsCompanyCodeRequired = false;

			var helper = GetNewHelper(page);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");
			helper.SetParamsValueForTest("UserPassword", "pass");
			helper.SetParamsValueForTest("RememberMe", "on");

			AssertEquals("Pre-condition", false, page.AppInstance.ApplicationCookie.CookieExist());

			helper.OnPageLoad();
			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should not be logged in", !page.SiteUser.IsLoggedIn);
			var redirectURL = HttpContext.Current.Response.RedirectLocation;
			AssertContains("Should redirect via login router", "LoginComplete.aspx", redirectURL);
		}

		protected override void AssertRedirectLocationAfterLogin(bool expectDefaultPage, string returnURL)
		{
			var org = CreateNewCompany();
			CreateNewContact(org);
			Factory.Save();

			HttpContext.Current.Request.QueryString["ReturnURL"] = returnURL;

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = GetNewHelper(page);
			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", "mehmeh@meh.com.au");
			helper.SetParamsValueForTest("UserPassword", "pass");
			helper.SetParamsValueForTest("RememberMe", "on");

			helper.OnPageLoad();

			AssertNotNull("SiteUser", page.SiteUser);
			Assert("User should not be logged in", !page.SiteUser.IsLoggedIn);
			Assert("Should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			var redirectURL = HttpContext.Current.Response.RedirectLocation;
			AssertContains("Should redirect via login router", "LoginComplete.aspx", redirectURL);
			var uriDeconstructor = new UriDeconstructor(new Uri(redirectURL, UriKind.RelativeOrAbsolute));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(expectDefaultPage ? page.AppInstance.DefaultPage : returnURL, originalUrlFromQuery);
		}

		#region Implementation

		protected OrgContact CreateNewContact(string username, string email, string password, OrgHeader company)
		{
			var result = company.Contacts.AddNew();
			result.OC_ContactName = username;
			result.OC_Email = email;
			result.SetHashedPassword(password);
			result.OC_WebAccessEnabled = true;
			return result;
		}

		protected override WebApplicationLoginHelper GetNewHelper(ZPage page)
		{
			return new OrgContactLoginHelperForTest(page);
		}

		protected virtual void SetupLoginDataFromParamsForTest(OrgContactLoginHelper helper)
		{
			((OrgContactLoginHelperForTest)helper).SetupLoginDataFromParamsForTest();
		}

		public class OrgContactLoginHelperForTest : OrgContactLoginHelper
		{
			public OrgContactLoginHelperForTest(ZPage page) : base(page)
			{
			}

			public override void RedirectViaLoginRouter(OrgContact contact)
			{
				var router = new LoginRouterForTest(new Uri(DefaultUrl, UriKind.RelativeOrAbsolute), contact);
				var redirectUri = router.GetRoutingUrl();
				SetRememberMe();
				RedirectToPage(redirectUri.IsAbsoluteUri ? redirectUri.AbsoluteUri : redirectUri.OriginalString);
			}

			public void SetupLoginDataFromParamsForTest()
			{
				SetUpLoginDataFromParams();
			}
		}

		public class LoginRouterForTest : LoginRouter
		{
			public LoginRouterForTest(Uri originalUrl, OrgContact contact) : base(originalUrl, contact)
			{
			}

			protected override Uri DefaultUrl { get; }

			protected override IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors
			{
				get
				{
					var descriptors = new List<ILoginRoutingDescriptor>(base.RoutingDescriptors)
					{
						new ChangeContactNameRoutingDescriptor(Contact)
					};

					return descriptors;
				}
			}

			protected override ZGlobal GetNewGlobal()
			{
				return new ZGlobalForTesting();
			}
		}

		class ChangeContactNameRoutingDescriptor : ILoginRoutingDescriptor
		{
			public ChangeContactNameRoutingDescriptor(OrgContact contact)
			{
				this.contact = contact;
			}
			readonly OrgContact contact;

			public Uri RoutingUrl => new Uri("https://myaccount.com/changename.aspx?token=12345");

			public bool IsRoutingRequired => contact.OC_ContactName == "user1";

			public void RoutingAction()
			{
			}
		}

		#endregion
	}
}
