using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CountryOfRoutingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfRouting = Factory.New<CountryOfRouting>();
			var lookups = countryOfRouting.Lookups;
			var countryOfExportList = lookups.CountryList as ZZRefCusCodeListCombinedCollection;
			countryOfExportList.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryOfExportList.Select(x => x.ZZD_Code));
		}
	}
}
