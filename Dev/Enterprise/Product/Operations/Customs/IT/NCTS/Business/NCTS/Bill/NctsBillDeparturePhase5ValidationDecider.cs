using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsBillDeparturePhase5ValidationDecider : INctsBillDeparturePhase5ValidationDecider
{
	public bool IsRuleB1895_1Active => true;

	public bool IsRuleB1896Active => true;

	public bool IsRuleB1964Active => false;

	public bool IsRuleC0001_4Active => false;

	public bool IsRuleC0001_6Active => true;

	public bool IsRuleC0001_7Active => true;

	public bool IsRuleC0343_2Active => false;

	public bool IsRuleC0502Active => true;

	public bool IsRuleC0909Active => true;

	public bool IsRuleE1301Active => true;

	public bool IsRuleG0001_1Active => false;

	public bool IsRuleG0026_1Active => true;

	public bool IsRuleN0002Active => true;

	public bool IsRuleNR0068Active => false;

	public bool IsRuleNR0069Active => false;

	public bool IsRuleNR0078Active => true;

	public bool IsRuleNR0079Active => false;

	public bool IsRuleR0221Active => true;

	public bool IsRuleR0364Active => true;

	public bool IsRuleR0474Active => true;

	public bool IsRuleR0474_1Active => true;

	public bool IsRuleR0506Active => false;

	public bool IsRuleR0983Active => true;

	public bool IsRuleR0983_1Active => false;

	public bool IsRuleTR0057Active => true;

	public bool IsRuleTR0058Active => true;

	public bool IsRuleTR0059Active => true;

	public bool IsRuleTR0077Active => false;

	public bool IsRuleTR0078Active => true;

	public bool IsRuleTR0094Active => false;
}
