using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public static class CusAuthorisationRuleTestHelper
{
	public static CusAuthorisationRule AddAuthorisationRule(CusAuthorisationHeader header, ZString ruleCode, ZString value, string description = "")
	{
		var authorisationRule = header.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = ruleCode;
		authorisationRule.CPR_ValueFrom = value;
		authorisationRule.CPR_Description = description;
		return authorisationRule;
	}
}
