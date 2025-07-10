namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsArrivalMovementHeaderValidationDecider : INctsMovementHeaderValidationDecider
	{
	}

	public interface INctsArrivalMovementHeaderPhase5ValidationDecider : INctsArrivalMovementHeaderValidationDecider
	{
		bool IsRuleNR0009Active { get; }

		bool IsRuleNR0026Active { get; }

		bool IsRuleNR0028Active { get; }

		bool IsRuleNR0076Active { get; }

		bool IsRuleTR0022Active { get; }

		bool IsRuleTR0034Active { get; }

		bool IsRuleTR0042Active { get; }

		bool IsRuleTR0063Active { get; }

		bool IsRuleTR0071Active { get; }

		bool IsRuleTR0072Active { get; }

		bool IsRuleTR0098Active { get; }

		bool IsRuleTR0091Active { get; }
	}
}
