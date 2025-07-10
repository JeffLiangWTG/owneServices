using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public sealed class NctsHeaderArrivalPhase5ValidationDecider : INctsHeaderArrivalPhase5ValidationDecider
{
	public bool IsRuleNR0015Active => true;

	public bool IsRuleTR0035Active => false;

	public bool IsRuleTR0047Active => true;
}
