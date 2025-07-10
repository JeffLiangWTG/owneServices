using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUTravelDocAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountriesList()
		{
			var travelDoc = Factory.New<TravelDocument>();
			AssertEquals("Countries", typeof(CMRICAOCountryCodes), travelDoc.AddInfoLookups.Countries.GetType());
		}
	}
}
