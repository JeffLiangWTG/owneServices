using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(CusTransportMeansConfiguration))]
sealed class CusTransportMeansConfigurationTest : EU.NCTS.Business.Testing.CusTransportMeansConfigurationAbstractTest<CusTransportMeansConfiguration>
{
	protected override Type ExpectedDepartureCusTransportMeansPhase5ValidationDeciderType => typeof(DepartureCusTransportMeansPhase5ValidationDecider);
}
