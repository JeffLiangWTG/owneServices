using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageConfiguration))]
sealed class NctsPackageConfigurationTest : NctsPackageConfigurationAbstractTest
{
	protected override Type NctsPackageDeparturePhase5ValidationDeciderForTest => typeof(NctsPackageDeparturePhase5ValidationDecider);

	protected override Type NctsPackageArrivalPhase5ValidationDeciderForTest => typeof(NctsPackageArrivalPhase5ValidationDecider);
}
