using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class BatchAggregatorTest : TestCaseWithFactory
	{
		public static class GLFlags
		{
			public const string Y = "Y";
			public const string N = "N";
		}

		#region Job Revenue Journal Control Account

		public void TestTakeUpSubledgersFor_EmptyJobRevenueJournalControlAccount_WithoutRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, Guid.Empty);
			Factory.Save();

			AssertEquals("Precondition: No error raised during Aggregate", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_EmptyJobRevenueJournalControlAccount_WithRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, Guid.Empty);
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetJobRevenueJournal(TestHelper.TestDataSet);
			Factory.Save();

			AssertCorrectResultForInvalidControlAccount(AccountingUtils.JobRevenueJournalControlAccount, shouldFail: true);

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_ValidJobRevenueJournalControlAccount_WithoutRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			Factory.Save();

			AssertEquals("Precondition: No error raised during Aggregate", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_ValidJobRevenueJournalControlAccount_WithRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetJobRevenueJournal(TestHelper.TestDataSet);
			Factory.Save();

			AssertEquals("Precondition: No error raised during Aggregate", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Aggregation Succeed", true, aggregate.Length > 0);
		}

		public void TestTakeUpSubledgersFor_InvalidJobRevenueJournalControlAccount_WithoutRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, Guid.NewGuid());
			Factory.Save();

			AssertEquals("Precondition: No error raised during Aggregate", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_InvalidJobRevenueJournalControlAccount_WithRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, Guid.NewGuid());
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetJobRevenueJournal(TestHelper.TestDataSet);
			Factory.Save();

			AssertCorrectResultForInvalidControlAccount(AccountingUtils.JobRevenueJournalControlAccount, shouldFail: true);

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		#endregion

		#region CFX Account

		public void TestTakeUpSubledgersFor_EmptyCFXControlAccount_WithoutRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, Guid.Empty);
			Factory.Save();

			AssertEquals("Precondition: Empty Job Revenue Journal Control Account without transaction line is not a problem", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_EmptyCFXControlAccount_WithRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, Guid.Empty);
			var journal = TestObjectCreator.CreateJCJournalHeader(new ZDateTime(2003, 01, 15), 0m);
			var line1 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, new ZDateTime(2003, 01, 15), 50m);
			var line2 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, new ZDateTime(2003, 01, 15), 120m);
			Factory.Save();

			AssertCorrectResultForInvalidControlAccount(AccountingUtils.CFXAccount, shouldFail: true);

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_ValidCFXControlAccount_WithRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, TestHelper.CFXAccount);
			var journal = TestObjectCreator.CreateJCJournalHeader(new ZDateTime(2003, 01, 15), 0m);
			var line1 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, new ZDateTime(2003, 01, 15), 50m);
			var line2 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, new ZDateTime(2003, 01, 15), 120m);
			Factory.Save();

			AssertEquals("Precondition: Valid Job Revenue Journal Control Account without transaction line is not a problem", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Aggregation Succeed", true, aggregate.Length > 0);
		}

		public void TestTakeUpSubledgersFor_ValidCFXControlAccount_WithoutRecord()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, TestHelper.CFXAccount);
			Factory.Save();

			AssertEquals("Precondition: Valid Job Revenue Journal Control Account without transaction line is not a problem", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_InvalidCFXControlAccount_WithRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, Guid.NewGuid());

			var journal = TestObjectCreator.CreateJCJournalHeader(new ZDateTime(2003, 01, 15), 0m);
			var line1 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, new ZDateTime(2003, 01, 15), 50m);
			var line2 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, new ZDateTime(2003, 01, 15), 120m);
			Factory.Save();

			AssertCorrectResultForInvalidControlAccount(AccountingUtils.CFXAccount, shouldFail: true);

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		public void TestTakeUpSubledgersFor_InvalidCFXControlAccount_WithoutRecords()
		{
			ClearTables();
			TestAggregator.SetControlAccount(AccountingUtils.CFXAccount, Guid.NewGuid());
			Factory.Save();

			AssertEquals("Precondition: Invalid Job Revenue Journal Control Account without transaction line is not a problem", true, TestAggregator.Aggregate());

			var aggregate = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestHelper.JobRevenueJournalControlAccount));
			AssertEquals("Precondition: No record should be created.", 0, aggregate.Length);
		}

		#endregion

		public void TestUnknownErrorHandling()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			AssertAggregatorRaisesException(testAggregator, @"NO ACCOUNTING PERIODS ARE SPECIFIED FOR THE CURRENT COMPANY. PLEASE SET UP THE ACCOUNTING PERIODS.", "An unknown exception was caused and caught");

			ErrorReporter.Clear();
		}

		[TestDate(2014, 02, 28)]
		public void TestValidateTransactionPeriodErrorHandling()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupSinglePeriod(201401, new ZDateTime(2014, 1, 1, 0, 0, 0), new ZDateTime(2014, 1, 31, 23, 58, 0));
			periodHelper.SetupSinglePeriod(201402, new ZDateTime(2014, 2, 1, 0, 0, 0), new ZDateTime(2014, 2, 28, 23, 59, 0));

			ErrorReporter.Clear();

			AssertAggregatorRaisesException(testAggregator, @"THE FOLLOWING ACCOUNTING PERIOD HAS A GAP/OVERLAP FROM THE PREVIOUS PERIOD. PLEASE FIX THE ACCOUNTING PERIODS.", "An accounting period has a gap/overlap from the previous period");
			AssertEquals("No error reported.", 0, ErrorReporter.TotalErrorCount);

			testAggregator = new TestBatchAggregator(TestHelper);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			periodHelper.SetupSinglePeriod(201401, new ZDateTime(2014, 1, 1, 0, 0, 0), new ZDateTime(2014, 2, 1, 0, 0, 0));
			periodHelper.SetupSinglePeriod(201402, new ZDateTime(2014, 2, 1, 0, 0, 0), new ZDateTime(2014, 2, 28, 23, 59, 0));

			AssertAggregatorRaisesException(testAggregator, @"THE FOLLOWING ACCOUNTING PERIOD HAS A GAP/OVERLAP FROM THE PREVIOUS PERIOD. PLEASE FIX THE ACCOUNTING PERIODS.", "An accounting period has a gap/overlap from the previous period");
			AssertEquals("No error reported.", 0, ErrorReporter.TotalErrorCount);
		}

		[TestDate(2003, 04, 28)]
		public void TestAritmeticOverFlowExceptionHandling()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 522337203685400M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var chargeCodeToTest = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCodeToTest.AC_ChargeType = "OVR";
				FillChargeCode(chargeCodeToTest);
				chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;

				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = LedgerTypes.AccountsPayable;
				header.AH_TransactionType = TransactionTypes.Invoice;
				header.AH_TransactionNum = "123456";
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_PostDate = new ZDateTime(2003, 2, 10);

				var lineToTest = Factory.NewWithValidTestData<AccTransactionLines>();
				lineToTest.AL_LineType = TransactionLineTypes.Cost;
				lineToTest.AL_GE = TestHelper.Department1;
				lineToTest.AL_GB = TestHelper.Branch1;
				lineToTest.AL_ReverseDate = new ZDateTime(2003, 04, 28);
				lineToTest.AL_AH = header.PK;
				lineToTest.AL_AC = chargeCodeToTest.PK;
				lineToTest.AL_PostDate = new ZDateTime(2003, 04, 28);
				lineToTest.AL_ReverseToGL = "Y";
				lineToTest.AL_AG = chargeCodeToTest.AC_AG_CostAccount.ToGuid();
				lineToTest.AL_LineAmount = 522337203685400;

				var lineToTest2 = Factory.NewWithValidTestData<AccTransactionLines>();
				lineToTest2.AL_LineType = TransactionLineTypes.Cost;
				lineToTest2.AL_GE = TestHelper.Department1;
				lineToTest2.AL_GB = TestHelper.Branch1;
				lineToTest2.AL_ReverseDate = new ZDateTime(2003, 04, 28);
				lineToTest2.AL_AH = header.PK;
				lineToTest2.AL_AC = chargeCodeToTest.PK;
				lineToTest2.AL_PostDate = new ZDateTime(2003, 04, 28);
				lineToTest2.AL_ReverseToGL = "Y";
				lineToTest2.AL_AG = chargeCodeToTest.AC_AG_CostAccount.ToGuid();
				lineToTest2.AL_LineAmount = 422337203685478;

				Factory.Save();

				var testAggregator = new TestBatchAggregator(TestHelper);

				testAggregator.ExceptionOnCommit = BatchAggregatorTestSqlExceptionHelper.CreateArithmeticOverflowSqlException<SqlException>();
				AssertAggregatorRaisesException(testAggregator, @"An extremely large value transaction has been processed. Please find and reverse any transactions with a transaction or outstanding balances close to 999,999,999,999. ", "A arithmetic overflow exception was caused and caught");
				Assert("should be an arithmetic overflow exception",
					new SqlExceptionWrapper(testAggregator.LastException as System.Data.Common.DbException).Number
				 == new SqlExceptionWrapper(testAggregator.ExceptionOnCommit as System.Data.Common.DbException).Number);
			}
		}

		public void TestDeadlockExceptionHandling()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			AccTransactionLines lineToTest = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();

			var chargeCodeToTest = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeToTest.AC_ChargeType = "OVR";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = "123456";
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_PostDate = new ZDateTime(2003, 2, 10);

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			lineToTest.AL_GE = TestHelper.Department1;
			lineToTest.AL_GB = TestHelper.Branch1;
			lineToTest.AL_ReverseDate = ZDateTime.BrettsBirthday;
			lineToTest.AL_AH = header.PK;
			lineToTest.AL_AC = chargeCodeToTest.PK;
			lineToTest.AL_PostDate = new ZDateTime(2004, 2, 10);
			lineToTest.AL_ReverseToGL = "Y";
			lineToTest.AL_AG = chargeCodeToTest.AC_AG_CostAccount.ToGuid();

			Factory.Save();

			lineToTest.AL_PostDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_ReverseToGL = "Y";
			Factory.Save();
			testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.ExceptionOnCommit = BatchAggregatorTestSqlExceptionHelper.CreateDeadlockSqlException<SqlException>();
			AssertAggregatorRaisesException(testAggregator, @"Database deadlock occurred most due to other batch operation running simultaneously.", "A deadlock exception was caused and caught");
			Assert("should be a deadlock exception", testAggregator.LastException == testAggregator.ExceptionOnCommit);
		}

		public void TestValidationChargeCodes()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			AccChargeCode chargeCodeToTest = Factory.New(typeof(AccChargeCode)) as AccChargeCode;

			string expectedErrorMessage = "Charge codes incorrectly setup. Please check the GL Account Setup for these charges: Charge code not specified\r\nPlease retry aggregation once you have corrected this error.";

			chargeCodeToTest.AC_ChargeType = "MRG";
			chargeCodeToTest.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MRG and code is missing revenue account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MRG and Charge Code is missing WIP account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MRG and Charge Code is missing cost account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_AccrualAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MRG and Charge Code is missing accrual account");

			chargeCodeToTest.AC_ChargeType = "DSB";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is DSB and Charge Code is missing revenue account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is DSB and Charge Code is missing WIP account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is DSB and Charge Code is missing cost account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_AccrualAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is DSB and Charge Code is missing accrual account");

			chargeCodeToTest.AC_ChargeType = "MJA";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MJA and Charge Code is missing revenue account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MJA and Charge Code is missing WIP account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MJA and Charge Code is missing cost account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_AccrualAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is MJA and Charge Code is missing accrual account");

			chargeCodeToTest.AC_ChargeType = "NON";

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is NON and Charge Code is missing revenue account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is NON and Charge Code is missing cost account");

			chargeCodeToTest.AC_ChargeType = "REV";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is REV and Charge Code is missing revenue account");

			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is REV and Charge Code is missing revenue account");

			chargeCodeToTest.AC_ChargeType = "OVR";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is OVR and Charge Code is missing cost account");

			chargeCodeToTest.AC_ChargeType = "CMT";
			FillChargeCode(chargeCodeToTest);
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is CMT and Charge Code has all accounts  set");
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is CMT and Charge Code has all accounts set except Revenue");
			chargeCodeToTest.AC_AG_WIPAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is CMT and Charge Code has all accounts set except Revenue and WIP");
			chargeCodeToTest.AC_AG_CostAccount = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedErrorMessage, "Charge Code type is CMT and Charge Code has all accounts set except Revenue, WIP and Cost");
			chargeCodeToTest.AC_AG_AccrualAccount = ZGuid.Empty;
			Factory.Save();
			ExectuteAggregatorButExpectNoErrorsAsNothingIsInvalid(testAggregator, "Charge Code type is CMT and Charge Code has no accounts set");
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestValidationTransactionLineFields()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			AccTransactionLines lineToTest = Factory.New<AccTransactionLines>();
			AccTransactionHeader header = Factory.New<AccTransactionHeader>();

			AccChargeCode testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_ChargeType = "OVR";
			FillChargeCode(testChargeCode);
			testChargeCode.FillWithValidTestData();

			string expectedMessage = "The following transactions have been posted with lines that do not have a Charge or GL code:\r\nAP\tINV\t123456\r\nPlease contact support for assistance with this error.\r\nGL accounts were last taken up on";

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = "123456";
			header.AH_PostDate = new ZDateTime(2003, 2, 10);
			header.FillWithValidTestData();

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			lineToTest.AL_GE = TestHelper.Department1;
			lineToTest.AL_GB = TestHelper.Branch1;
			lineToTest.AL_PostDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_ReverseDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_AH = header.PK;
			lineToTest.FillWithValidTestData();

			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line AL_AC and AL_GB both is empty");

			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AG = testChargeCode.AC_AG_AccrualAccount;
			Factory.Save();
			testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.Aggregate();
			AssertNull("Expect No Errors As Nothing Is Invalid", testAggregator.AggregateResult);
		}

		[SuspendCriticalValidation]
		public void TestValidationCashVATPeriod()
		{
			var expectedMessage =
@"The following transactions have Cash VAT post dates for which accounting periods do not exist. Please create the appropriate periods.
AP	CRD	InvCrdAdjCST1	Mar 16 2002
Please retry aggregation once you have corrected this error.
GL accounts were last taken up on";

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
			testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);
			TestHelper.SetGSTCashBasis(true);

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			var line = TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, TestHelper.PostDate1, true, 0.9m);
			var cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 60, 6);
			cashVAT.YC_PostDate = profitRecognitionDate.AddYears(-1);
			Factory.Save();

			AssertAggregatorRaisesException(testAggregator, expectedMessage, "post date is fall in periods that are not defined");
		}

		public void TestValidationTransactionPeriod()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			AccTransactionLines lineToTest = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();

			var chargeCodeToTest = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeToTest.AC_ChargeType = "OVR";
			FillChargeCode(chargeCodeToTest);
			chargeCodeToTest.AC_AG_RevenueAccount = ZGuid.Empty;

			string expectedMessage = "The following transactions have post or reverse dates for which accounting periods do not exist. Please create the appropriate periods.\r\nAP\tINV\t123456\tFeb 10 2004\r\nPlease retry aggregation once you have corrected this error.\r\nGL accounts were last taken up on";

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = "123456";
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_PostDate = new ZDateTime(2003, 2, 10);

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			lineToTest.AL_GE = TestHelper.Department1;
			lineToTest.AL_GB = TestHelper.Branch1;
			lineToTest.AL_ReverseDate = ZDateTime.BrettsBirthday;
			lineToTest.AL_AH = header.PK;
			lineToTest.AL_AC = chargeCodeToTest.PK;
			lineToTest.AL_PostDate = new ZDateTime(2004, 2, 10);
			lineToTest.AL_ReverseToGL = "Y";
			lineToTest.AL_AG = chargeCodeToTest.AC_AG_CostAccount.ToGuid();

			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "post date is fall in periods that are not setup yet");

			lineToTest.AL_PostDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_ReverseToGL = "Y";
			Factory.Save();
			testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.Aggregate();
			AssertNull("Expect No Errors As Nothing Is Invalid", testAggregator.AggregateResult);
		}

		[SuspendCriticalValidation]
		public void TestValidationLineType()
		{
			var testAggregator = new TestBatchAggregator(TestHelper);

			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var expectedMessage = "Transaction line(s) have been posted to charge codes that are incorrectly setup. Please check the GL Account Setup for these charges:Charge code not specified\r\nPlease retry aggregation once you have corrected this error.";

			var lineToTest = Factory.New<AccTransactionLines>();
			lineToTest.AL_GE = TestHelper.Department1;
			lineToTest.AL_GB = TestHelper.Branch1;
			lineToTest.AL_LineType = TransactionLineTypes.Revenue;
			lineToTest.AL_AC = testChargeCode.PK;

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = "123456";
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			lineToTest.AL_AH = header.PK;

			header.AH_PostDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_PostDate = header.AH_PostDate;

			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;

			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is revenue and line's charge revenue account is empty");

			lineToTest.AL_LineType = TransactionLineTypes.WIP;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is WIP and line's charge WIP account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is WIP and line's GL  account is empty and no charge code");

			lineToTest.AL_LineType = TransactionLineTypes.Accrual;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is accrual and line's charge accrual account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is accrual and line's GL account is empty and no charge code");

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_CostAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = header.PK;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is cost and line's charge cost account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is cost and line's GL account is empty and not charge code");

			lineToTest.AL_LineType = TransactionLineTypes.Revenue;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = header.PK;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is revenue and line's charge revenue account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is revenue and line's GL account is empty and not charge code");

			SetChargeCodeEmpty(testChargeCode);
			testChargeCode.AC_ChargeType = "CMT";
			lineToTest.AL_AC = testChargeCode.PK;
			Factory.Save();
			ExectuteAggregatorButExpectNoErrorsAsNothingIsInvalid(testAggregator, "Line is revenue and line's charge code has CMT type");

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			Factory.Save();
			ExectuteAggregatorButExpectNoErrorsAsNothingIsInvalid(testAggregator, "Line is cost and line's charge code has CMT type");
		}

		[SuspendCriticalValidation]
		public void TestValidationLineType_ReverseToGL()
		{
			var testAggregator = new TestBatchAggregator(TestHelper);

			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var expectedMessage = "Transaction line(s) have been posted to charge codes that are incorrectly setup. Please check the GL Account Setup for these charges:Charge code not specified\r\nPlease retry aggregation once you have corrected this error.";

			var lineToTest = Factory.New<AccTransactionLines>();
			lineToTest.AL_GE = TestHelper.Department1;
			lineToTest.AL_GB = TestHelper.Branch1;
			lineToTest.AL_LineType = TransactionLineTypes.Revenue;
			lineToTest.AL_AC = testChargeCode.PK;

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = "123456";
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			lineToTest.AL_AH = header.PK;

			header.AH_PostDate = new ZDateTime(2003, 2, 10);
			lineToTest.AL_PostDate = header.AH_PostDate;
			lineToTest.AL_ReverseDate = lineToTest.AL_PostDate;

			header.AH_PostToGL = "Y";
			lineToTest.AL_PostToGL = "Y";

			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;

			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is revenue and line's charge revenue account is empty");

			lineToTest.AL_LineType = TransactionLineTypes.WIP;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is WIP and line's charge WIP account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is WIP and line's charge WIP account is empty");

			lineToTest.AL_LineType = TransactionLineTypes.Accrual;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is accrual and line's charge accrual account is empty");

			lineToTest.AL_AC = ZGuid.Empty;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is accrual and line's charge accrual account is empty");

			lineToTest.AL_LineType = TransactionLineTypes.Cost;
			FillChargeCode(testChargeCode);
			testChargeCode.AC_AG_CostAccount = ZGuid.Empty;
			lineToTest.AL_AC = testChargeCode.PK;
			lineToTest.AL_AH = header.PK;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line is cost and line's charge cost account is empty");

			SetChargeCodeEmpty(testChargeCode);
			testChargeCode.AC_ChargeType = "CMT";
			lineToTest.AL_AC = testChargeCode.PK;
			Factory.Save();
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "Line charge code has CMT type must be marked as Aggregated");

			lineToTest.AL_ReverseToGL = "Y";
			Factory.Save();
			ExectuteAggregatorButExpectNoErrorsAsNothingIsInvalid(testAggregator, "Line charge code has CMT type must be marked as Aggregated");
		}

		[TestDate(2003, 04, 28)]
		[SuspendCriticalValidation]
		public void TestBankAccountOnCashBookTransactionsValidation()
		{
			var testAggregator = new TestBatchAggregator(TestHelper);
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var transactions = new TransactionHeaderCollection(Factory);
			var apPayment = SetupCashBookTransaction<APPayment>(bankAccount, transactions);
			var arPayment = SetupCashBookTransaction<ARPayment>(bankAccount, transactions);
			var apReceipt = SetupCashBookTransaction<APReceipt>(bankAccount, transactions);
			var arReceipt = SetupCashBookTransaction<ARReceipt>(bankAccount, transactions);
			var directPayment = SetupCashBookTransaction<DirectPayment>(bankAccount, transactions);
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_OSExTaxAmount = 500m;
			DirectReceipt directReceipt = SetupCashBookTransaction<DirectReceipt>(bankAccount, transactions);
			directReceipt.Lines.AddNew(directReceipt.DependentTransactionLineType);
			directReceipt.Lines[0].AL_LineAmount = 500m;
			directReceipt.Lines[0].AL_OSAmount = 500m;

			foreach (TransactionHeader transaction in transactions)
			{
				transaction.AH_AB = ZGuid.Empty;
			}
			Factory.Save();

			testAggregator.Aggregate();
			ZString error = testAggregator.AggregateResult;
			AssertContains("Cash Book Transactions With No Bank Account Have Been Found:", error);
			foreach (TransactionHeader transaction in transactions)
			{
				AssertContains("\t" + transaction.AH_Ledger + "\t" + transaction.AH_TransactionType + "\t" + transaction.AH_TransactionNum, error);
			}

			// Fix transaction Bank Accounts
			foreach (TransactionHeader transaction in transactions)
			{
				transaction.AH_AB = bankAccount.PK;
			}
			Factory.Save();

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.Aggregate();
			AssertEquals("Should be no error", null, testAggregator2.AggregateResult);
		}

		T SetupCashBookTransaction<T>(AccBankAccount bankAccount, TransactionHeaderCollection collectionToAddTo) where T : TransactionHeader
		{
			T header = Factory.New<T>();
			header.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			header.AH_AB = bankAccount.PK;
			header.AH_OSExTaxAmount = 500m;
			collectionToAddTo.Add(header);
			return header;
		}

		void ExectuteAggregatorButExpectNoErrorsAsNothingIsInvalid(TestBatchAggregator testBatchAggregator, string reason)
		{
			testBatchAggregator.AggregateResult = null;
			testBatchAggregator.Aggregate();
			AssertNull($"Aggregator should have no exceptions because: {reason}", testBatchAggregator.AggregateResult);
		}

		void AssertAggregatorRaisesException(TestBatchAggregator testBatchAggregator, string expectedErrorMessage, string reason)
		{
			testBatchAggregator.AggregateResult = null;
			testBatchAggregator.Aggregate();
			AssertStartsWith($"Aggregator should have raised exception because: {reason}\r\nExpected:\r\n{expectedErrorMessage}\r\nActual:\r\n{testBatchAggregator.AggregateResult}", expectedErrorMessage, testBatchAggregator.AggregateResult);
		}

		protected void SetChargeCodeEmpty(AccChargeCode testChargeCode)
		{
			testChargeCode.AC_AG_RevenueAccount = ZGuid.Empty;
			testChargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			testChargeCode.AC_AG_CostAccount = ZGuid.Empty;
			testChargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
		}

		protected void FillChargeCode(AccChargeCode testChargeCode)
		{
			testChargeCode.AC_AG_RevenueAccount = TestHelper.GLHeaders[27].PK;
			testChargeCode.AC_AG_WIPAccount = TestHelper.GLHeaders[27].PK;
			testChargeCode.AC_AG_CostAccount = TestHelper.GLHeaders[27].PK;
			testChargeCode.AC_AG_AccrualAccount = TestHelper.GLHeaders[27].PK;
		}

		public void TestValidationControlAccounts()
		{
			TestAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			TestAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, testHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, testHelper.ACRControl);
			TestAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, TestHelper.ARSuspenseControlAccount);
			TestAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, TestHelper.APSuspenseControlAccount);
			TestAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			TestAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			TestAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
			TestAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

			Action<bool, bool> assertCorrectResultForInvalidControlAccount = (shouldGSTFail, shouldPendingGSTFail) =>
			{
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.ARControl);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.APControl);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.AccruedRevenue);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.AccruedCost);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.ARSuspenseControlAccount);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.APSuspenseControlAccount);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.GSTInput, shouldGSTFail);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.GSTOutput, shouldGSTFail);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.PendingGSTInput, shouldPendingGSTFail);
				AssertCorrectResultForInvalidControlAccount(AccountingUtils.PendingGSTOutput, shouldPendingGSTFail);
			};

			GlbCompany currentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, GlbCompany.CurrentCompany.GC_Code));
			currentCompany.GC_IsGSTRegistered = false;
			currentCompany.GC_IsGSTCashBasis = false;
			Factory.Save();
			TestAggregator.Aggregate();
			AssertNull("Precondition: Aggregator does not fail aggregation because all control accounts are correct", TestAggregator.AggregateResult);
			assertCorrectResultForInvalidControlAccount(false, false);

			currentCompany.GC_IsGSTRegistered = true;

			currentCompany.GC_IsGSTCashBasis = false;
			Factory.Save();
			TestAggregator.Aggregate();
			AssertNull("Precondition: Aggregator does not fail aggregation because all control accounts are correct", TestAggregator.AggregateResult);
			assertCorrectResultForInvalidControlAccount(true, false);

			currentCompany.GC_IsGSTCashBasis = true;
			Factory.Save();
			TestAggregator.Aggregate();
			AssertNull("Precondition: Aggregator does not fail aggregation because all control accounts are correct", TestAggregator.AggregateResult);
			assertCorrectResultForInvalidControlAccount(true, true);
		}

		public void TestValidationAppropriationAccounts()
		{
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestHelper.PendingGSTOut);
			Factory.Save();
			TestAggregator.Aggregate();
			AssertNull("Precondition: Aggregator does not fail aggregation because appropriation account is set up correctly", TestAggregator.AggregateResult);

			using (AccountingConfigurationRegistry.Instance.PLAppropriationAccount.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Factory.Save();
			}
			TestAggregator.Aggregate();

			AssertStartsWith("AggregateResult", "Please set up the Appropriation Account in the registry (Accounting > Framework > PL Appropriation Account)", TestAggregator.AggregateResult);
			TestAggregator.AggregateResult = null;
		}

		void AssertCorrectResultForInvalidControlAccount(string controlAccount, bool shouldFail = true)
		{
			Guid currentContorlAccountValue = TestAggregator.SetControlAccount(controlAccount, Guid.Empty);
			TestAggregator.Aggregate();
			if (shouldFail)
			{
				AssertStartsWith("AggregateResult", "Please set up the following Control Accounts in the registry", TestAggregator.AggregateResult);
				var readableAccountName = TestAggregator.GetNameOfControlAccountRegistrySetting(controlAccount);
				AssertContains(readableAccountName + " is not specified", readableAccountName, TestAggregator.AggregateResult);
				TestAggregator.AggregateResult = null;
			}
			else
			{
				AssertNull("Precondition: Aggregator does not fail aggregation because all control accounts are correct", TestAggregator.AggregateResult);
			}
			TestAggregator.SetControlAccount(controlAccount, currentContorlAccountValue);
		}

		[TestDate(2009, 11, 24)]
		public void TestAGGUserandTimeStored()
		{
			GlbStaff user = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code));
			AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, user.PK.ToGuid());
			AssertEquals("Precondition: last agg User has precondition value", user.PK, AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);

			AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2008, 1, 1, 0, 0, 0));
			AssertEquals("Precondition: Last AGG time is User has value " + new DateTime(2008, 1, 1, 0, 0, 0).ToLongDateString(), new DateTime(2008, 1, 1, 0, 0, 0), AccountingConfigurationRegistry.Instance.LastAggregationDate.Value);

			GlbStaff savedAgguser;

			TestBatchAggregator aggregator = new TestBatchAggregator(Db.Connection, false, TestHelper);
			aggregator.Aggregate();

			savedAgguser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value));

			AssertEquals("Last agg user should be current user", GlbStaff.CurrentUser.GS_Code, savedAgguser.GS_Code);
			Assert("Last aggregation time should be greateer than 01 jan 2008", AccountingConfigurationRegistry.Instance.LastAggregationDate.Value > new DateTime(2008, 1, 1, 0, 0, 0));

			AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, user.PK.ToGuid());
			AssertEquals("Precondition: last agg User has precondition value", user.PK, AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);
			AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2008, 1, 1, 0, 0, 0));
			AssertEquals("Precondition: Last AGG time is User has value " + new DateTime(2008, 1, 1, 0, 0, 0).ToLongDateString(), new DateTime(2008, 1, 1, 0, 0, 0), AccountingConfigurationRegistry.Instance.LastAggregationDate.Value);

			aggregator = new TestBatchAggregator(Db.Connection, true, TestHelper);
			aggregator.Aggregate();

			savedAgguser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value));
			AssertEquals("Last agg user should Have rolled back ", user.GS_Code, savedAgguser.GS_Code);
			AssertEquals("Last agg time should have rolled back ", new DateTime(2008, 1, 1, 0, 0, 0), AccountingConfigurationRegistry.Instance.LastAggregationDate.Value);

			ErrorReporter.Clear();
		}

		public void Testsp_getapplock()
		{
			// Transaction level 1
			BatchAggregator foregroundAggregator = new TestBatchAggregator(TestHelper);
			if (!foregroundAggregator.Aggregate())
			{
				Fail("Didn't aggregate! Fix test data");
			}

			using (DbConnection backgroundConnection = Db.NewExtraConnectionToMainDb())
			{
				backgroundConnection.BeginTransaction();
				try
				{
					BatchAggregator aggregator = new TestBatchAggregator(backgroundConnection, false, TestHelper);
					bool success = aggregator.Aggregate(); // lock should be held at this point

					Assert("Shouldn't have been able to run aggregator because another one is already running", !success);
					AssertEquals("Aggregate error should be that another user is already running the process",
							"GL Accounts for this Company are currently being taken up by user: " + Env.CurrentUser.FullName,
							aggregator.AggregateResult);
					ErrorReporter.Clear();
				}
				finally
				{
					backgroundConnection.RollbackTransaction();
					backgroundConnection.CloseConnection();
				}
			}
		}

		public void Testsp_getapplockIsCompanySpecific()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AssertNotNull("NonCurrentCompanyBranch", testObjectCreator.NonCurrentCompanyBranch);

			// Transaction level 1
			BatchAggregator foregroundAggregator = new TestBatchAggregator(TestHelper);
			if (!foregroundAggregator.Aggregate())
			{
				Fail("Didn't aggregate! Fix test data");
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, testObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (DbConnection backgroundConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.DatabaseName))
			{
				backgroundConnection.BeginTransaction();
				try
				{
					var secondTestHelper = new BatchTestHelper(Factory);

					secondTestHelper.UpdateChargeAccounts(); // TODO This part is slow. Need to Improve...

					AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();

					periodHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1, 0, 0, 0), new ZDateTime(2003, 1, 31, 23, 59, 0));
					periodHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1, 0, 0, 0), new ZDateTime(2003, 2, 28, 23, 59, 0));
					periodHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1, 0, 0, 0), new ZDateTime(2003, 3, 31, 23, 59, 0));
					periodHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1, 0, 0, 0), new ZDateTime(2003, 4, 30, 23, 59, 0));

					BatchAggregator aggregator = new TestBatchAggregator(backgroundConnection, false, secondTestHelper);
					bool success = aggregator.Aggregate();

					AssertEquals("Agregation did not succeed due to an incomplete data setup", false, success);
					AssertNotContains("Aggregate error should NOT be that another user is already running the process",
							"GL Accounts for this Company are currently being taken up",
							aggregator.AggregateResult);
					ErrorReporter.Clear();
				}
				finally
				{
					backgroundConnection.RollbackTransaction();
					backgroundConnection.CloseConnection();
				}
			}
		}

		protected BatchTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new BatchTestHelper(Factory)); }
		}
		BatchTestHelper testHelper;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;

		void ClearTables()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLAggregateSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
		}

		ZDateTime profitRecognitionDate;
		ZInt PostDate1Period;
		ZInt profitRecognitionDatePeriod;

		protected override void SetUp()
		{
			TestHelper.InsertBanks();

			TestHelper.UpdateChargeAccounts(); // TODO This part is slow. Need to Improve...

			var periodHelper = new AccountingPeriodTestHelper();

			periodHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1, 0, 0, 0), new ZDateTime(2003, 1, 31, 23, 59, 0));
			periodHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1, 0, 0, 0), new ZDateTime(2003, 2, 28, 23, 59, 0));
			periodHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1, 0, 0, 0), new ZDateTime(2003, 3, 31, 23, 59, 0));
			periodHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1, 0, 0, 0), new ZDateTime(2003, 4, 30, 23, 59, 0));

			TestHelper.SetControlAccounts();

			TestHelper.Branch1 = GlbBranch.CurrentBranch.PK.ToGuid();
			TestHelper.Department1 = GlbDepartment.CurrentDepartment.PK.ToGuid();
			TestHelper.PostDate1 = new ZDateTime(2003, 4, 15);
			profitRecognitionDate = testHelper.PostDate1.AddDays(-30);

			PostDate1Period = 200304;
			profitRecognitionDatePeriod = 200303;

			WIPLinePKs = new Guid[15];
			ACRLinePKs = new Guid[15];
		}

		[TestDate(2003, 02, 20)]
		public void TestGLHighWaterMarkIsSet()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TestHelper.GLHeaders[0].PK.ToGuid(), 100.0M, TestHelper.PostDate200302);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("AP Journal Row Count", 2, testAggregator.AllAggregatedData.Count);
		}

		public void TestGetTotalForGL()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetAPARJournal(TestHelper.TestDataSet, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[1].PK.ToGuid(), -100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -50.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -50.0M, TestHelper.PostDate200303);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestHelper.ARControlAccount);

			testAggregator.Aggregate();

			AccGLAggregateCollection aggregates = new AccGLAggregateCollection(Factory);
			aggregates.Load();

			decimal expectedResult = testAggregator.GetTotalForGL(TestHelper.GLHeaders[0].PK.ToGuid());
			AssertEquals(200M, expectedResult);

			expectedResult = testAggregator.GetTotalForGL(TestHelper.GLHeaders[0].PK.ToGuid(), 200302);
			AssertEquals(150M, expectedResult);

			expectedResult = testAggregator.GetTotalForGL(TestHelper.GLHeaders[0].PK.ToGuid(), 200303);
			AssertEquals(50M, expectedResult);
		}

		[TestDate(2003, 03, 15)]
		public void TestARJournal()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[1].PK.ToGuid(), -100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -50.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TestHelper.GLHeaders[0].PK.ToGuid(), -50.0M, TestHelper.PostDate200303);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("AR Journal Row Count", 5, testAggregator.AllAggregatedData.Count);

			int accountIndex1 = GetGLAccountIndex(TestHelper.GLHeaders[0].PK.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int accountIndex2 = GetGLAccountIndex(TestHelper.GLHeaders[1].PK.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int accountIndex3 = GetGLAccountIndex(TestHelper.GLHeaders[0].PK.ToGuid(), 200303, testAggregator.AllAggregatedData);
			int accountIndex4 = GetGLAccountIndex(TestHelper.ARControlAccount, 200302, testAggregator.AllAggregatedData);
			int accountIndex5 = GetGLAccountIndex(TestHelper.ARControlAccount, 200303, testAggregator.AllAggregatedData);

			AssertEquals("AR GL Account", TestHelper.GLHeaders[0].PK.ToString(), testAggregator.AllAggregatedData[accountIndex1].AA_AG.ToString());
			AssertEquals("AR GL Account", TestHelper.GLHeaders[1].PK.ToString(), testAggregator.AllAggregatedData[accountIndex2].AA_AG.ToString());
			AssertEquals("AR Account 1 Amount", 150.0M, testAggregator.AllAggregatedData[accountIndex1].AA_Amount);
			AssertEquals("AR Account 2 Amount", 100.0M, testAggregator.AllAggregatedData[accountIndex2].AA_Amount);
			AssertEquals("AR Account 3 Amount", 50.0M, testAggregator.AllAggregatedData[accountIndex3].AA_Amount);

			AssertEquals("AR Control GL Account", TestHelper.ARControlAccount.ToString(), testAggregator.AllAggregatedData[accountIndex4].AA_AG.ToString());
			AssertEquals("AR Control Account Amount", -250.0M, testAggregator.AllAggregatedData[accountIndex4].AA_Amount);
			AssertEquals("AR Control GL Account", TestHelper.ARControlAccount.ToString(), testAggregator.AllAggregatedData[accountIndex5].AA_AG.ToString());
			AssertEquals("AR Control Account Amount", -50.0M, testAggregator.AllAggregatedData[accountIndex5].AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		protected int GetGLAccountIndex(Guid pK, int period, AccGLAggregateCollection dataSetToSearch)
		{
			for (int i = 0; i < dataSetToSearch.Count; i++)
			{
				if (dataSetToSearch[i].AA_AG == pK && dataSetToSearch[i].AA_Period == period)
				{
					return i;
				}
			}
			return -1;
		}

		[TestDate(2003, 03, 15)]
		public void TestAPJournal()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TestHelper.GLHeaders[0].PK.ToGuid(), 100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TestHelper.GLHeaders[1].PK.ToGuid(), 100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TestHelper.GLHeaders[0].PK.ToGuid(), 50.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARJournal(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TestHelper.GLHeaders[1].PK.ToGuid(), 100.0M, TestHelper.PostDate200303);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			int accountIndex1 = GetGLAccountIndex(TestHelper.GLHeaders[0].PK.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int accountIndex2 = GetGLAccountIndex(TestHelper.GLHeaders[1].PK.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int accountIndex3 = GetGLAccountIndex(TestHelper.GLHeaders[1].PK.ToGuid(), 200303, testAggregator.AllAggregatedData);
			int accountIndex4 = GetGLAccountIndex(TestHelper.APControlAccount, 200302, testAggregator.AllAggregatedData);
			int accountIndex5 = GetGLAccountIndex(TestHelper.APControlAccount, 200303, testAggregator.AllAggregatedData);

			AssertEquals("AP Journal Row Count", 5, testAggregator.AllAggregatedData.Count);
			AssertEquals("AP GL Account", TestHelper.GLHeaders[0].PK.ToString(), testAggregator.AllAggregatedData[accountIndex1].AA_AG.ToString());
			AssertEquals("AP GL Account", TestHelper.GLHeaders[1].PK.ToString(), testAggregator.AllAggregatedData[accountIndex2].AA_AG.ToString());
			AssertEquals("AP Account 1 Amount", -150.0M, testAggregator.AllAggregatedData[accountIndex1].AA_Amount);
			AssertEquals("AP Account 2 Amount", -100.0M, testAggregator.AllAggregatedData[accountIndex2].AA_Amount);
			AssertEquals("AP Account 3 Amount", -100.0M, testAggregator.AllAggregatedData[accountIndex3].AA_Amount);

			AssertEquals("AP Control 1 GL Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[accountIndex4].AA_AG.ToString());
			AssertEquals("AP Control 1 Account Amount", 250.0M, testAggregator.AllAggregatedData[accountIndex4].AA_Amount);
			AssertEquals("AP Control Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[accountIndex5].AA_AG.ToString());
			AssertEquals("AP Control Account Amount", 100.0M, testAggregator.AllAggregatedData[accountIndex5].AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestARAPContra()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, 100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, -70.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, 50.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, 50.0M, TestHelper.PostDate200303);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, -100.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, 70.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, -50.0M, TestHelper.PostDate200302);
			TestHelper.SetAPARContra(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, -50.0M, TestHelper.PostDate200303);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("AR AP Control Row Count", 4, testAggregator.AllAggregatedData.Count);

			AssertEquals("AP Control 1 Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[0].AA_AG.ToString());
			AssertEquals("AP Control 1 Amount", -80.0M, testAggregator.AllAggregatedData[0].AA_Amount);
			AssertEquals("AP Control 1 Period", 200302, testAggregator.AllAggregatedData[0].AA_Period);

			AssertEquals("AR Control 1 Account", TestHelper.ARControlAccount.ToString(), testAggregator.AllAggregatedData[1].AA_AG.ToString());
			AssertEquals("AR Control 1 Amount", 80.0M, testAggregator.AllAggregatedData[1].AA_Amount);
			AssertEquals("AR Control 1 Period", 200302, testAggregator.AllAggregatedData[1].AA_Period);

			AssertEquals("AP Control 2 Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[2].AA_AG.ToString());
			AssertEquals("AP Control 2 Amount", -50.0M, testAggregator.AllAggregatedData[2].AA_Amount);
			AssertEquals("AP Control 2 Period", 200303, testAggregator.AllAggregatedData[2].AA_Period);

			AssertEquals("AR Control 2 Account", TestHelper.ARControlAccount.ToString(), testAggregator.AllAggregatedData[3].AA_AG.ToString());
			AssertEquals("AR Control 2 Amount", 50.0M, testAggregator.AllAggregatedData[3].AA_Amount);
			AssertEquals("AR Control 2 Period", 200303, testAggregator.AllAggregatedData[3].AA_Period);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestExchangeDifferences()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference, -50.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference, 100.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference, 100.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[0].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[1].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[2].AH_AG = TestHelper.ExchangeDifference;

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();
			testAggregator.RunAggregateForTest();

			AssertEquals("Exchange Difference Row Count", 3, testAggregator.AllAggregatedData.Count);
			AssertEquals("Exchange Difference Account", TestHelper.ExchangeDifference.ToString(), testAggregator.AllAggregatedData[1].AA_AG.ToString());
			AssertEquals("Exchange Difference Amount", -150.0M, testAggregator.AllAggregatedData[1].AA_Amount);

			AssertEquals("AR Control Account", TestHelper.ARControlAccount.ToString(), testAggregator.AllAggregatedData[2].AA_AG.ToString());
			AssertEquals("AR Control Amount", 50.0M, testAggregator.AllAggregatedData[2].AA_Amount);
			AssertEquals("AP Control Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[0].AA_AG.ToString());
			AssertEquals("AP Control Amount", 100.0M, testAggregator.AllAggregatedData[0].AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestCashBookExchangeDifferences()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.CashBook, TransactionTypes.ExchangeDifference, -50.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[0].AH_AB = TestHelper.Banks[0].PK.ToGuid();
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.CashBook, TransactionTypes.ExchangeDifference, 100.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[1].AH_AB = TestHelper.Banks[0].PK.ToGuid();
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.CashBook, TransactionTypes.ExchangeDifference, 80.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[2].AH_AB = TestHelper.Banks[1].PK.ToGuid();
			TestHelper.TestDataSet[0].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[1].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[2].AH_AG = TestHelper.ExchangeDifference;
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.RunAggregateForTest();

			AssertEquals("Exchange Difference Row Count", 3, testAggregator.AllAggregatedData.Count);

			int bankAccountIndex1 = GetGLAccountIndex(TestHelper.Banks[0].AB_AG.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int bankAccountIndex2 = GetGLAccountIndex(TestHelper.Banks[1].AB_AG.ToGuid(), 200302, testAggregator.AllAggregatedData);
			int exxDiffIndex = GetGLAccountIndex(TestHelper.ExchangeDifference, 200302, testAggregator.AllAggregatedData);

			AssertEquals("Bank Account 1 Account", TestHelper.Banks[0].AB_AG, testAggregator.AllAggregatedData[bankAccountIndex1].AA_AG);
			AssertEquals("Bank Account 1 Amount", 50.0M, testAggregator.AllAggregatedData[bankAccountIndex1].AA_Amount);
			AssertEquals("Bank Account 2 Account", TestHelper.Banks[1].AB_AG, testAggregator.AllAggregatedData[bankAccountIndex2].AA_AG);
			AssertEquals("Bank Account 2 Amount", 80.0M, testAggregator.AllAggregatedData[bankAccountIndex2].AA_Amount);
			AssertEquals("Exchange Control Account", TestHelper.ExchangeDifference, testAggregator.AllAggregatedData[exxDiffIndex].AA_AG);
			AssertEquals("Exchange Control Account Amount", -130.0M, testAggregator.AllAggregatedData[exxDiffIndex].AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestOverpayment()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment, -50.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment, 100.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Overpayment, -40.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Overpayment, 80.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[0].AH_AG = TestHelper.Overpayment;
			TestHelper.TestDataSet[1].AH_AG = TestHelper.Overpayment;
			TestHelper.TestDataSet[2].AH_AG = TestHelper.Overpayment;
			TestHelper.TestDataSet[3].AH_AG = TestHelper.Overpayment;

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();

			testAggregator.RunAggregateForTest();

			AssertEquals("Overpayment Row Count", 3, testAggregator.AllAggregatedData.Count);
			AssertEquals("Overpayment Difference Amount", -90.0M, testAggregator.GetAggregateRow(TestHelper.Overpayment).AA_Amount);
			AssertEquals("AR Control Amount", 50.0M, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);
			AssertEquals("AP Control Amount", 40.0M, testAggregator.GetAggregateRow(TestHelper.APControlAccount).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();

			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestDiscount()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Discount, -50.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Discount, 100.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Discount, -40.0M, TestHelper.PostDate200302);
			TestHelper.SetHeaderOnlyLines(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Discount, 80.0M, TestHelper.PostDate200302);
			TestHelper.TestDataSet[0].AH_AG = TestHelper.Discount;
			TestHelper.TestDataSet[1].AH_AG = TestHelper.Discount;
			TestHelper.TestDataSet[2].AH_AG = TestHelper.Discount;
			TestHelper.TestDataSet[3].AH_AG = TestHelper.Discount;

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();
			testAggregator.RunAggregateForTest();

			AssertEquals("Discount Difference Row Count", 3, testAggregator.AllAggregatedData.Count);
			AssertEquals("AccGLAggregate row should exist for APControl account and amount should be as expected", 40.0M, testAggregator.GetAggregateRow(TestHelper.APControlAccount).AA_Amount);
			AssertEquals("AccGLAggregate row should exist for Dicount control account and amount should be as expected", -90.0M, testAggregator.GetAggregateRow(TestHelper.Discount).AA_Amount);
			AssertEquals("AccGLAggregate row should exist for ARControl account and amount should be as expected", 50.0M, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			Factory.Save();
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestDirectReceipt()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetDirectTransaction(TransactionTypes.DirectReceipt, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), 50, 5);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectReceipt, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), 100, 0);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectReceipt, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), 40, 4);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectReceipt, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), 80, 0);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator.RunAggregateForTest();

			AssertEquals("Direct Receipt Line Row Count", 5, testAggregator.AllAggregatedData.Count);
			AssertEquals("Direct Receipt Line Account", TestHelper.GLHeaders[4].PK.ToGuid(), testAggregator.GetAggregateRow(TestHelper.GLHeaders[4].PK.ToGuid()).AA_AG);
			AssertEquals("Direct Receipt Line Account", -90.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[4].PK.ToGuid()).AA_Amount);
			AssertEquals("Direct Receipt Line Account", -180.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[6].PK.ToGuid()).AA_Amount);
			AssertEquals("Direct Receipt Line Account", 155.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Direct Receipt Line Account", 124.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Direct Receipt Line Account", -9.0m, testAggregator.GetAggregateRow(TestHelper.GSTOut).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestDirectPayment()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), -50, -5);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), -100, 0);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), -40, -4);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), -80, 0);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.RunAggregateForTest();

			AssertEquals("Direct Payment Line Row Count", 5, testAggregator.AllAggregatedData.Count);
			AssertEquals("Direct Payment Line Account", TestHelper.GLHeaders[4].PK.ToGuid(), testAggregator.GetAggregateRow(TestHelper.GLHeaders[4].PK.ToGuid()).AA_AG);
			AssertEquals("Direct Payment Line Account", 90.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[4].PK).AA_Amount);
			AssertEquals("Direct Payment Line Account", 180.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[6].PK).AA_Amount);
			AssertEquals("Direct Payment Line Account", -155.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Direct Payment Line Account", -124.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Direct Payment Line Account", 9.0m, testAggregator.GetAggregateRow(TestHelper.GSTIn).AA_Amount);

			var expected =
