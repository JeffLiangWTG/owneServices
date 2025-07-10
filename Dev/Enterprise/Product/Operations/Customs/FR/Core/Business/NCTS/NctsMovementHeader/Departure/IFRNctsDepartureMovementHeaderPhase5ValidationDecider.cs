using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public interface IFRNctsDepartureMovementHeaderPhase5ValidationDecider : INctsDepartureMovementHeaderPhase5ValidationDecider
	{
		public bool IsRuleNAT050Active { get; }
		public bool IsRuleNAT103Active { get; }
	}
}
