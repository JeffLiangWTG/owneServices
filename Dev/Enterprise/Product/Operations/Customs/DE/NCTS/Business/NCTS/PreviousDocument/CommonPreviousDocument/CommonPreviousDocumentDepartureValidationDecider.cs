namespace Enterprise.Customs.DE.NCTS.Business;

public sealed class CommonPreviousDocumentDepartureValidationDecider : EU.NCTS.Business.ICommonPreviousDocumentDepartureValidationDecider
{
	public bool IsRuleG0026_1Active => false;

	public bool IsRuleG0321Active => false;

	public bool IsRuleNR0008Active => true;

	public bool IsRuleR0416Active => false;

	public bool IsRuleE1301Active => false;

	public bool IsRuleTR0030_1Active => false;
}
