using System;
using Enterprise.Customs.GB.Business.NCTS;

namespace Enterprise.Customs.GB.Business.Testing;

sealed class MovementHeaderConfigurationTest : EU.NCTS.Business.Testing.MovementHeaderConfigurationAbstractTest<MovementHeaderConfiguration>
{
	protected override Type ExpectedDeparturePhase4ValidationDeciderType => typeof(EU.NCTS.Business.NctsDepartureMovementHeaderPhase4ValidationDecider);

	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase5ValidationDecider);

	protected override Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType => typeof(EU.NCTS.Business.DeparturePhase5CusGoodsLocationValidationDecider);

	protected override Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType => typeof(EU.NCTS.Business.ArrivalPhase5CusGoodsLocationValidationDecider);

	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(EU.NCTS.Business.NctsArrivalMovementHeaderPhase5ValidationDecider);
}
