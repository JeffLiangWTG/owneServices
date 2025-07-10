using System.Linq;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(RefCusPackListProvider))]
sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
{
	public void TestLoaderIN()
	{
		var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.India);
		AssertType<RefCusPackListProvider>("Provider should be Enterprise.Customs.IN.Business.RefCusPackListProvider", provider);
	}

	public void TestGetCustomsPackListIN()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);
		var provider = new RefCusPackListProvider();
		var actualList = provider.GetCustomsPackList(Factory, RPTypeList.Codes.CommercialInvoice, Core.Constants.CountryCodes.India);
		AssertContainsExactElementsInAnyOrder(new string[] { "KGS", "PCS" }, actualList.GetAllCodes());
	}

	public void TestGetCommercialPackListIN()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);
		var expectedList = new string[] { "KGS", "PCS" };
		var provider = new RefCusPackListProvider();
		CombineAssertions(() =>
		{
			var actualListCIP = provider.GetCommercialPackList(Factory, RPTypeList.Codes.CommercialInvoice);
			AssertContainsExactElementsInAnyOrder("For type CIP", expectedList, actualListCIP.GetAllCodes());

			var actualListPKD = provider.GetCommercialPackList(Factory, RPTypeList.Codes.PackingDeclaration);
			AssertCollectionNotContains("For type not CIP", actualListPKD.GetAllCodes(), expectedList.Contains);
		});
	}

	public void TestGetRefCusPackList()
	{
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM1", 1);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM2", 1);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CR1", "CR1", 1);
		var packs = RefCusPackListProvider.GetRefCusPackList(Factory, "CMM");
		AssertEquals("Count", 2, packs.Count);
		AssertContainsExactElementsInAnyOrder("Code", new string[] { "CM1", "CM2" }, packs.Select(x => x.RP_CustomsPack).ToArray());
	}
}
