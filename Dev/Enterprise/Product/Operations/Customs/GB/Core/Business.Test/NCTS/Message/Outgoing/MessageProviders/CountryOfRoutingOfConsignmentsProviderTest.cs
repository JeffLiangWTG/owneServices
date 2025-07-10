using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class CountryOfRoutingOfConsignmentsProviderTest : DataProviderTestCase<CountryOfRoutingOfConsignmentsProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestCountry()
		{
			AssertEquals(Core.Constants.CountryCodes.Belgium, provider.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = CreateProvider(Core.Constants.CountryCodes.Belgium, 1);
		}

		CountryOfRoutingOfConsignmentsProvider provider;

		static CountryOfRoutingOfConsignmentsProvider CreateProvider(ZString countryCode, int sequenceNumber) => new CountryOfRoutingOfConsignmentsProvider(countryCode, sequenceNumber);

		protected override CountryOfRoutingOfConsignmentsProvider GetProvider() => provider;
	}
}
