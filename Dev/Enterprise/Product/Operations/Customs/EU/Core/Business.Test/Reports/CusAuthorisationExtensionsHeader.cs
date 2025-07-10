using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Reports.Testing;

public static class CusAuthorisationExtensionsHeader
{
	public static CusAuthorisationHeader WithNumber(this CusAuthorisationHeader auth, string number)
	{
		auth.CPH_Number = new ZString(number).Left(AutoCusPermitHeader.Schema.CPH_NumberMaxLength);
		return auth;
	}

	public static CusAuthorisationHeader WithCountry(this CusAuthorisationHeader auth, string countryCode)
	{
		auth.CPH_RN_NKCountryCode = countryCode;
		return auth;
	}

	public static CusAuthorisationRule SetupAuthorisationRule(this CusAuthorisationHeader auth, string ruleCode)
	{
		var rule = auth.CusAuthorisationRules.Cast<CusAuthorisationRule>().FirstOrDefault(x => x.CPR_RuleCode == ruleCode);
		if (rule == null)
		{
			rule = auth.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = ruleCode;
		}
		return rule;
	}
}
