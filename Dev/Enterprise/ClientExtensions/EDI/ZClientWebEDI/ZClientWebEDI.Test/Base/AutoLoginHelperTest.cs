using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class AutoLoginHelperTest : ZPageTestCase
	{
		public void TestUserRoutingLoginNoContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = false;
			var db = licence.Database;
			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			var user1 = Factory.New<EdiCustomerUserAccount>();
			user1.EUA_LD = db.PK;
			user1.EUA_UserID = "TST";
			user1.EUA_FullName = "Test User";
			user1.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			var redirectUrl = AutoLoginHelper.UserRoutingLogin("https://google.com", user1, new Global());
			AssertContains("Should redirect to user email verification", "/Login/EmailSentNotification.aspx", redirectUrl.OriginalString);
		}

		public void TestSetAuthCookie()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var page = Page as BasePage;
			AutoLoginHelper.UserRoutingLogin("https://google.com", user, new Global());
			page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
			AutoLoginHelper.SetAuthCookie(page, page.SiteUser);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"].Value);
		}

		public void TestSetAuthCookie_SiteUserNotLoggedIn()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var page = Page as BasePage;
			AutoLoginHelper.UserRoutingLogin("https://google.com", user, new Global());
			AutoLoginHelper.SetAuthCookie(page, page.SiteUser);
			AssertEquals("Should not set auth cookie if not logged into session", null, page.Response.Cookies[".ASPXAUTH"].Value);
		}

		public void TestSetAuthCookie_LiteViewMode()
		{
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://google.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var page = Page as BasePage;
			page.SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword);
			AutoLoginHelper.SetAuthCookie(page, page.SiteUser);
			AssertNotEquals(string.Empty, page.Response.Cookies[".ASPXAUTH"].Value);
			AssertNotEquals(string.Empty, page.Response.Cookies["EDIPROD_LOGGED_IN_USER_INFO"].Value);
			AssertNotEquals(string.Empty, page.Response.Cookies["SECURITYRIGHTS"].Value);
		}

		public void TestPreviousLoggedInContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			NameValueCollection GetQueryString(Uri address)
			{
				if (address.IsAbsoluteUri)
				{
					return address.ParseQueryString();
				}
				else
				{
					var uri = address.ToString();
					return HttpUtility.ParseQueryString(uri.Contains("?") ? uri.Split('?').Last() : "");
				}
			}

			foreach (var uri in new[] { "https://www.a.com", "https://www.a.com/a?k=1", "/index.html", "/index.html?k=1" })
			{
				var redirectUrl = AutoLoginHelper.UserRoutingLogin(uri, user, new Global());
				AssertEquals(false, AutoLoginHelper.CheckPreviousLoginStatus(GetQueryString(redirectUrl)));

				var redirectUrl2 = AutoLoginHelper.UserRoutingLogin(uri, user, new Global(), previousLoggedInContactPK: contact.PK);
				AssertEquals(true, AutoLoginHelper.CheckPreviousLoginStatus(GetQueryString(redirectUrl2)));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		protected override ZPage GetNewZPage() => new BasePageForTest()
		{ IsCreateNewAppInstanceIfNullForTest = true };
		class BasePageForTest : BasePage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
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
