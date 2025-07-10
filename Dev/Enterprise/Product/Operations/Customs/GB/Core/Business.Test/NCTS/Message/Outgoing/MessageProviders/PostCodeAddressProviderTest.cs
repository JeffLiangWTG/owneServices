using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class PostCodeAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PostCodeAddressProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PostCodeAddressProvider(null));
		}

		public void TestHouseNumber()
		{
			AssertEquals("47", Provider.HouseNumber);
		}

		public void TestPostcode()
		{
			AssertEquals("2600", Provider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, Provider.Country);
		}

		protected override PostCodeAddressProvider GetProvider()
		{
			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_AdditionalIdentifier = "47";
			goodsLocation.Address.E2_Postcode = "2600";
			goodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			return new PostCodeAddressProvider(goodsLocation);
		}
	}
}
