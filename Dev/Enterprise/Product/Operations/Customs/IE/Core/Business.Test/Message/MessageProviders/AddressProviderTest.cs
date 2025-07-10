using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AddressProviderTest : Customs.Business.Testing.DataProviderTestCase<AddressProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", AddressProvider.New(null));
				AssertNotNull("Valid argument", GetProvider());
			});
		}

		public void TestStreetAndNumber()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.Address1 = "Unit 3A";
			orgAddress.Address2 = "72 O'Riordan ST";
			var provider = AddressProvider.New(orgAddress);
			AssertEquals("Unit 3A, 72 O'Riordan ST", provider.StreetAndNumber);

			orgAddress.Address1 = "Number 14";
			orgAddress.Address2 = "Sunset Drive";
			AssertEquals("Number 14, Sunset Drive", provider.StreetAndNumber);
		}

		public void TestCountry()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "CN";
			var provider = AddressProvider.New(orgAddress);
			AssertEquals("CN", provider.Country);

			orgAddress.OA_RN_NKCountryCode = "IE";
			AssertEquals("IE", provider.Country);
		}

		public void TestPostcode()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_PostCode = "1000000";
			var provider = AddressProvider.New(orgAddress);
			AssertEquals("1000000", provider.Postcode);

			orgAddress.OA_PostCode = "Y35 WX12";
			AssertEquals("Y35 WX12", provider.Postcode);
		}

		public void TestCity()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_City = "NanJing";
			var provider = AddressProvider.New(orgAddress);
			AssertEquals("NanJing", provider.City);

			orgAddress.OA_City = "Dublin";
			AssertEquals("Dublin", provider.City);
		}

		protected override AddressProvider GetProvider() => AddressProvider.New(Factory.New<OrgAddress>());
	}
}
