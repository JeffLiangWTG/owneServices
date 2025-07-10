using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
	{
		protected override INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();

		protected override INctsArrivalMovementHeaderPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalMovementHeaderPhase5ValidationDecider();

		protected override ICusGoodsLocationValidationDecider GetDeparturePhase5CusGoodsLocationValidationDecider() => new DeparturePhase5CusGoodsLocationValidationDecider();

		protected override ICusGoodsLocationValidationDecider GetArrivalPhase5CusGoodsLocationValidationDecider() => new ArrivalPhase5CusGoodsLocationValidationDecider();
	}
}
