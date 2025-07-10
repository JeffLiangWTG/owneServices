namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
{
	protected override EU.NCTS.Business.INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();

	protected override EU.NCTS.Business.INctsArrivalMovementHeaderPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalMovementHeaderPhase5ValidationDecider();

	protected override EU.NCTS.Business.ICusGoodsLocationValidationDecider GetDeparturePhase5CusGoodsLocationValidationDecider() => new DeparturePhase5CusGoodsLocationValidationDecider();

	protected override EU.NCTS.Business.ICusGoodsLocationValidationDecider GetArrivalPhase5CusGoodsLocationValidationDecider() => new ArrivalPhase5CusGoodsLocationValidationDecider();
}