@"200304 1010.10.20 FREIGHT REVENUE ACCRUED    90.00
200304 1010.20.10 FREIGHT COSTS ACTUAL   180.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL     9.00
200304 1050.20.20 CONTAINER COSTS ACCRUED  -155.00
200304 1060.00.00 GROSS HANDLING REVENUE  -124.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		public void TestDirectPaymentWithVatRecoverablePercentageOverridden()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), -50, -5, 0.9m);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[0].PK.ToGuid(), -100, 0);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[4].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), -40, -4, 0.9m);
			TestHelper.SetDirectTransaction(TransactionTypes.DirectPayment, TestHelper.TestDataSet, TestHelper.GLHeaders[6].PK.ToGuid(), TestHelper.Banks[1].PK.ToGuid(), -80, 0);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.RunAggregateForTest();

			AssertEquals("Direct Payment Line Row Count", 6, testAggregator.AllAggregatedData.Count);
			AssertEquals("Direct Payment Line Account", TestHelper.GLHeaders[4].PK.ToGuid(), testAggregator.GetAggregateRow(TestHelper.GLHeaders[4].PK.ToGuid()).AA_AG);
			AssertEquals("Direct Payment Line Account", 90.90m, testAggregator.GetTotalForGL(TestHelper.GLHeaders[4].PK));
			AssertEquals("Direct Payment Line Account", 180.00m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[6].PK).AA_Amount);
			AssertEquals("Direct Payment Line Account", -155.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Direct Payment Line Account", -124.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Direct Payment Line Account", 8.1m, testAggregator.GetAggregateRow(TestHelper.GSTIn).AA_Amount);

			var expected =
