using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class OrganisationAddressWrapperTest : Customs.Business.Testing.DataProviderTestCase<OrganisationAddressWrapper>
	{
		public void TestCity()
		{
			AssertEquals("City should equal OA_City.", "EYRAUD CREMPSE MAURENS", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("Country should equal OA_RN_NKCountryCode.", Core.Constants.CountryCodes.France, Provider.Country);
		}

		public void TestPostcode()
		{
			AssertEquals("Postcode should equal OA_PostCode.", "24140", Provider.Postcode);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("StreetAndNumber should equal OA_Address1 + OA_Address2 comma seperated", "177 Impasse Jane Poupelet, Lescuretie", Provider.StreetAndNumber);
		}

		protected override OrganisationAddressWrapper GetProvider()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Address1 = "177 Impasse Jane Poupelet";
			orgAddress.OA_Address2 = "Lescuretie";
			orgAddress.OA_PostCode = "24140";
			orgAddress.OA_City = "EYRAUD CREMPSE MAURENS";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			return OrganisationAddressWrapper.New(orgAddress);
		}

		internal static void AssertOrganisationAddressWrapper(IAddress address, string country = "", string city = "", string streetAndNumber = "", string postcode = "")
		{
			AssertEquals("OrganisationAddressWrapper: Country", country, address.Country);
			AssertEquals("OrganisationAddressWrapper: City", city, address.City);
			AssertEquals("OrganisationAddressWrapper: Street And Number", streetAndNumber, address.StreetAndNumber);
			AssertEquals("OrganisationAddressWrapper: Postcode", postcode, address.Postcode);
		}
	}
}
