using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class MovementHeaderConfigurationTest
		: MovementHeaderConfigurationAbstractTest<MovementHeaderConfiguration>
	{
		protected override Type ExpectedDeparturePhase4ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase4ValidationDecider);

		protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase5ValidationDecider);

		protected override Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType => typeof(DeparturePhase5CusGoodsLocationValidationDecider);

		protected override Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType => typeof(ArrivalPhase5CusGoodsLocationValidationDecider);

		protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalMovementHeaderPhase5ValidationDecider);
	}
}
