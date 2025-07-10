using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class MovementHeaderConfigurationTest : MovementHeaderConfigurationAbstractTest<MovementHeaderConfiguration>
{
	protected override Type ExpectedDeparturePhase4ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase4ValidationDecider);

	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase5ValidationDecider);

	protected override Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType => typeof(DepartureCusGoodsLocationValidationDecider);

	protected override Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType => typeof(ArrivalCusGoodsLocationValidationDecider);
	
	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalMovementHeaderPhase5ValidationDecider);
}
