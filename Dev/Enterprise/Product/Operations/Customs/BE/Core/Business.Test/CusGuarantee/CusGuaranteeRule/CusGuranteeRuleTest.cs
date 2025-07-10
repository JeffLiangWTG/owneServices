using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusGuaranteeRule))]
class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
{
	public void TestAccessCodePinRuleValidation()
	{
		CombineAssertions(() =>
		{
			var guaranteeRule = Factory.New<CusGuaranteeRule>();
			guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
			AssertType<AccessCodePinRuleValidation>("CPR_RuleCode = 'DPN'", guaranteeRule.Validation);

			guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			AssertType<AccessCodePinRuleValidation>("CPR_RuleCode = 'PIN'", guaranteeRule.Validation);
		});
	}
}
