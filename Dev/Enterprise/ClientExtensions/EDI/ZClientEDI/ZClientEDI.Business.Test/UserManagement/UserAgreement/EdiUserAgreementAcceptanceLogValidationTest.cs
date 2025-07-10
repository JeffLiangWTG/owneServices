namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.MasterFiles.Business;

	public class EdiUserAgreementAcceptanceLogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEUL_AcceptanceTimeUtcShouldNotBeEmpty()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_AcceptanceTimeUtc.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_AcceptanceTimeUtc();
			AssertHasError(acceptanceLog.EUL_AcceptanceTimeUtcInfo, "Please enter a value.");

			acceptanceLog.EUL_AcceptanceTimeUtc = ZDateTime.Today;
			AssertNoError(acceptanceLog.EUL_AcceptanceTimeUtcInfo, "Please enter a value.");
		}

		public void TestEUL_EUAShouldBeValid()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_EUA.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_EUA();
			AssertHasError(acceptanceLog.EUL_EUAInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_EUA = ZGuid.Invalid;
			AssertHasError(acceptanceLog.EUL_EUAInfo, "Enter a valid selection.");

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			acceptanceLog.EUL_EUA = userAccount.PK;
			AssertNoErrors(acceptanceLog.EUL_EUAInfo);
		}

		public void TestEmptyEUL_EUAShouldBeValidWhenEnterpriseOrDatabaseEntered()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db = Factory.NewWithValidTestData<LicenceDatabase>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_EUA.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_EUA();
			AssertHasError(acceptanceLog.EUL_EUAInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_LE = enterprise.PK;
			acceptanceLog.Validation.ValidateEUL_EUA();
			AssertNoErrors(acceptanceLog.EUL_EUAInfo);

			acceptanceLog.EUL_LE = ZGuid.Empty;
			acceptanceLog.EUL_LD = db.PK;
			acceptanceLog.Validation.ValidateEUL_EUA();
			AssertNoErrors(acceptanceLog.EUL_EUAInfo);
		}

		public void TestEmptyEUL_OHShouldBeValidWhenEnterpriseOrDatabaseEntered()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db = Factory.NewWithValidTestData<LicenceDatabase>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_OH.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_OH();
			AssertHasError(acceptanceLog.EUL_OHInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_LE = enterprise.PK;
			acceptanceLog.Validation.ValidateEUL_OH();
			AssertNoErrors(acceptanceLog.EUL_OHInfo);

			acceptanceLog.EUL_LE = ZGuid.Empty;
			acceptanceLog.EUL_LD = db.PK;
			acceptanceLog.Validation.ValidateEUL_OH();
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
		}

		public void TestEmptyEUL_GSShouldBeValidWhenEnterpriseOrDatabaseEntered()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db = Factory.NewWithValidTestData<LicenceDatabase>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_GS.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_GS();
			AssertHasError(acceptanceLog.EUL_GSInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_LE = enterprise.PK;
			acceptanceLog.Validation.ValidateEUL_GS();
			AssertNoErrors(acceptanceLog.EUL_GSInfo);

			acceptanceLog.EUL_LE = ZGuid.Empty;
			acceptanceLog.EUL_LD = db.PK;
			acceptanceLog.Validation.ValidateEUL_GS();
			AssertNoErrors(acceptanceLog.EUL_GSInfo);
		}

		public void TestEUL_EUAShouldNotBeEnteredWithEUL_GSAndEUL_OH()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_EUA.IsEmpty);
			AssertEquals("Precondition", true, acceptanceLog.EUL_GS.IsEmpty);
			AssertEquals("Precondition", true, acceptanceLog.EUL_OH.IsEmpty);
			AssertNoError(acceptanceLog.EUL_EUAInfo, "Please choose only a user account or a staff + organization combination for the agreement.");
			AssertNoError(acceptanceLog.EUL_OHInfo, "Please choose only a user account or a staff + organization combination for the agreement.");
			AssertNoError(acceptanceLog.EUL_GSInfo, "Please choose only a user account or a staff + organization combination for the agreement.");

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			acceptanceLog.EUL_EUA = userAccount.PK;
			AssertNoErrors(acceptanceLog.EUL_EUAInfo);
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
			AssertNoErrors(acceptanceLog.EUL_GSInfo);

			acceptanceLog.EUL_GS = staff.PK;
			AssertHasError(acceptanceLog.EUL_EUAInfo, "Please choose only a user account or a staff + organization combination for the agreement.");
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
			AssertHasError(acceptanceLog.EUL_GSInfo, "Please choose only a user account or a staff + organization combination for the agreement.");

			acceptanceLog.EUL_GS = ZGuid.Empty;
			acceptanceLog.EUL_OH = organisation.PK;
			AssertHasError(acceptanceLog.EUL_EUAInfo, "Please choose only a user account or a staff + organization combination for the agreement.");
			AssertHasError(acceptanceLog.EUL_OHInfo, "Please choose only a user account or a staff + organization combination for the agreement.");
			AssertNoErrors(acceptanceLog.EUL_GSInfo);

			acceptanceLog.EUL_GS = staff.PK;
			acceptanceLog.EUL_EUA = ZGuid.Empty;
			AssertNoErrors(acceptanceLog.EUL_EUAInfo);
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
			AssertNoErrors(acceptanceLog.EUL_GSInfo);
		}

		public void TestEUL_GSAndEUL_OHValidation()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_EUA.IsEmpty);
			AssertEquals("Precondition", true, acceptanceLog.EUL_GS.IsEmpty);
			AssertEquals("Precondition", true, acceptanceLog.EUL_OH.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_GS();
			acceptanceLog.Validation.ValidateEUL_OH();
			AssertHasError(acceptanceLog.EUL_GSInfo, "Please enter a user account or a staff member or a organization and enterprise.");
			AssertHasError(acceptanceLog.EUL_OHInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_GS = ZGuid.Invalid;
			acceptanceLog.EUL_OH = ZGuid.Invalid;
			AssertHasError(acceptanceLog.EUL_GSInfo, "Enter a valid selection.");
			AssertHasError(acceptanceLog.EUL_OHInfo, "Enter a valid selection.");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			acceptanceLog.EUL_GS = staff.PK;
			acceptanceLog.EUL_OH = ZGuid.Empty;
			AssertNoErrors(acceptanceLog.EUL_GSInfo);
			AssertHasError(acceptanceLog.EUL_OHInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_OH = organisation.PK;
			acceptanceLog.EUL_GS = ZGuid.Empty;
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
			AssertHasError(acceptanceLog.EUL_GSInfo, "Please enter a user account or a staff member or a organization and enterprise.");

			acceptanceLog.EUL_GS = staff.PK;
			AssertNoErrors(acceptanceLog.EUL_GSInfo);
			AssertNoErrors(acceptanceLog.EUL_OHInfo);
		}

		public void TestEUL_ERAShouldBeValid()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			AssertEquals("Precondition", true, acceptanceLog.EUL_ERA.IsEmpty);

			acceptanceLog.Validation.ValidateEUL_ERA();
			AssertHasError(acceptanceLog.EUL_ERAInfo, "Please enter a value.");

			acceptanceLog.EUL_ERA = ZGuid.Invalid;
			AssertHasError(acceptanceLog.EUL_ERAInfo, "Enter a valid selection.");

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			acceptanceLog.EUL_ERA = agreement.PK;
			AssertNoError(acceptanceLog.EUL_ERAInfo, "Please enter a value.");
			AssertNoError(acceptanceLog.EUL_ERAInfo, "Enter a valid selection.");
		}

		public void TestManuallyAddingLogValidation()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			Assert(!acceptanceLog.IsAddedManually);
			AssertNullOrEmpty(acceptanceLog.EUL_AcceptedByName);
			AssertNullOrEmpty(acceptanceLog.EUL_AcceptedByJobTitle);
			AssertNullOrEmpty(acceptanceLog.EUL_AcceptedByEmail);

			acceptanceLog.Validation.ValidateAll();
			AssertNoErrors(acceptanceLog.EUL_AcceptedByNameInfo);
			AssertNoErrors(acceptanceLog.EUL_AcceptedByJobTitleInfo);
			AssertNoErrors(acceptanceLog.EUL_AcceptedByEmailInfo);

			acceptanceLog.IsAddedManually = true;
			acceptanceLog.Validation.ValidateAll();
			AssertHasError("The Name property should be mandatory", acceptanceLog.EUL_AcceptedByNameInfo, "Please enter a value.");
			AssertHasError("The job title property should be mandatory", acceptanceLog.EUL_AcceptedByJobTitleInfo, "Please enter a value.");
			AssertHasError("The email property should be mandatory", acceptanceLog.EUL_AcceptedByEmailInfo, "Please enter a value.");

			acceptanceLog.EUL_AcceptedByName = "Manager Name";
			acceptanceLog.EUL_AcceptedByJobTitle = "Job Title";
			acceptanceLog.EUL_AcceptedByEmail = "123";

			acceptanceLog.Validation.ValidateAll();
			AssertNoErrors(acceptanceLog.EUL_AcceptedByNameInfo);
			AssertNoErrors(acceptanceLog.EUL_AcceptedByJobTitleInfo);
			AssertHasErrorContaining(acceptanceLog.EUL_AcceptedByEmailInfo, "Email Address is not valid");

			acceptanceLog.Validation.ValidateEUL_AcceptedByEmail();
			acceptanceLog.EUL_AcceptedByEmail = "123@123.com";
			AssertNoErrors(acceptanceLog.EUL_AcceptedByEmailInfo);
		}

		public void TestCheckEUL_AcceptedByEmail()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			Assert(!acceptanceLog.IsAddedManually);
			AssertNullOrEmpty(acceptanceLog.EUL_AcceptedByEmail);

			acceptanceLog.Validation.ValidateAll();
			AssertNoErrors(acceptanceLog.EUL_AcceptedByEmailInfo);

			acceptanceLog.Validation.ValidateAll();
			acceptanceLog.EUL_AcceptedByEmail = "123";
			AssertHasErrorContaining(acceptanceLog.EUL_AcceptedByEmailInfo, "Email Address is not valid");

			acceptanceLog.Validation.ValidateAll();
			acceptanceLog.EUL_AcceptedByEmail = "123@ABC.COM";
			AssertNoErrors(acceptanceLog.EUL_AcceptedByEmailInfo);
		}
	}
}

