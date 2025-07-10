namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsPackageValidationDecider
	{
	}

	public interface INctsPackagePhase5ValidationDecider : INctsPackageValidationDecider
	{
		bool IsRuleB1919Active { get; }
		bool IsRuleC0670Active { get; }
		bool IsRuleR0220Active { get; }
	}

	public interface INctsPackageDeparturePhase5ValidationDecider : INctsPackagePhase5ValidationDecider
	{
		bool IsRuleB1819Active { get; }
		bool IsRuleC0060Active { get; }
		bool IsRuleC0060_1Active { get; }
		bool IsRuleC0060_2Active { get; }
		bool IsRuleC0060_3Active { get; }
		bool IsRuleE1111Active { get; }
		bool IsRuleNR0003Active { get; }
		bool IsRuleNR0027Active { get; }
		bool IsRuleR0219Active { get; }
		bool IsRuleR0364_1Active { get; }
		bool IsRuleR0364_2Active { get; }
		bool IsRuleR0364_3Active { get; }
		bool IsRuleTR0066Active { get; }
		bool IsRuleTR0083Active { get; }
	}

	public interface INctsPackageArrivalPhase5ValidationDecider : INctsPackagePhase5ValidationDecider
	{
		bool IsRuleNR0029Active { get; }
		bool IsRuleNR0061Active { get; }
		bool IsRuleTR0097Active { get; }
	}
}
