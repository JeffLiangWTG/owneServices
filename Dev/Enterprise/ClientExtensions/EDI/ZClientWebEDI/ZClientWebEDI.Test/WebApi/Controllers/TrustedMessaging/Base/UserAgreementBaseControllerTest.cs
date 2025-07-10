using System;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementBaseControllerTest : TestCaseWithFactory
	{
		#region GetRequiredUserAgreement

		public void TestGetRequiredUserAgreementShouldReturnAgreementOk()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_MinorVersion = 3;
			agreement.ERA_VariantCode = "AUS";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement - 1", data.Title);
			AssertEquals("Enterprise User Agreement Content - Jim Green - some org", data.Content);
			AssertEquals("Version 1", 1, data.VersionNumber);
			AssertEquals("Minor Version 3", 3, data.MinorVersionNumber);
			AssertEquals("Variant AUS", "AUS", data.Variant);
			AssertEquals("USR", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOk_COP()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_MinorVersion = 3;

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement - 1", data.Title);
			AssertEquals("Enterprise User Agreement Content - Jim Green - some org", data.Content);
			AssertEquals("Version 1", 1, data.VersionNumber);
			AssertEquals("Minor Version 3", 3, data.MinorVersionNumber);
			AssertEquals("COP", data.Level);

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_EUA = UserAccount.PK;
			agreementLog.EUL_ERA = agreement.PK;
			Factory.Save();
			context.ResponseInfo = null;
			controller.GetRequiredUserAgreementCore_Exposed(context);
			data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOk_NotAllowOnlineClick()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";

			var myaAgreement = SetupAgreement(EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, "", ZDateTime.UtcNow.AddDays(-1));
			myaAgreement.ERA_Title = "MyAccount Agreement";
			myaAgreement.ERA_Content = "";

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;
			assignment.EAE_AllowOnlineAcceptance = false;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);

			var data = context.ResponseInfo;
			var message = context.Messages.Messages[0];
			AssertNull(data);
			AssertEquals("For assignable agreement, the response should return an error when the agreement is not assigned to the parent", ErrorCodes.Codes.Authorization_ActionNotPermitted, message.Code);
			AssertEquals("For assignable agreement, the response should return an error when the agreement is not assigned to the parent", UserAgreementBaseControllerForTest.AgreementIsNotAllowedOnlineAcceptance, message.Message);

			var myaContext = BuildAgreementRequestContext(controller, TrustedSystem, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(myaContext);

			var result = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, "AU");
			Assert("MYA is not assignable, The value of AllowOnlineAcceptance should be true forever", result.AllowOnlineAcceptance);

			var myaData = myaContext.ResponseInfo;
			AssertNotNull(myaData);
			AssertEquals("MYA is not assignable, the method will return required agreement content", true, myaData.Required);
			AssertEquals("MYA is not assignable, the method will return required agreement content", myaAgreement.ERA_Title, myaData.Title);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOkForOrgAcceptedAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			var staffAcceptanceProxy = Factory.NewWithValidTestData<GlbStaff>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceDatabase.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Alex";
			contact.OC_Email = "alex@ard.com";
			UserAccount.EUA_OC_WebAccessContact = contact.PK;
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staffAcceptanceProxy.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreement_OtherUserAcceptance()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();

			const string type = "UAC";
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_MinorVersion = 0;
			agreement.ERA_VariantCode = "";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);

			var context1 = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "NZ", "9554292294480102693", "Test User", "test.user@123.com");

			controller.AcknowledgeUserAgreementCore_Exposed(context1);

			var context2 = BuildAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, "641994084321439549", type, "NZ");

			controller.GetRequiredUserAgreementCore_Exposed(context2);
			var data = context2.ResponseInfo;

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOkForOrgAcceptedAgreement_COP()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;

			var staffAcceptanceProxy = Factory.NewWithValidTestData<GlbStaff>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceDatabase.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Alex";
			contact.OC_Email = "alex@ard.com";
			UserAccount.EUA_OC_WebAccessContact = contact.PK;
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staffAcceptanceProxy.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOkForOrgAcceptedAgreementWhenNoUserAccountExists_USR()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			UserAccount.WebAccessContact.Delete();
			UserAccount.Delete();
			const string type = "MYA";
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var staffAcceptanceProxy = Factory.NewWithValidTestData<GlbStaff>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceDatabase.LD_OH_WebAccessOrg = org.PK;
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_OH = org.PK;
			agreementLog.EUL_GS = staffAcceptanceProxy.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();
			var newUserAccountCode = "ZXG";

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, newUserAccountCode, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOkForDatabaseAcceptedAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LD = LicenceDatabase.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldReturnAgreementOkForEnterpriseAcceptedAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals(false, data.Required);
			AssertEquals("", data.Title);
			AssertEquals("", data.Content);
			AssertEquals(0, data.VersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
			AssertEquals("", data.Level);
		}

		public void TestGetRequiredUserAgreementShouldNotGenerateToken_WhenERA_TypeIsNotCWN()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.DeniedPartyScreening;
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();
			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement - 1", data.Title);
			AssertEquals("Enterprise User Agreement Content - Jim Green - some org", data.Content);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("USR", data.Level);
			var tokenQuery = new ZQuery();
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, LicenceEnterprise.PK);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix);
			AssertEquals("should not generate token", 0, new BusinessObjectFactory().Load<StmAccessToken>(tokenQuery).Length);
		}

		public void TestGetRequiredUserAgreementShouldNotGenerateToken_WhenAgreementIsAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*) - (*GetAgreementDocsURL*)";
			var doc = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "MLA");
			doc.IsPublished = true;
			agreement.DocManagerInfo.Save();
			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			var tokenQuery = new ZQuery();
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, LicenceEnterprise.PK);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix);
			var existingToken = Factory.Load<StmAccessToken>(tokenQuery);
			AssertEquals("should not generate token", 0, new BusinessObjectFactory().Load<StmAccessToken>(tokenQuery).Length);
		}

		public void TestGetRequiredUserAgreementShouldNotGenerateToken_WhenExistingTokenIsFoundOnTheSameLicenceEnterprise()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*) - (*GetAgreementDocsURL*)";
			var doc = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "MLA");
			doc.IsPublished = true;
			agreement.DocManagerInfo.Save();
			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			var tokenQuery = new ZQuery();
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, LicenceEnterprise.PK);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix);
			var existingToken = Factory.Load<StmAccessToken>(tokenQuery);
			AssertEquals("Agreement has not been accepted yet", 1, existingToken.Length);
			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement - 1", data.Title);
			var firstcontent = data.Content;
			AssertContains("Enterprise User Agreement Content - Jim Green - some org - https://myaccount-portal.cargowise.com/myaccount/admin/UserAgreementEDocRequestHandler.axd?qdata=", firstcontent);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("COP", data.Level);
			context.ResponseInfo = null;
			controller.GetRequiredUserAgreementCore_Exposed(context);
			data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement - 1", data.Title);
			AssertEquals("The doc Link url generated twice should be the same", firstcontent, data.Content);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("COP", data.Level);
			AssertEquals("if existing token is found on the same licence enterprise, use existing token", 1, new BusinessObjectFactory().Load<StmAccessToken>(tokenQuery).Length);
		}

		public void TestGetRequiredUserAgreementShouldPrioritiseCountryCode()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var agreement2 = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement2.ERA_Title = "Agreement 2";
			agreement2.ERA_Content = "Agreement 2 Content";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Should return the agreement with country code", "Enterprise User Agreement", data.Title);
			AssertEquals("Should return the agreement with country code", "Enterprise User Agreement Content", data.Content);
			AssertEquals("Version 1 (the first agreement should be returned)", 1, data.VersionNumber);
			AssertEquals("Version 1.0", 0, data.MinorVersionNumber);
			AssertEquals(0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementEmptyCountryCodeShouldReturnAgreementOk()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, string.Empty);
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement", data.Title);
			AssertEquals("Enterprise User Agreement Content", data.Content);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("Minor Version 0 (autogenerated on save)", 0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementShouldFallBackToEmptyCountryCodeAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, string.Empty);
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Enterprise User Agreement", data.Title);
			AssertEquals("Enterprise User Agreement Content", data.Content);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("Minor Version 0 (autogenerated on save)", 0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementWhenNoFallbackShouldReturnAcceptedOk()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "NZ");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Since the agreement has a different country code and there are no agreements with empty country codes, it should respond as if there are no current agreements", false, data.Required);
			AssertEquals("Empty title", string.Empty, data.Title);
			AssertEquals("Empty title", string.Empty, data.Content);
			AssertEquals("No version", 0, data.VersionNumber);
			AssertEquals("No version", 0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementWhenNoneCurrentShouldReturnAcceptedOk()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string countryCode = "AU";
			SetupAgreement(type, countryCode, ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, "ZZZ", type, countryCode);
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Precondition: No user should exist with code ZZZ", false, Factory.Exists(typeof(EdiCustomerUserAccount), new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "ZZZ")));
			AssertEquals("Valid agreements with no existing user account should return not yet accepted agreement", true, data.Required);
			AssertEquals("Enterprise User Agreement", data.Title);
			AssertEquals("Enterprise User Agreement Content", data.Content);
			AssertEquals("Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("Minor Version 0 (autogenerated on save)", 0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementWhenNoUserShouldReturnAcceptedOkWithContent()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, "MYA", "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Valid agreement type with no current agreements should act as if one was accepted", false, data.Required);
			AssertEquals("Empty title", string.Empty, data.Title);
			AssertEquals("Empty title", string.Empty, data.Content);
			AssertEquals("No version", 0, data.VersionNumber);
			AssertEquals("No version", 0, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementShouldGetCurrentAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string countryCode = "AU";
			var oldAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			oldAgreement.ERA_Type = type;
			oldAgreement.ERA_Title = "Old Agreement";
			oldAgreement.ERA_Content = "Old Agreement Content";
			oldAgreement.ERA_RN_NKCountryCode = countryCode;
			oldAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			oldAgreement.ERA_VersionNumber = 1;
			var newAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			newAgreement.ERA_Type = type;
			newAgreement.ERA_Title = "New Agreement";
			newAgreement.ERA_Content = "New Agreement Content";
			newAgreement.ERA_RN_NKCountryCode = countryCode;
			newAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			newAgreement.ERA_VersionNumber = 2;
			newAgreement.ERA_MinorVersion = 5;
			var fallbackAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			fallbackAgreement.ERA_Type = type;
			fallbackAgreement.ERA_Title = "Fallback Agreement";
			fallbackAgreement.ERA_Content = "Fallback Agreement Content";
			fallbackAgreement.ERA_RN_NKCountryCode = string.Empty;
			fallbackAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			fallbackAgreement.ERA_VersionNumber = 1;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, countryCode);
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Agreement has not been accepted yet", true, data.Required);
			AssertEquals("New Agreement", data.Title);
			AssertEquals("New Agreement Content", data.Content);
			AssertEquals("Version 2", 2, data.VersionNumber);
			AssertEquals("Version 2.5", 5, data.MinorVersionNumber);
		}

		public void TestGetRequiredUserAgreementShouldNotReturnContentOfInactiveAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string countryCode = "AU";
			var agreement = SetupAgreement(type, countryCode, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_IsActive = false;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, countryCode);
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Since the agreement is inactive, it should respond as if there are no current agreements", false, data.Required);
			AssertEquals("Empty title", string.Empty, data.Title);
			AssertEquals("Empty title", string.Empty, data.Content);
			AssertEquals("No version", 0, data.VersionNumber);
			AssertEquals("No version", 0, data.MinorVersionNumber);
		}

		[UseSnapshotProtection(true)]
		public void TestGetRequiredUserAgreementShouldNotErrorWhenNotAllowOnlineClickAndAlreadyAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;
			assignment.EAE_AllowOnlineAcceptance = false;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);

			var data = context.ResponseInfo;
			var message = context.Messages.Messages[0];
			AssertNull(data);
			AssertEquals("Agreement is not accepted so error should be thrown", ErrorCodes.Codes.Authorization_ActionNotPermitted, message.Code);
			AssertEquals("Agreement is not accepted so error should be thrown", UserAgreementBaseControllerForTest.AgreementIsNotAllowedOnlineAcceptance, message.Message);

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			context = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);

			data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Should send non-required response data", false, data.Required);
			AssertEquals("Empty title", string.Empty, data.Title);
			AssertEquals("Empty title", string.Empty, data.Content);
			AssertEquals("No version", 0, data.VersionNumber);
			AssertEquals("No version", 0, data.MinorVersionNumber);
		}

		#region Bad Request

		public void TestGetRequiredUserAgreementInvalidTypeShouldReturnBadRequest()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();

			var controller = CreateController(logger);
			var context = BuildAgreementRequestContext(controller, TrustedSystem, "EXA", "AU");
			controller.GetRequiredUserAgreementCore_Exposed(context);
			var data = context.Messages;
			AssertNull(context.ResponseInfo);
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", data.Messages[0].Message);
		}

		#endregion Bad Request

		#endregion GetRequiredUserAgreement

		#region Acknowledge User Agreement

		public void TestAcknowledgeUserAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			SetupCustomerUserAccount();
			const string type = "MYA";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, "UA", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "UA", newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			AssertNotEquals("Should not have overwritten name from request", newUserName, UserAccount.EUA_FullName);
			AssertNotEquals("Should not have overwritten email from request", newUserEmail, UserAccount.EUA_Email);
			AssertNotEquals("Should not have overwritten country from request", "UA", UserAccount.EUA_RN_NKCountry);
			AssertEquals("Should have been acknowledged", true, UserAccount.HasAcknowledgedUserAgreement(agreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, UserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(UserAccount.PK, agreementLog.EUL_EUA);
			AssertEquals(LicenceDatabase.PK, agreementLog.EUL_LD);
			AssertEquals(LicenceEnterprise.PK, agreementLog.EUL_LE);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Enterprise User Agreement Content - alexander blah - some org", email.Body);
			AssertEquals("Enterprise User Agreement - 1", email.Subject);
			AssertEquals("alex@g.com", email.Recipients[0].Email);
		}

		public void TestAcknowledgeUserAgreement_NoUserSpecified()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			LicenceDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			const string name = "Steve da Dragon";
			const string email = "stevist@firefactory.com";
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, null, ZDateTime.UtcNow.AddDays(-1));

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var userInfo = new UserAgreementInfo()
			{
				Product = LicenceDatabase.LD_Product,
				SystemId = TrustedSystem.ETS_SystemID,
				TenantId = LicenceDatabase.LD_TenantID,
				UserAgreementType = type,
				FullName = name,
				Email = email,
				AgreementDate = ZDateTime.UtcNow.ToDateTime(),
				IPAddress = "10.6.6.6",
				MajorVersion = agreement.ERA_VersionNumber.ToString(),
				MinorVersion = "0",
			};
			var context = new TrustedContextForTest<UserAgreementInfo, bool>(LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, userInfo, TrustedSystem, controller) { Success = true };
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_AcceptedByName, name));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals("10.6.6.6", agreementLog.EUL_AcceptedByIPAddress);
			AssertEquals(name, agreementLog.EUL_AcceptedByName);
			AssertEquals(email, agreementLog.EUL_AcceptedByEmail);
			AssertEquals(agreement.ERA_VersionNumber.ToString(), agreementLog.EUL_MajorVersion);
			AssertEquals("0", agreementLog.EUL_MinorVersion);
			AssertEquals(type, agreementLog.EUL_Type);
			AssertEquals(LicenceDatabase.PK, agreementLog.EUL_LD);
			AssertEquals(LicenceEnterprise.PK, agreementLog.EUL_LE);
		}

		public void TestAcknowledgeUserAgreement_NotAllowOnlineClick()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			LicenceDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			const string name = "Steve da Dragon";
			const string email = "stevist@firefactory.com";
			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, null, ZDateTime.UtcNow.AddDays(-1));

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;
			assignment.EAE_AllowOnlineAcceptance = false;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var userInfo = new UserAgreementInfo()
			{
				Product = LicenceDatabase.LD_Product,
				SystemId = TrustedSystem.ETS_SystemID,
				TenantId = LicenceDatabase.LD_TenantID,
				UserAgreementType = type,
				FullName = name,
				Email = email,
				AgreementDate = ZDateTime.UtcNow.ToDateTime(),
				IPAddress = "10.6.6.6",
				MajorVersion = agreement.ERA_VersionNumber.ToString(),
				MinorVersion = "0",
			};
			var context = new TrustedContextForTest<UserAgreementInfo, bool>(LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, userInfo, TrustedSystem, controller) { Success = true };
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			var message = context.Messages.Messages[0];

			AssertEquals(false, context.ResponseInfo);
			AssertEquals(ErrorCodes.Codes.Authorization_ActionNotPermitted, message.Code);
			AssertEquals(UserAgreementBaseControllerForTest.AgreementIsNotAllowedOnlineAcceptance, message.Message);
		}

		public void TestAcknowledgeUserAgreementCreateNewUser()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			LicenceDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			const string type = "MYA";
			const string newUserID = "XAX";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, "UA", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "UA", newUserID, newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			var newUserAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, newUserID));
			AssertNotNull(newUserAccount);
			AssertEquals("Should have been associated to the LD", LicenceDatabase.PK, newUserAccount.EUA_LD);
			AssertEquals("Should have populated name from request", newUserName, newUserAccount.EUA_FullName);
			AssertEquals("Should have populated email from request", newUserEmail, newUserAccount.EUA_Email);
			AssertEquals("Should have populated country from request", "UA", newUserAccount.EUA_RN_NKCountry);
			AssertEquals("Should require email verification", true, newUserAccount.EUA_IsEmailVerificationRequired);

			AssertEquals("Should have been acknowledged", true, newUserAccount.HasAcknowledgedUserAgreement(agreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, newUserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(newUserAccount.PK, agreementLog.EUL_EUA);
			AssertEquals(LicenceDatabase.PK, agreementLog.EUL_LD);
			AssertEquals(LicenceEnterprise.PK, agreementLog.EUL_LE);
		}

		public void TestSubmitUserAgreementCreateNewUser_BadCountry()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string newUserID = "XAX";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, "99", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "99", newUserID, newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			var newUserAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, newUserID));
			AssertNotNull(newUserAccount);
			AssertEquals("Should have been associated to the LD", LicenceDatabase.PK, newUserAccount.EUA_LD);
			AssertEquals("Should have populated name from request", newUserName, newUserAccount.EUA_FullName);
			AssertEquals("Should have populated email from request", newUserEmail, newUserAccount.EUA_Email);
			AssertEquals("Should have populated country from request", "", newUserAccount.EUA_RN_NKCountry);

			AssertEquals("Should have been acknowledged", true, newUserAccount.HasAcknowledgedUserAgreement(agreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, newUserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(newUserAccount.PK, agreementLog.EUL_EUA);
		}

		public void TestSubmitUserAgreementCreateNewUser_LongCountryCode()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string newUserID = "XAX";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, "11", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "123456", newUserID, newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(false, context.ResponseInfo);

			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals("There are no current User Agreements for the given UserAgreementType and UserCountry combination.", context.Messages.Messages[0].Message);
		}

		public void TestAcknowledgeUserAgreementCreateNewUserNullEmail()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string newUserID = "XAX";
			const string newUserName = "alexander blah";
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "AU", newUserID, newUserName, email: null);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			var newUserAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, newUserID));
			AssertNotNull(newUserAccount);
			AssertEquals("Should have been associated to the LD", LicenceDatabase.PK, newUserAccount.EUA_LD);
			AssertEquals("Should have populated name from request", newUserName, newUserAccount.EUA_FullName);
			AssertEquals("Should have empty email", string.Empty, newUserAccount.EUA_Email);

			AssertEquals("Should have been acknowledged", true, newUserAccount.HasAcknowledgedUserAgreement(agreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, newUserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(newUserAccount.PK, agreementLog.EUL_EUA);
		}

		public void TestAcknowledgeUserAgreementEmptyCountryCode()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "AU", newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			AssertNotEquals("Should not have overwritten name from request", newUserName, UserAccount.EUA_FullName);
			AssertNotEquals("Should not have overwritten email from request", newUserEmail, UserAccount.EUA_Email);

			AssertEquals("Should have been acknowledged", true, UserAccount.HasAcknowledgedUserAgreement(agreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, UserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(UserAccount.PK, agreementLog.EUL_EUA);
		}

		public void TestAcknowledgeUserAgreementShouldSubmitCurrentAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			const string countryCode = "AU";
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var oldAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			oldAgreement.ERA_Type = type;
			oldAgreement.ERA_Title = "Old Agreement";
			oldAgreement.ERA_Content = "Old Agreement Content";
			oldAgreement.ERA_RN_NKCountryCode = countryCode;
			oldAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			oldAgreement.ERA_VersionNumber = 1;
			var newAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			newAgreement.ERA_Type = type;
			newAgreement.ERA_Title = "New Agreement";
			newAgreement.ERA_Content = "New Agreement Content";
			newAgreement.ERA_RN_NKCountryCode = countryCode;
			newAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			newAgreement.ERA_VersionNumber = 2;
			var fallbackAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			fallbackAgreement.ERA_Type = type;
			fallbackAgreement.ERA_Title = "Fallback Agreement";
			fallbackAgreement.ERA_Content = "Fallback Agreement Content";
			fallbackAgreement.ERA_RN_NKCountryCode = string.Empty;
			fallbackAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			fallbackAgreement.ERA_VersionNumber = 1;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, countryCode, newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			AssertNotEquals("Should not have overwritten name from request", newUserName, UserAccount.EUA_FullName);
			AssertNotEquals("Should not have overwritten email from request", newUserEmail, UserAccount.EUA_Email);

			AssertEquals("Should have been acknowledged", true, UserAccount.HasAcknowledgedUserAgreement(newAgreement));
			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, UserAccount.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(newAgreement.PK, agreementLog.EUL_ERA);
			AssertEquals(UserAccount.PK, agreementLog.EUL_EUA);
		}

		public void TestAcknowledgeUserAgreement_DatabaseLevelAgreement_ShouldNotSetEUL_LE()
		{
			var logger = new NLogWrapperForTest(GetType());

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			const string newUserName = "alexander blah";
			const string newUserEmail = "alex@g.com";
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VariantCode = "AAA";
			SaveFactoryWithoutAgreementDateTriggers();

			var assignmentCollection = new EdiUserAgreementAssignmentCollection(LicenceEnterprise);
			var assignment = assignmentCollection.AddNew();
			assignment.EAE_AgreementType = type;
			assignment.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			assignment.EAE_ParentID = LicenceDatabase.PK;
			assignment.EAE_VariantCode = "AAA";
			assignment.EAE_OH_ClientAgreementOrg = LicenceEnterprise.LE_OH;
			Factory.Save();

			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "UA", newUserName, newUserEmail);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(true, context.ResponseInfo);

			var agreementLog = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LD, LicenceDatabase.PK));
			AssertGreaterThan(agreementLog.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(agreement.PK, agreementLog.EUL_ERA);
			AssertEquals(LicenceDatabase.PK, agreementLog.EUL_LD);
			AssertEquals("Acknowledgement should not set LE", Guid.Empty, agreementLog.EUL_LE);
		}

		#region BadRequest

		public void TestAcknowledgeUserAgreementInvalidTypeShouldCreateErrorMessage()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, "EUA", "AU", UserAccount.EUA_UserID, "alexander blah", "alex@g.com");
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(false, context.ResponseInfo);

			var data = context.Messages;
			AssertEquals($"Please enter a valid {nameof(UserAgreementInfo.UserAgreementType)}.", data.Messages[0].Message);
		}

		public void TestAcknowledgeUserAgreementInvalidAgreementShouldCreateErrorMessage()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, "MYA", "AU", UserAccount.EUA_UserID, "alexander blah", "alex@g.com");
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(false, context.ResponseInfo);
			var data = context.Messages;
			AssertEquals(FormattableString.Invariant($"There are no current User Agreements for the given {nameof(UserAgreementInfo.UserAgreementType)} and {nameof(UserAgreementInfo.UserCountry)} combination."),
				data.Messages[0].Message);
		}

		public void TestValidateSubmissionRequestDataMaxLengthErrors()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();

			var fullName = ZString.Replicate('a', EdiCustomerUserAccountSchema.EUA_FullName.MaxLength + 1);
			var email = ZString.Replicate('a', EdiCustomerUserAccountSchema.EUA_Email.MaxLength + 1);
			var controller = CreateController(logger);
			var context = BuildAcknowledgeRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, "MYA", "AU", UserAccount.EUA_UserID, fullName, email);
			controller.AcknowledgeUserAgreementCore_Exposed(context);
			AssertEquals(false, context.ResponseInfo);
			var data = context.Messages;
			AssertEquals($"{nameof(UserAgreementInfo.FullName)} exceeds its max length of {EdiCustomerUserAccountSchema.EUA_FullName.MaxLength}", data.Messages[0].Message);
			AssertEquals($"{nameof(UserAgreementInfo.Email)} exceeds its max length of {EdiCustomerUserAccountSchema.EUA_Email.MaxLength}", data.Messages[1].Message);
		}

		#endregion BadRequest

		#endregion Acknowledge User Agreement

		#region GetRequiredEnterpriseUserAgreementUrl

		public void TestGetRequiredEnterpriseUserAgreementUrlShouldReturnOk()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceEnterprise;
			Factory.Save();

			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise CWN Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise CWN Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertEquals("Agreement has not been accepted yet", true, data.Required);

			var url = data.Url;
			AssertStartsWith("Should generate link to UserAgreement.aspx", FormattableString.Invariant($"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.Trim('/')}/Admin/UserAgreement.aspx?data="), url);
			var indexOfPreToken = url.IndexOf('=');
			var token = WebUtility.UrlDecode(url.Substring(indexOfPreToken + 1));
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();

			AssertEquals("Token should be valid", true, tokenControl.TryPeek(token, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));
			var scope = tokenInfo.Scope;
			var tokenScopeObj = JObject.Parse(scope);
			AssertEquals("UserAgreementController", tokenScopeObj[UserAgreementUrlProvider.SourceKey].ToString());
			AssertEquals(UserAgreementTokenTypes.Verify, tokenScopeObj[UserAgreementUrlProvider.TypeKey].ToString());
			AssertEquals(LicenceDatabase.PK.ToString(), tokenScopeObj[UserAgreementUrlProvider.DatabaseKey].ToString());
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, tokenScopeObj[UserAgreementUrlProvider.AgreementTypeKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientNameKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientJobTitleKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientEmailKey].ToString());
			AssertEquals(true, tokenScopeObj[UserAgreementUrlProvider.SendAgreementCopyKey].ToObject<bool>());
		}

		public void TestGetRequiredEnterpriseUserAgreementUrlDatabaseAssignment()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = LicenceDatabase;
			Factory.Save();

			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise CWN Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise CWN Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertEquals("Agreement has not been accepted yet", true, data.Required);

			var url = data.Url;
			AssertStartsWith("Should generate link to UserAgreement.aspx", FormattableString.Invariant($"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.Trim('/')}/Admin/UserAgreement.aspx?data="), url);
			var indexOfPreToken = url.IndexOf('=');
			var token = WebUtility.UrlDecode(url.Substring(indexOfPreToken + 1));
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();

			AssertEquals("Token should be valid", true, tokenControl.TryPeek(token, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));
			var scope = tokenInfo.Scope;
			var tokenScopeObj = JObject.Parse(scope);
			AssertEquals("UserAgreementController", tokenScopeObj[UserAgreementUrlProvider.SourceKey].ToString());
			AssertEquals(UserAgreementTokenTypes.Verify, tokenScopeObj[UserAgreementUrlProvider.TypeKey].ToString());
			AssertEquals(LicenceDatabase.PK.ToString(), tokenScopeObj[UserAgreementUrlProvider.DatabaseKey].ToString());
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, tokenScopeObj[UserAgreementUrlProvider.AgreementTypeKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientNameKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientJobTitleKey].ToString());
			AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientEmailKey].ToString());
			AssertEquals(true, tokenScopeObj[UserAgreementUrlProvider.SendAgreementCopyKey].ToObject<bool>());
		}

		public void TestGetRequiredEnterpriseUserAgreementUrl_AlreadyAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise CWN Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise CWN Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			Factory.Save();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals("Agreement has been accepted", false, data.Required);
			Assert("Agreement has been accepted", string.IsNullOrEmpty(data.Url));
		}

		public void TestGetRequiredEnterpriseUserAgreementUrlDatabaseAssignment_AlreadyAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise CWN Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise CWN Agreement Content - (*UserFullName*) - (*ClientName*)";
			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceDatabase;

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LD = LicenceDatabase.PK;
			agreementLog.EUL_ERA = agreement.PK;
			Factory.Save();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals("Agreement has been accepted", false, data.Required);
			Assert("Agreement has been accepted", string.IsNullOrEmpty(data.Url));
		}

		public void TestGetRequiredEnterpriseUserAgreementUrl_NoAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			AssertNotNull(data);
			AssertEquals("Agreement has been accepted", false, data.Required);
			Assert("Agreement has been accepted", string.IsNullOrEmpty(data.Url));
		}

		public void TestGetRequiredEnterpriseUserAgreementUrl_NotAllowedOnline()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();

			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, string.Empty, ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise CWN Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise CWN Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VariantCode = "va1";
			agreement.ERA_VariantDescription = "va1 desc";

			SaveFactoryWithoutAgreementDateTriggers();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;
			assignment.EAE_VariantCode = "va1";
			assignment.EAE_AllowOnlineAcceptance = false;

			Factory.Save();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			var message = context.Messages.Messages[0];
			AssertNull(data);
			AssertEquals(ErrorCodes.Codes.Authorization_ActionNotPermitted, message.Code);
			AssertEquals(UserAgreementBaseControllerForTest.AgreementIsNotAllowedOnlineAcceptance, message.Message);

			assignment.EAE_AllowOnlineAcceptance = true;
			Factory.Save();

			var context2 = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context2);
			var data2 = context2.ResponseInfo;
			AssertNotNull(data2);
			AssertEquals("Response should return required agreement when AllowOnlineAcceptance is true", true, data2.Required);
		}

		public void TestGetRequiredEnterpriseUserAgreementUrlShouldNotErrorWhenNotAllowOnlineClickAndAlreadyAccepted()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupCustomerUserAccount();

			var type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement = SetupAgreement(type, "", ZDateTime.UtcNow.AddDays(-1));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = type;
			assignment.Parent = LicenceEnterprise;
			assignment.EAE_AllowOnlineAcceptance = false;
			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, shouldSendAgreementCopy: true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);
			var data = context.ResponseInfo;
			var message = context.Messages.Messages[0];
			AssertNull(data);
			AssertEquals("Agreement is not accepted so error should be thrown", ErrorCodes.Codes.Authorization_ActionNotPermitted, message.Code);
			AssertEquals("Agreement is not accepted so error should be thrown", UserAgreementBaseControllerForTest.AgreementIsNotAllowedOnlineAcceptance, message.Message);

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			SaveFactoryWithoutAgreementDateTriggers();

			context = BuildEnterpriseAgreementRequestContext(controller, TrustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, type, true);
			controller.GetRequiredEnterpriseAgreementCore_Exposed(context);

			data = context.ResponseInfo;
			AssertNotNull(data);

			AssertEquals("Should send non-required response data", false, data.Required);
			Assert("Should send non-required response data", string.IsNullOrEmpty(data.Url));
		}

		#endregion

		#region GetAcceptances

		public void TestGetAcceptancesShouldReturnAgreementOk()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = "MYA";
			var agreement = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-2));
			agreement.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_MinorVersion = 3;
			agreement.ERA_VariantCode = "AUS";

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_LE = LicenceEnterprise.PK;
			agreementLog.EUL_ERA = agreement.PK;
			agreementLog.EUL_AcceptedByEmail = "alex@email.com";
			agreementLog.EUL_AcceptedByIPAddress = "0.0.0.0";
			agreementLog.EUL_AcceptedByName = "alex";
			agreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			agreementLog.EUL_MajorVersion = "1";
			agreementLog.EUL_MinorVersion = "0";
			agreementLog.EUL_VariantCode = "AUS";
			agreementLog.EUL_Type = type;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcceptanceRequestContext(controller, TrustedSystem, LicenceDatabase, type);
			controller.GetAcceptancesCore_Exposed(context);
			var data = context.ResponseInfo;

			AssertEquals("Should return acceptance for agreement", 1, data.Acceptances.Count);
			AssertEquals("Should match log", agreementLog.EUL_AcceptedByEmail, data.Acceptances[0].AcceptedByEmail);
			AssertEquals("Should match log", agreementLog.EUL_AcceptedByIPAddress, data.Acceptances[0].AcceptedIPAddresss);
			AssertEquals("Should match log", agreementLog.EUL_AcceptedByName, data.Acceptances[0].AcceptedByName);
			AssertEquals("Should match log", agreementLog.EUL_AcceptanceTimeUtc.Date, data.Acceptances[0].AcceptedTimeUtc.ConvertToZDateTime().Date);
			AssertEquals("Should match log", agreementLog.EUL_MajorVersion, data.Acceptances[0].MajorVersion);
			AssertEquals("Should match log", agreementLog.EUL_MinorVersion, data.Acceptances[0].MinorVersion);
			AssertEquals("Should match log", agreementLog.EUL_VariantCode, data.Acceptances[0].Variant);
			AssertEquals("Should match log", type, data.Acceptances[0].Type);
			AssertEquals("Should match agreement", agreement.ERA_Content, data.Acceptances[0].Content);
			AssertEquals("Should match agreement", agreement.ERA_Title, data.Acceptances[0].Title);
			AssertEquals("Should match agreement", agreement.ERA_EffectiveTimeUtc.Date, data.Acceptances[0].EffectiveStartUtc.ConvertToZDateTime().Date);
		}

		public void TestGetAcceptancesDatabaseLevelAgreement()
		{
			var logger = new NLogWrapperForTest(GetType());

			SetupCustomerUserAccount();
			const string type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			var agreement1 = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-2));
			agreement1.ERA_Title = "Enterprise User Agreement - (*AgreementVersionNumber*)";
			agreement1.ERA_Content = "Enterprise User Agreement Content - (*UserFullName*) - (*ClientName*)";
			agreement1.ERA_VersionNumber = 1;
			agreement1.ERA_MinorVersion = 3;
			agreement1.ERA_VariantCode = "AUS";
			var agreement2 = SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-2));
			agreement2.ERA_Title = "Enterprise User Agreement 2 - (*AgreementVersionNumber*)";
			agreement2.ERA_Content = "Enterprise User Agreement Content 2 - (*UserFullName*) - (*ClientName*)";
			agreement2.ERA_VersionNumber = 1;
			agreement2.ERA_MinorVersion = 3;
			agreement2.ERA_VariantCode = "NZD";

			var otherLicenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();

			var agreementLog1 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog1.EUL_LD = LicenceDatabase.PK;
			agreementLog1.EUL_ERA = agreement1.PK;
			agreementLog1.EUL_AcceptedByEmail = "alex@email.com";
			agreementLog1.EUL_AcceptedByIPAddress = "0.0.0.0";
			agreementLog1.EUL_AcceptedByName = "alex";
			agreementLog1.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			agreementLog1.EUL_MajorVersion = "1";
			agreementLog1.EUL_MinorVersion = "0";
			agreementLog1.EUL_VariantCode = "AUS";
			agreementLog1.EUL_Type = type;
			var agreementLog2 = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog2.EUL_LD = otherLicenceDatabase.PK;
			agreementLog2.EUL_ERA = agreement2.PK;
			agreementLog2.EUL_AcceptedByEmail = "frank@email.com";
			agreementLog2.EUL_AcceptedByIPAddress = "0.0.0.0";
			agreementLog2.EUL_AcceptedByName = "frank";
			agreementLog2.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			agreementLog2.EUL_MajorVersion = "1";
			agreementLog2.EUL_MinorVersion = "0";
			agreementLog2.EUL_VariantCode = "NZD";
			agreementLog2.EUL_Type = type;

			SaveFactoryWithoutAgreementDateTriggers();

			var controller = CreateController(logger);
			var context = BuildAcceptanceRequestContext(controller, TrustedSystem, LicenceDatabase, type);
			controller.GetAcceptancesCore_Exposed(context);
			var data = context.ResponseInfo;

			AssertEquals("Should only return acceptances for specified LicenceDatabase", 1, data.Acceptances.Count);
			AssertEquals("Should match log", agreementLog1.EUL_AcceptedByEmail, data.Acceptances[0].AcceptedByEmail);
			AssertEquals("Should match log", agreementLog1.EUL_AcceptedByIPAddress, data.Acceptances[0].AcceptedIPAddresss);
			AssertEquals("Should match log", agreementLog1.EUL_AcceptedByName, data.Acceptances[0].AcceptedByName);
			AssertEquals("Should match log", agreementLog1.EUL_AcceptanceTimeUtc.Date, data.Acceptances[0].AcceptedTimeUtc.ConvertToZDateTime().Date);
			AssertEquals("Should match log", agreementLog1.EUL_MajorVersion, data.Acceptances[0].MajorVersion);
			AssertEquals("Should match log", agreementLog1.EUL_MinorVersion, data.Acceptances[0].MinorVersion);
			AssertEquals("Should match log", agreementLog1.EUL_VariantCode, data.Acceptances[0].Variant);
			AssertEquals("Should match log", type, data.Acceptances[0].Type);
			AssertEquals("Should match agreement", agreement1.ERA_Content, data.Acceptances[0].Content);
			AssertEquals("Should match agreement", agreement1.ERA_Title, data.Acceptances[0].Title);
			AssertEquals("Should match agreement", agreement1.ERA_EffectiveTimeUtc.Date, data.Acceptances[0].EffectiveStartUtc.ConvertToZDateTime().Date);
		}

		#endregion

		#region Integration

		public void TestGetRequiredUserAgreementWhenAgreementAlreadySignedShouldReturnAcceptedOk()
		{
			SetupCustomerUserAccount();
			const string type = "MYA";
			SetupAgreement(type, "AU", ZDateTime.UtcNow.AddDays(-1));
			SaveFactoryWithoutAgreementDateTriggers();

			var logger = new NLogWrapperForTest(GetType());
			var controller = CreateController(logger);
			var agreementRequestContext = BuildAgreementRequestContext(controller, TrustedSystem, type, "AU");
			controller.GetRequiredUserAgreementCore_Exposed(agreementRequestContext);
			var data = agreementRequestContext.ResponseInfo;

			AssertEquals("Precondition: Agreement has not been accepted yet", true, data.Required);
			AssertEquals("Precondition", "Enterprise User Agreement", data.Title);
			AssertEquals("Precondition", "Enterprise User Agreement Content", data.Content);
			AssertEquals("Precondition: Version 1 (autogenerated on save)", 1, data.VersionNumber);
			AssertEquals("Precondition: Version 0 (autogenerated on save)", 0, data.MinorVersionNumber);

			var acknowledgeRequestContext = BuildAcknowledgeRequestContext(controller, TrustedSystem, type, "AU", "alexander blah", "alex@g.com");
			controller.AcknowledgeUserAgreementCore_Exposed(acknowledgeRequestContext);
			AssertEquals(true, acknowledgeRequestContext.ResponseInfo);

			agreementRequestContext.ResponseInfo = null;
			controller.GetRequiredUserAgreementCore_Exposed(agreementRequestContext);
			data = agreementRequestContext.ResponseInfo;
			AssertEquals("Agreement has now been accepted", false, data.Required);
			AssertEquals("No title returned once accepted", string.Empty, data.Title);
			AssertEquals("No content returned once accepted", string.Empty, data.Content);
			AssertEquals("No version number returned once accepted", 0, data.VersionNumber);
			AssertEquals("No version number returned once accepted", 0, data.MinorVersionNumber);

			acknowledgeRequestContext.ResponseInfo = false;
			acknowledgeRequestContext.Messages = null;
			controller.AcknowledgeUserAgreementCore_Exposed(acknowledgeRequestContext);
			AssertEquals(false, acknowledgeRequestContext.ResponseInfo);
			var errors = acknowledgeRequestContext.Messages;
			AssertEquals(FormattableString.Invariant($"The User Agreement has already been submitted for the specified {nameof(UserAgreementInfo.UserId)}."), errors.Messages[0].Message);
		}

		#endregion Integration

		#region Build Request

		TrustedContext<UserAgreementInfo, UserAgreementResponseData> BuildAgreementRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string agreementType, string country)
		{
			return BuildAgreementRequestContext(controller, trustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, UserAccount.EUA_UserID, agreementType, country);
		}

		TrustedContext<UserAgreementInfo, UserAgreementResponseData> BuildAgreementRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string userId, string agreementType, string country)
		{
			var userInfo = new UserAgreementInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				FullName = "Jim Green",
				UserCountry = country,
				UserAgreementType = agreementType,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};

			return new TrustedContextForTest<UserAgreementInfo, UserAgreementResponseData>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		TrustedContext<UserAgreementInfo, bool> BuildAcknowledgeRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string agreementType, string country, string fullName, string email)
		{
			return BuildAcknowledgeRequestContext(controller, trustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, agreementType, country, UserAccount.EUA_UserID, fullName, email);
		}

		TrustedContext<UserAgreementInfo, bool> BuildAcknowledgeRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string agreementType, string country, string userId, string fullName, string email)
		{
			return BuildAcknowledgeRequestContext(controller, trustedSystem, LicenceDatabase.LD_Product, TrustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID, agreementType, country, userId, fullName, email);
		}

		TrustedContext<UserAgreementInfo, bool> BuildAcknowledgeRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string agreementType, string country, string userId, string fullName, string email)
		{
			var userInfo = new UserAgreementInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserId = userId,
				UserCountry = country,
				UserAgreementType = agreementType,
				FullName = fullName,
				Email = email,
				ShouldSendAgreementCopy = true,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};
			return new TrustedContextForTest<UserAgreementInfo, bool>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		TrustedContext<EnterpriseAgreementInfo, EnterpriseAgreementResponseData> BuildEnterpriseAgreementRequestContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string tenantId, string agreementType, bool shouldSendAgreementCopy)
		{
			var enterpriseInfo = new EnterpriseAgreementInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserAgreementType = agreementType,
				ShouldSendAgreementCopy = shouldSendAgreementCopy,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};

			return new TrustedContextForTest<EnterpriseAgreementInfo, EnterpriseAgreementResponseData>(product, systemId, enterpriseInfo, trustedSystem, controller) { Success = true };
		}

		TrustedContext<GetAcceptancesInfo, GetAcceptancesResponse> BuildAcceptanceRequestContext(TrustedController controller, EdiTrustedSystem trustedSystem, LicenceDatabase database, string agreementType)
		{
			var userInfo = new GetAcceptancesInfo()
			{
				Product = database.LD_Product,
				SystemId = trustedSystem.ETS_SystemID,
				TenantId = database.LD_TenantID,
				UserAgreementType = agreementType,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};

			return BuildAcceptanceRequestContext(controller, trustedSystem, agreementType, LicenceDatabase.LD_Product, trustedSystem.ETS_SystemID, LicenceDatabase.LD_TenantID);
		}

		TrustedContext<GetAcceptancesInfo, GetAcceptancesResponse> BuildAcceptanceRequestContext(TrustedController controller, EdiTrustedSystem trustedSystem, string agreementType, string product, string systemId, string tenantId)
		{
			var userInfo = new GetAcceptancesInfo()
			{
				Product = product,
				SystemId = systemId,
				TenantId = tenantId,
				UserAgreementType = agreementType,
				InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime()
			};

			return new TrustedContextForTest<GetAcceptancesInfo, GetAcceptancesResponse>(product, systemId, userInfo, trustedSystem, controller) { Success = true };
		}

		#endregion Build Request

		#region Implementation

		EdiUserAgreement SetupAgreement(string type, string countryCode, ZDateTime effectiveTimeUtc)
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = type;
			agreement.ERA_Title = "Enterprise User Agreement";
			agreement.ERA_Content = "Enterprise User Agreement Content";
			agreement.ERA_RN_NKCountryCode = countryCode;
			agreement.ERA_EffectiveTimeUtc = effectiveTimeUtc;
			return agreement;
		}

		void SetupCustomerUserAccount()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "batman@gotham.city";

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "batman@gotham.city";
			applicant.HA_FullName = "batman";

			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_Code = "BAT";
			staff.GS_PER = contact.OC_PER;
			staff.GS_LoginName = "batman";

			TrustedSystem = Factory.New<EdiTrustedSystem>();
			TrustedSystem.ETS_Product = "CSP";
			TrustedSystem.ETS_SystemID = "Production";

			LicenceEnterprise = Factory.New<LicenceEnterprise>();
			LicenceEnterprise.LE_OH = org.PK;
			LicenceEnterprise.LE_EnterpriseCode = "BLA";

			var company = Factory.New<LicenceCompany>();
			company.LC_LE = LicenceEnterprise.PK;
			company.LC_CompanyCode = "BLA";
			company.LC_OH = org.PK;

			LicenceDatabase = Factory.New<LicenceDatabase>();
			LicenceDatabase.LD_LE = LicenceEnterprise.PK;
			LicenceDatabase.LD_DatabaseNumber = 0;
			LicenceDatabase.LD_ServerCode = "123";
			LicenceDatabase.LD_Product = "CSP";
			LicenceDatabase.LD_TenantID = "12345";
			LicenceDatabase.LD_OH_WebAccessOrg = org.PK;
			LicenceDatabase.LD_ETS_TrustedSystem = TrustedSystem.PK;

			UserAccount = Factory.New<EdiCustomerUserAccount>();
			UserAccount.EUA_OC_WebAccessContact = contact.PK;
			UserAccount.EUA_LD = LicenceDatabase.PK;
			UserAccount.EUA_UserID = staff.GS_Code;
			UserAccount.EUA_FullName = "name";
			UserAccount.EUA_Email = "batman@gotham.city";
			UserAccount.EUA_RN_NKCountry = "AU";

			Factory.Save();
		}

		LicenceEnterprise LicenceEnterprise;
		LicenceDatabase LicenceDatabase;
		EdiTrustedSystem TrustedSystem;
		EdiCustomerUserAccount UserAccount;

		protected override void SetUp()
		{
			base.SetUp();
			var collection = new RegistryUserAgreementTypeCollection();
		}

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

		#endregion Implementation

		UserAgreementBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/UserAgreementBase/Acknowledge");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new UserAgreementBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		public class UserAgreementBaseControllerForTest : UserAgreementBaseController<UserAgreementInfo, GetAcceptancesInfo, TrustedInfo>
		{
			public UserAgreementBaseControllerForTest() : base()
			{
			}

			public UserAgreementBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void AcknowledgeUserAgreementCore_Exposed(TrustedContext<UserAgreementInfo, bool> context)
				=> base.AcknowledgeUserAgreementCore(context, context.RequestInfo);

			public void GetRequiredUserAgreementCore_Exposed(TrustedContext<UserAgreementInfo, UserAgreementResponseData> context)
				=> base.GetRequiredUserAgreementCore(context, context.RequestInfo);

			public void GetRequiredEnterpriseAgreementCore_Exposed(TrustedContext<EnterpriseAgreementInfo, EnterpriseAgreementResponseData> context)
				=> base.GetRequiredEnterpriseUserAgreementUrlCore(context, context.RequestInfo);

			public void GetAcceptancesCore_Exposed(TrustedContext<GetAcceptancesInfo, GetAcceptancesResponse> context)
				=> base.GetAcceptancesCore(context, context.RequestInfo);

			protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, TrustedInfo info)
			{
				return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, info);
			}

			protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, IEnterpriseAgreementInfo info)
			{
				return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, (TrustedInfo)info);
			}
		}
	}
}
