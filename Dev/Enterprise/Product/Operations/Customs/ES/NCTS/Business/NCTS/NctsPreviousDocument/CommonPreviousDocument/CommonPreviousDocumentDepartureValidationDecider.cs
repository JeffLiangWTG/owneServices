using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business;

public sealed class CommonPreviousDocumentDepartureValidationDecider : CommonPreviousDocumentValidationDecider, ICommonPreviousDocumentDepartureValidationDecider
{
	public bool IsRuleG0026_1Active => false;

	public bool IsRuleNR0008Active => true;

	public bool IsRuleR0416Active => true;
}
