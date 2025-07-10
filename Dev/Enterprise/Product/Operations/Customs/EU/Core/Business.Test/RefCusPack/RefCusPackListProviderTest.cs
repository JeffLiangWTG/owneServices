using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetDeclarationPackTypeList()
		{
			var list = provider.GetDeclarationPackTypeList(Factory);
			AssertEquals("PT3", list.CodesAsString);
		}

		public void TestGetCustomsPackList()
		{
			var list = provider.GetCustomsPackList(Factory, "PKG", "EUN");
			AssertEquals("PT4", list.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ASYCO", "PT1", "PT1 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "PT2", "PT2 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT3", "PT3 Desc", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("EUN", "PKG", "PT4", "PT4 Desc", new ZDateTime(2000, 1, 1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			provider = new RefCusPackListProvider();
		}

		RefCusPackListProvider provider;
		UniversalReferenceTestDataHelper helper;
	}
}
