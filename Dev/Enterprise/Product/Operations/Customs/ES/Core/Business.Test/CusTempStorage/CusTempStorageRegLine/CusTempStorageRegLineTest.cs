using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageRegLine))]
public class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject(Factory);
		AssertType<CusTempStorageRegLineLookups>(line.Lookups);
	}

	public void TestCusTempStorageRegLineTransactions()
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject(Factory);
		AssertType<CusTempStorageRegLineTransactionCollection>("Type", line.CusTempStorageRegLineTransactions);
	}

	public void TestCusTempStorageRegLineTransactionsForFilter() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject(Factory);
		AssertType<CusTempStorageRegLineTransactionCollection>("Type", line.CusTempStorageRegLineTransactionsForFilter);

		_ = line.CusTempStorageRegLineTransactions.AddNew();

		AssertEquals("Contains the same elements of CusTempStorageRegLineTransactions", 1, line.CusTempStorageRegLineTransactionsForFilter.Count);
	});

	public void TestHasLineOBLTransaction()
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject(Factory);
		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("False if no OBL transaction in RegLine", false, line.HasRegLineOBLTransaction());

			transaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			AssertEquals("True if has OBL transaction in RegLine", true, line.HasRegLineOBLTransaction());
		});
	}

	public void TestSetRegLineLocationAndReference()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		var regLine = header.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;

		CombineAssertions(() =>
		{
			AssertEquals("[PreReq] Default value RegLine SRL_LocationOfGoods", ZString.Empty, regLine.SRL_LocationOfGoods);
			AssertEquals("[PreReq] Default value SRL_OwnerReference", ZString.Empty, regLine.SRL_OwnerReference);

			regLine.SetRegLineLocationAndReference(Factory, "NewLocation", true, "NewReference", true);
			Factory.Save();
			AssertEquals("RegLine SRL_LocationOfGoods after save", "NewLocation", regLine.SRL_LocationOfGoods);
			AssertEquals("RegLine SRL_OwnerReference after save", "NewReference", regLine.SRL_OwnerReference);

			regLine.SetRegLineLocationAndReference(Factory, "Location", false, "Reference", false);
			Factory.Save();
			AssertEquals("RegLine SRL_LocationOfGoods still same value if shouldChangeLocation is false after save", "NewLocation", regLine.SRL_LocationOfGoods);
			AssertEquals("RegLine SRL_OwnerReference still same value if shouldChangeReference is false after save", "NewReference", regLine.SRL_OwnerReference);
		});
	}

	public void TestSRL_CustomStatus()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(regLine.SRL_CustomsStatusInfo, "Customs Status", "Status", "St", "Customs Status for the selected packages");
		});
	}

	public void TestSRL_PackageMarks()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(regLine.SRL_PackageMarksInfo, "Marks & Numbers", "Marks & Num", "Marks", "Marks and Numbers for selected packages");
		});
	}

	public void TestNumberOfItems()
	{
		regLine.RegLineItemPivots.AddNew();
		regLine.RegLineItemPivots.AddNew();

		AssertEquals("NumberOfItems should be the count of RegLineItemPivots", 2, regLine.NumberOfItems);
	}

	public void TestTSDItemNumbers()
	{
		var lineItem1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		var lineItem2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		lineItem1.SRI_GoodsItemNumber = 3;
		lineItem2.SRI_GoodsItemNumber = 1;

		var lineItemPivot1 = regLine.RegLineItemPivots.AddNew();
		lineItemPivot1.SRV_SRI_Item = lineItem1.PK;

		CombineAssertions(() =>
		{
			AssertEquals("TSDItemNumbers should be the SRI_GoodItemNumber of RegLineItem of RegLineItemPivot", "3", regLine.TSDItemNumbers);

			var lineItemPivot2 = regLine.RegLineItemPivots.AddNew();
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			AssertEquals("TSDItemNumbers should be the SRI_GoodItemNumber of RegLineItem of RegLineItemPivot", "1, 3", regLine.TSDItemNumbers);
		});
	}

	public void TestItemCommodityCode()
	{
		var lineItem1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		var lineItem2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		lineItem1.SRI_Tariff = "1234";
		lineItem2.SRI_Tariff = "5678";

		CombineAssertions(() =>
		{
			AssertEquals("ItemCommodityCode is empty", ZString.Empty, regLine.ItemCommodityCode);

			var lineItemPivot1 = regLine.RegLineItemPivots.AddNew();
			lineItemPivot1.SRV_SRI_Item = lineItem1.PK;

			AssertEquals("ItemCommodityCode is equal to the unique Tariff in the RegLine", "1234", regLine.ItemCommodityCode);

			var lineItemPivot2 = regLine.RegLineItemPivots.AddNew();
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			AssertEquals("ItemCommodityCode is equal to Multiple due to more than 1 Tariff in the RegLine", "Multiple", regLine.ItemCommodityCode);

			lineItem1.SRI_Tariff = ZString.Empty;
			AssertEquals("ItemCommodityCode is equal to the unique Tariff in the RegLine", "5678", regLine.ItemCommodityCode);
		});
	}

	public void TestItemGoodsDescription()
	{
		var lineItem1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		var lineItem2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		lineItem1.SRI_GoodsDescription = "1234";
		lineItem2.SRI_GoodsDescription = "5678";

		CombineAssertions(() =>
		{
			AssertEquals("ItemGoodsDescription is empty", ZString.Empty, regLine.ItemGoodsDescription);

			var lineItemPivot1 = regLine.RegLineItemPivots.AddNew();
			lineItemPivot1.SRV_SRI_Item = lineItem1.PK;

			AssertEquals("ItemGoodsDescription is equal to the unique Description in the RegLine", "1234", regLine.ItemGoodsDescription);

			var lineItemPivot2 = regLine.RegLineItemPivots.AddNew();
			lineItemPivot2.SRV_SRI_Item = lineItem2.PK;
			AssertEquals("ItemGoodsDescription is equal to Multiple due to more than 1 Description in the RegLine", "Multiple", regLine.ItemGoodsDescription);

			lineItem1.SRI_GoodsDescription = ZString.Empty;
			AssertEquals("ItemGoodsDescription is equal to the unique Description in the RegLine", "5678", regLine.ItemGoodsDescription);
		});
	}
	public void TestGetStorageRegLineItemPivotType()
		=> AssertEquals(typeof(CusTempStorageRegLineItemPivot), regLine.GetStorageRegLineItemPivotType());

	public void TestDelete()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var transaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		var pivot = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot.SRV_SRL_Line = regLine.PK;

		regLine.Delete();
		CombineAssertions(() =>
		{
			Assert("RegLine is delete", regLine.IsDeleted);
			Assert("Transaction is delete", transaction.IsDeleted);
			Assert("Pivot is delete", pivot.IsDeleted);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		return line;
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		regLine = header.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
	}
	CusTempStorageRegLine regLine;
}
