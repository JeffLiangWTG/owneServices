using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_ValueFrom_Location_List_EU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "LOCAT");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "A003", "LV LOCAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "B045";
				AssertHasError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, "Enter a valid Value.");

				cusAuthorisationRule.CPR_ValueFrom = "A003";
				AssertNoError("Valid", cusAuthorisationRule.CPR_ValueFromInfo, "Enter a valid Value.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
		}
		CusAuthorisationRule cusAuthorisationRule;
	}
}
