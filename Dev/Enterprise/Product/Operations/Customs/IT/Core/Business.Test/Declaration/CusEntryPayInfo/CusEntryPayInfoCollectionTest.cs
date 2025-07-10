using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryPayInfoCollection<CusEntryPayInfo>))]
sealed class CusEntryPayInfoCollectionTest : Customs.Business.Testing.CusEntryPayInfoCollectionTest
{
	public void TestInsertOrUpdateEntryPayInfoWithMethodOfPaymentPredicate()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryPayInfoCollection = new CusEntryPayInfoCollection<CusEntryPayInfo>(entryHeader);
		AssertEquals("PRE-CONDITION", 0, entryPayInfoCollection.Count);

		var entryPayInfo1 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == "G", 1m, "4 T", new ZDateTime(2021, 01, 01), "000001", "G");
		CombineAssertions("entryPayInfo1 added", () =>
		{
			entryPayInfo1.AssertEntryPayInfo("000001", "G", 1m, "4 T", new ZDateTime(2021, 01, 01), CusEntryPayInfoStatusList.Codes.Pending);
		});

		var entryPayInfo2 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == "T", 1m, "4 T", new ZDateTime(2021, 01, 01), "000001", "T");
		CombineAssertions("entryPayInfo2 added", () =>
		{
			entryPayInfo2.AssertEntryPayInfo("000001", "T", 1m, "4 T", new ZDateTime(2021, 01, 01), CusEntryPayInfoStatusList.Codes.Pending);
		});

		CombineAssertions("entryPayInfo2 updated", () =>
		{
			var entryPayInfo3 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == "T", 999m, "2", new ZDateTime(2021, 12, 31), "999999", "T");
			entryPayInfo3.AssertEntryPayInfo("999999", "T", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			AssertSame("Same object", entryPayInfo2, entryPayInfo3);
		});

		CombineAssertions("Edge Case 1: add element with empty method of payment", () =>
		{
			var entryPayInfo4 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == "", 999m, "2", new ZDateTime(2021, 12, 31), "999999", "");
			entryPayInfo4.AssertEntryPayInfo("999999", "", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
		});

		CombineAssertions("Edge Case 2: add element with empty A93 number", () =>
		{
			var entryPayInfo5 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x.MethodOfPayment == "X", 999m, "2", new ZDateTime(2021, 12, 31), "", "X");
			entryPayInfo5.AssertEntryPayInfo("", "X", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
		});

		CombineAssertions("Full EntryPayInfo collection", () =>
		{
			entryPayInfoCollection[0].AssertEntryPayInfo("000001", "G", 1m, "4 T", new ZDateTime(2021, 01, 01), CusEntryPayInfoStatusList.Codes.Pending);
			entryPayInfoCollection[1].AssertEntryPayInfo("999999", "T", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			entryPayInfoCollection[2].AssertEntryPayInfo("999999", "", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			entryPayInfoCollection[3].AssertEntryPayInfo("", "X", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
		});
	}

	public void TestElementsAsEnumerable()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryPayInfoCollection = new CusEntryPayInfoCollection<CusEntryPayInfo>(entryHeader);

		AssertNotNull(nameof(CusEntryPayInfoCollection<CusEntryPayInfo>.ElementsAsEnumerable), entryPayInfoCollection.ElementsAsEnumerable);

		entryPayInfoCollection.AddNew();
		entryPayInfoCollection.AddNew();

		AssertEquals($"{nameof(CusEntryPayInfoCollection<CusEntryPayInfo>.ElementsAsEnumerable)} Count()", 2, entryPayInfoCollection.ElementsAsEnumerable.Count());
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryPayInfoCollection<CusEntryPayInfo>(Factory.New<CusEntryHeader>());
}
