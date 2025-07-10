namespace Enterprise.Customs.FR.Business.NCTS
{
	public interface INctsGuaranteeDeparturePhase5ValidationDecider : EU.NCTS.Business.INctsGuaranteeDeparturePhase5ValidationDecider
	{
		bool IsRuleNAT086Active { get; }

		bool IsRuleNAT085Active { get; }
	}
}
