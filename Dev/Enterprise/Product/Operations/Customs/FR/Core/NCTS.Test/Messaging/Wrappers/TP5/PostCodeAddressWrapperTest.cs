namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class PostCodeAddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<PostCodeAddressWrapper>
	{
		public void TestHouseNumber()
		{
			AssertEquals("HouseNumber should be mapped to E2_AdditionalAddressInformation", "6ter", Provider.HouseNumber);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode should be mapped to E2_Postcode.", "24140", Provider.PostCode);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be mapped to E2_RN_NKCountryCode.", Core.Constants.CountryCodes.France, Provider.Country);
		}

		protected override PostCodeAddressWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocationAddress>();
			locationGoodsAddress.E2_AdditionalAddressInformation = "6ter";
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24140";

			return PostCodeAddressWrapper.New(locationGoodsAddress);
		}
	}
}
