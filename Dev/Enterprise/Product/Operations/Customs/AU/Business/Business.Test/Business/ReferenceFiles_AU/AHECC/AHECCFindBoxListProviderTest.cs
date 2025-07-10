using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AHECCFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatch()
		{
			var listProvider = new AHECCFindBoxListProvider();
			AssertEquals("2203.00.10", ((IFindBoxListProvider)listProvider).NearestMatch("2203", explicitAutoComplete: true, cursor: -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			var listProvider = new AHECCFindBoxListProvider();
			AssertEquals("Bottled beer made from malt", ((IFindBoxListProvider)listProvider).DescriptionFromCode("2203.00.10"));
		}
	}
}
