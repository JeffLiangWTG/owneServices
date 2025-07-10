using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class AddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<AddressWrapper>
	{
		public void TestCity()
		{
			AssertEquals("City should equal OA_City.", "MAURENS", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country should equal OA_RN_NKCountryCode.", Core.Constants.CountryCodes.France, Provider.Country);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode should equal OA_PostCode.", "24140", Provider.PostCode);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("StreetAndNumber should equal OA_Address1.", "177 Impasse Jane Poupelet", Provider.StreetAndNumber);
		}

		protected override AddressWrapper GetProvider()
		{
			var address = Factory.New<OrgAddress>();

			address.OA_Address1 = "177 Impasse Jane Poupelet";
			address.OA_City = "MAURENS";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			address.OA_PostCode = "24140";

			return AddressWrapper.New(address);
		}
	}
}
