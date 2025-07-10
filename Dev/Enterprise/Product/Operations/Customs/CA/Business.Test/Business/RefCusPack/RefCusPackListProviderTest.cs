using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCIPCustomsPackListCA()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCIPCustomsPackList(Factory, ZString.Empty);
			var expectedList = Factory.GetCachedValue<CustomsUnitOfMeasureList>();
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestGetCommercialPackListCA()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCommercialPackList(Factory, ZString.Empty);
			var expectedList = new MasterFiles.Business.RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			AssertContainsExactElementsInAnyOrder(expectedList, list);

			var list2 = provider.GetCommercialPackList(Factory, RPTypeList.Codes.CommercialInvoice);
			var caIIDList = Factory.GetCachedValue<IIDUnitOfCountCodeList>();
			expectedList.AddRangeOverwriteIfExists(caIIDList);
			AssertContainsExactElementsInAnyOrder(expectedList, list2);

			var caProvider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.Canada);
			var caCommercialList = caProvider.GetCommercialPackList(Factory, RPTypeList.Codes.CommercialInvoice);
			AssertSame(list2, caCommercialList);
		}

		public void TestLoaderCA()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.Canada);
			AssertEquals("Enterprise.Customs.CA.Business.RefCusPackListProvider", provider.GetType().ToString());
		}

		public void TestGetDeclarationPackTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ASYCO", "PT1", "PT1 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "PT2", "PT2 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT3", "PT3 Desc", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("BR", "PKG", "PT4", "PT4 Desc", new ZDateTime(2000, 1, 1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("CADeclarationPackTypeList Count", 152, list.Count);
				AssertEquals("CADeclarationPackTypeList should include AMM", true, list.ContainsCode("AMM"));
				AssertEquals("CADeclarationPackTypeList should include WRP", true, list.ContainsCode("WRP"));
				AssertEquals("CADeclarationPackTypeList should include PT3", true, list.ContainsCode("PT3"));
			});
		}
	}
}
