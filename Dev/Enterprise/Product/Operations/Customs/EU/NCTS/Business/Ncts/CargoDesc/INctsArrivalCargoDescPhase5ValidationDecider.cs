namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsArrivalCargoDescPhase5ValidationDecider : INctsCargoDescValidationDecider
	{
		bool IsRuleE1109_1Active { get; }

		bool IsRuleNR0004Active { get; }

		bool IsRuleNR0029Active { get; }

		bool IsRuleNR0055Active { get; }
	}
}
