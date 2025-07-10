using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARevenueSummaryFormValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationType()
		{
			var validation = cusStatementHeader.Validation;
			AssertType<CSARevenueSummaryFormValidation>(validation);
		}

		public void TestCheckB2_OH_Importer()
		{
			var validation = cusStatementHeader.Validation;
			validation.ValidateB2_OH_Importer();
			AssertHasError(cusStatementHeader.B2_OH_ImporterInfo, "Please enter a value.");

			var org = Factory.New<OrgHeader>();
			cusStatementHeader.B2_OH_Importer = org.PK;
			validation.ValidateB2_OH_Importer();
			AssertNoError(cusStatementHeader.B2_OH_ImporterInfo, "Please enter a value.");
			AssertHasError(cusStatementHeader.B2_OH_ImporterInfo, "This importer doesn't contain Business Number For Importer/Export.");

			org.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "TESTBN123456");
			validation.ValidateB2_OH_Importer();
			AssertNoError(cusStatementHeader.B2_OH_ImporterInfo, "This importer doesn't contain Business Number For Importer/Export.");
		}

		public void TestCheckB2_PeriodEndDate()
		{
			var validation = cusStatementHeader.Validation;
			validation.ValidateB2_PeriodEndDate();
			AssertHasError(cusStatementHeader.B2_PeriodEndDateInfo, "Please enter a value.");

			cusStatementHeader.B2_PeriodEndDate = new ZDate(2020, 01, 19);
			validation.ValidateB2_PeriodEndDate();
			AssertHasError(cusStatementHeader.B2_PeriodEndDateInfo, "Please choose 18th or end date as the day of this month.");

			cusStatementHeader.B2_PeriodEndDate = new ZDate(2020, 01, 18);
			validation.ValidateB2_PeriodEndDate();
			AssertNoError(cusStatementHeader.B2_PeriodEndDateInfo, "Please choose 18th or end date as the day of this month.");

			cusStatementHeader.B2_PeriodEndDate = new ZDate(2020, 02, 18);
			validation.ValidateB2_PeriodEndDate();
			AssertNoError(cusStatementHeader.B2_PeriodEndDateInfo, "Please choose 18th or end date as the day of this month.");

			cusStatementHeader.B2_PeriodEndDate = new ZDate(2020, 02, 28);
			validation.ValidateB2_PeriodEndDate();
			AssertHasError(cusStatementHeader.B2_PeriodEndDateInfo, "Please choose 18th or end date as the day of this month.");

			cusStatementHeader.B2_PeriodEndDate = new ZDate(2020, 02, 29);
			validation.ValidateB2_PeriodEndDate();
			AssertNoError(cusStatementHeader.B2_PeriodEndDateInfo, "Please choose 18th or end date as the day of this month.");
		}

		public void TestCheckB2_StatementNumber()
		{
			cusStatementHeader.B2_StatementNumber = "TEST000001";

			var newHeader = Factory.New<CusStatementHeader>();
			newHeader.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			newHeader.B2_StatementNumber = "TEST000001";
			newHeader.Validation.ValidateB2_StatementNumber();
			AssertHasError(newHeader.B2_StatementNumberInfo, "There is already exists a CSA Revenue Summary Form with statement number TEST000001.");

			newHeader.B2_StatementNumber = "TEST000002";
			newHeader.Validation.ValidateB2_StatementNumber();
			AssertNoError(newHeader.B2_StatementNumberInfo, "There is already exists a CSA Revenue Summary Form with statement number TEST000001.");
		}

		public void TestCheckPeriodMonth()
		{
			var org = Factory.New<OrgHeader>();
			var impAddInfo = OrgImpAddInfo.Get(org);
			impAddInfo.ZO_IsCSAApprovedImporter = true;
			impAddInfo.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;

			cusStatementHeader.PeriodMonth = 0;
			AssertNoMessageErrorContaining(cusStatementHeader.PeriodMonthInfo, "Period Month range from 1 to 12.");

			cusStatementHeader.B2_OH_Importer = org.PK;
			((CSARevenueSummaryFormValidation)cusStatementHeader.Validation).ValidatePeriodMonth();
			AssertHasErrorContaining(cusStatementHeader.PeriodMonthInfo, "Period Month range from 1 to 12.");

			cusStatementHeader.PeriodMonth = 2;
			AssertNoMessageErrorContaining(cusStatementHeader.PeriodMonthInfo, "Period Month range from 1 to 12.");

			cusStatementHeader.PeriodMonth = 15;
			AssertHasErrorContaining(cusStatementHeader.PeriodMonthInfo, "Period Month range from 1 to 12.");
		}

		public void TestCheckPeriodYear()
		{
			var org = Factory.New<OrgHeader>();
			var impAddInfo = OrgImpAddInfo.Get(org);
			impAddInfo.ZO_IsCSAApprovedImporter = true;
			impAddInfo.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;

			cusStatementHeader.PeriodYear = 1;
			AssertNoMessageErrorContaining(cusStatementHeader.PeriodYearInfo, "Please enter a valid year");

			cusStatementHeader.B2_OH_Importer = org.PK;
			((CSARevenueSummaryFormValidation)cusStatementHeader.Validation).ValidatePeriodYear();
			AssertHasErrorContaining(cusStatementHeader.PeriodYearInfo, "Please enter a valid year");

			cusStatementHeader.PeriodYear = 2000;
			AssertNoMessageErrorContaining(cusStatementHeader.PeriodYearInfo, "Please enter a valid year");

			cusStatementHeader.PeriodYear = 3000;
			AssertHasErrorContaining(cusStatementHeader.PeriodYearInfo, "Please enter a valid year");
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusStatementHeader = Factory.New<CusStatementHeader>();
			cusStatementHeader.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
		}

		CusStatementHeader cusStatementHeader;
	}
}
