namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsHeaderMessageSendingObjectValidationDecider
	{
		bool IsRuleC0220Active { get; }
		bool IsRuleC0315Active { get; }
		bool IsRuleTR0020Active { get; }
		bool IsRuleTR0021Active { get; }
	}
}
