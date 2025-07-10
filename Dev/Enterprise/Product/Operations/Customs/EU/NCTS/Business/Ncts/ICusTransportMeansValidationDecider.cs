namespace Enterprise.Customs.EU.NCTS.Business;

public interface ICusTransportMeansValidationDecider
{
}

public interface ICusTransportMeansPhase5ValidationDecider : ICusTransportMeansValidationDecider
{
}

public interface IDepartureCusTransportMeansPhase5ValidationDecider : ICusTransportMeansPhase5ValidationDecider
{
	bool IsRuleB2101Active { get; }

	bool IsRuleG0789_1Active { get; }

	bool IsRuleTR0078Active { get; }
}
