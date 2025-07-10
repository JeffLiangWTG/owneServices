using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class RoutingEnabledPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRedirectViaLoginRouter_OriginalUrlString()
		{
			var originalUrl = "https://google.com.au/";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "all@work.com";
			contact.SetHashedPassword("noplay");
			Factory.Save();
			var page = GetNewControl();
			page.RedirectViaLoginRouterExposed(originalUrl, contact);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("Should match original redirect url", originalUrl, originalUrlFromQuery);
		}

		[ExpectNoExceptions]
		public void TestRedirectViaLoginRouter_OriginalUrlRelativeString()
		{
			var relativeUrl = "~/Admin/SwitchCompany.aspx";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "all@work.com";
			contact.SetHashedPassword("noplay");
			Factory.Save();
			var page = GetNewControl();
			page.RedirectViaLoginRouterExposed(relativeUrl, contact);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("Should remain as relative path", "~/Admin/SwitchCompany.aspx", originalUrlFromQuery);
		}

		[ExpectNoExceptions]
		public void TestRedirectViaLoginRouter_NoOriginalUrl()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "all@work.com";
			contact.SetHashedPassword("noplay");
			Factory.Save();
			var page = GetNewControl();
			page.RedirectViaLoginRouterExposed(string.Empty, contact);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("Should not have an original redirect url", null, originalUrlFromQuery);
		}

		[ExpectNoExceptions]
		public void TestRedirectViaLoginRouter_OriginalUri()
		{
			var originalUri = new Uri("https://google.com.au/");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "all@work.com";
			contact.SetHashedPassword("noplay");
			Factory.Save();
			var page = GetNewControl();
			page.RedirectViaLoginRouterExposed(originalUri, contact);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("Should match original redirect url", originalUri.AbsoluteUri, originalUrlFromQuery);
		}

		[ExpectNoExceptions]
		public void TestRedirectViaLoginRouter_OriginalUrlRelative()
		{
			var relativeUri = new Uri("~/Admin/SwitchCompany.aspx", UriKind.Relative);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "all@work.com";
			contact.SetHashedPassword("noplay");
			Factory.Save();
			var page = GetNewControl();
			page.RedirectViaLoginRouterExposed(relativeUri, contact);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals("Should remain as relative path", "~/Admin/SwitchCompany.aspx", originalUrlFromQuery);
		}

		public void TestMyAccountLoginRouterIdentityManager()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
			var page = GetNewControl();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var identityManager = page.IdentityManagerExposed;
			AssertEquals(contact.PK, identityManager.Contact.PK);
		}

		public void TestShouldSetupSessionOnLoad()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = token };
			var page = GetNewControl();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var identityManager = page.IdentityManagerExposed;
			AssertEquals("Precondition", true, identityManager.IsValidID());
			page.OnLoadExposed();
			AssertNotEquals("Site user should be setup", null, page.AppInstance.SiteUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		public void TestShouldNotSetupSessionOnLoadIfInvalidIdentityManager()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			var page = GetNewControl();
			var identityManager = page.IdentityManagerExposed;
			AssertEquals("Precondition", false, identityManager.IsValidID());
			page.OnLoadExposed();
			AssertNotEquals("Site user should be setup", null, page.AppInstance.SiteUser);
			AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentDepartmentPK);
		}

		#region Implementation
		RoutingEnabledPageForTest GetNewControl()
		{
			var testPage = new RoutingEnabledPageForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		class RoutingEnabledPageForTest : RoutingEnabledPage
		{
			public void RedirectViaLoginRouterExposed(string originalUrl, OrgContact contact)
			{
				RedirectViaLoginRouter(originalUrl, contact);
			}

			public void RedirectViaLoginRouterExposed(Uri originalUrl, OrgContact contact)
			{
				RedirectViaLoginRouter(originalUrl, contact);
			}

			public MyAccountLoginRouterIdentityManager IdentityManagerExposed => IdentityManager;
			public void OnLoadExposed() => OnLoad(EventArgs.Empty);
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
		#endregion
	}
}
