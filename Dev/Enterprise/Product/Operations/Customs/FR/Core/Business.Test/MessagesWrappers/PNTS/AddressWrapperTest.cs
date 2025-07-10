using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class AddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<AddressWrapper>
	{
		public void TestCity()
		{
			AssertEquals("City should equal E2_City.", "MAURENS", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country should equal E2_RN_NKCountryCode.", "FR", Provider.Country);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode should equal E2_Postcode.", "24140", Provider.PostCode);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("StreetAndNumber should equal E2_Address1.", "177 Impasse Jane Poupelet", Provider.StreetAndNumber);
		}

		protected override AddressWrapper GetProvider()
		{
			var address = Factory.New<CusGoodsLocationAddress>();

			address.E2_City = "MAURENS";
			address.E2_RN_NKCountryCode = "FR";
			address.E2_Postcode = "24140";
			address.E2_Address1 = "177 Impasse Jane Poupelet";

			return AddressWrapper.New(address);
		}
	}
}
