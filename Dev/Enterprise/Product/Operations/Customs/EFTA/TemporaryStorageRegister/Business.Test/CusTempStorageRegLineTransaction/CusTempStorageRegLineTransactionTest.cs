using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransaction))]
sealed class CusTempStorageRegLineTransactionTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultValues()
	{
		AssertEquals("Transaction Status is set to default", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
	}

	public void TestSRL_PackagesRemaining_Update() => CombineAssertions(() =>
	{
		var line = Factory.New<CusTempStorageRegLine>();
		AssertEquals("Initially SRL_PackagesRemaining = 0", 0, line.SRL_PackagesRemaining);
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_PackageQty = 1;
		AssertEquals("SRL_PackagesRemaining added by 1", 1, line.SRL_PackagesRemaining);
		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		transaction2.SRT_PackageQty = 4;
		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		transaction3.SRT_PackageQty = 6;
		var transaction4 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
		transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction4.SRT_PackageQty = 3;
		AssertEquals("SRL_PackagesRemaining added by 10", 11, line.SRL_PackagesRemaining);

		var transaction5 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		transaction5.SRT_PackageQty = -5;
		AssertEquals("SRL_PackagesRemaining reduced by 5", 6, line.SRL_PackagesRemaining);

		transaction2.SRT_PackageQty = 7;
		AssertEquals("SRL_PackagesRemaining added by 3", 9, line.SRL_PackagesRemaining);
	});

	public void TestSRT_TransactionType_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_TransactionType)
			.WithCaption("Transaction Type");
	});

	public void TestSRT_ReferenceType_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_ReferenceType)
			.WithCaption("Reference Type");
	});

	public void TestSRT_PackageQty_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_PackageQty)
			.WithCaption("Package Quantity")
			.WithShortCaption("Package Qty.");
	});

	public void TestSRT_Reference_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_Reference)
			.WithCaption("Reference Number");
	});

	public void TestSRT_Comments_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_Comments)
			.WithCaption("Comments");
	});

	public void TestSRT_SystemCreateTimeUtc_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_SystemCreateTimeUtc)
			.WithCaption("Create Time");
	});

	public void TestSRT_SystemCreateUser_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_SystemCreateUser)
			.WithCaption("Create User");
	});

	public void TestTransactionTypeDescription_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.TransactionTypeDescription)
			.WithCaption("Transaction Type Description");
	});

	public void TestSRT_BondAmount_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLineTransaction>()
			.HasProperty(t => t.SRT_BondAmount)
			.WithCaption("Goods Value");
	});

	public void TestPhysicalInOutDate()
	{
		transaction.PhysicalInOutDate = new ZDateTime(2022, 02, 15, 16, 43, 27);
		CombineAssertions(() =>
		{
			AssertEquals("SRT_PhysicalInOutDate is filled with the datetimeoffset in PhysicalInOutDate", new ZDateTimeOffset(2022, 02, 15, 16, 43, 27), transaction.SRT_PhysicalInOutDate);
			AssertEquals("PhysicalInOutDate is filled with the datetime in SRT_PhysicalInOutDate", new ZDateTime(2022, 02, 15, 16, 43, 27), transaction.PhysicalInOutDate);
		});
	}

	public void TestTransactionDate()
	{
		transaction.TransactionDate = new ZDateTime(2022, 02, 15, 16, 43, 27);
		CombineAssertions(() =>
		{
			AssertEquals("SRT_TransactionDate is filled with the datetimeoffset in TransactionDate", new ZDateTimeOffset(2022, 02, 15, 16, 43, 27), transaction.SRT_TransactionDate);
			AssertEquals("TransactionDate is filled with the datetime in SRT_TransactionDate", new ZDateTime(2022, 02, 15, 16, 43, 27), transaction.TransactionDate);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	static BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_ArrivalDate = ZDate.Today;
		header.SRH_PresentationDate = ZDate.Today;
		header.SRH_AppCode = "123";
		header.SRH_Status = "OK";
		header.SRH_Reference = "TEST";

		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_SRH = header.PK;
		line.SRL_LimitDate = ZDate.Today;
		line.SRL_LineNumber = 1;
		line.SRL_PackagesRemaining = 0;
		line.SRL_LocationOfGoods = "GB";

		var regLineTransaction = line.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_SRL = line.PK;
		regLineTransaction.SRT_TransactionType = "TRN";
		regLineTransaction.SRT_GrossWeight = 1;
		regLineTransaction.SRT_InternalReferenceNumber = "R1";

		return regLineTransaction;
	}

	protected override void SetUp()
	{
		base.SetUp();
		transaction = Factory.New<CusTempStorageRegLineTransaction>();
	}
	CusTempStorageRegLineTransaction transaction;
}
