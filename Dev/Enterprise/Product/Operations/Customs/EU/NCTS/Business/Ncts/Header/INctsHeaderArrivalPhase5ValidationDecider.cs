namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsHeaderArrivalPhase5ValidationDecider : INctsHeaderValidationDecider
	{
		bool IsRuleNR0015Active { get; }

		bool IsRuleTR0035Active { get; }

		bool IsRuleTR0047Active { get; }
	}
}
