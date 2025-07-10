namespace Enterprise.Customs.EU.NCTS.Business;

public interface INctsSupportingDocumentValidationDecider
{
}

public interface INctsSupportingDocumentPhase5ValidationDecider : INctsSupportingDocumentValidationDecider
{
}

public interface INctsSupportingDocumentDeparturePhase5ValidationDecider : INctsSupportingDocumentPhase5ValidationDecider
{
	bool IsRuleE1301Active { get; }
	bool IsRuleG0321Active { get; }
	bool IsRuleNR0006Active { get; }
	bool IsRuleRP30Active { get; }
}
