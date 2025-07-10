namespace Enterprise.Customs.EU.NCTS.Business;

public interface INctsPreviousDocumentValidationDecider
{
}

public interface INctsPreviousDocumentPhase5ValidationDecider : INctsPreviousDocumentValidationDecider
{
	bool IsRuleG0321Active { get; }
}

public interface INctsPreviousDocumentDeparturePhase5ValidationDecider : INctsPreviousDocumentPhase5ValidationDecider
{
	bool IsRuleC0298Active { get; }

	bool IsRuleNR0008Active { get; }

	bool IsRuleNR0046Active { get; }

	bool IsRuleG0058_1Active { get; }

	bool IsRuleTR0030_1Active { get; }

	bool IsRuleNR0066Active { get; }
}

public interface INctsPreviousDocumentArrivalPhase5ValidationDecider : INctsPreviousDocumentPhase5ValidationDecider
{
}
