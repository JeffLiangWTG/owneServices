namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsGuaranteeValidationDecider
	{
	}

	public interface INctsGuaranteePhase5ValidationDecider : INctsGuaranteeValidationDecider
	{
	}

	public interface INctsGuaranteeDeparturePhase5ValidationDecider : INctsGuaranteePhase5ValidationDecider
	{
		bool IsRuleB1898_1ActiveForPW_RX_NKCurrency { get; }

		bool IsRuleB2101Active { get; }

		bool IsRuleC0085Active { get; }

		bool IsRuleC0085_1Active { get; }

		bool IsRuleC0085_2Active { get; }

		bool IsRuleC0086Active { get; }

		bool IsRuleC0086_1Active { get; }

		bool IsRuleC0130Active { get; }

		bool IsRuleNR0005Active { get; }

		bool IsRuleNR0014Active { get; }

		bool IsRuleNR0064Active { get; }

		bool IsRuleNR0065Active { get; }

		bool IsRuleR0318Active { get; }

		bool IsRuleR0900Active { get; }

		bool IsRuleR0900_1Active { get; }

		bool IsRuleR0900_2Active { get; }

		bool IsRuleR0900_3Active { get; }

		bool IsRuleTR0019Active { get; }

		bool IsRuleTR0065Active { get; }

		bool IsRuleTR0093Active { get; }

		bool IsRuleTR0096Active { get; }
	}
}
