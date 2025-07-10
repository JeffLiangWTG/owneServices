namespace Enterprise.Customs.EU.NCTS.Business;

public interface INctsContainerValidationDecider
{
}

public interface INctsContainerPhase5ValidationDecider : INctsContainerValidationDecider
{
	bool IsRuleTR0043Active { get; }
	bool IsRuleTR0044Active { get; }
	bool IsRuleTR0045Active { get; }
	bool IsRuleTR0046Active { get; }
}