@"200304 1010.10.20 FREIGHT REVENUE ACCRUED     0.90
200304 1010.10.20 FREIGHT REVENUE ACCRUED    90.00
200304 1010.20.10 FREIGHT COSTS ACTUAL   180.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL     8.10
200304 1050.20.20 CONTAINER COSTS ACCRUED  -155.00
200304 1060.00.00 GROSS HANDLING REVENUE  -124.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjREV()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentREV(TestHelper.TestDataSet);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

			AssertEquals("AR Control Amount", -110.0M, testAggregator.GetTotalForGL(TestHelper.ARControlAccount, PostDate1Period));
			AssertEquals("Revenue GST Out Amount", 10.0M, testAggregator.GetTotalForGL(TestHelper.GSTOut, PostDate1Period));
			AssertEquals("AR Suspense Control Account", 0.0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));
			AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetAggregateRow(TestHelper.ReveneueAccount).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjREVProfitRecognitionDate()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentREV(TestHelper.TestDataSet, profitRecognitionDate);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, testHelper.APSuspenseControlAccount);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

			AssertEquals("AR Control Amount", -110.0M, testAggregator.GetTotalForGL(testHelper.ARControlAccount, PostDate1Period));
			AssertEquals("AR Suspense Control Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));
			AssertEquals("Revenue GST Out Amount", 10.0M, testAggregator.GetTotalForGL(testHelper.GSTOut, PostDate1Period));

			AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
			AssertEquals("AR Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator2.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APSuspenseControlAccount, testHelper.APSuspenseControlAccount);

			TestHelper.SetGSTCashBasis(false);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjREVProfitRecognitionDateWithPendingGST()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				TestHelper.SetInvoiceCreditAdjustmentREV(TestHelper.TestDataSet, profitRecognitionDate, true);
				Factory.Save();

				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
				AssertEquals("AR Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));
				AssertEquals("AR Control Amount", -110.0M, testAggregator.GetTotalForGL(testHelper.ARControlAccount, PostDate1Period));
				AssertEquals("AR Suspense Control Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));
				AssertEquals("Pending Revenue GST Out Amount", 10.0M, testAggregator.GetTotalForGL(testHelper.PendingGSTOut, PostDate1Period));

				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
				AssertEquals("AR Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));

				var testAggregator2 = new TestBatchAggregator(TestHelper);
				testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator2.RunAggregateForTest();

				AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
				Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjREVProfitRecognitionDateWithPendingGST_ForTransactionWithLastEditDateWellInThePast()
		{
			AssertInvCrdAdjREVProfitRecognitionDateWithPendingGST_ForTransactionWithLastEditDateWellInThePast(true);
		}

		[TestDate(2003, 03, 15)]
		public void TestTakeUpSubledgersWithPendingGSTCashBasisInvoiceAndCompanyIsNotGSTCashBasis()
		{
			AssertInvCrdAdjREVProfitRecognitionDateWithPendingGST_ForTransactionWithLastEditDateWellInThePast(false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestTakeUpSubledgersRepeatedlyCreatesNoSQLPlansPolutionForTemporaryTables()
		{
			ClearTables();

			using (DbConnection connection = Db.NewAdminConnection())
			{
				//Clear the cached SQL plans first
				string clearCacheSQL = "DBCC FREEPROCCACHE";
				connection.ExecuteNonQuery(clearCacheSQL);

				string sql = @"select substring(text, 1, 100), count(distinct value) Plans from sys.dm_Exec_query_Stats
cross apply sys.dm_exec_plan_attributes(plan_handle) epa
cross apply sys.dm_Exec_sql_text(plan_handle)
where attribute = 'optional_spid' and value <> 0
group by substring(text, 1, 100)
order by 1"
;				
				for (int i = 0; i < 10; i++)
				{
					using (var newConnection = Db.NewExtraConnectionToMainDb())
					{
						TestBatchAggregator testAggregator = new TestBatchAggregator(newConnection, testHelper);
						Guid id = testAggregator.SetControlAccount(AccountingUtils.CFXAccount, TestHelper.CFXAccount);
						var journal = TestObjectCreator.CreateJCJournalHeader(new ZDateTime(2003, 01, 15), 0m);
						var line1 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, new ZDateTime(2003, 01, 15), 50m);
						var line2 = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, new ZDateTime(2003, 01, 15), 120m);
						Factory.Save();

						TestAggregator.Aggregate();
					}
					var dataSet = DataUtils.GetDataSetFromQuery(connection, sql);
					Assert("The number of SQL Server cached plans with 'optional_spid' attribute should be zero", dataSet.Tables[0].Rows.Count == 0);
				}
			}
		}

		void AssertInvCrdAdjREVProfitRecognitionDateWithPendingGST_ForTransactionWithLastEditDateWellInThePast(bool companyGSTCashBasis)
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				var line = TestHelper.SetInvoiceCreditAdjustmentREV(TestHelper.TestDataSet, profitRecognitionDate, true);
				Factory.Save();

				TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_SystemLastEditTimeUtc = '{0}', AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{1}'", "2003-02-15 00:00:00", line.AL_AH));

				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(companyGSTCashBasis);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
				AssertEquals("AR Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));
				AssertEquals("AR Control Account", -110.0M, testAggregator.GetTotalForGL(testHelper.ARControlAccount, PostDate1Period));
				AssertEquals("AR Suspense Control Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));
				AssertEquals("Pending GST Out", 10.0M, testAggregator.GetTotalForGL(testHelper.PendingGSTOut, PostDate1Period));

				var testAggregator2 = new TestBatchAggregator(TestHelper);
				testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
				Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTProfitRecognitionDate()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate, true, 0.9m);
				Factory.Save();

				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(testHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));
				AssertEquals("Pending Revenue GST In Amount", -10.0M, testAggregator.GetTotalForGL(testHelper.PendingGSTIn, PostDate1Period));

				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.CostAccounts, profitRecognitionDatePeriod));
				AssertEquals("AP Suspense Control Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

				var expected =
	@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTProfitRecognitionDateWithPendingGSTAndVatRecoverablePercentageOverridden()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate, true, 0.9m);
				Factory.Save();

				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(testHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));
				AssertEquals("Pending Revenue GST In Amount", -10.0M, testAggregator.GetTotalForGL(testHelper.PendingGSTIn, PostDate1Period));

				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", -100.0M, testAggregator.GetTotalForGL(testHelper.CostAccounts, profitRecognitionDatePeriod));
				AssertEquals("AP Suspense Control Amount", 100.0M, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

				var expected =
	@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjREVWithPendingAndRecognisedGST()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
				testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);
				TestHelper.SetGSTCashBasis(true);

				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				var line = TestHelper.SetInvoiceCreditAdjustmentREV(TestHelper.TestDataSet, TestHelper.PostDate1, true);
				var cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, -60, -6);
				cashVAT.YC_PostDate = profitRecognitionDate;
				Factory.Save();

				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 7, testAggregator.AllAggregatedData.Count);

				AssertEquals("AR Control Amount", -110.0M, testAggregator.GetTotalForGL(TestHelper.ARControlAccount, PostDate1Period));
				AssertEquals("AR Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(TestHelper.ReveneueAccount, PostDate1Period));

				AssertEquals("Pending Revenue GST Out Amount", 10.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTOut, PostDate1Period));
				AssertEquals("Pending Revenue GST Out Amount", -6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTOut, profitRecognitionDatePeriod));
				AssertEquals("Revenue GST Out Amount", 6M, testAggregator.GetTotalForGL(TestHelper.GSTOut, profitRecognitionDatePeriod));

				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 60, 6);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, -50, -5);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				Factory.Save();

				testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 9, testAggregator.AllAggregatedData.Count);

				AssertEquals("AR Control Amount", -110.0M, testAggregator.GetTotalForGL(TestHelper.ARControlAccount, PostDate1Period));
				AssertEquals("AR Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetTotalForGL(TestHelper.ReveneueAccount, PostDate1Period));

				AssertEquals("Pending Revenue GST Out Amount", 11.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTOut, PostDate1Period)); //2 records in this total
				AssertEquals("Pending Revenue GST Out Amount", -6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTOut, profitRecognitionDatePeriod));
				AssertEquals("Revenue GST Out Amount", 6M, testAggregator.GetTotalForGL(TestHelper.GSTOut, profitRecognitionDatePeriod));
				AssertEquals("Revenue GST Out Amount", -1M, testAggregator.GetTotalForGL(TestHelper.GSTOut, PostDate1Period));
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjREVWithoutChargeCode()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentREVWithoutChargeCode(TestHelper.TestDataSet);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 5, testAggregator.AllAggregatedData.Count);

			AssertEquals("Invoice Credit & Adjustment Revenue Line Amount", 100.0M, testAggregator.GetAggregateRow(TestHelper.GLHeaders[0].PK).AA_Amount);
			AssertEquals("AR Control Amount", -110.0M, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);
			AssertEquals("Revenue GST Out Amount", 10.0M, testAggregator.GetAggregateRow(TestHelper.GSTOut).AA_Amount);
			AssertEquals("AR Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjCST()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 5, testAggregator.AllAggregatedData.Count);

			AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
			AssertEquals("COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));
			AssertEquals("AP Suspense Control Account", -100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

			AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, profitRecognitionDatePeriod));
			AssertEquals("AP Suspense Control Account", 100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

			var expected =
@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTWithVatRecoverablePercentageOverridden()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate, inputGSTVATRecoverable: 0.9m);

			Factory.Save();

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 6, testAggregator.AllAggregatedData.Count);

			AssertEquals("AP Control Amount", 110.00M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
			AssertEquals("COST GST In Amount", -9.00M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));
			AssertEquals("Invoice Credit & Adjustment COST Line Amount", -1.00M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, PostDate1Period));
			AssertEquals("AP Suspense Control Account", -100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

			AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, profitRecognitionDatePeriod));
			AssertEquals("AP Suspense Control Account", 100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

			var expected =
@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL    -9.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00
200304 1050.10.20 CONTAINER REVENUE ACCRUED    -1.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjCSTWithPendingGST()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate, true);
				Factory.Save();

				var testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("Pending COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period));
				AssertEquals("AP Suspense Control Account", -100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, profitRecognitionDatePeriod));
				AssertEquals("AP Suspense Control Account", 100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

				var expected =
	@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());

				var testAggregator2 = new TestBatchAggregator(TestHelper);
				testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator2.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator2.RunAggregateForTest();

				AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
				Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTWithPendingGSTAndVatRecoverablePercentageOverridden()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, profitRecognitionDate, true, 0.9m);
				Factory.Save();

				TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);

				TestHelper.SetGSTCashBasis(true);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 5, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("Pending COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period));
				AssertEquals("AP Suspense Control Account", -100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, profitRecognitionDatePeriod));
				AssertEquals("AP Suspense Control Account", 100.0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, profitRecognitionDatePeriod));

				var expected =
	@"200303 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTWithPendingAndRecognisedGST()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
				testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);
				TestHelper.SetGSTCashBasis(true);

				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				var line = TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, TestHelper.PostDate1, true);
				var cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 60, 6);
				cashVAT.YC_PostDate = profitRecognitionDate;
				Factory.Save();

				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 7, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, PostDate1Period));

				AssertEquals("Pending COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period));
				AssertEquals("Pending COST GST In Amount", 6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", -6M, testAggregator.GetTotalForGL(TestHelper.GSTIn, profitRecognitionDatePeriod));

				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, -60, -6);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 50, 5);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				Factory.Save();

				testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 9, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, PostDate1Period));

				AssertEquals("Pending COST GST In Amount", -11.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period)); //2 records in this total
				AssertEquals("Pending COST GST In Amount", 6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", -6M, testAggregator.GetTotalForGL(TestHelper.GSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", 1M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));

				var expected =
	@"200303 1030.20.10 PORT & TERMINAL COSTS ACTUAL    -6.00
