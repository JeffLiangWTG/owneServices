namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsEuOfficeCodeValidationDecider
	{
	}

	public interface INctsEuOfficeCodePhase5ValidationDecider : INctsEuOfficeCodeValidationDecider
	{
		bool IsRuleR0006Active { get; }
	}

	public interface INctsEuOfficeCodeDeparturePhase5ValidationDecider : INctsEuOfficeCodePhase5ValidationDecider
	{
		bool IsRuleB1831Active { get; }

		bool IsRuleB1836Active { get; }

		bool IsRuleB1904Active { get; }

		bool IsRuleC0030Active { get; }

		bool IsRuleC0030_1Active { get; }

		bool IsRuleC0598Active { get; }

		bool IsRuleG0034Active { get; }

		bool IsRuleR0005Active { get; }

		bool IsRuleR0103Active { get; }
	}

	public interface INctsEuOfficeCodeArrivalPhase5ValidationDecider : INctsEuOfficeCodePhase5ValidationDecider
	{
	}
}
