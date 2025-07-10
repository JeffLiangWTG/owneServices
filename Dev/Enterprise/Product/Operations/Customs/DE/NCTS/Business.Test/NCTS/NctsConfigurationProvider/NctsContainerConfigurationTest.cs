using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsContainerConfiguration))]
sealed class NctsContainerConfigurationTest : NctsContainerConfigurationAbstractTest<NctsContainerConfiguration>
{
	protected override Type NctsDepartureHeaderContainerPhase5ValidationDecider => typeof(NctsDepartureHeaderContainerPhase5ValidationDecider);
}
