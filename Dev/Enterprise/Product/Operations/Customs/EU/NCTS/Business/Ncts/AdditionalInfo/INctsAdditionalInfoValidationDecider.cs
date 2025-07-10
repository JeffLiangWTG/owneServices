namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsAdditionalInfoValidationDecider
	{
		bool IsRuleG0321Active { get; }

		bool IsRuleTR0031Active { get; }

		bool IsRuleTR0032Active { get; }

		bool IsRuleTR0033Active { get; }

		bool IsRuleTR0062Active { get; }

		bool IsRuleE1301Active { get; }
	}

	public interface INctsAdditionalInfoPhase5ValidationDecider : INctsAdditionalInfoValidationDecider
	{
		bool IsRuleC0015Active { get; }

		bool IsRuleE1104_1Active { get; }

		bool IsRuleR0023Active { get; }

		bool IsRuleR3060Active { get; }

		bool IsRuleR3061Active { get; }
	}
}
