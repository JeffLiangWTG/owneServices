using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

class AddInfoCusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestRequestTypeList()
	{
		var addInfo = Factory.New<CusEntryInstruction>().AddInfo;
		var cachedList = addInfo.Lookups.RequestTypeList;
		AssertEquals("Lookups.RequestTypeList.CodesAsString", "01, 02", addInfo.Lookups.RequestTypeList.CodesAsString);
		AssertSame("List is cached", cachedList, addInfo.Lookups.RequestTypeList);
	}

	public void TestIndirectTypeList() => CombineAssertions(() =>
	{
		var addInfo = Factory.New<CusEntryInstruction>().AddInfo;
		var cachedList = addInfo.Lookups.IndirectTypeList;
		AssertEquals("IndirectTypeList codes", "IND1, IND2, IND3, IND4, IND5, IND6, IND7, IND8, IND9", cachedList.CodesAsString);
		AssertSame("List is cached", cachedList, addInfo.Lookups.IndirectTypeList);
	});
}
