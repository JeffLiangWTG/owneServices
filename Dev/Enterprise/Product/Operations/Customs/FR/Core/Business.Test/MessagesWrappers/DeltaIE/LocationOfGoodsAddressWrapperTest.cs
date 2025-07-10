using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class LocationOfGoodsAddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsPostcodeAddressWrapper>
	{
		protected override LocationOfGoodsPostcodeAddressWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Address1 = "address1";
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24000";

			return LocationOfGoodsPostcodeAddressWrapper.New(locationGoodsAddress);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be equal to E2_RN_NKCountryCode.", Core.Constants.CountryCodes.France, Provider.Country);
		}

		public void TestPostcode()
		{
			AssertEquals("Postcode should be equal to E2_Postcode.", "24000", Provider.Postcode);
		}

		public void TestHouseNumber()
		{
			AssertEquals("HouseNumber should be equal to E2_Address1.", "address1", Provider.HouseNumber);
		}
	}
}
