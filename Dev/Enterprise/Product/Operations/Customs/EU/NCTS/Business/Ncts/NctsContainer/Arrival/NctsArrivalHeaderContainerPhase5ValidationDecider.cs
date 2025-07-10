namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsArrivalHeaderContainerPhase5ValidationDecider : INctsArrivalHeaderContainerPhase5ValidationDecider
{
	public bool IsRuleNR0029Active => false;

	public bool IsRuleTR0043Active => true;

	public bool IsRuleTR0044Active => true;

	public bool IsRuleTR0045Active => true;

	public bool IsRuleTR0046Active => true;
}
