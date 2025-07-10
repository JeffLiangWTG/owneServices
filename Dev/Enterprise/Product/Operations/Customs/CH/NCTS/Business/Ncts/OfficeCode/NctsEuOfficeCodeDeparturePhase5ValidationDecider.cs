using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsEuOfficeCodeDeparturePhase5ValidationDecider : INctsEuOfficeCodeDeparturePhase5ValidationDecider
{
	public bool IsRuleB1831Active => true;

	public bool IsRuleB1836Active => false;

	public bool IsRuleB1904Active => false;

	public bool IsRuleC0030Active => false;

	public bool IsRuleC0030_1Active => false;

	public bool IsRuleC0598Active => false;

	public bool IsRuleG0034Active => true;

	public bool IsRuleR0005Active => true;

	public bool IsRuleR0006Active => false;

	public bool IsRuleR0103Active => true;
}
