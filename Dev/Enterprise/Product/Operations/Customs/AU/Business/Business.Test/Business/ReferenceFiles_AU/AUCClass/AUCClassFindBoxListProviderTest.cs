using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCClassFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatch()
		{
			var listProvider = new AUCClassFindBoxListProvider(Factory);
			AssertEquals("2203.00.20 10", ((IFindBoxListProvider)listProvider).NearestMatch("2203", true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			var listProvider = new AUCClassFindBoxListProvider(Factory);
			AssertEquals("Having an alcoholic strength by volume not exceeding 1.15% vol", ((IFindBoxListProvider)listProvider).DescriptionFromCode("2203.00.20 10"));
		}
	}
}
