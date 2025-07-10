using System;
using Enterprise.Customs.FR.NCTS.Configurations;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsContainerConfiguration))]
	sealed class NctsContainerConfigurationTest : EU.NCTS.Business.Testing.NctsContainerConfigurationAbstractTest<NctsContainerConfiguration>
	{
		protected override Type NctsArrivalHeaderContainerPhase5ValidationDecider => typeof(NctsArrivalHeaderContainerPhase5ValidationDecider);
	}
}
