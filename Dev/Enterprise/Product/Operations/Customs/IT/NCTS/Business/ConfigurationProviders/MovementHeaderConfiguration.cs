using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
{
	protected override INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();

	protected override ICusGoodsLocationValidationDecider GetDeparturePhase5CusGoodsLocationValidationDecider()
	{
		return new DeparturePhase5CusGoodsLocationValidationDecider();
	}
}
