using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NonPersistentItineraryCountryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCountryCode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var country1 = header.Itinerary.AddNew();
			country1.CountryCode = "GB";
			country1.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(country1, "At least two different countries/regions must be entered.");
			var country2 = header.Itinerary.AddNew();
			country2.CountryCode = "IT";
			country1.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(country1, "At least two different countries/regions must be entered.");
			AssertHasRowMessageErrorContaining(country1, "At least one of the entered countries/regions must be DE.");

			country1.CountryCode = "DE";
			country1.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(country1, "At least one of the entered countries/regions must be DE.");
		}
	}
}