200303 1040.10.10 DOCUMENTATION REVENUE ACTUAL     6.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL     1.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL    -1.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00
200304 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTWithPendingAndRecognisedGSTAndVatRecoverablePercentageOverridden()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTCashBasis;

			try
			{
				TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
				testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
				testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTInput, TestHelper.PendingGSTIn);
				testAggregator.SetControlAccount(AccountingUtils.PendingGSTOutput, TestHelper.PendingGSTOut);
				TestHelper.SetGSTCashBasis(true);

				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
				var line = TestHelper.SetInvoiceCreditAdjustmentCST(TestHelper.TestDataSet, TestHelper.PostDate1, true, 0.9m);
				var cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 60, 6);
				cashVAT.YC_PostDate = profitRecognitionDate;
				Factory.Save();

				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 8, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, PostDate1Period));

				AssertEquals("Pending COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period));
				AssertEquals("Pending COST GST In Amount", 6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", -5.4M, testAggregator.GetTotalForGL(TestHelper.GSTIn, profitRecognitionDatePeriod));

				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, -60, -6);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				cashVAT = TestHelper.TestObjectCreator.CreateCashBasisVAT(line, 50, 5);
				cashVAT.YC_PostDate = TestHelper.PostDate1;
				Factory.Save();

				testAggregator = new TestBatchAggregator(TestHelper);
				testAggregator.RunAggregateForTest();

				AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 11, testAggregator.AllAggregatedData.Count);

				AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
				AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period)); //2 records in this total
				AssertEquals("Invoice Credit & Adjustment COST Line Amount", -99.9M, testAggregator.GetTotalForGL(TestHelper.CostAccounts, PostDate1Period));

				AssertEquals("Pending COST GST In Amount", -11.0M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, PostDate1Period)); //2 records in this total
				AssertEquals("Pending COST GST In Amount", 6M, testAggregator.GetTotalForGL(TestHelper.PendingGSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", -5.4M, testAggregator.GetTotalForGL(TestHelper.GSTIn, profitRecognitionDatePeriod));
				AssertEquals("COST GST In Amount", 0.9M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));

				var expected =
	@"200303 1030.20.10 PORT & TERMINAL COSTS ACTUAL    -5.40
200303 1040.10.10 DOCUMENTATION REVENUE ACTUAL     6.00
200303 1050.10.20 CONTAINER REVENUE ACCRUED    -0.60
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL     0.90
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL    -1.00
200304 1040.10.10 DOCUMENTATION REVENUE ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00
200304 1050.10.20 CONTAINER REVENUE ACCRUED     0.10
200304 1050.10.20 CONTAINER REVENUE ACCRUED  -100.00";
				AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
			}
			finally
			{
				TestHelper.SetGSTCashBasis(prevValue);
			}
		}

		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjCSTWithoutChargeCode()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentCSTWithoutChargeCode(TestHelper.TestDataSet);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 5, testAggregator.AllAggregatedData.Count);

			AssertEquals("Invoice Credit & Adjustment COST Line Amount", -100.0M, testAggregator.GetTotalForGL(TestHelper.GLHeaders[1].PK, PostDate1Period));
			AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
			AssertEquals("COST GST In Amount", -10.0M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));

			AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

			var expected =
