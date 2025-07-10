using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegisterProcessor<>))]
sealed class CusTempStorageRegisterProcessorTest : TestCaseWithFactory
{
	public void TestCalculateTransactionsAndLockMutexIfNeededForAddingTransaction()
	{
		CreateDataForCalculateTransactionTests();

		var logger = new LoggingInformation();
		var provider = new ProviderForTest();
		var processor = new CusTempStorageRegisterProcessor<EDIMessage>(provider, logger);
		var transactionData1 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Updating Packages and Gross Weight transaction"
		};
		provider.temporaryStorageRegisterTransactionData.Add(transactionData1);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		CombineAssertions("First transaction data recalculation results", () =>
		{
			AssertEquals("New transaction Package Quantity should be 10+7-30 = -13.", -13, transactionData1.PackageQuantity);
			AssertEquals("New transaction GrossMass should be unsigned(2000+1400-6000) = 2600.", 2600m, transactionData1.GrossMass);
		});

		var transactionData2 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "UnmatchingReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction with new InternalReferenceNumber"
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData2);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		CombineAssertions("Second transaction data recalculation results", () =>
		{
			AssertEquals("New transaction Package Quantity should be 0 - 30 = -30, because all existing transactions are skipped from calculation as they don't match current transactionData.", -30, transactionData2.PackageQuantity);
			AssertEquals("New transaction GrossMass should be unsigned(0 -6000) = 6000, because all existing transactions are skipped from calculation as they don't match current transactionData.", 6000m, transactionData2.GrossMass);
		});

		var transactionData3 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 3, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction with wrong line number"
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData3);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		CombineAssertions("Third transaction data recalculation results", () =>
		{
			AssertEquals("New transaction Package Quantity should be 10+7-30 = -13, because reg line #3 not existing, reg line#1 should have been used for calculation.", -13, transactionData1.PackageQuantity);
			AssertEquals("New transaction GrossMass should be unsigned(2000+1400-6000) = 2600.", 2600m, transactionData1.GrossMass);
		});

		var transactionData4 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 3400m,
			PackageQuantity = 17,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction with empty package and empty gross wight should not be lodged."
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData4);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		Assert("Processor log should have been updated.", logger.Logs.Any(x => x.ToString().Contains("Register HEADER, Line Number 1 is already up to date, no transaction added.")));

		var transactionData5 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 99, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction tied to a register without specified register line."
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData5);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		AssertEquals("Register line #1 should have been used as a substitute to Register line #99.", 1, transactionData1.RegisterLineNo);

		var transactionData6 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = emptyRegisterHeader, RegisterLineNo = 99, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction tied to a register without any line."
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData6);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		Assert("Processor log should have been updated.", logger.Logs.Any(x => x.ToString().Contains("Register EmptyRegister: unable to find the Register Line #1 to default to.")));

		var transactionData7 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = null, RegisterLineNo = 99, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Transaction not tied to any register (not likely to happen)."
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData7);
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.UnlockRegistersMutexes();

		Assert("Processor log should have been updated.", logger.Logs.Any(x => x.ToString().Contains("No register found.")));
	}

	void CreateDataForCalculateTransactionTests()
	{
		emptyRegisterHeader = Factory.New<CusTempStorageRegHeader>();
		emptyRegisterHeader.SRH_Reference = "EmptyRegister";
		emptyRegisterHeader.SRH_AppCode = "IST";

		registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "HEADER";
		registerHeader.SRH_AppCode = "IST";

		registerLine1 = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine1.SRL_LineNumber = 1;
		registerLine1.SRL_PackageType = "CTN";

		//Opening Balance transaction will be skipped from balance calculation because SRT_TransactionType won't match.
		oblTransaction = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
		oblTransaction.SRT_Reference = "CustomsReference";
		oblTransaction.SRT_GrossWeight = 20000m;
		oblTransaction.SRT_PackageQty = 100;

		transaction1 = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction1.SRT_InternalReferenceNumber = "InternalReference";
		transaction1.SRT_Reference = "CustomsReference";
		transaction1.SRT_GrossWeight = 1400m;
		transaction1.SRT_PackageQty = -7;

		transaction2 = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction2.SRT_InternalReferenceNumber = "InternalReference";
		transaction2.SRT_Reference = "CustomsReference";
		transaction2.SRT_GrossWeight = 2000m;
		transaction2.SRT_PackageQty = -10;

		//transaction3 will be skipped from balance calculation because SRT_InternalReferenceNumber won't match.
		transaction3 = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction3.SRT_InternalReferenceNumber = ZString.Empty;
		transaction3.SRT_Reference = "CustomsReference";
		transaction3.SRT_GrossWeight = 2000m;
		transaction3.SRT_PackageQty = -23;

		//transaction4 will be skipped from balance calculation because SRT_Reference won't match.
		transaction4 = registerLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction4.SRT_InternalReferenceNumber = "InternalReference";
		transaction4.SRT_Reference = ZString.Empty;
		transaction4.SRT_GrossWeight = 2000m;
		transaction4.SRT_PackageQty = -10;

		registerLine2 = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine2.SRL_LineNumber = 2;
		transaction5 = registerLine2.CusTempStorageRegLineTransactions.AddNew();
		transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction5.SRT_InternalReferenceNumber = "InternalReference";
		transaction5.SRT_Reference = ZString.Empty;
		transaction5.SRT_GrossWeight = 2000m;
		transaction5.SRT_PackageQty = 10;

		AssertEquals("Prerequisite.", 50, registerLine1.SRL_PackagesRemaining);
	}
	CusTempStorageRegHeader emptyRegisterHeader;
	CusTempStorageRegHeader registerHeader;
	CusTempStorageRegLine registerLine1;
	CusTempStorageRegLine registerLine2;
	CusTempStorageRegLineTransaction oblTransaction;
	CusTempStorageRegLineTransaction transaction1;
	CusTempStorageRegLineTransaction transaction2;
	CusTempStorageRegLineTransaction transaction3;
	CusTempStorageRegLineTransaction transaction4;
	CusTempStorageRegLineTransaction transaction5;

	public void TestRollbackRegisterTransactionDataAndLockMutextIfNeeded()
	{
		CreateDataForCalculateTransactionTests();

		var logger = new LoggingInformation();
		var provider = new ProviderForTest();
		var processor = new CusTempStorageRegisterProcessor<EDIMessage>(provider, logger);
		var transactionData1 = new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader, RegisterLineNo = 1, CustomsReferenceNumber = "CustomsReference",
			InternalReferenceNumber = "InternalReference",
			GrossMass = 6000m,
			PackageQuantity = 30,
			ReferenceType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction,
			Comments = "Updating Packages and Gross Weight transaction"
		};
		provider.temporaryStorageRegisterTransactionData.Clear();
		provider.temporaryStorageRegisterTransactionData.Add(transactionData1);
		processor.CalculateTransactionsAndLockMutexIfNeededForRollingBackTransaction();
		processor.UnlockRegistersMutexes();

		CombineAssertions("First transaction data recalculation results", () =>
		{
			AssertEquals("New transaction Package Quantity should be 10+7 = 17.", 17, transactionData1.PackageQuantity);
			AssertEquals("New transaction GrossMass should be unsigned(2000+1400) = 3400.", 3400m, transactionData1.GrossMass);
		});
	}

	public void TestProcessor()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "HEADER";
		registerHeader.SRH_AppCode = "IST";

		var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine.SRL_LineNumber = 1;
		registerLine.SRL_OwnerReference = "MyReference";

		var dummyMessage = Factory.NewWithValidTestData<EDIMessage>();
		var provider = new ProviderForTest();

		provider.temporaryStorageRegisterTransactionData.Add(new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader,
			GrossMass = 1000m,
			PackageQuantity = -10,
			RegisterLineNo = 1,
		});

		provider.temporaryStorageRegisterTransactionData.Add(new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader,
			GrossMass = 2000m,
			PackageQuantity = -20,
			RegisterLineNo = 1,
		});

		var processor = new CusTempStorageRegisterProcessor<EDIMessage>(provider, new LoggingInformation());
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		processor.AddTransactionsWhenSaving(dummyMessage);
		processor.UnlockRegistersMutexes();
		Factory.Save();

		var query = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		_ = query.AddToFilter(CusTempStorageRegLineSchema.SRL_LineNumber, 1);
		var reloadedRegisterLine = Factory.Load<CusTempStorageRegLine>(query)[0];

		AssertEquals("Processor should have created 2 transactions against register line 1.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);
	}

	public void TestMutex()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "HEADER";
		registerHeader.SRH_AppCode = "IST";

		var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
		registerLine.SRL_LineNumber = 1;
		registerLine.SRL_OwnerReference = "MyReference";

		var dummyMessage = Factory.NewWithValidTestData<EDIMessage>();
		var provider = new ProviderForTest();
		provider.temporaryStorageRegisterTransactionData.Add(new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader,
			GrossMass = 1000m,
			PackageQuantity = -10,
			RegisterLineNo = 1,
		});

		provider.temporaryStorageRegisterTransactionData.Add(new TemporaryStorageRegisterTransactionData
		{
			PreviousRegisterHeader = registerHeader,
			GrossMass = 2000m,
			PackageQuantity = -20,
			RegisterLineNo = 1,
		});

		var logger = new LoggingInformation();
		var processor = new CusTempStorageRegisterProcessor<EDIMessage>(provider, logger);

		_ = registerHeader.LockMutex();
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		Factory.Save();

		var query = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		_ = query.AddToFilter(CusTempStorageRegLineSchema.SRL_LineNumber, 1);
		var reloadedRegisterLine = Factory.Load<CusTempStorageRegLine>(query)[0];
		CombineAssertions("Case of Register Mutex locked", () =>
		{
			Assert("Logs should show mutex was locked.", logger.Logs.Any(x => x.ToString().Contains("Cannot add the requested weight/package quantity for register HEADER; someone else is locking the Register.")));
		});

		logger.ClearLogs();
		processor.UnlockRegistersMutexes();
		processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
		Factory.Save();

		reloadedRegisterLine = Factory.Load<CusTempStorageRegLine>(query)[0];
		CombineAssertions("Case of Register Mutex unlocked", () =>
		{
			Assert("Logs should not show mutex was locked.", !logger.Logs.Any(x => x.ToString().Contains("Cannot add the requested weight/package quantity for register HEADER; someone else is locking the Register.")));
		});
		processor.UnlockRegistersMutexes();
	}

	public class ProviderForTest : ITemporaryStorageRegisterTransactionDataProvider
	{
		public ProviderForTest()
		{
			temporaryStorageRegisterTransactionData = new List<TemporaryStorageRegisterTransactionData>();
		}

		public IEnumerable<TemporaryStorageRegisterTransactionData> GetTemporaryStorageRegisterTransactionData() => temporaryStorageRegisterTransactionData;

		public List<TemporaryStorageRegisterTransactionData> temporaryStorageRegisterTransactionData;
	}
}
