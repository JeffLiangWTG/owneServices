using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public sealed class TP5MessageSendingObjectValidationDecider : INctsHeaderMessageSendingObjectValidationDecider
	{
		public bool IsRuleC0220Active => false;
		public bool IsRuleC0315Active => true;
		public bool IsRuleTR0020Active => false;
		public bool IsRuleTR0021Active => false;
	}
}
