using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	[TestedType(typeof(AddressProvider))]
	sealed class AddressProviderTest : DataProviderTestCase<AddressProvider>
	{
		protected override AddressProvider GetProvider() => provider;

		public void TestCountry()
		{
			AssertEquals(Core.Constants.CountryCodes.Ireland, provider.Country);
		}

		public void TestCity()
		{
			AssertEquals("AddressCity", provider.City);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("21 PartyStreet", provider.StreetAndNumber);
		}

		public void TestPostcode()
		{
			AssertEquals("1234", provider.Postcode);
		}

		protected override void SetUp()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.City = "AddressCity";
			orgAddress.Address1 = "21 PartyStreet";
			orgAddress.Postcode = "1234";
			provider = new AddressProvider(orgAddress);
		}

		AddressProvider provider;
	}
}
