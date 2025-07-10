namespace Enterprise.Customs.EU.NCTS.Business;

public interface ICommonPreviousDocumentValidationDecider
{
	bool IsRuleG0321Active { get; }

	bool IsRuleTR0030_1Active { get; }

	bool IsRuleE1301Active { get; }
}

public interface ICommonPreviousDocumentDepartureValidationDecider : ICommonPreviousDocumentValidationDecider
{
	bool IsRuleG0026_1Active { get; }

	bool IsRuleNR0008Active { get; }

	bool IsRuleR0416Active { get; }
}

public interface ICommonPreviousDocumentArrivalValidationDecider : ICommonPreviousDocumentValidationDecider
{
}
