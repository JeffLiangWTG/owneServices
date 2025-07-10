using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	sealed class H7ManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAgentType()
		{
			var header = Factory.New<H7ManifestHeader>();
			var codeList = header.Lookups.AgentTypeList;
			var sameCodeList = header.Lookups.AgentTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("There are three elements in the list", 3, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SEL", "DIR", "IND" }, codeList.GetAllCodes());
				AssertEquals("Agent Type List is using cache", sameCodeList, codeList);
			});
		}
	}
}
