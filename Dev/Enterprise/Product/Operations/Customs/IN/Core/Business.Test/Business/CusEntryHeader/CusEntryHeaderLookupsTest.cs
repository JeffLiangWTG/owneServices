using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryHeaderLookups))]
sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var messageStatusList = entryHeader.Lookups.MessageStatusList;
		AssertContainsExactElementsInAnyOrder("MessageStatusList have values in lookup", new[] { "SNT", "ACC", "QUE", "FAL", "ERR", "INV" }, messageStatusList.GetAllCodes());
		AssertSame("cached", messageStatusList, entryHeader.Lookups.MessageStatusList);
	}

	public void TestCH_EntryStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var customsStatusList = entryHeader.Lookups.CH_EntryStatusList;
		AssertContainsExactElementsInAnyOrder("CH_EntryStatusList have values in lookup", new[] { "SBA", "FSC", "LEO", "GPR" }, customsStatusList.GetAllCodes());
		AssertSame("cached", customsStatusList, entryHeader.Lookups.CH_EntryStatusList);
	}
}
