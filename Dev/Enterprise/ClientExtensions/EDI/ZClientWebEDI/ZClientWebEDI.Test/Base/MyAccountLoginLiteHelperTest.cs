using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class MyAccountLoginLiteHelperTest : ZArchitecture.Web.GUI.WebControls.Testing.ZPageTestCase
	{
		public void TestRedirect()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://127.0.0.1/"))
			{
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "/myaccount/abc.htm?a=1");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginLiteHelperForTest(Page as BasePage);
				Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
				helper.Redirect_Exposed();
				AssertContains("/myaccount/abc.htm?a=1", helper.DefaultUrl_Exposed);
				AssertContains("/myaccount/abc.htm?a=1", Page.Response.RedirectLocation);
			}
		}

		public void TestRedirect_RelativeURI_NoException()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				var address1 = new Uri("http://www.contoso.com/");
				var address2 = new Uri("http://www.contoso.com/index.htm?date=today");
				var relativeUri = address1.MakeRelativeUri(address2);
				var prop = Page.Request.GetType().GetField("_referrer", BindingFlags.NonPublic | BindingFlags.Instance);
				prop.SetValue(Page.Request, relativeUri);
				AssertEquals(false, Page.Request.UrlReferrer.IsAbsoluteUri);
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "/myaccount/abc.htm?a=1&LoginRedirect=1");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginLiteHelperForTest(Page as BasePage);
				Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
				helper.Redirect_Exposed();
				AssertContains("/myaccount/abc.htm?a=1&LoginRedirect=1", helper.DefaultUrl_Exposed);
				AssertEquals("/myaccount/abc.htm?a=1&LoginRedirect=1", Page.Response.RedirectLocation);
			}
		}

		public void TestRedirect_ContainsResizeInlineFramePage()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "/myaccount/resize-iframe.html?id=abc");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginLiteHelperForTest(Page as BasePage);
				Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
				helper.Redirect_Exposed();
				AssertContains("https://myaccount.com/Home.aspx", helper.DefaultUrl_Exposed);
				AssertContains("https://myaccount.com/Home.aspx", Page.Response.RedirectLocation);
			}
		}

		public void TestRedirectViaLoginRouterDefaultUrl()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_WebAccessEnabled = true;
				contact.OC_Email = "hamlet@shakespeare.com";
				Factory.Save();
				var helper = new MyAccountLoginLiteHelper(Page as BasePage);
				helper.RedirectViaLoginRouter(contact);
				AssertStartsWith("Should be redirected via MyAccountLoginRouter", "/webapp/Login/", Page.Response.RedirectLocation);
				var uriDeconstructor = new UriDeconstructor(new Uri(Page.Response.RedirectLocation, UriKind.Relative));
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
				AssertEquals("Should not have an original redirect url", ((Global)Page.AppInstance).HostingSiteHomePage, originalUrl);
			}
		}

		public void TestRedirect_OpenRedirectAttack()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "www.supersafe.com");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginLiteHelperForTest(Page as BasePage);
				Assert(string.IsNullOrEmpty(Page.Response.RedirectLocation));
				helper.Redirect_Exposed();
				AssertEquals("https://myaccount.com/Home.aspx", Page.Response.RedirectLocation);
			}
		}

		public void TestDefaultUrl_OpenRedirectAttack()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				Page.Request.QueryString.Add("a", "1");
				Page.Request.QueryString.Add("ReturnUrl", "www.supersafe.com");
				Page.Request.QueryString.Add("b", "2");
				var helper = new MyAccountLoginLiteHelperForTest(Page as BasePage);
				AssertEquals("https://myaccount.com/Home.aspx", helper.DefaultUrl_Exposed);
				Page.Request.QueryString["ReturnUrl"] = "/myaccount/abc.htm?a=1";
				AssertEquals("https://myaccount.com/myaccount/abc.htm?a=1", helper.DefaultUrl_Exposed);
			}
		}

		public void TestCookies()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_Email = "TestUser@wisetechglobal.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			contact.Person.PER_FullName = "Franky";
			contact.Person.SetHashedPassword("5678");
			Factory.Save();
			Page.SiteUser.Login(contact.OrgCode, contact.OC_Email, "5678");

			var basePage = Page as BasePage;
			MyAccountLoginLiteHelper.CreateLiteViewModeCookies(basePage);

			AssertNull("Old cookie name should not be used", basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER"].Value);

			Assert(basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"].Expires.Minute <= HttpContext.Current.Session.Timeout);
			Assert(basePage.Response.Cookies["SECURITYRIGHTS"].Expires.Minute <= HttpContext.Current.Session.Timeout);

			var loggedInUserCookieValue = basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]?.Value ?? string.Empty;
			AssertNotEquals(string.Empty, loggedInUserCookieValue);
			AssertNotEquals($"ContactPk={contact.PK}&IsSuperUser=False&ContactName=Franky&CompanyName=WiseTech+Global", loggedInUserCookieValue);

			var decryptedCookieValue = Encoding.ASCII.GetString(MachineKey.Unprotect(HttpServerUtility.UrlTokenDecode(loggedInUserCookieValue)));
			AssertEquals($"ContactPk={contact.PK}&IsSuperUser=False&ContactName=Franky&CompanyName=WiseTech+Global", decryptedCookieValue);
		}

		public void TestRefreshCookiesExpire()
		{
			HttpContext.Current.Request.Cookies.Add(new HttpCookie(MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed) { Expires = ZDateTime.Now.AddMinutes(1).ToDateTime() });
			HttpContext.Current.Request.Cookies.Add(new HttpCookie(MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed) { Expires = ZDateTime.Now.AddMinutes(1).ToDateTime() });

			MyAccountLoginLiteHelper.RefreshCookiesExpire();

			Assert(HttpContext.Current.Response.Cookies[MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed].Expires > ZDateTime.Now.AddMinutes(1).ToDateTime());
			Assert(HttpContext.Current.Response.Cookies[MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed].Expires > ZDateTime.Now.AddMinutes(1).ToDateTime());
		}

		public void TestRefreshCookiesExpire_DoNotOverwriteDifferentLoggedInUserCookie()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WiseTech";
			org.OH_FullName = "WiseTech Global";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test User 1";
			contact1.OC_Email = "TestUser1@wisetechglobal.com";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test User 2";
			contact2.OC_Email = "TestUser2@wisetechglobal.com";
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;

			var orgSecurityRight = org.SecurityRights.Cast<OrgSecurity>().First();
			var securityRight1 = contact1.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().First(s => s.OZ_OX == orgSecurityRight.PK);
			securityRight1.OZ_Granted = true;
			var securityRight2 = contact2.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().First(s => s.OZ_OX == orgSecurityRight.PK);
			securityRight1.OZ_Granted = false;

			Factory.Save();

			contact1.Person.PER_FullName = "Test User 1";
			contact1.Person.SetHashedPassword("1234");
			contact2.Person.PER_FullName = "Test User 2";
			contact2.Person.SetHashedPassword("5678");

			Factory.Save();

			var basePage = Page as BasePage;
			Page.SiteUser.Login(contact1.OrgCode, contact1.OC_Email, "1234");
			MyAccountLoginLiteHelper.CreateLiteViewModeCookies(basePage);
			MyAccountLoginLiteHelper.RefreshCookiesExpire();

			var loggedInUserCookieValue1 = basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"].Value;
			var securityRightsCookieValue1 = basePage.Response.Cookies["SECURITYRIGHTS"].Value;

			HttpContext.Current.Request.Cookies.Add(basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"]);
			HttpContext.Current.Request.Cookies.Add(basePage.Response.Cookies["SECURITYRIGHTS"]);
			Page.SiteUser.Logout();
			basePage.Response.Cookies.Clear();

			Page.SiteUser.Login(contact2.OrgCode, contact2.OC_Email, "5678");
			MyAccountLoginLiteHelper.CreateLiteViewModeCookies(basePage);
			MyAccountLoginLiteHelper.RefreshCookiesExpire();

			var loggedInUserCookieValue2 = basePage.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"].Value;
			var securityRightsCookieValue2 = basePage.Response.Cookies["SECURITYRIGHTS"].Value;
			AssertNotEquals(loggedInUserCookieValue1, loggedInUserCookieValue2);
			AssertNotEquals(securityRightsCookieValue1, securityRightsCookieValue2);
		}

		void InsertLiteViewCookie()
		{
			HttpContext.Current.Request.Cookies.Add(new HttpCookie(MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed) { Expires = ZDateTime.Now.AddDays(100).ToDateTime() });
			HttpContext.Current.Request.Cookies.Add(new HttpCookie(MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed) { Expires = ZDateTime.Now.AddDays(100).ToDateTime() });
		}

		public void TestRedirectIfLoggedIn()
		{
			InsertLiteViewCookie();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			using (var page = GetNewZPage() as BasePage)
			{
				page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
				var helper = new MyAccountLoginLiteHelperForTest(page);
				helper.SetIsAuthenticated(true);
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				helper.RedirectIfLoggedIn();

				Assert(!page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed));
				Assert(!page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed));
			}
		}

		public void TestRedirectIfLoggedIn_NotLoggedIn()
		{
			InsertLiteViewCookie();
			using (var page = GetNewZPage() as BasePage)
			{
				var helper = new MyAccountLoginLiteHelperForTest(page);
				helper.SetIsAuthenticated(true);
				AssertEquals(false, page.SiteUser.IsLoggedIn);
				helper.RedirectIfLoggedIn();

				Assert(page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed));
				Assert(page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed));
				Assert(page.Response.Cookies[MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed].Expires < ZDateTime.Now.ToDateTime());
				Assert(page.Response.Cookies[MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed].Expires < ZDateTime.Now.ToDateTime());
			}
		}

		public void TestRedirectIfLoggedIn_NotAuthenticated()
		{
			InsertLiteViewCookie();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "hamlet@shakespeare.com";
			Factory.Save();
			using (var page = GetNewZPage() as BasePage)
			{
				page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
				var helper = new MyAccountLoginLiteHelperForTest(page);
				helper.SetIsAuthenticated(false);
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				helper.RedirectIfLoggedIn();

				Assert(page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed));
				Assert(page.Response.Cookies.AllKeys.Contains(MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed));
				Assert(page.Response.Cookies[MyAccountLoginLiteHelperForTest.LoggedInUserInfoCookieName_Exposed].Expires < ZDateTime.Now.AddDays(90).ToDateTime());
				Assert(page.Response.Cookies[MyAccountLoginLiteHelperForTest.SecurityRightsCookieName_Exposed].Expires < ZDateTime.Now.AddDays(90).ToDateTime());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		protected override ZPage GetNewZPage()
		{
			var result = new BasePageForTest()
			{ IsCreateNewAppInstanceIfNullForTest = true };
			result.TestDataSource = new LoginManager();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			method.Invoke(result, new object[] { HttpContext.Current });
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, result, null);
			return result;
		}

		class MyAccountLoginLiteHelperForTest : MyAccountLoginLiteHelper
		{
			public MyAccountLoginLiteHelperForTest(BasePage page) : base(page)
			{
			}

			public void Redirect_Exposed() => base.Redirect();
			public string DefaultUrl_Exposed => DefaultUrl;

			bool isAuthenticated;

			public const string HomePageDomain = ".cargowise.com";

			public static string LoggedInUserInfoCookieName_Exposed => LoggedInUserInfoCookieName;

			public static string SecurityRightsCookieName_Exposed => SecurityRightsCookieName;

			protected override bool IsAuthenticated => isAuthenticated;

			public void SetIsAuthenticated(bool value)
			{
				isAuthenticated = value;
			}
		}

		class BasePageForTest : BasePage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

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
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
