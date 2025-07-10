using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class LoginServiceBaseControllerTest : TestCaseWithFactory
	{
		#region Auto Login

		public void TestLogin_NewUser()
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
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);
			AssertEquals("Access to MyAccount is not granted.", context.Messages.Messages[0].Message);

			var user = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "U048173"));
			AssertNull(user);
		}

		[HttpContextEnabledTest]
		public void TestLogin_NewUser_ShouldCreateUserIfAgreementAcceptedByOrg()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "SMF";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "SYS0399481";
			system.GetOrCreateCertificateConfig();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var db = licence.Database;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LD_OH_WebAccessOrg = org.PK;

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;

			SaveFactoryWithoutAgreementDateTriggers();

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staff.PK;
			agreementLog.EUL_ERA = agreement1.PK;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Email verification URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", data.AutoLoginUrl.ToString());
		}

		[HttpContextEnabledTest]
		public void TestLogin_ExistingUserRequiresEmailVerification()
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

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_IsEmailVerificationRequired = true;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Email verification URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", data.AutoLoginUrl.ToString());
		}

		[HttpContextEnabledTest]
		public void TestLogin_ExistingUserNewContact()
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
			var org = db.WebAccessOrg;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			user.Reload();
			var contact = user.WebAccessContact;
			AssertNotNull(contact);
			AssertEquals("Org", org.PK, contact.OC_OH);
			AssertEquals("Name", "User One", contact.OC_ContactName);
			AssertEquals("Email", "user.one@test.com", contact.OC_Email);
			AssertEquals("Web Access", true, contact.OC_WebAccessEnabled);
		}

		[HttpContextEnabledTest]
		public void TestLogin_ExistingUserExistingContact()
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
			var org = db.WebAccessOrg;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			contact.Reload();
			AssertEquals("Name is updated", "User Two", contact.OC_ContactName);
			AssertEquals("Email is updated", "user.two@test.com", contact.Email);
		}

		[HttpContextEnabledTest]
		public void TestLogin_ReturnUrl()
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
			var org = db.WebAccessOrg;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com", returnUrl: "http://www.cw1.com/download.aspx?1=1&org=\u4E2D\u6587&key=2");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			var scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
			AssertEquals(true, scope.ReturnUrl.EndsWith("http://www.cw1.com/download.aspx?1=1&org=%E4%B8%AD%E6%96%87&key=2"));

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com", returnUrl: "download.aspx?1=1&org=\u4E2D\u6587&key=2");
			controller.GetAutoLoginUrlCore_Exposed(context);

			data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			tokenCtrl = new TokenizedAccessControl();
			token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out info);
			scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
			AssertEquals(true, scope.ReturnUrl.EndsWith("download.aspx?1=1&org=%E4%B8%AD%E6%96%87&key=2"));
		}

		public void TestLogin_AgreementNotYetAccepted()
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
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertEquals("User agreement is not acknowledged.", context.Messages.Messages[0].Message);

			user.Reload();
			AssertNull("Should not create contact", user.WebAccessContact);
		}

		public void TestLogin_LatestAgreementNotAccepted()
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

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement1.ERA_Title = "Agreement";
			agreement1.ERA_Content = "Old Agreement Content";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement1.ERA_VersionNumber = 1;
			var agreement2 = Factory.New<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement2.ERA_Title = "Agreement";
			agreement2.ERA_Content = "New Agreement Content";
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			agreement2.ERA_VersionNumber = 2;

			var agreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_EUA = user.PK;
			agreementLog.EUL_ERA = agreement1.PK;
			agreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertEquals(ErrorCodes.Descriptions.Authorization_AgreementNotAcknowledged, context.Messages.Messages[0].Message);
		}

		[HttpContextEnabledTest]
		public void TestLogin_AgreementAcceptedByOrg()
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

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.UserAccountCollection;
			agreement.ERA_Title = "Agreement";
			agreement.ERA_Content = "Old Agreement Content";
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			agreement.ERA_VersionNumber = 1;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var agreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staff.PK;
			agreementLog.EUL_ERA = agreement.PK;
			agreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;

			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			var scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
			AssertEquals(true, scope.ReturnUrl.EndsWith("https://myaccount.cargowise.com/"));
		}

		public void TestLogin_SystemAutoLoginDisabled()
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
			db.LD_AllowAutoLogin = false;
			db.LD_ETS_TrustedSystem = system.PK;
			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);
			AssertEquals("Auto login to MyAccount is disabled on this system.", context.Messages.Messages[0].Message);
		}

		[HttpContextEnabledTest]
		public void TestLogin_ContactHasNoWebAccess()
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
			var org = db.WebAccessOrg;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U049000", "User Two", "user.one@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);
			AssertEquals("Access to MyAccount is not granted.", context.Messages.Messages[0].Message);
		}

		public void TestLogin_ExistingUserIncompleteInfo()
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

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "");
			controller.GetAutoLoginUrlCore_Exposed(context);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage, context.Messages.Messages[0].Message);
		}

		public void TestLogin_ClaimValidation()
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

			var invalidUserId = ZString.Replicate('U', EdiCustomerUserAccountSchema.EUA_UserID.MaxLength + 1);
			var invalidUserName = ZString.Replicate('N', EdiCustomerUserAccountSchema.EUA_FullName.MaxLength + 1);
			var invalidEmail1 = ZString.Replicate('E', EdiCustomerUserAccountSchema.EUA_Email.MaxLength + 1);
			var invalidEmail2 = "user.one.abc.com";

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, invalidUserId, invalidUserName, invalidEmail1);
			controller.GetAutoLoginUrlCore_Exposed(context);
			var errors = context.Messages.Messages;
			AssertEquals("'user_id' exceeds its max length of 36 characters.", errors[0].Message);
			AssertEquals("'full_name' exceeds its max length of 256 characters.", errors[1].Message);
			AssertEquals("'user_email' exceeds its max length of 254 characters.", errors[2].Message);

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, invalidUserId, invalidUserName, invalidEmail2);
			controller.GetAutoLoginUrlCore_Exposed(context);
			errors = context.Messages.Messages;
			AssertEquals("'user_id' exceeds its max length of 36 characters.", errors[0].Message);
			AssertEquals("'full_name' exceeds its max length of 256 characters.", errors[1].Message);
			AssertEquals("'user_email' is not valid.", errors[2].Message);
		}

		[HttpContextEnabledTest]
		public void TestLogin_MyAccountHostingSiteLandingPageUrl()
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
			var org = db.WebAccessOrg;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = false;

			user.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var collection = new MyAccountHostingSiteLandingPageUrlCollection();
			collection.AddNew(ZString.Empty, ZString.Empty);
			collection.AddNew("ARC", "https://myaccount.cargowise.com/#smartfreight");
			collection.AddNew(product, "https://myaccount.cargowise.com/#cargowise");
			EDIDataRegistry.Instance.MyAccountHostingSiteLandingPageUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var controller = CreateController(logger);
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			AssertEquals(true, info.Scope.Contains("https://myaccount.cargowise.com/#cargowise"));
		}

		public void TestLogin_AgreementBypass_NoUserAccount()
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
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail);
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Authorization_ActionNotPermitted, message1.Code);
			AssertEquals("Access to MyAccount is not granted.", message1.Message);

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

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail);
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertNull("Should not have any error messages", context.Messages);
			var newUserAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, newUserID));
			AssertNotNull(newUserAccount);
			AssertEquals("Should have been associated to the LD", db.PK, newUserAccount.EUA_LD);
			AssertEquals("Should have populated name from request", newUserName, newUserAccount.EUA_FullName);
			AssertEquals("Should have populated email from request", newUserEmail, newUserAccount.EUA_Email);
			AssertEquals("Should require email verification", true, newUserAccount.EUA_IsEmailVerificationRequired);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx?qdata=", data.AutoLoginUrl.ToString());
		}

		public void TestLogin_AgreementBypass_UserAccountExists()
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
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email);
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

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email);
			controller.GetAutoLoginUrlCore_Exposed(context);

			AssertEquals("Should not have created any new user accounts", initialUserAccountCount, Factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, db.PK)).Length);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());

			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			AssertEquals(true, info.Scope.Contains("https://myaccount.cargowise.com/"));
		}

		public void TestLogin_AgreementBypass_NoUserAccount_UserInfoValidation()
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
			var context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "", newUserName, newUserEmail);
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message1 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message1.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDEmptyErrorMessage, message1.Message);

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, newUserID, "", newUserEmail);
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message2 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message2.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameEmptyErrorMessage, message2.Message);

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, "");
			controller.GetAutoLoginUrlCore_Exposed(context);

			var message3 = context.Messages.Messages[0];
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, message3.Code);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage, message3.Message);

			context = CreateAutoLoginContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, newUserID, newUserName, newUserEmail);
			controller.GetAutoLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx?qdata=", data.AutoLoginUrl.ToString());
		}

		#endregion Auto Login

		#region Update User Account

		[HttpContextEnabledTest]
		public void TestUpdateUserAccount()
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

			var org = db.WebAccessOrg;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_IsActive = true;

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = db.PK;
			userAccount.EUA_UserID = "U048173";
			userAccount.EUA_FullName = "User One";
			userAccount.EUA_IsActive = true;
			userAccount.EUA_Email = "user.one@test.com";
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			var controller = CreateController(logger);
			//UserIdNotFound
			var context = CreateUpdateUserContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173xxx", "User One", "user.one@test.com", isActive: false);
			controller.UpdateUserAccountCore_Exposed(context);
			AssertEquals("User Id is not found.", context.Messages.Messages[0].Message);

			// isActive: false
			context = CreateUpdateUserContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com", isActive: false);
			controller.UpdateUserAccountCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);
			var newFactory = new BusinessObjectFactory();
			var userAccountInNewFactory = newFactory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "U048173"));
			AssertNotNull(userAccountInNewFactory);
			AssertEquals("Licence", 8000, userAccountInNewFactory.Database.LD_DatabaseNumber);
			AssertEquals("Name", "User One", userAccountInNewFactory.EUA_FullName);
			AssertEquals("Email", "user.one@test.com", userAccountInNewFactory.EUA_Email);
			AssertEquals(false, userAccountInNewFactory.EUA_IsActive);
			AssertNull("Should be null if user isn't activate", userAccountInNewFactory.WebAccessContact);

			// isActive: true
			context = CreateUpdateUserContext(controller, system, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com", isActive: true);
			controller.UpdateUserAccountCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);
			newFactory = new BusinessObjectFactory();
			userAccountInNewFactory = newFactory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "U048173"));
			AssertNotNull(userAccountInNewFactory);
			AssertEquals("Licence", 8000, userAccountInNewFactory.Database.LD_DatabaseNumber);
			AssertEquals("Name", "User One", userAccountInNewFactory.EUA_FullName);
			AssertEquals("Email", "user.one@test.com", userAccountInNewFactory.EUA_Email);
			AssertEquals(true, userAccountInNewFactory.EUA_IsActive);
			var contactInNewFactory = userAccountInNewFactory.WebAccessContact;
			AssertEquals("Org", org.PK, contactInNewFactory.OC_OH);
			AssertEquals("Name", "User One", contactInNewFactory.OC_ContactName);
			AssertEquals("Email", "user.one@test.com", contactInNewFactory.OC_Email);
			AssertEquals(true, contactInNewFactory.OC_IsActive);
		}

		#endregion Update User Account

		#region Auto Login Url

		public void TestGetOAuthLoginUrl_TrustedUser()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "29880";
			system.GetOrCreateCertificateConfig();

			var resourceProduct = "CCS";
			SetupTrustedResourceSystem(resourceProduct);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 29880;
			db.LD_Product = product;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_ETS_TrustedSystem = system.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "AC1";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateTokenContext(controller, system, resourceProduct, string.Empty, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "http://spa-demo.org/auto-login?type=myaccount");
			controller.GetOAuthLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertStartsWith("Should be redirect url", "http://spa-demo.org/auto-login?type=myaccount&token=", data.RedirectUrl.ToString());

			var parsedToken = data.RedirectUrl.ToString().Substring("http://spa-demo.org/auto-login?type=myaccount&token=".Length);
			AssertNotNullOrEmpty(data.Token);
			AssertEquals("Should be token", parsedToken, data.Token);
		}

		public void TestGetOAuthLoginUrl_InvalidRedirectUri()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "29880";
			system.GetOrCreateCertificateConfig();

			var resourceProduct = "CCS";
			SetupTrustedResourceSystem(resourceProduct);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 29880;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LD_TenantID = "DDDABCSYD";

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "AC1";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateTokenContext(controller, system, resourceProduct, string.Empty, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "invalid_url");
			controller.GetOAuthLoginUrlCore_Exposed(context);

			var data = context.Messages;
			AssertEquals("Invalid redirect_uri value.", data.Messages[0].Message);
		}

		public void TestGetOAuthLoginUrl_AutoLoginNotAllowed()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "29880";
			system.GetOrCreateCertificateConfig();

			var resourceProduct = "CCS";
			SetupTrustedResourceSystem(resourceProduct);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 29880;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_AllowAutoLogin = false;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "AC1";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateTokenContext(controller, system, resourceProduct, string.Empty, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "http://spa-demo.org/auto-login?type=myaccount");
			controller.GetOAuthLoginUrlCore_Exposed(context);
			var data = context.Messages;
			AssertEquals("Auto login to MyAccount is disabled on this system.", data.Messages[0].Message);
		}

		[HttpContextEnabledTest]
		public void TestGetOAuthLoginUrl_ExistingUserRequiresEmailVerification()
		{
			var logger = new NLogWrapperForTest(GetType());

			var product = "CW1";

			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "29880";
			system.GetOrCreateCertificateConfig();

			var resourceProduct = "CCS";
			SetupTrustedResourceSystem(resourceProduct);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 29880;
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LD_TenantID = "DDDABCSYD";

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "AC1";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_IsEmailVerificationRequired = true;

			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateTokenContext(controller, system, resourceProduct, string.Empty, product, system.ETS_SystemID, db.LD_TenantID, user.EUA_UserID, user.EUA_FullName, user.EUA_Email, "http://spa-demo.org/auto-login?type=myaccount");
			controller.GetOAuthLoginUrlCore_Exposed(context);

			var data = context.ResponseInfo;
			AssertStartsWith("Email verification URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", data.RedirectUrl.ToString());
			AssertNull("Should not have token", data.Token);
		}

		#endregion Auto Login Url

		#region Helper Methods

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

		static TrustedContext<TrustedAutoLoginInfo, AutoLoginResponse> CreateAutoLoginContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive = null, string returnUrl = null)
		{
			return CreateAutoLoginContext(controller, trustedSystem, product, systemId, tenantId, userId, userName, email, isActive, ZDateTime.UtcNow.AddMinutes(5).ToDateTime(), returnUrl);
		}

		static TrustedContext<TrustedAutoLoginInfo, AutoLoginResponse> CreateAutoLoginContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive, DateTime infoExpires, string returnUrl = null, string country = null)
		{
			var userInfo = new TrustedAutoLoginInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				FullName = userName,
				Email = email,
				ReturnUrl = string.IsNullOrEmpty(returnUrl) ? null : (returnUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(returnUrl) : new Uri(returnUrl, UriKind.Relative)),
				InfoExpires = infoExpires,
				UserCountry = country
			};
			return new TrustedContextForTest<TrustedAutoLoginInfo, AutoLoginResponse>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		static TrustedContext<UpdateUserInfo, bool> CreateUpdateUserContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive = null, string returnUrl = null)
		{
			return CreateUpdateUserContext(controller, trustedSystem, product, systemId, tenantId, userId, userName, email, isActive, ZDateTime.UtcNow.AddMinutes(5).ToDateTime(), returnUrl);
		}

		static TrustedContext<UpdateUserInfo, bool> CreateUpdateUserContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive, DateTime infoExpires, string returnUrl = null, string country = null)
		{
			var userInfo = new UpdateUserInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				FullName = userName,
				Email = email,
				IsActive = isActive,
				InfoExpires = infoExpires,
				UserCountry = country
			};
			return new TrustedContextForTest<UpdateUserInfo, bool>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		static TrustedContext<AuthenticationTokenInfo, OAuthLoginResponse> CreateTokenContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string resourceProduct, string resourceSystemId, string product, string systemId, string tenantId, string userId, string userName, string email, string redirectUrlString)
		{
			var tokenInfo = new AuthenticationTokenInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				FullName = userName,
				Email = email,
				ResourceProduct = resourceProduct,
				ResourceSystemId = resourceSystemId,
				RedirectUriString = redirectUrlString,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime(),
				Claims = new List<Claim>()
				{
					new Claim() { Key = "is_admin", Value = "true" }
				}
			};
			return new TrustedContextForTest<AuthenticationTokenInfo, OAuthLoginResponse>(product, systemId, tokenInfo, trustedSystem, controller) { Success = true };
		}

		LoginServiceBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/LoginService/AutoLoginUrl");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new LoginServiceBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		EdiTrustedSystem SetupTrustedResourceSystem(string resourceProduct)
		{
			var regCollection = new CodeDescriptionBoolCollection
			{
				resourceProduct
			};
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regCollection);

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = resourceProduct;
			trustedSystem.ETS_SystemID = resourceProduct;

			var clientSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			clientSystemConfig.ETM_Product = resourceProduct;
			clientSystemConfig.ETM_CertificateType = "TSC";
			clientSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Client.cer");

			trustedSystem.ETS_ETM_Certificate = clientSystemConfig.PK;

			var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = resourceProduct;
			centralSystemConfig.ETM_CertificateType = "CSC";
			centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");

			Factory.Save();

			return trustedSystem;
		}

		public class LoginServiceBaseControllerForTest : LoginServiceBaseController<TrustedUserInfo>
		{
			public LoginServiceBaseControllerForTest() : base()
			{
			}

			public LoginServiceBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void GetAutoLoginUrlCore_Exposed(TrustedContext<TrustedAutoLoginInfo, AutoLoginResponse> context)
				=> GetAutoLoginUrlCore(context, context.RequestInfo);

			public void UpdateUserAccountCore_Exposed(TrustedContext<UpdateUserInfo, bool> context)
				=> UpdateUserAccountCore(context, context.RequestInfo);

			public void GetOAuthLoginUrlCore_Exposed(TrustedContext<AuthenticationTokenInfo, OAuthLoginResponse> context)
				=> GetOAuthLoginUrlCore(context);

			protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, TrustedUserInfo info)
			{
				return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, info);
			}
		}

		#endregion
	}
}
