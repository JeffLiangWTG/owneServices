namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsBillValidationDecider
	{
	}

	public interface INctsBillPhase5ValidationDecider : INctsBillValidationDecider
	{
		bool IsRuleB1964Active { get; }

		bool IsRuleC0909Active { get; }
	}

	public interface INctsBillDeparturePhase5ValidationDecider : INctsBillPhase5ValidationDecider
	{
		bool IsRuleB1895_1Active { get; }

		bool IsRuleB1896Active { get; }

		bool IsRuleC0001_4Active { get; }

		bool IsRuleC0001_6Active { get; }

		bool IsRuleC0001_7Active { get; }

		bool IsRuleC0343_2Active { get; }

		bool IsRuleC0502Active { get; }

		bool IsRuleE1301Active { get; }

		bool IsRuleG0001_1Active { get; }

		bool IsRuleG0026_1Active { get; }

		bool IsRuleN0002Active { get; }

		bool IsRuleNR0068Active { get; }

		bool IsRuleNR0069Active { get; }

		bool IsRuleNR0078Active { get; }

		bool IsRuleNR0079Active { get; }

		bool IsRuleR0221Active { get; }

		bool IsRuleR0364Active { get; }

		bool IsRuleR0474Active { get; }

		bool IsRuleR0474_1Active { get; }

		bool IsRuleR0506Active { get; }

		bool IsRuleR0983Active { get; }

		bool IsRuleR0983_1Active { get; }

		bool IsRuleTR0057Active { get; }

		bool IsRuleTR0058Active { get; }

		bool IsRuleTR0059Active { get; }

		bool IsRuleTR0077Active { get; }

		bool IsRuleTR0078Active { get; }

		bool IsRuleTR0094Active { get; }
	}

	public interface INctsBillArrivalPhase5ValidationDecider : INctsBillPhase5ValidationDecider
	{
		bool IsRuleNR0062Active { get; }
	}
}
