using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class AutoLoginTest : TestCaseWithFactory
	{
		#region Login from Query String
		public void TestDoAutoLoginFromQueryString()
		{
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate()
			{ EmailBody = "AAA" }))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				Factory.Save();
				var secureQueryString = new SecureQueryString { [OrgHeaderSchema.Constants.OH_Code] = "SCWAAASYD", [OrgContactSchema.Constants.Prefix] = ZGuid.Missing.ToString() };
				var page = GetPageForTest();
				page.Request.QueryString.Add("qdata", secureQueryString.ToString());
				page.DoPageLoad();
				AssertEquals("Login should succeed", true, page.SiteUser.IsLoggedIn);
			}
		}

		public void TestDoAutoLoginFromQueryString_ShouldLogoutIfLoggedIn()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = "sam@abc.com";
			Factory.Save();
			var finalURL = "/TestPage.aspx";
			var secureQueryString = new SecureQueryString { [OrgHeaderSchema.Constants.OH_Code] = org.OH_Code, [OrgContactSchema.Constants.Prefix] = contact.PK.ToString(), ["ReturnUrl"] = "/TestPage.aspx" };

			var page = GetPageForTest();
			page.SiteUser.LoginForTest(org.OH_Code, contact.OC_Email, "1234");
			page.Request.QueryString.Add("qdata", secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals("Should have logged out", false, page.SiteUser.IsLoggedIn);
			var router = new MyAccountLoginRouterTest.MyAccountLoginRouterForTest(new Uri(finalURL, UriKind.Relative), contact);
			AssertEquals("Precondition", false, router.HasAnyRoutingRequired);
			AssertStartsWith("The response should have been a redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var responseQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			AssertContains("Query string should contain the finalUrl", finalURL, responseQueryString[LoginRouter.OriginalUrlQueryStringKey]);
			AssertEquals(true, AutoLoginHelper.CheckPreviousLoginStatus(queryDictionary));
		}

		[ExpectNoExceptions]
		public void TestPageLoadNoInvalidQueryStringException()
		{
			var page = GetPageForTest();
			page.Request.QueryString.Add("qdata", "cf%2bcmbyz%2fest4qxf1hcegpqo2luvs1juqru9xuf1x6cyw8%2fd86d6y8g23j76jcjmv19gzzzum4mqzd3smewab308yqz4tv0jgstxofa%2f7oo%3d");
			page.DoPageLoad();
		}

		public void TestDoAutoLoginForSuperUser()
		{
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate()
			{ EmailBody = "AAA" });
			WebDataRegistry.Instance.WebActivityLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var secureQueryString = new SecureQueryString();
			secureQueryString[OrgHeaderSchema.Constants.OH_Code] = "SCWAAASYD";
			secureQueryString[OrgContactSchema.Constants.Prefix] = ZGuid.Missing.ToString();
			var page = GetPageForTest();
			page.Request.QueryString.Add("qdata", secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals("Login should succeed", true, page.SiteUser.IsLoggedIn);
			var query = new ZQuery(StmALogSchema.SL_Parent, ZGuid.Missing);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Login.Code);
			AssertNull(Factory.LoadTop1<StmALog>(query));
		}

		#endregion
		#region Login from Token
		public void TestDoAutoLoginFromToken_InvalidToken_ShouldReturnTheIndexPage()
		{
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			{
				var finalURL = "https://someImportantAPI.com";
				var info = new AccessTokenInfo(finalURL, Guid.Empty, "Z0");
				var token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
				var page = GetPageForTest();
				CombineAssertions(() =>
				{
					page.Request.QueryString.Add("token", token);
					page.DoPageLoad();
					AssertEquals("Login should not succeed", false, page.SiteUser.IsLoggedIn);
					AssertEquals("The response should have been a redirect to the indexpage", page.Response.RedirectLocation, "https://indexwise.com");
				});
			}
		}

		public void TestDoAutoLoginFromToken_ValidToken_ShouldReturnUrl()
		{
			var accessControl = new TokenizedAccessControl();
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "_X1";
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = licEnterprise.PK;
			licCompany.LC_CompanyCode = "_X2";
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "_X3";
			var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "fellowship";
			licCompany.LC_OH = org.PK;
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Meriadoc Brandybock";
			contact.OC_Email = "merry@shireweb.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			CombineAssertions(() =>
			{
				var finalURL = "https://someimportantapi.com/";

				var scope = new AutoLoginTokenScope
				{
					OrgCode = org.OH_Code,
					DatabaseNumber = null,
					ReturnUrl = finalURL
				};
				var serializedScope = AutoLoginHelper.SerializeToXml(scope);
				var info = new AccessTokenInfo(serializedScope, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
				var token = accessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, info, maxUses: 1);
				var page1 = GetPageForTest();
				page1.Request.QueryString.Add("token", token);
				page1.DoPageLoad();
				var router = new MyAccountLoginRouterTest.MyAccountLoginRouterForTest(new Uri(finalURL), contact);
				AssertEquals("Precondition", false, router.HasAnyRoutingRequired);
				AssertContains("The response should have been a redirect to LoginComplete", "/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
				var uriDeconstructor = new UriDeconstructor(new Uri(page1.Response.RedirectLocation, UriKind.Relative));
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				AssertEquals("Query string should contain the finalUrl", finalURL, secureQueryString[LoginRouter.OriginalUrlQueryStringKey]);
				var page2 = GetPageForTest();
				page2.Request.QueryString.Add("token", token);
				page2.DoPageLoad();
				AssertEquals("Login should not have succeeded a second time", false, page2.SiteUser.IsLoggedIn);
				AssertEquals(false, AutoLoginHelper.CheckPreviousLoginStatus(queryDictionary));
			});
		}

		public void TestDoAutoLoginFromToken_CustomerUserAccount()
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
			var finalURL = "https://someimportantapi.com/";
			var accessControl = new TokenizedAccessControl();

			var scope = new AutoLoginTokenScope
			{
				OrgCode = org.OH_Code,
				DatabaseNumber = null,
				ReturnUrl = finalURL
			};
			var serializedScope = AutoLoginHelper.SerializeToXml(scope);
			var info = new AccessTokenInfo(serializedScope, user.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);

			var token = accessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, info, maxUses: 1);
			var page1 = GetPageForTest();
			page1.Request.QueryString.Add("token", token);
			page1.DoPageLoad();
			var router = new MyAccountLoginRouterTest.MyAccountLoginRouterForTest(new Uri(finalURL), user);
			AssertEquals("Precondition", false, router.HasAnyRoutingRequired);
			AssertStartsWith("The response should have been a redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page1.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			AssertEquals("Query string should contain the finalUrl", finalURL, secureQueryString[LoginRouter.OriginalUrlQueryStringKey]);
			AssertEquals(false, AutoLoginHelper.CheckPreviousLoginStatus(queryDictionary));
		}

		public void TestDoAutoLoginFromToken_ShouldLogoutIfLoggedIn()
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
			var finalURL = "https://someimportantapi.com/";
			var accessControl = new TokenizedAccessControl();

			var scope = new AutoLoginTokenScope
			{
				OrgCode = org.OH_Code,
				DatabaseNumber = null,
				ReturnUrl = finalURL
			};
			var serializedScope = AutoLoginHelper.SerializeToXml(scope);
			var info = new AccessTokenInfo(serializedScope, user.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);

			var token = accessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, info, maxUses: 1);
			var page = GetPageForTest();
			page.SiteUser.LoginForTest(org.OH_Code, contact.OC_Email, "1234");
			page.Request.QueryString.Add("token", token);
			page.DoPageLoad();
			AssertEquals("Should have logged out", false, page.SiteUser.IsLoggedIn);
			var router = new MyAccountLoginRouterTest.MyAccountLoginRouterForTest(new Uri(finalURL), user);
			AssertEquals("Precondition", false, router.HasAnyRoutingRequired);
			AssertStartsWith("The response should have been a redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page.Response.RedirectLocation);
			var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			AssertEquals("Query string should contain the finalUrl", finalURL, secureQueryString[LoginRouter.OriginalUrlQueryStringKey]);
			AssertEquals(true, AutoLoginHelper.CheckPreviousLoginStatus(queryDictionary));
		}

		public void TestDoAutoLoginFromInvalidXml_ShouldReportErrorLog()
		{
			ErrorReporter.Clear();
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
			var accessControl = new TokenizedAccessControl();
			var invalidXml = "</>";
			var info = new AccessTokenInfo(invalidXml, user.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);

			var token = accessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, info, maxUses: 1);
			var page1 = GetPageForTest();
			page1.Request.QueryString.Add("token", token);
			page1.DoPageLoad();
			AssertEquals(ErrorReporter.LastMessageReported, "Deserialization of XML failed during AutoLogin, The original xml is: </>");
			ErrorReporter.Clear();
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		AutoLoginForTest GetPageForTest()
		{
			var page = new AutoLoginForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class AutoLoginForTest : AutoLogin
		{
			public void DoPageLoad()
			{
				try
				{
					base.Page_Load(null, EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
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
