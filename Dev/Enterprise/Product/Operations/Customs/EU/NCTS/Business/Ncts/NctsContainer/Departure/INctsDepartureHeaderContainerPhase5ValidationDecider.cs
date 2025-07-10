namespace Enterprise.Customs.EU.NCTS.Business;

public interface INctsDepartureHeaderContainerPhase5ValidationDecider : INctsContainerPhase5ValidationDecider
{
	bool IsRuleC0055Active { get; }

	bool IsRuleN0003Active { get; }

	bool IsRuleR0448Active { get; }

	bool IsRuleTR0095Active { get; }
}
