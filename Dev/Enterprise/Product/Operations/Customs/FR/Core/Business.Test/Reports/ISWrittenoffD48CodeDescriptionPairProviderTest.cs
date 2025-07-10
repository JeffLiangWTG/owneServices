using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class ISWrittenoffD48CodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			var provider = new ISWrittenoffD48CodeDescriptionPairProvider();
			var list = provider.GetCodeDescriptionPairList();
			AssertEquals("There should be 3 elements in the list.", 3, list.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "YES", "NO", "BOTH" }, list.GetAllCodes());
		}
	}
}
