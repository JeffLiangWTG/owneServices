using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsPackageArrivalPhase5ValidationDecider : INctsPackageArrivalPhase5ValidationDecider
{
	public bool IsRuleB1919Active => false;
	public bool IsRuleC0670Active => true;
	public bool IsRuleNR0029Active => true;
	public bool IsRuleNR0061Active => false;
	public bool IsRuleR0220Active => false;
	public bool IsRuleTR0097Active => false;
}
