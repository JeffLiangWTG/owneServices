using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsBillDeparturePhase5ValidationDecider : INctsBillDeparturePhase5ValidationDecider
{
	public bool IsRuleB1895_1Active => false;

	public bool IsRuleB1896Active => false;

	public bool IsRuleB1964Active => false;

	public bool IsRuleC0001_4Active => false;

	public bool IsRuleC0001_6Active => false;

	public bool IsRuleC0343_2Active => false;

	public bool IsRuleC0001_7Active => false;

	public bool IsRuleC0502Active => true;

	public bool IsRuleC0909Active => false;

	public bool IsRuleE1301Active => false;

	public bool IsRuleG0001_1Active => false;

	public bool IsRuleG0026_1Active => false;

	public bool IsRuleN0002Active => false;

	public bool IsRuleNR0068Active => false;

	public bool IsRuleNR0069Active => false;

	public bool IsRuleNR0078Active => false;

	public bool IsRuleNR0079Active => false;

	public bool IsRuleR0221Active => true;

	public bool IsRuleR0364Active => false;

	public bool IsRuleR0474Active => false;

	public bool IsRuleR0474_1Active => false;

	public bool IsRuleR0506Active => false;

	public bool IsRuleR0983Active => false;

	public bool IsRuleR0983_1Active => false;

	public bool IsRuleTR0057Active => true;

	public bool IsRuleTR0058Active => true;

	public bool IsRuleTR0059Active => true;

	public bool IsRuleTR0077Active => true;

	public bool IsRuleTR0078Active => false;

	public bool IsRuleTR0094Active => true;

	public bool IsRuleNS30018Active => !CH.Business.FuncsHelper.IsCHNT015V4Active;
}
