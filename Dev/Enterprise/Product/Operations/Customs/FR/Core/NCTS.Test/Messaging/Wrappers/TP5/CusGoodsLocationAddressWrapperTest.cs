namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CusGoodsLocationAddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<CusGoodsLocationAddressWrapper>
	{
		public void TestCity()
		{
			AssertEquals("City should be equal to E2_City.", "city", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be equal to E2_RN_NKCountryCode.", Core.Constants.CountryCodes.France, Provider.Country);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode should be equal to E2_Postcode.", "24000", Provider.PostCode);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("StreetAndNumber should be equal to E2_Address1.", "address1", Provider.StreetAndNumber);
		}

		protected override CusGoodsLocationAddressWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Address1 = "address1";
			locationGoodsAddress.E2_City = "city";
			locationGoodsAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locationGoodsAddress.E2_Postcode = "24000";

			return CusGoodsLocationAddressWrapper.New(locationGoodsAddress);
		}
	}
}
