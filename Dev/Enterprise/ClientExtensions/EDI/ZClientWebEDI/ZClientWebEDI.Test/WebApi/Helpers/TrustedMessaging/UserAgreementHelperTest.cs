using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementHelperTest : TestCaseWithFactory
	{
		public void TestGetEnterpriseUserAgreementResponse()
		{
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase, out var trustedSystem);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			AttachAgreementToEnterprise(agreement, licenceEnterprise);

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = licenceEnterprise.Organisation?.OH_FullName ?? ZString.Empty,
				UserCountry = licenceEnterprise.Organisation.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			var nullResponse = UserAgreementHelper.GetEnterpriseUserAgreementResponse((LicenceEnterprise)null, agreementInfo, null, null);
			Assert(!nullResponse.ResponseData.Required);
			AssertNullOrEmpty(nullResponse.ResponseData.Title);

			var response = UserAgreementHelper.GetEnterpriseUserAgreementResponse(licenceEnterprise, agreementInfo, null, null);
			Assert(response.ResponseData.Required);
			AssertEquals(agreement.ERA_Title, response.ResponseData.Title);
			AssertEquals(agreement.ERA_VersionNumber, response.ResponseData.VersionNumber);
			AssertEquals(agreement.ERA_MinorVersion, response.ResponseData.MinorVersionNumber);
			AssertEquals(agreement.ERA_VariantCode, response.ResponseData.Variant);
		}

		public void TestGetEnterpriseUserAgreementResponse_DatabaseLevelAgreement()
		{
			var agreementOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase1, out _);
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceEnterprise.Databases.Add(licenceDatabase2);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			var assignment1 = AttachAgreementToEnterprise(agreement, licenceEnterprise);
			assignment1.Parent = licenceDatabase1;
			var assignment2 = agreement.Factory.New<EdiUserAgreementAssignment>();
			assignment2.EAE_AgreementType = agreement.ERA_Type;
			assignment2.Parent = licenceDatabase2;
			assignment2.EAE_AllowOnlineAcceptance = true;
			assignment2.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment1.EAE_OH_ClientAgreementOrg = assignment2.EAE_OH_ClientAgreementOrg = agreementOrg.PK;

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = agreementOrg.OH_FullName,
				UserCountry = agreementOrg.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			Assert("Precondition: Acknowledge should succeed", UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceDatabase1, agreementInfo));
			var response = UserAgreementHelper.GetEnterpriseUserAgreementResponse(new[] { assignment1, assignment2 }, agreementInfo, null, null);
			Assert("Should be required since one assignment has not been acknowledged", response.ResponseData.Required);
			AssertEquals(agreement.ERA_Title, response.ResponseData.Title);
			AssertEquals(agreement.ERA_VersionNumber, response.ResponseData.VersionNumber);
			AssertEquals(agreement.ERA_MinorVersion, response.ResponseData.MinorVersionNumber);
			AssertEquals(agreement.ERA_VariantCode, response.ResponseData.Variant);

			Assert("Precondition: Acknowledge should succeed", UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceDatabase2, agreementInfo));
			response = UserAgreementHelper.GetEnterpriseUserAgreementResponse(new[] { assignment1, assignment2 }, agreementInfo, null, null);
			Assert("Should no longer be required since both assignments have been acknowledged", !response.ResponseData.Required);
		}

		public void TestGetEnterpriseUserAgreementResponse_NoAssignments()
		{
			var agreementOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase1, out _);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			var assignment1 = AttachAgreementToEnterprise(agreement, licenceEnterprise);
			assignment1.Parent = licenceDatabase1;
			assignment1.EAE_OH_ClientAgreementOrg = agreementOrg.PK;

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = agreementOrg.OH_FullName,
				UserCountry = agreementOrg.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			Assert("Precondition: Acknowledge should succeed", UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceDatabase1, agreementInfo));
			var response = UserAgreementHelper.GetEnterpriseUserAgreementResponse(Array.Empty<EdiUserAgreementAssignment>(), agreementInfo, null, null);
			AssertEquals("Response should be empty since no assignments are specified", false, response.ResponseData.Required);
			AssertEquals("Response should be empty since no assignments are specified", string.Empty, response.ResponseData.Title);
		}

		public void TestAcknowledgeEnterpriseUserAgreement()
		{
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out _, out _);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			AttachAgreementToEnterprise(agreement, licenceEnterprise);

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = licenceEnterprise.Organisation?.OH_FullName ?? ZString.Empty,
				UserCountry = licenceEnterprise.Organisation.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				MajorVersion = "1",
				MinorVersion = "3",
				Variant = "AUS",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			Assert("Should fail if no enterprise provided", !UserAgreementHelper.AcknowledgeEnterpriseUserAgreement((LicenceEnterprise)null, agreementInfo));

			var contactQuery = new ZQuery(OrgContactSchema.OC_OH, licenceEnterprise.LE_OH)
				.AddToFilter(OrgContactSchema.OC_Email, agreementInfo.Email)
				.AddToFilter(OrgContactSchema.OC_ContactName, agreementInfo.FullName)
				.AddToFilter(OrgContactSchema.OC_Title, agreementInfo.JobTitle);

			var logQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LE, licenceEnterprise.PK);

			Assert("Precondition: Contact should not exist", !Factory.Exists(typeof(OrgContact), contactQuery));
			Assert("Precondition: Agreement should not be accepted", !Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), logQuery));

			Assert(UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceEnterprise, agreementInfo));

			var contact2 = Factory.LoadTop1<OrgContact>(contactQuery);

			AssertNotNull("The approver should be added in to enterprise's parent org", contact2);
			Assert(!contact2.OC_WebAccessEnabled);

			var log = Factory.LoadTop1<EdiUserAgreementAcceptanceLog>(logQuery);
			AssertNotNull("The agreement should be acknowledged", log);
			AssertEquals("1", log.EUL_MajorVersion);
			AssertEquals("3", log.EUL_MinorVersion);
			AssertEquals("AUS", log.EUL_VariantCode);
		}

		public void TestAcknowledgeEnterpriseUserAgreement_DatabaseLevelAgreement_ShouldNotLogClientAgreementOrg()
		{
			var agreementOrg = Factory.NewWithValidTestData<OrgHeader>();
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase, out _);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			var assignment = AttachAgreementToEnterprise(agreement, licenceEnterprise);
			assignment.Parent = licenceDatabase;
			assignment.EAE_OH_ClientAgreementOrg = agreementOrg.PK;
			Factory.Save();
			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = agreementOrg.OH_FullName,
				UserCountry = agreementOrg.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",
				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};
			Assert(UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceDatabase, agreementInfo));
			var logQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LD, licenceDatabase.PK);
			logQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_OH, null);
			logQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LE, null);
			Assert("The acceptance should have an EUL_OH and have an empty licence enterprise FK", Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), logQuery));
		}

		public void TestAcknowledgeEnterpriseUserAgreement_DatabaseLevelAgreement_ShouldCreateContactInClientAgreementOrg()
		{
			var agreementOrg = Factory.NewWithValidTestData<OrgHeader>();
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase, out _);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			var assignment = AttachAgreementToEnterprise(agreement, licenceEnterprise);
			assignment.Parent = licenceDatabase;
			assignment.EAE_OH_ClientAgreementOrg = agreementOrg.PK;

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = agreementOrg.OH_FullName,
				UserCountry = agreementOrg.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			var contactQuery = new ZQuery(OrgContactSchema.OC_OH, agreementOrg.PK)
				.AddToFilter(OrgContactSchema.OC_Email, agreementInfo.Email)
				.AddToFilter(OrgContactSchema.OC_ContactName, agreementInfo.FullName)
				.AddToFilter(OrgContactSchema.OC_Title, agreementInfo.JobTitle);
			Assert("Precondition: Contact should not exist", !Factory.Exists(typeof(OrgContact), contactQuery));

			Assert(UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceDatabase, agreementInfo));
			Assert("Contact approver should be created in the client agreement org", Factory.Exists(typeof(OrgContact), contactQuery));
		}

		public void TestCorporateAgreementAcknowledgementNotificationMessageTemplate_WhenLevelIsCOR()
		{
			SetupLicenceEnterprise(Factory, out var licenceEnterprise, out var licenceDatabase, out var trustedSystem);
			var agreement = SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, licenceEnterprise.Organisation.CountryCode, ZDateTime.UtcNow.AddMonths(-1));
			AttachAgreementToEnterprise(agreement, licenceEnterprise);
			var doc1 = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "MLA");
			doc1.IsPublished = true;
			agreement.DocManagerInfo.Save();

			Factory.Save();

			var agreementInfo = new EnterpriseUserAgreementInfo
			{
				ClientAgreementOrgFullName = licenceEnterprise.Organisation?.OH_FullName ?? ZString.Empty,
				UserCountry = licenceEnterprise.Organisation.CountryCode,
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Email = "123@123.com",
				FullName = "name",
				JobTitle = "Title",

				ShouldSendAgreementCopy = true,
				UserId = string.Empty,
			};

			Assert("AcknowledgeEnterpriseUserAgreement should be true", UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(licenceEnterprise, agreementInfo));
			AssertEquals("An email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("The email should have an attachment named TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23)", email.Attachments.Cast<AttachmentDef>().Any(x => x.DisplayName == "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf"));
			AssertEquals("The subject of the email should be the same as ERA_Title", agreement.ERA_Title, email.Subject);
		}

		public static EdiUserAgreement SetupAgreement(DbConnection connection, BusinessObjectFactory factory, string type, string countryCode, ZDateTime effectiveTimeUtc)
		{
			var agreement = factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = type;
			agreement.ERA_Title = "Enterprise User Agreement";
			agreement.ERA_Content = "Enterprise User Agreement (*ClientName*)  (*ClientBusinessRegistration*) (*ClientMainAddressInSingleLine*)  (*GetAgreementDocsURL*) Content";
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_MinorVersion = 3;
			agreement.ERA_VariantDescription = "Variant 1";
			agreement.ERA_RN_NKCountryCode = countryCode;
			agreement.ERA_EffectiveTimeUtc = effectiveTimeUtc;

			connection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
			factory.Save();
			connection.ExecuteNonQuery("ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; ENABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
			return agreement;
		}

		public static EdiUserAgreementAssignment AttachAgreementToEnterprise(EdiUserAgreement agreement, LicenceEnterprise enterprise)
		{
			var assignment = agreement.Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = agreement.ERA_Type;
			assignment.Parent = enterprise;
			assignment.EAE_AllowOnlineAcceptance = true;
			assignment.EAE_VariantCode = agreement.ERA_VariantCode;

			agreement.Factory.Save();
			return assignment;
		}

		public static void SetupLicenceEnterprise(BusinessObjectFactory factory, out LicenceEnterprise licenceEnterprise, out LicenceDatabase licDatabase, out EdiTrustedSystem trustedSystem)
		{
			var org = factory.New<OrgHeader>();
			org.OH_FullName = "some org";
			org.OH_Code = "code";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "batman@gotham.city";

			var applicant = factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "batman@gotham.city";
			applicant.HA_FullName = "batman";

			factory.Save();

			trustedSystem = factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";

			licenceEnterprise = factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = org.PK;
			licenceEnterprise.LE_EnterpriseCode = "BLA";

			licDatabase = factory.New<LicenceDatabase>();
			licDatabase.LD_LE = licenceEnterprise.PK;
			licDatabase.LD_DatabaseNumber = 0;
			licDatabase.LD_ServerCode = "123";
			licDatabase.LD_Product = "CSP";
			licDatabase.LD_TenantID = "12345";
			licDatabase.LD_OH_WebAccessOrg = org.PK;
			licDatabase.LD_ETS_TrustedSystem = trustedSystem.PK;

			var licenceCompany = factory.New<LicenceCompany>();
			licenceCompany.LC_LE = licenceEnterprise.PK;
			licenceCompany.LC_OH = org.PK;
			licenceCompany.LC_CompanyCode = "NOC";

			factory.Save();
		}
	}
}
