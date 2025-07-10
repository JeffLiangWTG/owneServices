using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCustomsPackListCN()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "030", "Unit 1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "001", "Unit 2", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "003", "Unit 3", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var provider = new RefCusPackListProvider();
			var list = provider.GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);
			var expectedList = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.China, RefCusCodeListTypesCodes.CustomsUQ, ZDateTime.Today);
			AssertContainsExactElementsInAnyOrder(expectedList, list);
			AssertEquals(3, expectedList.Count);
		}

		public void TestGetCommercialPackListCN()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetCommercialPackList(Factory, ZString.Empty);
			var expectedList = new MasterFiles.Business.RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestLoaderCN()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.China);
			AssertEquals("Enterprise.Customs.CN.Business.RefCusPackListProvider", provider.GetType().ToString());
		}

		public void TestGetDeclarationPackTypeList()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("CNDeclarationPackTypeList Count", 76, list.Count);
				AssertEquals("Should include BOX", true, list.ContainsCode("BOX"));
				AssertEquals("Should include UNT", true, list.ContainsCode("UNT"));
				AssertEquals("Should include CM", true, list.ContainsCode("CM"));
				AssertEquals("Should include Y2", true, list.ContainsCode("Y2"));
				AssertEquals("Should not include CNT", false, list.ContainsCode("CNT"));
			});
		}
	}
}
