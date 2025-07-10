using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusCalculationRuleValidation))]
	sealed class CusCalculationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRuleType()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();

			cusCalculationRule.CCR_RuleType = "";
			cusCalculationRule.Validation.ValidateCCR_RuleType();
			AssertHasErrors(cusCalculationRule.CCR_RuleTypeInfo);

			cusCalculationRule.CCR_RuleType = CusCalculationRuleTypeList.Codes.INS;
			cusCalculationRule.Validation.ValidateCCR_RuleType();
			AssertNoErrors(cusCalculationRule.CCR_RuleTypeInfo);

			cusCalculationRule.CCR_RuleType = "ERR";
			cusCalculationRule.Validation.ValidateCCR_RuleType();
			AssertHasErrors(cusCalculationRule.CCR_RuleTypeInfo);
		}

		public void TestBasedOn()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();

			cusCalculationRule.CCR_BasedOn = "";
			cusCalculationRule.Validation.ValidateCCR_BasedOn();
			AssertNoErrors(cusCalculationRule.CCR_BasedOnInfo);

			cusCalculationRule.CCR_BasedOn = Core.Constants.IncoTerms.CostAndFreight;
			cusCalculationRule.Validation.ValidateCCR_BasedOn();
			AssertNoErrors(cusCalculationRule.CCR_BasedOnInfo);

			cusCalculationRule.CCR_BasedOn = "ERR";
			cusCalculationRule.Validation.ValidateCCR_BasedOn();
			AssertHasErrors(cusCalculationRule.CCR_BasedOnInfo);
		}

		public void TestTransportMode()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();

			cusCalculationRule.CCR_TransportMode = "";
			cusCalculationRule.Validation.ValidateCCR_TransportMode();
			AssertNoErrors(cusCalculationRule.CCR_TransportModeInfo);

			cusCalculationRule.CCR_TransportMode = TransportTypeList.Codes.Air;
			cusCalculationRule.Validation.ValidateCCR_TransportMode();
			AssertNoErrors(cusCalculationRule.CCR_TransportModeInfo);

			cusCalculationRule.CCR_TransportMode = "ERR";
			cusCalculationRule.Validation.ValidateCCR_TransportMode();
			AssertHasErrors(cusCalculationRule.CCR_TransportModeInfo);
		}

		public void TestCurrency()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();

			cusCalculationRule.CCR_RX_NKCurrency = "";
			cusCalculationRule.Validation.ValidateCCR_RX_NKCurrency();
			AssertHasErrors(cusCalculationRule.CCR_RX_NKCurrencyInfo);

			cusCalculationRule.CCR_RX_NKCurrency = "AUD";
			cusCalculationRule.Validation.ValidateCCR_RX_NKCurrency();
			AssertNoErrors(cusCalculationRule.CCR_RX_NKCurrencyInfo);

			cusCalculationRule.CCR_RX_NKCurrency = "ERR";
			cusCalculationRule.Validation.ValidateCCR_RX_NKCurrency();
			AssertHasErrors(cusCalculationRule.CCR_RX_NKCurrencyInfo);
		}

		public void TestCheckCCR_EndDate() => CombineAssertions(() =>
		{
			const string message = "End Date cannot be earlier than Start Date";
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			cusCalculationRule.CCR_StartDate = new ZDateTime(2024, 04, 21).ToOffset();
			cusCalculationRule.CCR_EndDate = new ZDateTime(2024, 04, 21).ToOffset();
			AssertNoError("CCR_EndDate = CCR_StartDate", cusCalculationRule.CCR_EndDateInfo, message);

			cusCalculationRule.CCR_EndDate = new ZDateTime(2024, 04, 20).ToOffset();
			AssertHasError("CCR_EndDate < CCR_StartDate", cusCalculationRule.CCR_EndDateInfo, message);

			cusCalculationRule.CCR_EndDate = new ZDateTime(2024, 04, 22).ToOffset();
			AssertNoError("CCR_EndDate > CCR_StartDate", cusCalculationRule.CCR_EndDateInfo, message);

			cusCalculationRule.CCR_EndDate = ZDateTimeOffset.Empty;
			AssertNoError("CCR_EndDate = max", cusCalculationRule.CCR_EndDateInfo, message);
		});

		public void TestCheckCCR_EndDateIsOptional()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(cusCalculationRule.CCR_EndDateInfo);
		}

		public void TestCheckCCR_EndDateIsValidZDateTimeOffsetRange()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			cusCalculationRule.CCR_EndDate = ZDateTimeOffset.Now.AddYears(10);
			AssertEquals("Check of hold date range should be deactivated and thus not showing 'is more than 5 years from now and thus is not valid' error message.", false, cusCalculationRule.CCR_EndDateInfo.HasNotifications());
		}

		public void TestValidateEffectiveDates_MatchingCriteria() => CombineAssertions(() =>
		{
			const string message = "There's another rule with overlapping effective dates. Please enter different Start Date and/or End Date.";
			var startDate = new ZDateTime(2024, 05, 30).ToOffset();
			var endDate = new ZDateTime(2024, 06, 04).ToOffset();

			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_StartDate = startDate;
			cusCalculationRule1.CCR_EndDate = endDate;
			Factory.Save();

			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			cusCalculationRule2.CCR_StartDate = startDate;
			cusCalculationRule2.CCR_EndDate = endDate;
			cusCalculationRule2.CCR_RuleType = cusCalculationRule1.CCR_RuleType;
			cusCalculationRule2.CCR_OH_Importer = cusCalculationRule1.CCR_OH_Importer;
			cusCalculationRule2.CCR_TransportMode = cusCalculationRule1.CCR_TransportMode;
			cusCalculationRule2.CCR_GC_Company = cusCalculationRule1.CCR_GC_Company;
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("Same (CCR_RuleType, CCR_OH_Importer, CCR_TransportMode, CCR_GC_Company) with overlapping dates", cusCalculationRule2, message);

			cusCalculationRule2.CCR_RuleType = "XYZ";
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("Different CCR_RuleType with overlapping dates", cusCalculationRule2, message);

			cusCalculationRule2.CCR_RuleType = cusCalculationRule1.CCR_RuleType;
			cusCalculationRule2.CCR_OH_Importer = Factory.New<OrgHeader>().PK;
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("Different CCR_OH_Importer with overlapping dates", cusCalculationRule2, message);

			cusCalculationRule2.CCR_OH_Importer = cusCalculationRule1.CCR_OH_Importer;
			cusCalculationRule2.CCR_TransportMode = "XYZ";
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("Different CCR_TransportMode with overlapping dates", cusCalculationRule2, message);

			cusCalculationRule2.CCR_TransportMode = cusCalculationRule1.CCR_TransportMode;
			cusCalculationRule2.CCR_GC_Company = Factory.New<GlbCompany>().PK;
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("Different CCR_GC_Company with overlapping dates", cusCalculationRule2, message);
		});

		public void TestValidateEffectiveDates() => CombineAssertions(() =>
		{
			const string message = "There's another rule with overlapping effective dates. Please enter different Start Date and/or End Date.";
			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_StartDate = new ZDateTime(2024, 05, 30).ToOffset();
			cusCalculationRule1.CCR_EndDate = new ZDateTime(2024, 06, 04).ToOffset();
			Factory.Save();

			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			cusCalculationRule2.CCR_RuleType = cusCalculationRule1.CCR_RuleType;
			cusCalculationRule2.CCR_OH_Importer = cusCalculationRule1.CCR_OH_Importer;
			cusCalculationRule2.CCR_TransportMode = cusCalculationRule1.CCR_TransportMode;
			cusCalculationRule2.CCR_GC_Company = cusCalculationRule1.CCR_GC_Company;
			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 05, 31).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 06, 05).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("31/05/24 - 05/06/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 05, 28).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("28/05/24 - 05/06/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 06, 02).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("28/05/24 - 02/06/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 05, 31).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 06, 02).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("31/05/24 - 02/06/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 05, 28).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 05, 30).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("28/05/24 - 30/05/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 05, 28).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 05, 29).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("28/05/24 - 29/05/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 06, 04).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 06, 05).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertHasRowError("04/06/24 - 05/06/24", cusCalculationRule2, message);

			cusCalculationRule2.CCR_StartDate = new ZDateTime(2024, 06, 05).ToOffset();
			cusCalculationRule2.CCR_EndDate = new ZDateTime(2024, 06, 05).ToOffset();
			cusCalculationRule2.Validation.ValidateAll();
			AssertNoRowError("05/06/24 - 05/06/24", cusCalculationRule2, message);
		});
	}
}
