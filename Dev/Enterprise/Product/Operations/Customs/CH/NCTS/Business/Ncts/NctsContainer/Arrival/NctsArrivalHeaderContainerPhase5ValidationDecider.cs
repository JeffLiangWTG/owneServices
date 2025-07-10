using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalHeaderContainerPhase5ValidationDecider : INctsArrivalHeaderContainerPhase5ValidationDecider
{
	public bool IsRuleNR0029Active => false;

	public bool IsRuleTR0043Active => true;

	public bool IsRuleTR0044Active => false;

	public bool IsRuleTR0045Active => true;

	public bool IsRuleTR0046Active => true;
}
