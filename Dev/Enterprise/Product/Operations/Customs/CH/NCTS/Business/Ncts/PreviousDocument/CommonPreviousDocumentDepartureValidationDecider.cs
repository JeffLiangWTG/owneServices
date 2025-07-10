namespace Enterprise.Customs.CH.NCTS;

public sealed class CommonPreviousDocumentDepartureValidationDecider : CommonPreviousDocumentValidationDecider, EU.NCTS.Business.ICommonPreviousDocumentDepartureValidationDecider
{
	public bool IsRuleG0026_1Active => false;

	public bool IsRuleNR0008Active => false;

	public bool IsRuleR0416Active => false;
}
