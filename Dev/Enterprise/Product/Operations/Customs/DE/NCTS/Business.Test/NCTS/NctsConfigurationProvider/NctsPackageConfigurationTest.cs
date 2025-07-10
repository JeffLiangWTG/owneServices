using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageConfiguration))]
sealed class NctsPackageConfigurationTest : NctsPackageConfigurationAbstractTest
{
	protected override Type NctsPackageDeparturePhase5ValidationDeciderForTest => typeof(NctsPackageDeparturePhase5ValidationDecider);

	protected override Type NctsPackageArrivalPhase5ValidationDeciderForTest => typeof(EU.NCTS.Business.NctsPackageArrivalPhase5ValidationDecider);
}
