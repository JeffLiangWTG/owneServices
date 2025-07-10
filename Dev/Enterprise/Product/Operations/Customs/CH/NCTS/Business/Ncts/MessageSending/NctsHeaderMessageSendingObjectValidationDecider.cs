using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsHeaderMessageSendingObjectValidationDecider : INctsHeaderMessageSendingObjectValidationDecider
{
	public bool IsRuleC0220Active => false;

	public bool IsRuleC0315Active => false;

	public bool IsRuleTR0020Active => false;

	public bool IsRuleTR0021Active => false;
}
