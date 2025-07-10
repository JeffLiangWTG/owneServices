using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CustomsFiscalTerritoriesListTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetSpainFullList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
			helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "06", "Test 6");
			helper.CreateCusCodeListCanaryIsland(countryCode, "07", "Test 7");

			var emptyList = CustomsFiscalTerritoriesList.GetSpainFullList(null);

			var list = CustomsFiscalTerritoriesList.GetSpainFullList(Factory);
			var listCached = CustomsFiscalTerritoriesList.GetSpainFullList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(new CodeDescriptionPairList(), emptyList);
				AssertEquals(list, listCached);
			});
		}

		public void TestGetSpainList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListTerritory(Core.Constants.CountryCodes.Spain, "01", "Test 1");

			var emptyList = CustomsFiscalTerritoriesList.GetSpainList(null);

			var list = CustomsFiscalTerritoriesList.GetSpainList(Factory);
			var listCached = CustomsFiscalTerritoriesList.GetSpainList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(new CodeDescriptionPairList(), emptyList);
				AssertEquals(list, listCached);
			});
		}

		public void TestGetNorthAfricanSpainList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListNorthAfricanTerritory(Core.Constants.CountryCodes.Spain, "06", "Test 6");

			var emptyList = CustomsFiscalTerritoriesList.GetNorthAfricanSpainList(null);

			var list = CustomsFiscalTerritoriesList.GetNorthAfricanSpainList(Factory);
			var listCached = CustomsFiscalTerritoriesList.GetNorthAfricanSpainList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(new CodeDescriptionPairList(), emptyList);
				AssertEquals(list, listCached);
			});
		}

		public void TestGetCanaryIslandsList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "07", "Test 7");

			var emptyList = CustomsFiscalTerritoriesList.GetCanaryIslandsList(null);

			var list = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory);
			var listCached = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(new CodeDescriptionPairList(), emptyList);
				AssertEquals(list, listCached);
			});
		}
	}
}
