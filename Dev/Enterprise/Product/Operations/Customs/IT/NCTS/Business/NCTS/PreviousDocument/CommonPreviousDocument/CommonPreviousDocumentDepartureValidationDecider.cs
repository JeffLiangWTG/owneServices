using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CommonPreviousDocumentDepartureValidationDecider : ICommonPreviousDocumentDepartureValidationDecider
{
	public bool IsRuleE1301Active => true;

	public bool IsRuleG0321Active => false;

	public bool IsRuleG0026_1Active => true;

	public bool IsRuleNR0008Active => false;

	public bool IsRuleR0416Active => false;

	public bool IsRuleTR0030_1Active => false;
}
