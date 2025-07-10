using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusAuthorisationRuleValidation))]
sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPR_ValueFrom() => CombineAssertions(() =>
	{
		const string messageError = "ALE code should begin with ‘CH’ and have the following structure: CHnnnnnnZOnnnnNnnnnnn.";

		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();

		authorisationRule.Validation.ValidateCPR_ValueFrom();
		AssertNoMessageErrorContaining(authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec;
		authorisationRule.Validation.ValidateCPR_ValueFrom();
		AssertNoMessageErrorContaining(authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule.Validation.ValidateCPR_ValueFrom();
		AssertHasMessageErrorContaining("Empty Authorization Number", authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_ValueFrom = "AB123456ZO1234N123456";
		AssertHasMessageErrorContaining("Invalid Authorization Number", authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_ValueFrom = "CH123456ZO1234N";
		AssertHasMessageErrorContaining("Short Authorization Number", authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_ValueFrom = "CH1234ZO123456N123456";
		AssertHasMessageErrorContaining("Wrong digit numbers", authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_ValueFrom = "CHabcdefZOabcNabcdef";
		AssertHasMessageErrorContaining("Chars instead of numbers", authorisationRule.CPR_ValueFromInfo, messageError);

		authorisationRule.CPR_ValueFrom = "CH123456ZO1234N123456";
		AssertNoMessageErrorContaining("Valid Authorization Number", authorisationRule.CPR_ValueFromInfo, messageError);
	});
}
