using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Configurations;

public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
{
	protected override EU.NCTS.Business.INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();
}
