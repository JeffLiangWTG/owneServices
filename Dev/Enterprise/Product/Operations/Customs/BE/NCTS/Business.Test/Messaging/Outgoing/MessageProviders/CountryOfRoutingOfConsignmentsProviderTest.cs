using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CountryOfRoutingOfConsignmentsProvider))]
	class CountryOfRoutingOfConsignmentsProviderTest : Customs.Business.Testing.DataProviderTestCase<CountryOfRoutingOfConsignmentsProvider>
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
