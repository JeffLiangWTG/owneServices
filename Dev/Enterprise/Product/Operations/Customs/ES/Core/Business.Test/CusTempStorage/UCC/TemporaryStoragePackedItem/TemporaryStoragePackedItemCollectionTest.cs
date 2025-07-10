using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStoragePackedItemCollection))]
public class TemporaryStoragePackedItemCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var bill = Factory.New<TemporaryStorageBill>();
		return new TemporaryStoragePackedItemCollection(bill);
	}

	public void TestSequenceNumberCalculator()
	{
		var bill = Factory.New<TemporaryStorageBill>();
		var packedItems = bill.PackedItems;
		AssertType<TemporaryStoragePackedItemSequenceNumberGenerator>(packedItems.SequenceNumberCalculator);
	}

	public void TestShouldRecalculateLineNos()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		var packedItemsIterface = bill.PackedItems as ISequenceNumberHeaderWithFlagToRecalculate;

		AssertEquals("ShouldRecalculateLineNos is true when messageType is not TSM (empty)", true, packedItemsIterface.ShouldRecalculateLineNos);

		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		AssertEquals("ShouldRecalculateLineNos is false when messageType is TSM", false, packedItemsIterface.ShouldRecalculateLineNos);

		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		AssertEquals("ShouldRecalculateLineNos is true when messageType is not TSM (G5P)", true, packedItemsIterface.ShouldRecalculateLineNos);

		var bill2 = Factory.New<TemporaryStorageBill>();
		var packedItemsIterface2 = bill2.PackedItems as ISequenceNumberHeaderWithFlagToRecalculate;
		AssertEquals("ShouldRecalculateLineNos is true when bill has no header", true, packedItemsIterface2.ShouldRecalculateLineNos);
	}

	public void TestAPI_LineNoIsUpdatedWhenItemIsRemoved()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		var packedItems = bill.PackedItems;

		CombineAssertions("When MessageType is not TSM", () =>
		{
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			item2.API_LineNo = 20;
			var item3 = packedItems.AddNew();

			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (reset)", (ZInt)2, item3.API_LineNo);
		});

		packedItems.RemoveAndDeleteAll();
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		CombineAssertions("When MessageType is TSM", () =>
		{
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			item2.API_LineNo = 20;
			var item3 = packedItems.AddNew();

			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)20, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)21, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (not reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (not reset)", (ZInt)21, item3.API_LineNo);
		});
	}

	public void TestAPI_GrossWeightUQIsSetForNewItems()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		var packedItems = bill.PackedItems;

		var item = packedItems.AddNew();
		AssertEquals("item.API_GrossWeightUQ", Core.Constants.Weight.Kilograms, item.API_GrossWeightUQ);
	}
}
