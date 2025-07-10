using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Newtonsoft.Json;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class GlowPortalAutoLoginTest : TestCaseWithFactory
	{
		public void TestGlowAutoLoginFromQueryString()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			Factory.Save();
			var request = ERequestControllerTest.CreateERequestPortalRequest(securityKeyTestHelper, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "SYS0399481", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			var response = CallGetAutoLoginUrl(request);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var page = GetPageForTest();
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			AssertEquals("Precondition", false, page.Request.IsAuthenticated);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, data.AutoLoginUrl.ParseQueryString()[SecureQueryString.QueryStringKey]);
			page.DoPageLoad();
			var orignalUrl = MyAccountLoginRouterTest.ExtractOriginalUrl(page.Response.RedirectLocation);
			AssertEquals("The response should have data of the requested URL", glowUrl, orignalUrl);
			AssertNotEquals("Site user should not be null", null, page.SiteUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		public void TestGlowAutoLoginFromQueryString_AlreadyLoggedIn()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var request = ERequestControllerTest.CreateERequestPortalRequest(securityKeyTestHelper, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "SYS0399481", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			var response = CallGetAutoLoginUrl(request);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var page = GetPageForTest();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, data.AutoLoginUrl.ParseQueryString()[SecureQueryString.QueryStringKey]);
			page.SiteUser.LoginForTest(org.OH_Code, contact.OC_Email, "1234");
			AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
			page.DoPageLoad();
			AssertEquals("Should have logged out", false, page.SiteUser.IsLoggedIn);
			var originalUrl = MyAccountLoginRouterTest.ExtractOriginalUrl(page.Response.RedirectLocation);
			AssertEquals("The response should have data of the requested URL", glowUrl, originalUrl);
			var requestQueryString = HttpUtility.ParseQueryString(page.Response.RedirectLocation.Split('?').Last());
			AssertEquals(false, AutoLoginHelper.CheckPreviousLoginStatus(requestQueryString));
		}

		public void TestGlowAutoLoginFromQueryString_AlreadyLoggedIn_PreviousLoggedInContact()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SCWAAASYD";
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
			var request = ERequestControllerTest.CreateERequestPortalRequest(securityKeyTestHelper, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "SYS0399481", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			var response = CallGetAutoLoginUrl(request);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var page = GetPageForTest();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, data.AutoLoginUrl.ParseQueryString()[SecureQueryString.QueryStringKey]);
			page.SiteUser.LoginForTest(org.OH_Code, contact.OC_Email, "1234");
			AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
			page.DoPageLoad();
			AssertEquals("Should have logged out", false, page.SiteUser.IsLoggedIn);
			var originalUrl = MyAccountLoginRouterTest.ExtractOriginalUrl(page.Response.RedirectLocation);
			AssertEquals("The response should have data of the requested URL", glowUrl, originalUrl);
			var requestQueryString = HttpUtility.ParseQueryString(page.Response.RedirectLocation.Split('?').Last());
			AssertEquals(true, AutoLoginHelper.CheckPreviousLoginStatus(requestQueryString));
		}

		public void TestGlowAutoLoginFromQueryString_NoUser()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var request = ERequestControllerTest.CreateERequestPortalRequest(securityKeyTestHelper, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "SYS0399481", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			var response = CallGetAutoLoginUrl(request);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var page = GetPageForTest();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			page.IsCreateNewAppInstanceIfNullForTest = false;
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, data.AutoLoginUrl.ParseQueryString()[SecureQueryString.QueryStringKey]);
			AssertNull("Precondition", page.SiteUser);
			AssertNoExceptionThrown(delegate
			{
				page.DoPageLoad();
			});
		}

		public void TestGlowAutoLoginFromQueryString_LoginOptions()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_IsContactRelationshipActive = false;
			user.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			Factory.Save();
			var request = ERequestControllerTest.CreateERequestPortalRequest(securityKeyTestHelper, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "SYS0399481", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			var response = CallGetAutoLoginUrl(request);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			var page = GetPageForTest();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, data.AutoLoginUrl.ParseQueryString()[SecureQueryString.QueryStringKey]);
			page.DoPageLoad();
			AssertContains("Should redirect to login options page", "LoginOptions.aspx", page.Response.RedirectLocation);
		}

		#region Implementation
		readonly SecurityKeyTestHelper securityKeyTestHelper = new SecurityKeyTestHelper();
		HttpResponseMessage CallGetAutoLoginUrl(TrustedRequestForTest request)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/ERequest/AutoLoginUrl");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			using (ObjectFactory.Substitute<WTG.TrustedMessaging.ICertificatesProvider>(() => new CertificatesProviderForTest()))
			using (var controller = new ERequestV1Controller(new NLogWrapperForTest(GetType())))
			{
				return ControllerTestHelper.Execute(controller, requestMessage);
			}
		}

		GlowPortalAutoLoginForTest GetPageForTest()
		{
			var page = new GlowPortalAutoLoginForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class GlowPortalAutoLoginForTest : GlowPortalAutoLogin
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
