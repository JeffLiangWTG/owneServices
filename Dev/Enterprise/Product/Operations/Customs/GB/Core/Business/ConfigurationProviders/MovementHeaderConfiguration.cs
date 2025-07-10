using Enterprise.Customs.GB.Business.NCTS;

namespace Enterprise.Customs.GB.Business;

public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
{
	protected override EU.NCTS.Business.INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();
}
