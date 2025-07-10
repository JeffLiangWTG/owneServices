using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateLocRuleValueFromFormat()
	{
		const string errorMessage = "The Value must start with one or more digits and end with one upper alphabetical character";
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;

		CombineAssertions("When CPH_Type = TST", () =>
		{
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorisationRule.CPR_ValueFrom = "1234567";
			AssertNoMessageError("Last char must be alphabetical", authorisationRule.CPR_ValueFromInfo, errorMessage);

			authorisationRule.CPR_ValueFrom = "123456e";
			AssertNoMessageError("Last char must be alphabetical UPPERCASE", authorisationRule.CPR_ValueFromInfo, errorMessage);
		});

		CombineAssertions("When CPH_Type != TST", () =>
		{
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport;
			authorisationRule.CPR_ValueFrom = "";
			AssertNoMessageError("Empty value from", authorisationRule.CPR_ValueFromInfo, errorMessage);

			authorisationRule.CPR_ValueFrom = "1234567";
			AssertHasMessageError("Last char must be alphabetical", authorisationRule.CPR_ValueFromInfo, errorMessage);

			authorisationRule.CPR_ValueFrom = "123456e";
			AssertHasMessageError("Last char must be alphabetical UPPERCASE", authorisationRule.CPR_ValueFromInfo, errorMessage);

			authorisationRule.CPR_ValueFrom = "123456E";
			AssertNoMessageError("Valid", authorisationRule.CPR_ValueFromInfo, errorMessage);

			authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Document;
			authorisationRule.CPR_ValueFrom = "123";
			AssertNoMessageError("This validation applies only to 'LOC'", authorisationRule.CPR_ValueFromInfo, errorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
	}
	CusAuthorisationHeader authorisationHeader;
	CusAuthorisationRule authorisationRule;
}
