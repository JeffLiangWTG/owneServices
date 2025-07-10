using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class DestinationProviderTest : DataProviderTestCase<DestinationProvider>
{
	public void TestIDestination()
	{
		Assert("Should implement IDestination", Provider is IDestination);
	}

	public void TestCountryOfDestination()
	{
		AssertEquals("countryOfDestination", Provider.CountryOfDestination);
	}

	public void TestRegionOfDestination()
	{
		AssertEquals("regionOfDestination", Provider.RegionOfDestination);
	}

	protected override DestinationProvider GetProvider()
	{
		return new DestinationProvider("countryOfDestination", "regionOfDestination");
	}
}
