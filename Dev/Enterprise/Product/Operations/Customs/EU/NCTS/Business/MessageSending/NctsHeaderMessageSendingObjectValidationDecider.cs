namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsHeaderMessageSendingObjectValidationDecider : INctsHeaderMessageSendingObjectValidationDecider
	{
		public bool IsRuleC0220Active => true;
		public bool IsRuleC0315Active => true;
		public bool IsRuleTR0020Active => false;
		public bool IsRuleTR0021Active => false;
	}
}
