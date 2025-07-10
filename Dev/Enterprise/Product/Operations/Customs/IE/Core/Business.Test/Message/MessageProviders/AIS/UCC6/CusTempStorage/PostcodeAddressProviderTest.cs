using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class PostcodeAddressProviderTest : DataProviderTestCase<PostcodeAddressProvider>
	{
		public void TestHouseNumber()
		{
			AssertEquals("HouseNumber", "additional id", Provider.HouseNumber);
		}

		public void TestPostcode()
		{
			AssertEquals("Postcode", "12345", Provider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals("Country", "IE", Provider.Country);
		}

		protected override PostcodeAddressProvider GetProvider()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.Postcode = "12345";
			orgAddress.OA_RN_NKCountryCode = "IE";
			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_AdditionalIdentifier = "additional id";
			goodsLocation.Address.E2_OA_Address = orgAddress.PK;

			return PostcodeAddressProvider.New(goodsLocation);
		}
	}
}
