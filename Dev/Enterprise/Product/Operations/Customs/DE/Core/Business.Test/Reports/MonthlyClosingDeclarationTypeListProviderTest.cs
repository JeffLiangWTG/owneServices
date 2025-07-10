using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MonthlyClosingDeclarationTypeListProviderTest : TestCaseWithFactory
	{
		public void TestProviderReturnsCorrectCodeList()
		{
			var sut = new MonthlyClosingDeclarationTypeListProvider();

			var result = sut.GetCodeDescriptionPairList();

			AssertType<MonthlyClosingDeclarationTypeList>(result);
		}
	}
}
