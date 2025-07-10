using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentItineraryCountry))]
	class NonPersistentItineraryCountryTests : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var itineraryCountry = new NonPersistentItineraryCountry();
			AssertEquals("Itinerary Country", itineraryCountry.HumanReadableName);
		}

		public void TestTransportNationalityList()
		{
			var c = new NonPersistentItineraryCountry();
			AssertCollectionContains("XI", c.ItineraryCountries.GetAllCodes());
		}

		public void TestValidationCheckCountryCode()
		{
			var c = new NonPersistentItineraryCountry();
			c.CountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertNoMessageErrorContaining(c.CountryCodeInfo, "list");
			c.CountryCode = "";
			AssertNoMessageErrorContaining(c.CountryCodeInfo, "list");
			c.CountryCode = "X~";
			AssertHasMessageErrorContaining(c.CountryCodeInfo, "list");
		}
	}
}