@"200304 1010.00.00 GROSS FREIGHT REVENUE  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL   -10.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 5, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[SuspendCriticalValidation]
		public void TestInvCrdAdjCSTWithoutChargeCodeAndVatRecoverablePercentageOverridden()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetInvoiceCreditAdjustmentCSTWithoutChargeCode(TestHelper.TestDataSet, 0.9m);
			Factory.Save();

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);

			TestHelper.SetGSTCashBasis(false);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment COST Line Row Count", 6, testAggregator.AllAggregatedData.Count);

			AssertEquals("Invoice Credit & Adjustment COST Line Amount", -101.0M, testAggregator.GetTotalForGL(TestHelper.GLHeaders[1].PK, PostDate1Period));
			AssertEquals("AP Control Amount", 110.0M, testAggregator.GetTotalForGL(TestHelper.APControlAccount, PostDate1Period));
			AssertEquals("COST GST In Amount", -9.0M, testAggregator.GetTotalForGL(TestHelper.GSTIn, PostDate1Period));

			AssertEquals("AP Suspense Control Account", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));

			var expected =
@"200304 1010.00.00 GROSS FREIGHT REVENUE    -1.00
200304 1010.00.00 GROSS FREIGHT REVENUE  -100.00
200304 1030.10.00 PORT & TERMINAL REVENUE   110.00
200304 1030.20.10 PORT & TERMINAL COSTS ACTUAL    -9.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL   100.00
200304 1040.20.10 DOCUMENTATION COSTS ACTUAL  -100.00";
			AssertMultilineASCIIEquals("", expected, testAggregator.GetBatchPostingPreviewDataAsString());
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestInvCrdAdjTogether()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TestHelper.GLHeaders[8].PK.ToGuid(), Guid.Empty, 50, 0);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, Factory.Load<AccChargeCode>(TestHelper.RevenueChargeCode).AC_AG_RevenueAccount.ToGuid(), TestHelper.RevenueChargeCode, 60, 6);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, TransactionLineTypes.Revenue, TestHelper.GLHeaders[10].PK.ToGuid(), Guid.Empty, 80, 0);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, TransactionLineTypes.Revenue, TestHelper.GLHeaders[9].PK.ToGuid(), Guid.Empty, -110, 0);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, TransactionLineTypes.Revenue, Factory.Load<AccChargeCode>(TestHelper.RevenueChargeCode).AC_AG_RevenueAccount.ToGuid(), TestHelper.RevenueChargeCode, -120, -12);

			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionLineTypes.Cost, TestHelper.GLHeaders[8].PK.ToGuid(), Guid.Empty, -70, -7);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, TransactionLineTypes.Cost, Factory.Load<AccChargeCode>(TestHelper.CostChargeCode).AC_AG_CostAccount.ToGuid(), TestHelper.CostChargeCode, 90, 9);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, TransactionLineTypes.Cost, TestHelper.GLHeaders[11].PK.ToGuid(), Guid.Empty, -100, -10);
			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, TransactionLineTypes.Cost, Factory.Load<AccChargeCode>(TestHelper.CostChargeCode).AC_AG_CostAccount.ToGuid(), TestHelper.CostChargeCode, 130, 13);

			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);

			testAggregator.RunAggregateForTest();

			AssertEquals("Aggregate Line Row Count", 14, testAggregator.AllAggregatedData.Count);

			AssertEquals("Invoice Account", TestHelper.GLHeaders[8].PK, testAggregator.GetAggregateRow(TestHelper.GLHeaders[8].PK).AA_AG);
			AssertEquals("Inv/Adj/Crd", 20.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[8].PK).AA_Amount);
			AssertEquals("Inv/Adj/Crd", 110.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[9].PK).AA_Amount);
			AssertEquals("Inv/Adj/Crd", -80.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[10].PK).AA_Amount);
			AssertEquals("Inv/Adj/Crd", 100.0m, testAggregator.GetAggregateRow(TestHelper.GLHeaders[11].PK).AA_Amount);
			AssertEquals("Inv/Adj/Crd", 60.0m, testAggregator.GetAggregateRow(TestHelper.ReveneueAccount).AA_Amount);
			AssertEquals("Inv/Adj/Crd", -220.0m, testAggregator.GetAggregateRow(TestHelper.CostAccounts).AA_Amount);
			AssertEquals("Inv/Adj/Crd", 55.0m, testAggregator.GetAggregateRow(TestHelper.APControlAccount).AA_Amount);
			AssertEquals("Inv/Adj/Crd", -5.0m, testAggregator.GetAggregateRow(TestHelper.GSTIn).AA_Amount);
			AssertEquals("Inv/Adj/Crd", 6.0m, testAggregator.GetAggregateRow(TestHelper.GSTOut).AA_Amount);
			AssertEquals("Inv/Adj/Crd", -46.0m, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);

			AssertEquals("AP Suspense", 0m, testAggregator.GetTotalForGL(testHelper.APSuspenseControlAccount, PostDate1Period));
			AssertEquals("AR Suspense", 0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));

			var testAggregator2 = new TestBatchAggregator(TestHelper);

			testAggregator2.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator2.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);

			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 14, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestJobRevenueJournal()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetJobRevenueJournal(TestHelper.TestDataSet);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 4, testAggregator.AllAggregatedData.Count);

			AssertEquals("Job Revenue Journal Control Amount", -50.0M, testAggregator.GetTotalForGL(TestHelper.JobRevenueJournalControlAccount, PostDate1Period));
			AssertEquals("AR Suspense Control Account", 0.0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));
			AssertEquals("Revenue Line Amount", 50.0M, testAggregator.GetAggregateRow(TestHelper.ReveneueAccount).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestJobRevenueJournalProfitRecognitionDate()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetJobRevenueJournal(TestHelper.TestDataSet, profitRecognitionDate);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("Invoice Credit & Adjustment Revenue Line Row Count", 4, testAggregator.AllAggregatedData.Count);

			AssertEquals("Job Revenue Journal Control Amount", -50.0M, testAggregator.GetTotalForGL(testHelper.JobRevenueJournalControlAccount, PostDate1Period));
			AssertEquals("AR Suspense Control Amount", 50.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));

			AssertEquals("Revenue Line Amount", 50.0M, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
			AssertEquals("AR Suspense Control Amount", -50.0M, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, TestHelper.JobRevenueJournalControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestPayment()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetPayment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, 100.0M);
			TestHelper.SetPayment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, 50.0M);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("Payment Row Count", 2, testAggregator.AllAggregatedData.Count);
			AssertEquals("Paymente Account", TestHelper.Banks[0].AB_AG.ToString(), testAggregator.AllAggregatedData[1].AA_AG.ToString());
			AssertEquals("Payment Line Amount", -150.0M, testAggregator.AllAggregatedData[1].AA_Amount);

			AssertEquals("AP Control GL Account", TestHelper.APControlAccount.ToString(), testAggregator.AllAggregatedData[0].AA_AG.ToString());
			AssertEquals("AP Control Account Amount", 150.0M, testAggregator.AllAggregatedData[0].AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 2, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestReceipt()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetReceipt(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, -100.0M);
			TestHelper.SetReceipt(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, -50.0M);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("Receipt Row Count", 2, testAggregator.AllAggregatedData.Count);
			AssertEquals("Receipt Line Amount", 150.0M, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("AR Control Account Amount", -150.0M, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 2, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestPaymentReceipt()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, TestHelper.Banks[0].PK.ToGuid(), -50.0M);
			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, TestHelper.Banks[1].PK.ToGuid(), -60.0M);
			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, TransactionTypes.Payment, TestHelper.Banks[0].PK.ToGuid(), 70.0M);
			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Payment, TestHelper.Banks[1].PK.ToGuid(), 80.0M);
			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Payment, TestHelper.Banks[0].PK.ToGuid(), 90.0M);
			TestHelper.SetReceiptPayment(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, TransactionTypes.Receipt, TestHelper.Banks[1].PK.ToGuid(), -100.0M);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("Payment Receipt Row Count", 4, testAggregator.AllAggregatedData.Count);
			AssertEquals("Payment Receipt Account", TestHelper.Banks[0].AB_AG, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_AG);
			AssertEquals("Payment Receipt", -110.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Payment Receipt", 80.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Payment Receipt", -40.0m, testAggregator.GetAggregateRow(TestHelper.ARControlAccount).AA_Amount);
			AssertEquals("Payment Receipt", 70.0m, testAggregator.GetAggregateRow(TestHelper.APControlAccount).AA_Amount);
			Factory.Save();

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator2.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestCashBookTransfer()
		{
			Db.Connection.ExecuteNonQuery("Delete From dbo.AccTransactionHeader WHERE ah_ledger = 'CB' and ah_transactionType = 'TRF'");

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			ZGuid tf1Group = ZGuid.NewZGuid();
			ZGuid tf2Group = ZGuid.NewZGuid();
			ZGuid tf3Group = ZGuid.NewZGuid();
			ZGuid tf4Group = ZGuid.NewZGuid();

			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, 100, TestHelper.Banks[0].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferFromRow, tf1Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, 70, TestHelper.Banks[2].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferFromRow, tf2Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, 60, TestHelper.Banks[1].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferFromRow, tf3Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, 40, TestHelper.Banks[1].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferFromRow, tf4Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, -100, TestHelper.Banks[2].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferToRow, tf1Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, -70, TestHelper.Banks[0].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferToRow, tf2Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, -60, TestHelper.Banks[2].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferToRow, tf3Group);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.Transfer, -40, TestHelper.Banks[2].PK.ToGuid(), AccTransactionHeader.TransactionCountConstants.BankTransferToRow, tf4Group);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.RunAggregateForTest();

			AssertEquals("Cashbook Transfer Row Count", 3, testAggregator.AllAggregatedData.Count);
			AssertEquals("Cashbook Transfer Account", TestHelper.Banks[0].AB_AG, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_AG);
			AssertEquals("Cashbook Transfer", 30.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Cashbook Transfer", 100.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Cashbook Transfer", -130.0m, testAggregator.GetAggregateRow(TestHelper.Banks[2].AB_AG).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[TestDate(2003, 03, 15)]
		public void TestCashBookExchangeDiff()
		{
			Db.Connection.ExecuteNonQuery("Delete From dbo.AccTransactionHeader WHERE ah_ledger = 'CB' and ah_transactionType = 'TRF'");

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.ExchangeDifference, -50, TestHelper.Banks[0].PK.ToGuid());
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.ExchangeDifference, 100, TestHelper.Banks[0].PK.ToGuid());
			TestHelper.SetCashBookDataSet(TestHelper.TestDataSet, TransactionTypes.ExchangeDifference, 80, TestHelper.Banks[1].PK.ToGuid());
			TestHelper.TestDataSet[0].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[1].AH_AG = TestHelper.ExchangeDifference;
			TestHelper.TestDataSet[2].AH_AG = TestHelper.ExchangeDifference;
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.RunAggregateForTest();

			AssertEquals("CashBook Exchange Difference Row Count", 3, testAggregator.AllAggregatedData.Count);

			AssertEquals("Transfer Account 1", TestHelper.Banks[0].AB_AG, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_AG);
			AssertEquals("Cashbook Transfer", 50.0m, testAggregator.GetAggregateRow(TestHelper.Banks[0].AB_AG).AA_Amount);
			AssertEquals("Cashbook Transfer", 80.0m, testAggregator.GetAggregateRow(TestHelper.Banks[1].AB_AG).AA_Amount);
			AssertEquals("Cashbook Transfer", -130.0m, testAggregator.GetAggregateRow(TestHelper.ExchangeDifference).AA_Amount);

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 3, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		public void TestARAPTransfer()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, 100.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, -70.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, 60.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, 40.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, -100.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsReceivable, 70.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, -60.0M);
			TestHelper.SetARAPTransferDataSet(TestHelper.TestDataSet, LedgerTypes.AccountsPayable, -40.0M);
			Factory.Save();

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.RunAggregateForTest();

			AssertEquals("ARAP Transfer Row Count", 0, testAggregator.AllAggregatedData.Count);
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestCFX()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetInvoiceCreditAdjustment(TestHelper.TestDataSet, LedgerTypes.JobCosting, TransactionTypes.Journal, TransactionLineTypes.Revenue, Factory.Load<AccChargeCode>(TestHelper.RevenueChargeCode).AC_AG_RevenueAccount.ToGuid(), TestHelper.RevenueChargeCode, -50, 0, profitRecognitionDate);
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.RunAggregateForTest();

			AssertEquals("CFX Row Count", 4, testAggregator.AllAggregatedData.Count);
			AssertEquals("CFX Account", TestHelper.ReveneueAccount, testAggregator.GetAggregateRow(TestHelper.ReveneueAccount).AA_AG);

			AssertEquals("CFX Line Account", 0m, testAggregator.GetTotalForGL(Factory.Load<AccChargeCode>(TestHelper.RevenueChargeCode).AC_AG_RevenueAccount.ToGuid(), PostDate1Period));
			AssertEquals("CFX Charge Account", 50.0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, PostDate1Period));

			AssertEquals("CFX Charge Account", 50.0m, testAggregator.GetTotalForGL(testHelper.ReveneueAccount, profitRecognitionDatePeriod));
			AssertEquals("CFX Charge Account", -50.0m, testAggregator.GetTotalForGL(testHelper.ARSuspenseControlAccount, profitRecognitionDatePeriod));

			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		protected void AddNewRow(AccGLAggregateCollection dataSetToAdd, Guid aG_PK)
		{
			AccGLAggregate rowToAdd = dataSetToAdd.AddNew();
			rowToAdd.AA_AG = aG_PK;
			rowToAdd.AA_GB = Guid.NewGuid();
			rowToAdd.AA_GE = Guid.NewGuid();
		}

		[SuspendCriticalValidation]
		public void TestPostFlagUpdateWorkCorrectly()
		{
			ZDateTime reverseDateEmpty = ZDateTime.Empty;

			#region Setup

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			Guid line1 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, reverseDateEmpty);
			Guid line2 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, reverseDateEmpty);
			Guid line3 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 70, 0, TestHelper.PostDate200301, reverseDateEmpty);
			Guid line4 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 80, 0, TestHelper.PostDate200301, reverseDateEmpty);

			GetLineFromGuid(line1).AL_Desc = "1";
			GetLineFromGuid(line2).AL_Desc = "2";
			GetLineFromGuid(line3).AL_Desc = "3";
			GetLineFromGuid(line4).AL_Desc = "4";

			GetLineFromGuid(line1).AL_PostToGL = "Y";
			GetLineFromGuid(line3).AL_PostToGL = "Y";

			Factory.Save();

			#endregion

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.RunAggregateForTest();

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccTransactionLines));
			filter.OrderBy = AccTransactionLinesSchema.Constants.AL_Desc;

			AccTransactionLines[] transactionLines = new BusinessObjectFactory().Load<AccTransactionLines>(filter);

			AssertEquals(4, transactionLines.Length);

			AssertEquals("Y", transactionLines[0].AL_PostToGL);
			AssertEquals("Y", transactionLines[1].AL_PostToGL);
			AssertEquals("Y", transactionLines[2].AL_PostToGL);
			AssertEquals("Y", transactionLines[3].AL_PostToGL);
		}

		AccTransactionLines GetLineFromGuid(ZGuid linePK)
		{
			return Factory.Load<AccTransactionLines>(linePK);
		}

		[SuspendCriticalValidation]
		public void TestPostFlagUpdateWorkCorrectly_WhenPostDateIsNOTAggregated()
		{
			#region Setup
			Guid emptyGLAccount = Guid.Empty;

			ZDateTime reverseDate200301 = TestHelper.PostDate200301;
			ZDateTime reverseDate200302 = TestHelper.PostDate200302;

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			Guid line1 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, reverseDate200301);
			Guid line2 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			Guid line3 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.AccrualChargeCode, -70, 0, TestHelper.PostDate200302, reverseDate200301);
			Guid line4 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 80, 0, TestHelper.PostDate200301, reverseDate200302);
			Guid line5 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 90, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			Guid line6 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 100, 0, TestHelper.PostDate200302, reverseDate200302);

			GetLineFromGuid(line1).AL_Desc = "1";
			GetLineFromGuid(line2).AL_Desc = "2";
			GetLineFromGuid(line3).AL_Desc = "3";
			GetLineFromGuid(line4).AL_Desc = "4";
			GetLineFromGuid(line5).AL_Desc = "5";
			GetLineFromGuid(line6).AL_Desc = "6";

			GetLineFromGuid(line6).AL_ReverseToGL = "Y";

			Factory.Save();
			#endregion

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.RunAggregateForTest();

			ZQuery filter = new ZQuery();
			filter.OrderBy = AccTransactionLinesSchema.Constants.AL_Desc;
			AccTransactionLines[] transactionLines = new BusinessObjectFactory().Load<AccTransactionLines>(filter);

			AssertEquals(6, transactionLines.Length);

			#region Assert Updating Flags to Y

			transactionLines[0].Reload();
			transactionLines[1].Reload();
			transactionLines[2].Reload();
			transactionLines[3].Reload();
			transactionLines[4].Reload();
			transactionLines[5].Reload();

			AssertEquals("Y", transactionLines[0].AL_ReverseToGL);
			AssertEquals("N", transactionLines[1].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[2].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[3].AL_ReverseToGL);
			AssertEquals("N", transactionLines[4].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[5].AL_ReverseToGL);

			AssertEquals("Y", transactionLines[0].AL_PostToGL);
			AssertEquals("Y", transactionLines[1].AL_PostToGL);
			AssertEquals("Y", transactionLines[2].AL_PostToGL);
			AssertEquals("Y", transactionLines[3].AL_PostToGL);
			AssertEquals("Y", transactionLines[4].AL_PostToGL);
			AssertEquals("Y", transactionLines[5].AL_PostToGL);

			#endregion
		}

		[SuspendCriticalValidation]
		public void TestPostFlagUpdateWorkCorrectly_WhenPostDateIsAggregated()
		{
			#region Setup
			Guid emptyGLAccount = Guid.Empty;

			ZDateTime reverseDate200301 = TestHelper.PostDate200301;
			ZDateTime reverseDate200302 = TestHelper.PostDate200302;

			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			Guid line1 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, reverseDate200301);
			Guid line2 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			Guid line3 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, -70, 0, TestHelper.PostDate200302, reverseDate200301);
			Guid line4 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 80, 0, TestHelper.PostDate200301, reverseDate200302);
			Guid line5 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 90, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			Guid line6 = TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 100, 0, TestHelper.PostDate200302, reverseDate200302);

			GetLineFromGuid(line1).AL_Desc = "1";
			GetLineFromGuid(line2).AL_Desc = "2";
			GetLineFromGuid(line3).AL_Desc = "3";
			GetLineFromGuid(line4).AL_Desc = "4";
			GetLineFromGuid(line5).AL_Desc = "5";
			GetLineFromGuid(line6).AL_Desc = "6";

			GetLineFromGuid(line1).AL_PostToGL = "Y";
			GetLineFromGuid(line2).AL_PostToGL = "Y";
			GetLineFromGuid(line3).AL_PostToGL = "Y";
			GetLineFromGuid(line4).AL_PostToGL = "Y";
			GetLineFromGuid(line5).AL_PostToGL = "Y";
			GetLineFromGuid(line6).AL_PostToGL = "Y";

			Factory.Save();
			#endregion

			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);

			testAggregator.RunAggregateForTest();

			ZQuery filter = new ZQuery();
			filter.OrderBy = AccTransactionLinesSchema.Constants.AL_Desc;
			AccTransactionLines[] transactionLines = new BusinessObjectFactory().Load<AccTransactionLines>(filter);

			AssertEquals(6, transactionLines.Length);

			#region Assert Updating Flags to Y

			transactionLines[0].Reload();
			transactionLines[1].Reload();
			transactionLines[2].Reload();
			transactionLines[3].Reload();
			transactionLines[4].Reload();
			transactionLines[5].Reload();

			AssertEquals("Y", transactionLines[0].AL_ReverseToGL);
			AssertEquals("N", transactionLines[1].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[2].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[3].AL_ReverseToGL);
			AssertEquals("N", transactionLines[4].AL_ReverseToGL);
			AssertEquals("Y", transactionLines[5].AL_ReverseToGL);

			AssertEquals("Y", transactionLines[0].AL_PostToGL);
			AssertEquals("Y", transactionLines[1].AL_PostToGL);
			AssertEquals("Y", transactionLines[2].AL_PostToGL);
			AssertEquals("Y", transactionLines[3].AL_PostToGL);
			AssertEquals("Y", transactionLines[4].AL_PostToGL);
			AssertEquals("Y", transactionLines[5].AL_PostToGL);

			#endregion
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestWIPsACRs()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, new ZDateTime(1900, 1, 1));
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, new ZDateTime(1900, 1, 1));
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 70, 0, TestHelper.PostDate200301, new ZDateTime(1900, 1, 1));
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 80, 0, TestHelper.PostDate200301, new ZDateTime(1900, 1, 1));
			Factory.Save();

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			testAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			testAggregator.RunAggregateForTest();

			AssertEquals("WIP/ACR Row Count", 4, testAggregator.AllAggregatedData.Count);
			AssertEquals("Invoice Account", TestHelper.WIPAccounts, testAggregator.GetAggregateRow(TestHelper.WIPAccounts).AA_AG);

			AssertEquals("WIP/ACR", -110.0m, testAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));

			AssertEquals("WIP/ACR", 150.0m, testAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals("WIP/ACR", 150.0m, testAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals("WIP/ACR", 110.0m, testAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals("WIP/ACR", -150.0m, testAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));

			//Retrieve the dataset
			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			testAggregator2.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 4, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[SuspendCriticalValidation]
		[TestDate(2003, 03, 15)]
		public void TestWIPsACRsReversal()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -70, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -80, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -90, 0, TestHelper.PostDate200302, TestHelper.PostDate200301, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 100, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 110, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 120, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 130, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 140, 0, TestHelper.PostDate200302, TestHelper.PostDate200301, "N", "Y");

			var testAggregator = new TestBatchAggregator(TestHelper);
			testAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			testAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);
			Factory.Save();
			//TestAggregator.MarkPostFlagToM();
			testAggregator.RunAggregateForTest();

			AssertEquals("WIP/ACR Row Count", 16, testAggregator.AllAggregatedData.Count);
			AssertEquals("Invoice Account", TestHelper.WIPAccounts, testAggregator.GetAggregateRow(TestHelper.WIPAccounts).AA_AG);

			AssertEquals("WIP/ACR", 40.0m, testAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals("WIP/ACR", 200.0m, testAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals("WIP/ACR", -40.0m, testAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals("WIP/ACR", -200.0m, testAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));

			AssertEquals("WIP/ACR", -20.0m, testAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals("WIP/ACR", 60.0m, testAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals("WIP/ACR", 20.0m, testAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals("WIP/ACR", -60.0m, testAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));

			//Retrieve the dataset
			var testAggregator2 = new TestBatchAggregator(TestHelper);
			testAggregator2.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			testAggregator2.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);
			Factory.Save();
			//TestAggregator.MarkPostFlagToM();
			testAggregator2.RunAggregateForTest();

			AssertEquals("No transaction should be aggregated as no record created/updated", 16, testAggregator2.AllAggregatedData.Count);
			Assert("No transaction should be aggregated as no record created/updated", testAggregator2.AllAggregatedData.All(x => testAggregator.AllAggregatedData.Any(y => x.PK == y.PK)));
		}

		[SuspendCriticalValidation]
		public void TestInduceExceptionAndCheckMFlags()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -50, 0, TestHelper.PostDate200301, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -60, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -70, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -80, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.WIP, Factory.Load<AccChargeCode>(TestHelper.WIPChargeCode).AC_AG_WIPAccount.ToGuid(), TestHelper.WIPChargeCode, -90, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 100, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 110, 0, TestHelper.PostDate200301, TestHelper.PostDate200302);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 120, 0, TestHelper.PostDate200301, ZDateTime.Empty);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 130, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);
			TestHelper.SetWIPAccrualLinesOnly(TransactionLineTypes.Accrual, Factory.Load<AccChargeCode>(TestHelper.AccrualChargeCode).AC_AG_AccrualAccount.ToGuid(), TestHelper.AccrualChargeCode, 140, 0, TestHelper.PostDate200302, TestHelper.PostDate200301);

			Factory.Save();

			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			TestAggregator.Aggregate();

			Assert(string.IsNullOrEmpty(TestAggregator.AggregateResult));
		}

		[SuspendCriticalValidation]
		public void TestMixedWIPandACR1()
		{
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			SetupForTheWIPTest1();

			Factory.Save();

			TestAggregator.Aggregate();

			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(70.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(190.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(210.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-170.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(-190.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-210.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
		}

		[SuspendCriticalValidation]
		public void TestMixedWIPandACR2()
		{
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			SetupForTheWIPTest2();

			Factory.Save();

			TestAggregator.Aggregate();

			AssertEquals(-60.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(60.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(70.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(80.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(-200.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(-200.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(-100.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(200.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(170.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(200.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
		}

		[SuspendCriticalValidation]
		public void TestMixedWIPandACR3()
		{
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			SetupForTheWIPTest3();

			Factory.Save();

			TestAggregator.Aggregate();

			AssertEquals(40.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(-40.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(100.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-140.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(130.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(110.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-100.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(30.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-130.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
		}

		public void TestMixedWIPandACR4()
		{
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);

			SetupForTheWIPTest4();

			Factory.Save();

			TestAggregator.Aggregate();

			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(-80.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(110.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-40.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(-40.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(80.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(120.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
		}

		[SuspendCriticalValidation]
		public void TestWIPACRStepByStep()
		{
			SetupForTheWIPTest1();

			Factory.Save();

			TestAggregator.SetControlAccount(AccountingUtils.AccruedCost, TestHelper.ACRControl);
			TestAggregator.SetControlAccount(AccountingUtils.AccruedRevenue, TestHelper.WIPControl);
			TestAggregator.Aggregate();

			#region Assert Step 1
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-90.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(70.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(190.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(210.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-170.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(-190.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-210.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
			#endregion

			#region Update Reverse Date
			UpdateFlagForTheTransaction(WIPLinePKs[0], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(WIPLinePKs[1], TestHelper.PostDate200301);
			UpdateFlagForTheTransaction(WIPLinePKs[2], TestHelper.PostDate200303);
			UpdateFlagForTheTransaction(WIPLinePKs[3], TestHelper.PostDate200301);
			UpdateFlagForTheTransaction(WIPLinePKs[4], TestHelper.PostDate200303);
			UpdateFlagForTheTransaction(WIPLinePKs[5], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(ACRLinePKs[0], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(ACRLinePKs[1], TestHelper.PostDate200301);
			UpdateFlagForTheTransaction(ACRLinePKs[2], TestHelper.PostDate200303);
			UpdateFlagForTheTransaction(ACRLinePKs[3], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(ACRLinePKs[4], TestHelper.PostDate200303);
			UpdateFlagForTheTransaction(ACRLinePKs[5], TestHelper.PostDate200301);
			#endregion

			TestAggregator.Aggregate();

			#region Assert Step 2
			AssertEquals(-10.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(60.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-10.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(-130.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(120.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(-100.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(30.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(-20.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-10.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
			#endregion

			SetupForTheWIPTest3();

			Factory.Save();

			TestAggregator.Aggregate();

			#region Assert Step 3
			AssertEquals(30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-120.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(150.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(100.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-100.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-20.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(140.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-140.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
			#endregion

			#region Update Reverse Date Again
			UpdateFlagForTheTransaction(WIPLinePKs[7], TestHelper.PostDate200301);
			UpdateFlagForTheTransaction(WIPLinePKs[9], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(WIPLinePKs[10], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(ACRLinePKs[8], TestHelper.PostDate200302);
			UpdateFlagForTheTransaction(ACRLinePKs[10], TestHelper.PostDate200303);
			#endregion

			TestAggregator.Aggregate();

			#region Assert Step 4
			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-160.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(150.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(-50.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(-10.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(110.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-100.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-110.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(140.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(10.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(-110.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(100.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
			#endregion

			//TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			SetupForTheWIPTest4b(TestHelper.TestDataSet);

			Factory.Save();

			TestAggregator.Aggregate();

			#region Assert Step 5
			AssertEquals(00.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200301));
			AssertEquals(-150.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200302));
			AssertEquals(150.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200301));
			AssertEquals(20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200302));
			AssertEquals(-20.0m, TestAggregator.GetTotalForGL(TestHelper.WIPAccounts2, 200303));

			AssertEquals(0.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200301));
			AssertEquals(130.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200302));
			AssertEquals(-130.0m, TestAggregator.GetTotalForGL(TestHelper.WIPControl, 200303));

			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200301));
			AssertEquals(-60.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200302));
			AssertEquals(90.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts, 200303));

			AssertEquals(100m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200301));
			AssertEquals(-30.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200302));
			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.AccrualAccounts2, 200303));

			AssertEquals(-70.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200301));
			AssertEquals(90.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200302));
			AssertEquals(-20.0m, TestAggregator.GetTotalForGL(TestHelper.ACRControl, 200303));
			#endregion
		}

		[SuspendCriticalValidation]
		public void TestGLAggregationPostValidation()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			//create cash book transfer
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			BankTransfer transfer = new BankTransfer(Factory, null);
			transfer.PostDate = new ZDateTime(2003, 3, 3);  //Setup() creates the corresponding period
			transfer.BankTransferFromPK = testObjectCreator.USDBankAccount.PK;
			transfer.BankTransferToPK = testObjectCreator.USDBankAccount.PK;
			//update one side of the Transfer
			transfer.SellAmount = 100.0M;
			transfer.SellExchangeRate = 1.0M;
			transfer.BuyExchangeRate = 1.0M;
			transfer.BuyAmount = 1234.5M;
			transfer.ShouldForceImbalancedTransferRowTo_ForTestOnly = true;
			Factory.Save();

			string expectedMessage = "The following periods do not balance:";
			AssertAggregatorRaisesException(testAggregator, expectedMessage, "There is a period that does not balance");
		}

		protected void UpdateFlagForTheTransaction(Guid linePK, ZDateTime reverseDate)
		{
			string sQL = @"UPDATE dbo.AccTransactionLines SET AL_ReverseDate = @ReverseDate, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST'
								WHERE AL_PK = @LinePK";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.AddParameter("@ReverseDate", SqlDbType.SmallDateTime, reverseDate.ToDateTime());
			cmd.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);

			cmd.ExecuteNonQuery();
		}

		protected void SetupForTheWIPTest1()
		{
			#region Setup & Aliases for the variables
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			AccTransactionHeaderCollection data = TestHelper.TestDataSet;

			string wIP = TransactionLineTypes.WIP;
			string aCR = TransactionLineTypes.Accrual;

			Guid wIPCharge1 = TestHelper.WIPChargeCode;
			Guid wIPCharge2 = TestHelper.WIPChargeCode2;
			Guid aCRCharge3 = TestHelper.AccrualChargeCode;
			Guid aCRCharge4 = TestHelper.AccrualChargeCode2;

			ZDateTime postDate1 = TestHelper.PostDate200301;
			ZDateTime postDate2 = TestHelper.PostDate200302;
			ZDateTime postDate3 = TestHelper.PostDate200303;

			ZDateTime reverseDate1 = TestHelper.PostDate200301;
			ZDateTime reverseDate2 = TestHelper.PostDate200302;
			ZDateTime reverseDate3 = TestHelper.PostDate200303;
			ZDateTime nullReverseDate = ZDateTime.Empty;
			#endregion

			WIPLinePKs[0] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 10, 0, postDate1, nullReverseDate);
			WIPLinePKs[1] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 20, 0, postDate2, nullReverseDate);
			WIPLinePKs[2] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 30, 0, postDate3, nullReverseDate);
			WIPLinePKs[3] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 40, 0, postDate1, nullReverseDate);
			WIPLinePKs[4] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge2).AC_AG_WIPAccount.ToGuid(), wIPCharge2, 50, 0, postDate2, nullReverseDate);
			WIPLinePKs[5] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 60, 0, postDate3, nullReverseDate);
			ACRLinePKs[0] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 70, 0, postDate1, nullReverseDate);
			ACRLinePKs[1] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 80, 0, postDate2, nullReverseDate);
			ACRLinePKs[2] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 90, 0, postDate3, nullReverseDate);
			ACRLinePKs[3] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge4).AC_AG_AccrualAccount.ToGuid(), aCRCharge4, 100, 0, postDate1, nullReverseDate);
			ACRLinePKs[4] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 110, 0, postDate2, nullReverseDate);
			ACRLinePKs[5] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 120, 0, postDate3, nullReverseDate);
		}

		protected Guid[] WIPLinePKs;
		protected Guid[] ACRLinePKs;

		protected void SetupForTheWIPTest2()
		{
			#region Setup & Aliases for the variables
			if (TestHelper.TestDataSet == null)
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			}

			AccTransactionHeaderCollection data = TestHelper.TestDataSet;

			string wIP = TransactionLineTypes.WIP;
			string aCR = TransactionLineTypes.Accrual;

			Guid wIPCharge1 = TestHelper.WIPChargeCode;
			Guid wIPCharge2 = TestHelper.WIPChargeCode2;
			Guid aCRCharge3 = TestHelper.AccrualChargeCode;
			Guid aCRCharge4 = TestHelper.AccrualChargeCode2;

			ZDateTime postDate1 = TestHelper.PostDate200301;
			ZDateTime postDate2 = TestHelper.PostDate200302;
			ZDateTime postDate3 = TestHelper.PostDate200303;

			ZDateTime reverseDate1 = TestHelper.PostDate200301;
			ZDateTime reverseDate2 = TestHelper.PostDate200302;
			ZDateTime reverseDate3 = TestHelper.PostDate200303;
			ZDateTime nullReverseDate = ZDateTime.Empty;
			#endregion

			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 10, 0, postDate1, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 20, 0, postDate2, reverseDate1, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 30, 0, postDate3, reverseDate3, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 40, 0, postDate1, reverseDate1, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge2).AC_AG_WIPAccount.ToGuid(), wIPCharge2, 50, 0, postDate2, reverseDate3, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 60, 0, postDate3, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 70, 0, postDate1, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 80, 0, postDate2, reverseDate1, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 90, 0, postDate3, reverseDate3, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge4).AC_AG_AccrualAccount.ToGuid(), aCRCharge4, 100, 0, postDate1, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 110, 0, postDate2, reverseDate3, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 120, 0, postDate3, reverseDate1, "Y", "N");
		}

		protected void SetupForTheWIPTest3()
		{
			#region Setup & Aliases for the variables
			if (TestHelper.TestDataSet == null)
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			}

			AccTransactionHeaderCollection data = TestHelper.TestDataSet;

			string wIP = TransactionLineTypes.WIP;
			string aCR = TransactionLineTypes.Accrual;

			Guid wIPCharge1 = TestHelper.WIPChargeCode;
			Guid wIPCharge2 = TestHelper.WIPChargeCode2;
			Guid aCRCharge3 = TestHelper.AccrualChargeCode;
			Guid aCRCharge4 = TestHelper.AccrualChargeCode2;

			ZDateTime postDate1 = TestHelper.PostDate200301;
			ZDateTime postDate2 = TestHelper.PostDate200302;
			ZDateTime postDate3 = TestHelper.PostDate200303;

			ZDateTime reverseDate1 = TestHelper.PostDate200301;
			ZDateTime reverseDate2 = TestHelper.PostDate200302;
			ZDateTime reverseDate3 = TestHelper.PostDate200303;
			ZDateTime nullReverseDate = ZDateTime.Empty;
			#endregion

			WIPLinePKs[6] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 10, 0, postDate1, reverseDate1, "N", "N");
			WIPLinePKs[7] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 20, 0, postDate2, nullReverseDate, "N", "N");
			WIPLinePKs[8] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 30, 0, postDate3, reverseDate2, "N", "N");
			WIPLinePKs[9] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 40, 0, postDate1, nullReverseDate, "N", "N");
			WIPLinePKs[10] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge2).AC_AG_WIPAccount.ToGuid(), wIPCharge2, 50, 0, postDate2, nullReverseDate, "N", "N");
			WIPLinePKs[11] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 60, 0, postDate3, reverseDate2, "N", "N");
			ACRLinePKs[6] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 70, 0, postDate1, reverseDate1, "N", "N");
			ACRLinePKs[7] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 80, 0, postDate2, reverseDate3, "N", "N");
			ACRLinePKs[8] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 90, 0, postDate3, nullReverseDate, "N", "N");
			ACRLinePKs[9] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 100, 0, postDate1, reverseDate2, "N", "N");
			ACRLinePKs[10] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge4).AC_AG_AccrualAccount.ToGuid(), aCRCharge4, 110, 0, postDate2, nullReverseDate, "N", "N");
			ACRLinePKs[11] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 120, 0, postDate3, reverseDate2, "N", "N");
		}

		protected void SetupForTheWIPTest4()
		{
			if (TestHelper.TestDataSet == null)
			{
				TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);
			}
			SetupForTheWIPTest4a(TestHelper.TestDataSet);
			SetupForTheWIPTest4b(TestHelper.TestDataSet);
		}

		protected void SetupForTheWIPTest4a(AccTransactionHeaderCollection data)
		{
			#region Setup & Aliases for the variables

			string wIP = TransactionLineTypes.WIP;
			string aCR = TransactionLineTypes.Accrual;

			Guid wIPCharge1 = TestHelper.WIPChargeCode;
			Guid wIPCharge2 = TestHelper.WIPChargeCode2;
			Guid aCRCharge3 = TestHelper.AccrualChargeCode;
			Guid aCRCharge4 = TestHelper.AccrualChargeCode2;

			ZDateTime postDate1 = TestHelper.PostDate200301;
			ZDateTime postDate2 = TestHelper.PostDate200302;
			ZDateTime postDate3 = TestHelper.PostDate200303;

			ZDateTime reverseDate1 = TestHelper.PostDate200301;
			ZDateTime reverseDate2 = TestHelper.PostDate200302;
			ZDateTime reverseDate3 = TestHelper.PostDate200303;
			ZDateTime nullReverseDate = ZDateTime.Empty;
			#endregion

			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 20, 0, postDate2, reverseDate1, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 40, 0, postDate1, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge2).AC_AG_WIPAccount.ToGuid(), wIPCharge2, 50, 0, postDate2, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 90, 0, postDate3, reverseDate2, "Y", "N");
			TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge4).AC_AG_AccrualAccount.ToGuid(), aCRCharge4, 110, 0, postDate2, reverseDate3, "Y", "N");
		}

		protected void SetupForTheWIPTest4b(AccTransactionHeaderCollection data)
		{
			#region Setup & Aliases for the variables

			string wIP = TransactionLineTypes.WIP;
			string aCR = TransactionLineTypes.Accrual;

			Guid wIPCharge1 = TestHelper.WIPChargeCode;
			Guid wIPCharge2 = TestHelper.WIPChargeCode2;
			Guid aCRCharge3 = TestHelper.AccrualChargeCode;
			Guid aCRCharge4 = TestHelper.AccrualChargeCode2;

			ZDateTime postDate1 = TestHelper.PostDate200301;
			ZDateTime postDate2 = TestHelper.PostDate200302;
			ZDateTime postDate3 = TestHelper.PostDate200303;

			ZDateTime reverseDate1 = TestHelper.PostDate200301;
			ZDateTime reverseDate2 = TestHelper.PostDate200302;
			ZDateTime reverseDate3 = TestHelper.PostDate200303;
			ZDateTime nullReverseDate = ZDateTime.Empty;
			#endregion

			WIPLinePKs[12] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 10, 0, postDate2, reverseDate1, "N", "N");
			WIPLinePKs[13] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge1).AC_AG_WIPAccount.ToGuid(), wIPCharge1, 20, 0, postDate2, reverseDate2, "N", "N");
			WIPLinePKs[14] = TestHelper.SetWIPAccrualLinesOnly(wIP, Factory.Load<AccChargeCode>(wIPCharge2).AC_AG_WIPAccount.ToGuid(), wIPCharge2, 30, 0, postDate3, reverseDate2, "N", "N");
			ACRLinePKs[12] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge4).AC_AG_AccrualAccount.ToGuid(), aCRCharge4, 40, 0, postDate3, reverseDate2, "N", "N");
			ACRLinePKs[13] = TestHelper.SetWIPAccrualLinesOnly(aCR, Factory.Load<AccChargeCode>(aCRCharge3).AC_AG_AccrualAccount.ToGuid(), aCRCharge3, 50, 0, postDate2, reverseDate3, "N", "N");
		}

		protected TestBatchAggregator TestAggregator
		{
			get
			{
				if (fTestAggregator == null)
				{
					fTestAggregator = new TestBatchAggregator(TestHelper);
				}
				return fTestAggregator;
			}
		}
		TestBatchAggregator fTestAggregator;
	}
}
