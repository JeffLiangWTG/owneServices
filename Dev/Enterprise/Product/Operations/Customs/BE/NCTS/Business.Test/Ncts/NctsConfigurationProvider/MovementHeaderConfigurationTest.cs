using System;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class MovementHeaderConfigurationTest : EU.NCTS.Business.Testing.MovementHeaderConfigurationAbstractTest<MovementHeaderConfiguration>
{
	protected override Type ExpectedDeparturePhase4ValidationDeciderType => typeof(EU.NCTS.Business.NctsDepartureMovementHeaderPhase4ValidationDecider);

	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase5ValidationDecider);

	protected override Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType => typeof(DeparturePhase5CusGoodsLocationValidationDecider);

	protected override Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType => typeof(ArrivalPhase5CusGoodsLocationValidationDecider);

	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalMovementHeaderPhase5ValidationDecider);
}
