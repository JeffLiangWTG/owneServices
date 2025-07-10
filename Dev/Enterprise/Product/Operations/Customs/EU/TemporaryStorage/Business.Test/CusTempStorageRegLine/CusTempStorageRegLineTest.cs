using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLine))]
sealed class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestRegHeader()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		var storageRegLine = Factory.New<CusTempStorageRegLine>();
		storageRegLine.SRL_SRH = header.PK;
		AssertType<CusTempStorageRegHeader>(storageRegLine.RegHeader);
	}

	public void TestCusTempStorageRegLineTransactions()
	{
		AssertType<CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>>(line.CusTempStorageRegLineTransactions);
	}

	public void TestRegLineItemPivots()
	{
		AssertType<CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>>(line.RegLineItemPivots);
	}

	public void TestGetStorageRegLineTransaction()
	{
		AssertSame(typeof(CusTempStorageRegLineTransaction), line.GetStorageRegLineTransactionType());
	}

	public void TestOriginalPackagesQuantity() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZInt.Zero, line.OriginalPackagesQuantity);
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_PackageQty = 10;
		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_PackageQty = 6;
		AssertEquals(ZInt.Zero, line.OriginalPackagesQuantity);
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		AssertEquals(10, line.OriginalPackagesQuantity);
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		AssertEquals(16, line.OriginalPackagesQuantity);
	});

	public void TestPackagesQuantityOnHand() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZInt.Zero, line.PackagesRemainingCalculated);
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionStatus = "ABC";
		transaction1.SRT_PackageQty = 10;
		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionStatus = "ABC";
		transaction2.SRT_PackageQty = 6;
		AssertEquals(16, line.PackagesRemainingCalculated);
		transaction1.SRT_TransactionStatus = "DEL";
		AssertEquals(6, line.PackagesRemainingCalculated);
	});

	public void TestGrossWeightOnHand() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZDecimal.Zero, line.GrossWeightRemainingCalculated);
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionStatus = "ABC";
		transaction1.SRT_GrossWeight = (ZDecimal)10;
		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionStatus = "ABC";
		transaction2.SRT_GrossWeight = (ZDecimal)6;
		AssertEquals((ZDecimal)16, line.GrossWeightRemainingCalculated);
		transaction1.SRT_TransactionStatus = "DEL";
		AssertEquals((ZDecimal)6, line.GrossWeightRemainingCalculated);
	});

	public void TestTSDItemNumber() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZString.Empty, line.TSDItemNumber);

		CusTempStorageRegLineItem item1 = Factory.New<CusTempStorageRegLineItem>();
		item1.SRI_GoodsItemNumber = 1000;
		CusTempStorageRegLineItem item2 = Factory.New<CusTempStorageRegLineItem>();
		item2.SRI_GoodsItemNumber = 2000;

		var pivot1 = line.RegLineItemPivots.AddNew();
		pivot1.SRV_SRL_Line = line.PK;
		pivot1.SRV_SRI_Item = item1.PK;
		AssertEquals("1000", line.TSDItemNumber);

		var pivot2 = line.RegLineItemPivots.AddNew();
		pivot2.SRV_SRL_Line = line.PK;
		pivot2.SRV_SRI_Item = item2.PK;
		AssertEquals(ZString.Empty, line.TSDItemNumber);
	});

	public void TestGoodsDescription() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZString.Empty, line.GoodsDescription);

		CusTempStorageRegLineItem item1 = Factory.New<CusTempStorageRegLineItem>();
		item1.SRI_GoodsDescription = "Description 1";
		CusTempStorageRegLineItem item2 = Factory.New<CusTempStorageRegLineItem>();
		item2.SRI_GoodsDescription = "Description 2";

		var pivot1 = line.RegLineItemPivots.AddNew();
		pivot1.SRV_SRL_Line = line.PK;
		pivot1.SRV_SRI_Item = item1.PK;
		AssertEquals("Description 1", line.GoodsDescription);

		var pivot2 = line.RegLineItemPivots.AddNew();
		pivot2.SRV_SRL_Line = line.PK;
		pivot2.SRV_SRI_Item = item2.PK;
		AssertEquals(ZString.Empty, line.GoodsDescription);
	});

	public void TestCommodityCode() => CombineAssertions(() =>
	{
		var line = (CusTempStorageRegLine)GetNewBusinessObject();
		AssertEquals(ZString.Empty, line.CommodityCode);

		CusTempStorageRegLineItem item1 = Factory.New<CusTempStorageRegLineItem>();
		item1.SRI_Tariff = "123.456";
		CusTempStorageRegLineItem item2 = Factory.New<CusTempStorageRegLineItem>();
		item2.SRI_Tariff = "123.789";

		var pivot1 = line.RegLineItemPivots.AddNew();
		pivot1.SRV_SRL_Line = line.PK;
		pivot1.SRV_SRI_Item = item1.PK;
		AssertEquals("123.456", line.CommodityCode);

		var pivot2 = line.RegLineItemPivots.AddNew();
		pivot2.SRV_SRL_Line = line.PK;
		pivot2.SRV_SRI_Item = item2.PK;
		AssertEquals(ZString.Empty, line.CommodityCode);
	});

	public void TestGetStorageRegLineItemPivotType()
		=> AssertEquals(typeof(CusTempStorageRegLineItemPivot), line.GetStorageRegLineItemPivotType());

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "AAA";
		header.SRH_Reference = "reference";
		line = (CusTempStorageRegLine)header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
	}
	CusTempStorageRegLine line;

	static BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "AAA";
		header.SRH_Reference = "reference";
		var line = (CusTempStorageRegLine)header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		return line;
	}
}
