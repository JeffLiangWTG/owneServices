using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class AddressPartyWrapperTest : Customs.Business.Testing.DataProviderTestCase<AddressPartyWrapper>
	{
		public void TestCity()
		{
			AssertEquals("City should equal OA_City", "Insomnia", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country should equal OA_RN_NKCountryCode", "LS", Provider.Country);
		}

		public void TestNumber()
		{
			AssertEquals("If OA_AddressMap provided, Number should be equal to StreetNumber.", "28", Provider.Number);
		}

		public void TestNumber_AddressMapNotProvided()
		{
			AssertEquals("If OA_AddressMap not provided, Number should be empty.", string.Empty, AlternativeProvider.Number);
		}

		public void TestPoBox()
		{
			AssertEquals("waiting to be done in the future", string.Empty, Provider.PoBox);
		}

		public void TestPostCode()
		{
			AssertEquals("PostCode should equal OA_PostCode", "000000", Provider.PostCode);
		}

		public void TestStreet()
		{
			AssertEquals("If OA_AddressMap provided, Street should be equal to address.Street", "Kings Street", Provider.Street);
		}

		public void TestStreet_AddressMapNotProvided()
		{
			AssertEquals("If OA_AddressMap not provided, Street should equal OA_Address1", "28 Kings Street", AlternativeProvider.Street);
		}

		public void TestStreetAdditionalLine()
		{
			AssertEquals("StreetAdditionalLine should equal OA_Address2", "No.001", Provider.StreetAdditionalLine);
		}

		public void TestSubDivision()
		{
			AssertEquals("waiting to be done in the future", string.Empty, Provider.SubDivision);
		}

		protected override AddressPartyWrapper GetProvider()
		{
			var address = Factory.New<OrgAddress>();

			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "28 Kings Street";
			address.OA_Address2 = "No.001";
			address.OA_AddressMap = "SNA1[0-1]SA1[3-14]";

			return AddressPartyWrapper.New(address);
		}

		protected AddressPartyWrapper AlternativeProvider => alternativeProvider ?? (alternativeProvider = GetAlternativeProvider());
		AddressPartyWrapper alternativeProvider;

		AddressPartyWrapper GetAlternativeProvider()
		{
			var address = Factory.New<OrgAddress>();

			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "28 Kings Street";
			address.OA_Address2 = "No.001";

			return AddressPartyWrapper.New(address);
		}
	}
}
