using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComplaintMessageSendingObjectLineLookupsTest : TestCaseWithFactory
{
	public void TestLocationList()
	{
		var list = lookups.LocationList;

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, list.Count);
			AssertEquals("Header", true, list.ContainsCode(EComplaintLocationList.Codes.Header));
			AssertEquals("Line", true, list.ContainsCode(EComplaintLocationList.Codes.Line));
		});
	}

	public void TestEntryLineList()
	{
		var entryLine1 = entryHeader.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		entryLine1.CL_AdValoremTariff = "1111";
		entryLine1.CL_Description = "aaa";

		var entryLine2 = entryHeader.AllEntryLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		entryLine2.CL_AdValoremTariff = "2222";
		entryLine2.CL_Description = "bbb";

		var list = lookups.EntryLineList;

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, list.Count);
			AssertEquals("entryLine1 PK", entryLine1.PK, list[0].PK);
			AssertEquals("entryLine1 Code", "1", list[0].Code);
			AssertEquals("entryLine1 Description", "1111 aaa", list[0].Description);
			AssertEquals("entryLine2 PK", entryLine2.PK, list[1].PK);
			AssertEquals("entryLine2 Code", "2", list[1].Code);
			AssertEquals("entryLine2 Description", "2222 bbb", list[1].Description);
		});
	}

	public void TestFieldNameList()
	{
		RefCusCodeTestHelper.CreateEComplaintFieldNames(Factory);

		CombineAssertions(() =>
		{
			sendingObject.Location = EComplaintLocationList.Codes.Header;
			var list = lookups.FieldNameList;
			AssertEquals("Location=Header, Code=HeaderField", true, list.ContainsCode(RefCusCodeTestHelper.ValidEComplaintHeaderField));
			AssertEquals("Location=Header, Code=LineField", false, list.ContainsCode(RefCusCodeTestHelper.ValidEComplaintLineField));
			AssertEquals("Location=Header, Code=invalid", false, list.ContainsCode(RefCusCodeTestHelper.InvalidEComplaintField));

			sendingObject.Location = EComplaintLocationList.Codes.Line;
			list = lookups.FieldNameList;
			AssertEquals("Location=Line, Code=HeaderField", false, list.ContainsCode(RefCusCodeTestHelper.ValidEComplaintHeaderField));
			AssertEquals("Location=Line, Code=LineField", true, list.ContainsCode(RefCusCodeTestHelper.ValidEComplaintLineField));
			AssertEquals("Location=Line, Code=invalid", false, list.ContainsCode(RefCusCodeTestHelper.InvalidEComplaintField));

			sendingObject.Location = ZString.Empty;
			list = lookups.FieldNameList;
			AssertEquals("Location=empty, Count", 0, list.Count);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		sendingObject = new EComplaintMessageSendingObjectLine(entryHeader);
		lookups = sendingObject.Lookups;
	}
	CusEntryHeader entryHeader;
	EComplaintMessageSendingObjectLine sendingObject;
	EComplaintMessageSendingObjectLineLookups lookups;
}
