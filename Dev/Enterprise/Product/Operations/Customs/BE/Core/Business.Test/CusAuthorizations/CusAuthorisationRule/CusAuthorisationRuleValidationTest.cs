using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPR_ValueFrom_Location_List_FreeInput()
	{
		cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		cusAuthorisationRule.CPR_ValueFrom = "FREEINPUT";
		AssertHasMessageErrorContaining("Free input should be allowed", cusAuthorisationRule.CPR_ValueFromInfo, "The code you have selected is not in the list.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
	}
	CusAuthorisationRule cusAuthorisationRule;
}
