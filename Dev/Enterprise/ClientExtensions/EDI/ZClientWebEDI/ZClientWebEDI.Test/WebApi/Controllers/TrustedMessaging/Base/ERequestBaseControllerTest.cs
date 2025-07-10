using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class ERequestBaseControllerTest : TestCaseWithFactory
	{
		public void TestGetAutoLoginUrl_NewERequest()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LicEnterprise.LE_EnterpriseID = "E001001";

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "RAT", "AIRFreightRate", "CR4", "6E6B6A65-609E-41BE-97F6-19C743D790FC", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/eRequestPortal?sso_otp={Uri.EscapeDataString(token)}&LicenseCode=E001001.P9M&Product=SMF&Module=RAT&SubModule=AIRFreightRate&Criticality=CR4&ReferenceId=6E6B6A65-609E-41BE-97F6-19C743D790FC#/workflow"
				, resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_NewERequest_CW1()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "8000";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LicEnterprise.LE_EnterpriseID = "E001001";

			var clientCompany1 = db.ClientCompanies[0];
			clientCompany1.LCC_CreateTimeUtc = new ZDateTime(2020, 8, 1, 12, 0, 0);

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "IEQ";
			clientCompany2.LCC_CreateTimeUtc = new ZDateTime(2019, 1, 1, 9, 0, 0);

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, product, "8000", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "RAT", "AIRFreightRate", "CR4", "6E6B6A65-609E-41BE-97F6-19C743D790FC", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/eRequestPortal?sso_otp={Uri.EscapeDataString(token)}&LicenseCode=DDDABCSYD&Product=ENT&Module=RAT&SubModule=AIRFreightRate&Criticality=CR4&ReferenceId=6E6B6A65-609E-41BE-97F6-19C743D790FC#/workflow"
				, resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_NewERequest_LicenceCode()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "8000";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LicEnterprise.LE_EnterpriseID = "E001001";

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, product, "8000", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "RAT", "AIRFreightRate", "CR4", "6E6B6A65-609E-41BE-97F6-19C743D790FC", "", "DDDMSTSYD");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/eRequestPortal?sso_otp={Uri.EscapeDataString(token)}&LicenseCode=DDDMSTSYD&Product=ENT&Module=RAT&SubModule=AIRFreightRate&Criticality=CR4&ReferenceId=6E6B6A65-609E-41BE-97F6-19C743D790FC#/workflow"
				, resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_EditERequest()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow/5a354981-5850-421b-9f11-730f4087b024/{*PK*}");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00005899";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestEditLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "", "", "", "", "CS00005899", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;
			AssertEquals($"https://unit-testing/GlowPortal/eRequestPortal?sso_otp={Uri.EscapeDataString(token)}#/workflow/5a354981-5850-421b-9f11-730f4087b024/" + incident.IM_INC_Request.ToString(), resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_ERequestPortal()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/INC?sso_otp={Uri.EscapeDataString(token)}", resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_NotAllowed()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LD_AllowAutoLogin = false;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Authorization_ActionNotPermitted, message1.Code);
			AssertEquals("Auto login to MyAccount is disabled on this system.", message1.Message);
		}

		public void TestGetAutoLoginUrl_AgreementNotYetAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Authorization_AgreementNotAcknowledged, message1.Code);
			AssertEquals("User agreement is not acknowledged.", message1.Message);
		}

		public void TestGetAutoLoginUrl_AgreementAcceptedByOrg()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			var staffAcceptanceProxy = Factory.NewWithValidTestData<GlbStaff>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Alex";
			contact.OC_Email = "alex@ard.com";
			user.EUA_OC_WebAccessContact = contact.PK;
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staffAcceptanceProxy.PK;
			agreementLog.EUL_ERA = agreement1.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, "SYS0399481", "DDDABCSYD", "U048173", "User One", "user.one@test.com", "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertNotNull(data);
		}

		public void TestGetAutoLoginUrl_AgreementBypass_NoUserAccount()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			SaveFactoryWithoutAgreementDateTriggers();

			var newUserID = "U048173";
			var newUserName = "User One";
			var newUserEmail = "user.one@test.com";

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Authorization_ActionNotPermitted, message1.Code);
			AssertEquals("Access to eRequest Portal is not granted.", message1.Message);

			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var newList = new CodeDescriptionPairList();
				newList.AddPair(product, "Alphabet");
				return newList;
			});
			var productBypassList = new CodeSelectionCollection(listProvider);
			var productCodeSelection = productBypassList.AddNew();
			productCodeSelection.Code = product;
			EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productBypassList);

			context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertNull("Should not have any error messages", context.Messages);
			var newUserAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, newUserID));
			AssertNotNull(newUserAccount);
			AssertEquals("Should have been associated to the LD", db.PK, newUserAccount.EUA_LD);
			AssertEquals("Should have populated name from request", newUserName, newUserAccount.EUA_FullName);
			AssertEquals("Should have populated email from request", newUserEmail, newUserAccount.EUA_Email);
			AssertEquals("Should require email verification", true, newUserAccount.EUA_IsEmailVerificationRequired);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/INC?sso_otp={Uri.EscapeDataString(token)}", resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_AgreementBypass_UserAccountExists()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Authorization_AgreementNotAcknowledged, message1.Code);
			AssertEquals("User agreement is not acknowledged.", message1.Message);

			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var newList = new CodeDescriptionPairList();
				newList.AddPair(product, "Alphabet");
				return newList;
			});
			var productBypassList = new CodeSelectionCollection(listProvider);
			var productCodeSelection = productBypassList.AddNew();
			productCodeSelection.Code = product;
			EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productBypassList);
			var initialUserAccountCount = Factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, db.PK)).Length;

			context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertEquals("Should not have created any new user accounts", initialUserAccountCount, Factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, db.PK)).Length);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/INC?sso_otp={Uri.EscapeDataString(token)}", resultUri.ToString());
		}

		public void TestGetAutoLoginUrl_AgreementBypass_NoUserAccount_UserInfoValidation()
		{
			var logger = new NLogWrapperForTest(GetType());

			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			SaveFactoryWithoutAgreementDateTriggers();

			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var newList = new CodeDescriptionPairList();
				newList.AddPair(product, "Alphabet");
				return newList;
			});
			var productBypassList = new CodeSelectionCollection(listProvider);
			var productCodeSelection = productBypassList.AddNew();
			productCodeSelection.Code = product;
			EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productBypassList);

			var newUserID = "U048173";
			var newUserName = "User One";
			var newUserEmail = "user.one@test.com";

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, "", newUserName, newUserEmail, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message1.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDEmptyErrorMessage, message1.Message);

			context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, newUserID, "", newUserEmail, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message2 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message2.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameEmptyErrorMessage, message2.Message);

			context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, "", "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message3 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message3.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage, message3.Message);

			context = CreateAutoLoginContext(controller, system, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail, "", "", "", "", "", "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertContains("GlowPortalAutoLogin.aspx?qdata=", data.AutoLoginUrl.ToString());
			var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(data.AutoLoginUrl);
			var resultUri = new Uri(glowUrl.Trim('\"'));
			var queryParameters = UriExtensions.ParseQueryString(resultUri);
			var token = queryParameters["sso_otp"] ?? string.Empty;

			AssertEquals($"https://unit-testing/GlowPortal/INC?sso_otp={Uri.EscapeDataString(token)}", resultUri.ToString());
		}

		public void TestUpload()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			Factory.Save();

			var controller = CreateController(logger);

			//InvalidERequestDocumentXMLFormat
			var context = CreateUploadContext(controller, system, product, system.ETS_SystemID, "<xml>something bad</xml>");
			controller.UploadCore_Exposed(context);
			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_InvalidValue, message1.Code);
			AssertEquals("Invalid ERequestDocument XML format.", message1.Message);

			//InvalidERequestDocumentReferenceId
			var xml = "<?xml version='1.0' encoding='utf-8'?><ERequestDocument xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='http://www.edi.com.au/EnterpriseService/'><ReferenceId>xxxxxx</ReferenceId><Attachments><Attachment><FileName>xxx.png</FileName><Data>xxx</Data></Attachment><Attachment><FileName>ccccc</FileName><Data>222</Data></Attachment></Attachments></ERequestDocument>";
			context = CreateUploadContext(controller, system, product, system.ETS_SystemID, xml);
			controller.UploadCore_Exposed(context);
			message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_InvalidValue, message1.Code);
			AssertEquals("Invalid ERequestDocument referenceId.", message1.Message);

			//ERequestDocumentContainsNoElements
			xml = "<?xml version='1.0' encoding='utf-8'?><ERequestDocument xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='http://www.edi.com.au/EnterpriseService/'><ReferenceId>5655be3a-780b-49c8-a814-d39f930eb9e9</ReferenceId><Attachments></Attachments></ERequestDocument>";
			context = CreateUploadContext(controller, system, product, system.ETS_SystemID, xml);
			controller.UploadCore_Exposed(context);
			message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_InvalidValue, message1.Code);
			AssertEquals("ERequestDocument contains no elements.", message1.Message);

			// valid request
			var refId = ZGuid.NewZGuid();
			xml = CreateERequestDocumentAsXmString(refId, "20200218_000000");
			context = CreateUploadContext(controller, system, product, system.ETS_SystemID, xml);
			controller.UploadCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);
			var newFactory = new BusinessObjectFactory();
			var documentQueues = newFactory.Load<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, refId));
			AssertEquals(2, documentQueues.Length);
			AssertNotNull(documentQueues.Single(x => x.EDQ_FileName == "ScreenShot_20200218_000000.png" && !x.EDQ_Data.IsEmpty));
			AssertNotNull(documentQueues.Single(x => x.EDQ_FileName == "SystemReport_20200218_000000.zip" && !x.EDQ_Data.IsEmpty));
			AssertEquals(true, documentQueues.Single(x => x.EDQ_FileName == "ScreenShot_20200218_000000.png" && !x.EDQ_Data.IsEmpty).EDQ_IsPublished);
			AssertEquals(false, documentQueues.Single(x => x.EDQ_FileName == "SystemReport_20200218_000000.zip" && !x.EDQ_Data.IsEmpty).EDQ_IsPublished);
		}

		#region Create Requests

		static TrustedContext<ERequestInfo, AutoLoginResponse> CreateAutoLoginContext
			(TrustedController controller,
			EdiTrustedSystem trustedSystem,
			string landingPageId, string product, string systemId, string tenantId, string userId, string userName, string email,
			string module, string submodule, string criticality, string refId, string incidentNumber, string licenceCode
			)
		{
			var userInfo = new ERequestInfo()
			{
				LandingPageId = landingPageId,
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				FullName = userName,
				Email = email,
				Module = module,
				SubModule = submodule,
				Criticality = criticality,
				ReferenceId = refId,
				IncidentNumber = incidentNumber,
				LicenceCode = licenceCode,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};

			return new TrustedContextForTest<ERequestInfo, AutoLoginResponse>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		static TrustedContext<ERequestInfo, bool> CreateUploadContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string eRequestDocument)
		{
			var userInfo = new ERequestInfo()
			{
				Product = product,
				SystemId = systemId,
				ERequestDocument = eRequestDocument,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};
			return new TrustedContextForTest<ERequestInfo, bool>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		internal static string CreateERequestDocumentAsXmString(ZGuid referenceId, string timestamp)
		{
			var doc = new ERequestDocument();
			doc.ReferenceId = referenceId.ToString();
			var screenShot = doc.Attachments.AddNew();
			screenShot.FileName = $"ScreenShot_{timestamp}.png";
			screenShot.Data = CreateImage();
			screenShot.IsPublished = true;
			screenShot.IsPublishedSpecified = true;

			var systemReport = doc.Attachments.AddNew();
			systemReport.FileName = $"SystemReport_{timestamp}.zip";
			using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(new ExceptionReportBuilder(new ExceptionReportArgs(null, null, null, null)).GenerateReport())))
			using (var zipStream = new MemoryStream())
			{
				var creator = new ZipCreator();
				creator.ZipStream($"SystemReport_{timestamp}.xml", contentStream, zipStream);
				systemReport.Data = zipStream.ToArray();
			}

			var xsSubmit = ZXmlSerializer.New(typeof(ERequestDocument));

			using (var sww = new StringWriter())
			{
				using (var writer = XmlWriter.Create(sww))
				{
					xsSubmit.Serialize(writer, doc);
					var xml = sww.ToString();
					return xml;
				}
			}
		}

		static byte[] CreateImage()
		{
			using (var bitmap = new Bitmap(10, 20))
			{
				var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.WriteOnly, bitmap.PixelFormat);
				var noise = new byte[data.Width * data.Height * 3];
				new Random().NextBytes(noise);
				System.Runtime.InteropServices.Marshal.Copy(noise, 0, data.Scan0, noise.Length);
				bitmap.UnlockBits(data);

				using (var ms = new MemoryStream())
				{
					bitmap.Save(ms, ImageFormat.Png);
					return ms.GetBuffer();
				}
			}
		}

		#endregion Create Requests

		void SaveFactoryWithoutAgreementDateTriggers()
		{
			DisableEffectiveDateTriggers();
			Factory.Save();
			EnableEffectiveDateTriggers();
		}

		void DisableEffectiveDateTriggers()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		void EnableEffectiveDateTriggers()
		{
			TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; ENABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		ERequestBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/ERequest/AutoLoginUrl");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new ERequestBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		public class ERequestBaseControllerForTest : ERequestTrustedSystemBaseController
		{
			public ERequestBaseControllerForTest() : base()
			{
			}

			public ERequestBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void GetAutoLoginUrlCore_Exposed(TrustedContext<ERequestInfo, AutoLoginResponse> context) => base.GetAutoLoginUrlCore(context);

			public void UploadCore_Exposed(TrustedContext<ERequestInfo, bool> context) => base.UploadCore(context);
		}
	}
}
