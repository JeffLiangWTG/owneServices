namespace Enterprise.Customs.EU.NCTS.Business;

public interface INctsArrivalHeaderContainerPhase5ValidationDecider : INctsContainerPhase5ValidationDecider
{
	bool IsRuleNR0029Active { get; }
}
