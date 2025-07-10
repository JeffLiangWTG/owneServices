namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsHeaderValidationDecider
	{
	}

	public interface INctsHeaderDepartureValidationDecider : INctsHeaderValidationDecider
	{
	}

	public interface INctsHeaderDeparturePhase5ValidationDecider : INctsHeaderDepartureValidationDecider
	{
		bool IsRuleB1823Active { get; }

		bool IsRuleC0001Active { get; }

		bool IsRuleC0001_1Active { get; }

		bool IsRuleC0001_4Active { get; }

		bool IsRuleC0001_6Active { get; }

		bool IsRuleC0050Active { get; }

		bool IsRuleG0001_1Active { get; }

		bool IsRuleNR0068Active { get; }

		bool IsRuleNR0069Active { get; }

		bool IsRuleNR0071Active { get; }

		bool IsRuleNR0074Active { get; }

		bool IsRuleTR0079Active { get; }
	}

	public interface IRuleTR0087Decider
	{
		bool IsActive { get; }
	}
}
