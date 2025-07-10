using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class CountryOfRoutingConfigurationTest : CountryOfRoutingConfigurationAbstractTest<CountryOfRoutingConfiguration>
	{
		protected override Type GetCountryOfRoutingDeparturePhase5ValidationDeciderForTest() => typeof(CountryOfRoutingDeparturePhase5ValidationDecider);
	}
}
