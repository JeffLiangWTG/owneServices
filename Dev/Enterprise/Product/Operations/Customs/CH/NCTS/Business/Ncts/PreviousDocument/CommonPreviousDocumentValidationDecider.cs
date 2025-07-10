namespace Enterprise.Customs.CH.NCTS;

public class CommonPreviousDocumentValidationDecider : EU.NCTS.Business.ICommonPreviousDocumentValidationDecider
{
	public bool IsRuleG0321Active => false;

	public bool IsRuleTR0030_1Active => true;

	public bool IsRuleE1301Active => false;
}
