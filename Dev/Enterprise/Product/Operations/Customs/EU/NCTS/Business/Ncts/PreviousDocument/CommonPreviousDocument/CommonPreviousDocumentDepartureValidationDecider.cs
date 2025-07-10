namespace Enterprise.Customs.EU.NCTS.Business;

public sealed class CommonPreviousDocumentDepartureValidationDecider : CommonPreviousDocumentValidationDecider, ICommonPreviousDocumentDepartureValidationDecider
{
	public bool IsRuleG0026_1Active => false;

	public bool IsRuleNR0008Active => false;

	public bool IsRuleR0416Active => true;
}
