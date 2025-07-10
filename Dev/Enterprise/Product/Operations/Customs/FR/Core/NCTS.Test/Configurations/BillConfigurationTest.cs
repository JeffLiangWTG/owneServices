using System;
using Enterprise.Customs.FR.NCTS.Configurations;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(BillConfiguration))]
sealed class BillConfigurationTest : EU.NCTS.Business.Testing.BillConfigurationAbstractTest<BillConfiguration>
{
	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsBillDeparturePhase5ValidationDecider);

	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(EU.NCTS.Business.NctsBillArrivalPhase5ValidationDecider);
}
