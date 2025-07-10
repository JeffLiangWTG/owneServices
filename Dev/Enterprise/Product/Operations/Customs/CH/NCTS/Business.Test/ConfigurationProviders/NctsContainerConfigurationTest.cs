using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsContainerConfiguration))]
sealed class NctsContainerConfigurationTest : NctsContainerConfigurationAbstractTest<NctsContainerConfiguration>
{
	protected override Type NctsArrivalHeaderContainerPhase5ValidationDecider => typeof(NctsArrivalHeaderContainerPhase5ValidationDecider);
	protected override Type NctsDepartureHeaderContainerPhase5ValidationDecider => typeof(NctsDepartureHeaderContainerPhase5ValidationDecider);
}
