using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsArrivalCargoDescPhase5ValidationDecider : INctsArrivalCargoDescPhase5ValidationDecider
{
	public bool IsRuleE1109_1Active => true;

	public bool IsRuleNR0004Active => false;

	public bool IsRuleNR0029Active => true;

	public bool IsRuleNR0055Active => false;

	public bool IsRuleNR0058Active => false;

	public bool IsRuleNR0059Active => false;

	public bool IsRuleNR0060Active => false;
}
