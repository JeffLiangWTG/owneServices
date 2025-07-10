using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business.Testing
{
	public class EntryStatusListHelperTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			var entryStatusList = new CodeDescriptionPairList();
			entryStatusList.AddPair("E01");
			entryStatusList.AddPair("E02");
			entryStatusList.AddPair("L01");
			entryStatusList.AddPair("L02");
			entryStatusList.AddPair("S01");
			entryStatusList.AddPair("S02");

			AssertContainsExactElementsInAnyOrder(new string[] { "E01", "E02", "S01", "S02" }, entryStatusList.GetEntryStatusList(false).GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { "L01", "L02" }, entryStatusList.GetEntryStatusList(true).GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { "E01", "E02" }, entryStatusList.GetEntryStatusList("E").GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { "L01", "L02" }, entryStatusList.GetEntryStatusList("L").GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { "S01", "S02" }, entryStatusList.GetEntryStatusList("S").GetAllCodes());
		}
	}
}
