using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class DestinationProviderTest : DataProviderTestCase<DestinationProvider>
	{
		public void TestIDestination()
		{
			Assert("Should implement IDestination", Provider is IDestination);
		}

		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty, Empty", DestinationProvider.New(string.Empty, string.Empty));
				AssertNotNull("Has value, Empty", DestinationProvider.New("countryOfDestination", string.Empty));
				AssertNotNull("Empty, Has value", DestinationProvider.New(string.Empty, "regionOfDestination"));
			});
		}

		public void TestCountryOfDestination()
		{
			AssertEquals("countryOfDestination", Provider.CountryOfDestination);
		}

		public void TestRegionOfDestination()
		{
			AssertEquals("regionOfDestination", Provider.RegionOfDestination);
		}

		public void TestCcQualifier()
		{
			AssertNull("Do not populate", Provider.CcQualifier);
		}

		protected override DestinationProvider GetProvider()
		{
			return DestinationProvider.New("countryOfDestination", "regionOfDestination");
		}
	}
}
