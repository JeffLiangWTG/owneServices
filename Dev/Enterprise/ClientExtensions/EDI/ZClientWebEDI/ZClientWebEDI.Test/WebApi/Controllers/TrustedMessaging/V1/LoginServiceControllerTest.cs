using System;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class LoginServiceControllerTest : TestCaseWithFactory
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
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Access to MyAccount is not granted.", data.Messages[0].Message);
			var user = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "U048173"));
			AssertNull(user);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "3001 Access to MyAccount is not granted." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Email verification URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", data.AutoLoginUrl.ToString());
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx?qdata=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			user.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Email verification URL", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", data.AutoLoginUrl.ToString());
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx?qdata=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
			var org = db.WebAccessOrg;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());
			user.Reload();
			var contact = user.WebAccessContact;
			AssertNotNull(contact);
			AssertEquals("Org", org.PK, contact.OC_OH);
			AssertEquals("Name", "User One", contact.OC_ContactName);
			AssertEquals("Email", "user.one@test.com", contact.OC_Email);
			AssertEquals("Web Access", true, contact.OC_WebAccessEnabled);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());
			contact.Reload();
			AssertEquals("Name is updated", "User Two", contact.OC_ContactName);
			AssertEquals("Email is updated", "user.two@test.com", contact.Email);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com", returnUrl: "http://www.cw1.com/download.aspx?1=1&org=\u4E2D\u6587&key=2");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, system);
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());
			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			var scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
			AssertEquals(true, scope.ReturnUrl.EndsWith("http://www.cw1.com/download.aspx?1=1&org=%E4%B8%AD%E6%96%87&key=2"));
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com", returnUrl: "download.aspx?1=1&org=\u4E2D\u6587&key=2");
			response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			content = TrustedMessageResponseHelper.ReadMessage(response, system);
			data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("User agreement is not acknowledged.", data.Messages[0].Message);
			user.Reload();
			AssertNull("Should not create contact", user.WebAccessContact);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "3002 User agreement is not acknowledged." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(ErrorCodes.Descriptions.Authorization_AgreementNotAcknowledged, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "3002 User agreement is not acknowledged." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, system);
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());
			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			var scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
			AssertEquals(true, scope.ReturnUrl.EndsWith("https://myaccount.cargowise.com/"));

			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "AutoLoginUrl: https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogin_InvalidMessage()
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
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var request = new TrustedRequestForTest()
			{
				Request = new TrustedRequest()
				{ EncryptedContent = "Invalid.Message" },
				Signature = "Invalid.Sign",
				IV = "Invalid.IV"
			};
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Descriptions.Validation_InvalidSystem, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message failed to decrypt", "2001 The specified system is not found." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogin_SystemNotFound()
		{
			var logger = new NLogWrapperForTest(GetType());
			var product = "SMF";
			var request = CreateRequest(securityKeyTestHelper, product, "system.not.found", "DDDABCSYD", "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Descriptions.Validation_InvalidSystem, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message failed to decrypt", "2001 The specified system is not found." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Auto login to MyAccount is disabled on this system.", data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "3001 Auto login to MyAccount is disabled on this system." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U049000", "User Two", "user.one@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Access to MyAccount is not granted.", data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "3001 Access to MyAccount is not granted." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogin_InfoExpired()
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
			securityKeyTestHelper.SetSecretKey(db);
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
			Factory.Save();
			var now = ZDateTime.UtcNow.ToDateTime();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U049000", "User Two", "user.one@test.com", true, now.AddMinutes(-5));
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Descriptions.Critical_InfoExpired, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message failed to decrypt", "1003 The info within message has expired." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Codes.Validation_MissingRequiredField, data.Messages[0].Code);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", $"2002 {UserAccountCreationRequestDataValidation.EmailEmptyErrorMessage}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var invalidUserId = ZString.Replicate('U', EdiCustomerUserAccountSchema.EUA_UserID.MaxLength + 1);
			var invalidUserName = ZString.Replicate('N', EdiCustomerUserAccountSchema.EUA_FullName.MaxLength + 1);
			var invalidEmail1 = ZString.Replicate('E', EdiCustomerUserAccountSchema.EUA_Email.MaxLength + 1);
			var invalidEmail2 = "user.one.abc.com";
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, invalidUserId, invalidUserName, invalidEmail1);
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			var errors = data.Messages;
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDMaxLengthErrorMessage, errors[0].Message);
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameMaxLengthErrorMessage, errors[1].Message);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailMaxLengthErrorMessage, errors[2].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted",
$"2003 {UserAccountCreationRequestDataValidation.UserIDMaxLengthErrorMessage} 2003 {UserAccountCreationRequestDataValidation.FullNameMaxLengthErrorMessage} 2003 {UserAccountCreationRequestDataValidation.EmailMaxLengthErrorMessage}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, invalidUserId, invalidUserName, invalidEmail2);
			response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			content = response.Content.ReadAsStringAsync().Result;
			data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			errors = data.Messages;
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(UserAccountCreationRequestDataValidation.UserIDMaxLengthErrorMessage, errors[0].Message);
			AssertEquals(UserAccountCreationRequestDataValidation.FullNameMaxLengthErrorMessage, errors[1].Message);
			AssertEquals(UserAccountCreationRequestDataValidation.EmailInvalidErrorMessage, errors[2].Message);
		}

		[TestDate(2020, 2, 20)]
		[TestUtcOffset(10, 0, 0)]
		public void TestLogin_ExpiredSecretKey()
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
			db.GetOrCreateTrustedSystem().ETS_SecretKeyExpiryUtc = new ZDateTime(2020, 2, 19);
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals(ErrorCodes.Descriptions.Critical_SecretKeyNotUpToDate, data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message failed to decrypt", "1002 Secret key is missing or out of date." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User Two", "user.two@test.com");
			var response = CallServiceEndpoint(request, AutoLoginEndpointUrl, logger);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<AutoLoginResponse>(content);
			AssertStartsWith("Auto login URL", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token=", data.AutoLoginUrl.ToString());
			ITokenizedAccessControl tokenCtrl = new TokenizedAccessControl();
			var token = data.AutoLoginUrl.ParseQueryString()["token"];
			tokenCtrl.TryPeek(token, AccessTokenTypes.MyAccountAutoLogin, out var info);
			AssertEquals(true, info.Scope.Contains("https://myaccount.cargowise.com/#cargowise"));
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
			securityKeyTestHelper.SetSecretKey(db);
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
			//UserIdNotFound
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173xxx", "User One", "user.one@test.com", isActive: false);
			var response = CallServiceEndpoint(request, UpdateUserEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("User Id is not found.", data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 User Id is not found." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			// isActive: false
			request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com", isActive: false);
			response = CallServiceEndpoint(request, UpdateUserEndpointUrl, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var newFactory = new BusinessObjectFactory();
			var userAccountInNewFactory = newFactory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "U048173"));
			AssertNotNull(userAccountInNewFactory);
			AssertEquals("Licence", 8000, userAccountInNewFactory.Database.LD_DatabaseNumber);
			AssertEquals("Name", "User One", userAccountInNewFactory.EUA_FullName);
			AssertEquals("Email", "user.one@test.com", userAccountInNewFactory.EUA_Email);
			AssertEquals(false, userAccountInNewFactory.EUA_IsActive);
			AssertNull("Should be null if user isn't activate", userAccountInNewFactory.WebAccessContact);
			expectedLogMessages = new string[] { "LoginServiceControllerTest: Request received | 200", "LoginServiceControllerTest: Trusted message decrypted | 200", "LoginServiceControllerTest: success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			// isActive: true
			request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com", isActive: true);
			response = CallServiceEndpoint(request, UpdateUserEndpointUrl, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
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
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
		}

		[HttpContextEnabledTest]
		public void TestUpdateUserAccount_IsActiveNull()
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
			securityKeyTestHelper.SetSecretKey(db);
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
			//IsActive null
			var request = CreateRequest(securityKeyTestHelper, product, system.ETS_SystemID, db.LD_TenantID, "U048173", "User One", "user.one@test.com", isActive: null);
			var response = CallServiceEndpoint(request, UpdateUserEndpointUrl, logger);
			var content = response.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("user_active is not specified.", data.Messages[0].Message);
			var expectedLogMessages = new string[] { "Request received | 200", "Trusted message decrypted | 200", "2002 user_active is not specified. | 400" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
		}

		#endregion Update User Account
		#region Execute
		HttpResponseMessage CallServiceEndpoint(TrustedRequestForTest request, string endpointUrl, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			using (ObjectFactory.Substitute<WTG.TrustedMessaging.ICertificatesProvider>(() => new CertificatesProviderForTest()))
			using (var controller = new LoginServiceV1Controller(logger))
			{
				return ControllerTestHelper.Execute(controller, requestMessage);
			}
		}

		const string AutoLoginEndpointUrl = "http://unit-testing/api/LoginService/AutoLoginUrl";
		const string UpdateUserEndpointUrl = "http://unit-testing/api/LoginService/UserAccount";

		#endregion Execute

		#region Helper Methods
		readonly SecurityKeyTestHelper securityKeyTestHelper = new SecurityKeyTestHelper();
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

		static TrustedRequestForTest CreateRequest(SecurityKeyTestHelper securityKeyTestHelper, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive = null, string returnUrl = null)
		{
			return CreateRequest(securityKeyTestHelper, product, systemId, tenantId, userId, userName, email, isActive, ZDateTime.UtcNow.AddMinutes(5).ToDateTime(), returnUrl);
		}

		static TrustedRequestForTest CreateRequest(SecurityKeyTestHelper securityKeyTestHelper, string product, string systemId, string tenantId, string userId, string userName, string email, bool? isActive, DateTime infoExpires, string returnUrl = null, string country = null)
		{
			string json;
			if (string.IsNullOrWhiteSpace(returnUrl))
			{
				var userInfo = new UpdateUserInfo()
				{ Product = product, SystemId = systemId, TenantId = tenantId, UserId = userId, FullName = userName, Email = email, IsActive = isActive, InfoExpires = infoExpires, UserCountry = country };
				json = JsonConvert.SerializeObject(userInfo);
			}
			else
			{
				var userInfo = new TrustedAutoLoginInfo()
				{ Product = product, SystemId = systemId, TenantId = tenantId, UserId = userId, FullName = userName, Email = email, ReturnUrl = returnUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(returnUrl) : new Uri(returnUrl, UriKind.Relative), InfoExpires = infoExpires, UserCountry = country };
				json = JsonConvert.SerializeObject(userInfo);
			}

			var certProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(certProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var trustedRequest = new TrustedRequest()
			{ EncryptedContent = trustedMessage.EncryptedContent, Product = product, SystemId = systemId };
			return new TrustedRequestForTest()
			{ Request = trustedRequest, Signature = trustedMessage.Signature, IV = trustedMessage.IV };
		}

		#endregion Helper Methods
	}
}
