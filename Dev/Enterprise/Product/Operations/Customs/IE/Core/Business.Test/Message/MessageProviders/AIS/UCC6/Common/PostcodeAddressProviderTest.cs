using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class PostcodeAddressProviderTest : DataProviderTestCase<PostcodeAddressProvider>
	{
		public void TestIPostcodeAddress()
		{
			Assert("Should implement IPostcodeAddress", Provider is IPostcodeAddress);
		}

		public void TestHouseNumber()
		{
			AssertEquals("House 1", Provider.HouseNumber);
		}

		public void TestPostcode()
		{
			AssertEquals("D18", Provider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals("IE", Provider.Country);
		}

		protected override PostcodeAddressProvider GetProvider()
		{
			return PostcodeAddressProvider.New("House 1", "D18", "IE");
		}
	}
}
