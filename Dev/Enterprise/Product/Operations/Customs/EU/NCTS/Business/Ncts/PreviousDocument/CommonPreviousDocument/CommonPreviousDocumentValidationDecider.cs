namespace Enterprise.Customs.EU.NCTS.Business;

public class CommonPreviousDocumentValidationDecider : ICommonPreviousDocumentValidationDecider
{
	public bool IsRuleG0321Active => false;

	public bool IsRuleTR0030_1Active => false;

	public bool IsRuleE1301Active => false;
}
