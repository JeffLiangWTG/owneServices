using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

public class TemporaryStoragePackedItemSequenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestRecalculateWhenAdded_RecalculateFlagTrue()
	{
		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		item2.API_LineNo = 20;
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);
		});
	}

	public void TestRecalculateWhenAdded_RecalculateFlagFalse()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		item2.API_LineNo = 20;
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)20, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)21, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (not reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (not reset)", (ZInt)21, item3.API_LineNo);
		});
	}

	public void TestRecalculateWhenRenumbered_RecalculateFlagTrue()
	{
		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			item2.API_LineNo = 20;
			AssertEquals("item1.API_LineNo after item2 is changed", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is changed", (ZInt)2, item3.API_LineNo);
			AssertEquals("item2.API_LineNo after item2 is changed", (ZInt)3, item2.API_LineNo);
		});
	}

	public void TestRecalculateWhenRenumbered_RecalculateFlagFalse()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			item2.API_LineNo = 20;
			AssertEquals("item1.API_LineNo after item2 is changed", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo after item2 is changed", (ZInt)20, item2.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is changed", (ZInt)3, item3.API_LineNo);
		});
	}

	public void TestRecalculateWhenAboutToBeDetachedOrDeleted_RecalculateFlagTrue()
	{
		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (reset)", (ZInt)2, item3.API_LineNo);
		});
	}

	public void TestRecalculateWhenAboutToBeDetachedOrDeleted_RecalculateFlagFalse()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (not reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (not reset)", (ZInt)3, item3.API_LineNo);
		});
	}

	public void TestReCalculateAll_RecalculateFlagTrue()
	{
		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		item2.API_LineNo = 20;
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			item2.Delete();

			packedItems.SequenceNumberCalculator.ReCalculateAll();
			AssertEquals("item1.API_LineNo after item2 is removed (recalculation)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (recalculation)", (ZInt)2, item3.API_LineNo);
		});
	}

	public void TestReCalculateAll_RecalculateFlagFalse()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		var item1 = packedItems.AddNew();
		var item2 = packedItems.AddNew();
		item2.API_LineNo = 20;
		var item3 = packedItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)20, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)21, item3.API_LineNo);

			item2.Delete();

			packedItems.SequenceNumberCalculator.ReCalculateAll();
			AssertEquals("item1.API_LineNo after item2 is removed (not reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (not reset)", (ZInt)21, item3.API_LineNo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		packedItems = bill.PackedItems;
	}

	TemporaryStoragePackedItemCollection packedItems;
	TemporaryStorageHeader header;
}
