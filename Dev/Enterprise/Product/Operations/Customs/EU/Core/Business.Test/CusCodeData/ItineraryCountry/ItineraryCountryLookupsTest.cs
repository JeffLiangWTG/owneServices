using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ItineraryCountryLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCountryList()
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			NUnit.Framework.Assert.That(itineraryCountry.Lookups.CountryList, NUnit.Framework.Is.TypeOf<RefCountryCollection>());
		}
	}
}
