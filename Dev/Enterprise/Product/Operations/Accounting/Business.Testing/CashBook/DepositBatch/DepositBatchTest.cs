using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatch))]
	public class DepositBatchTest : TransactionHeaderTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<DepositBatch>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		public void TestZDecimalsHaveCorrectDecimalPlacesDespositBatch()
		{
			var deposit = Factory.New<DepositBatch>();
			deposit.AH_AB = Factory.New<AccBankAccount>().PK;

			var osList = new List<string>
				{
					nameof(deposit.TotalDepositInvoiceAmount),
					nameof(deposit.TotalDepositGSTAmount),
					nameof(deposit.TotalDepositOSAmount),
					nameof(deposit.TotalDepositNotSelectedAmount),
					nameof(deposit.CashSelectedAmount),
					nameof(deposit.CashNotSelectedAmount),
					nameof(deposit.ChequeSelectedAmount),
					nameof(deposit.ChequeNotSelectedAmount),
					nameof(deposit.CreditCardSelectedAmount),
					nameof(deposit.CreditCardNotSelectedAmount),
					nameof(deposit.DirectCreditSelectedAmount),
					nameof(deposit.DirectCreditNotSelectedAmount)
				};

			var tester = new DecimalPlacesAttributeTester(deposit, deposit.Company);
			tester.CheckNonLocalCurrency(osList, nameof(deposit.BankCurrencyDecimalsAsInt), nameof(deposit.BankAccount.AB_RX_NKAccountCurrency), deposit.BankAccount);
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestAH_PostDate_ReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public void TestTransactionNumMaxLengthIsLessThanOrEqualToAH_ReceiptBatchNoMaxLength()
		{
			var depositBatch = Factory.New<DepositBatch>();
			using (Db.Connection.BeginTransactionWithManager())
			{
				var transactionNum = depositBatch.NumberFountainForTransactionNumber_ForTestOnly.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment));
				var developerMessage = string.Format("{0} max length is {1}, but you will assign {2} with length {3} to {0} in SetReceiptBatchNo method",
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name,
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength,
					AccTransactionHeaderSchema.AH_TransactionNum.Name,
					transactionNum.Length);
				Assert(developerMessage, transactionNum.Length <= AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength);
			}
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert("Not applicable", true);
		}

		public void TestOnSaving()
		{
			ARReceipt testReceipt1 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt2.AH_GSTAmount = -10M;
			testReceipt2.AH_OSTotal = -110M;
			testReceipt2.AH_OutstandingAmount = -110M;

			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			AssertEquals(string.Empty, testBatch.AH_TransactionNum);
			AssertEquals(string.Empty, testBatch.AH_ReceiptBatchNo);
			AssertEquals(string.Empty, testReceipt1.AH_ReceiptBatchNo);
			AssertEquals(string.Empty, testReceipt2.AH_ReceiptBatchNo);

			Factory.Save();

			AssertEquals(310m, testBatch.AH_InvoiceAmount);
			AssertEquals(310m, testBatch.AH_OSTotal);
			AssertEquals(testBatch.AH_InvoiceDate, testBatch.AH_DueDate);
			Assert("Transaction Number should be set during saving", !testBatch.AH_TransactionNum.IsEmpty);
			AssertEquals(testBatch.AH_ReceiptBatchNo, testBatch.AH_TransactionNum);
			AssertEquals(testBatch.AH_TransactionNum, testReceipt1.AH_ReceiptBatchNo);
			AssertEquals(testBatch.AH_TransactionNum, testReceipt2.AH_ReceiptBatchNo);
		}

		public void TestCreateRCBForeNettDirectCreditReceipts()
		{
			ARReceipt testeNettReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.eNettDirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			testeNettReceipt.AH_AB = BankAccount.PK;
			testeNettReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testeNettReceipt.AH_OSExTaxAmount = 90M;
			testeNettReceipt.AH_ExchangeRate = 1M;
			testeNettReceipt.AH_TransactionReference = "abc";
			testeNettReceipt.AH_Desc = "test receipt";
			testeNettReceipt.AH_ChequeOrReference = "00123";
			testeNettReceipt.AH_ChequeDrawer = "bbb";
			Factory.Save();

			AssertNotNull(testeNettReceipt.RelatedDepositBatch);
			AssertEquals(90m, testeNettReceipt.RelatedDepositBatch.AH_InvoiceAmount);
		}

		public void TestTransactionNumWillNotBeChangedOnSaving()
		{
			try
			{
				Db.Connection.BeginTransaction();
				ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
				Factory.Save();
				DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
				DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

				Factory.Save();
				ZString numberFotTest = testBatch.AH_TransactionNum;
				Factory.Save();

				AssertEquals(100m, testBatch.AH_InvoiceAmount);
				AssertEquals(100m, testBatch.AH_OSTotal);
				AssertEquals(testBatch.AH_InvoiceDate, testBatch.AH_DueDate);
				AssertEquals("Transaction Number should not be changed during saving", numberFotTest, testBatch.AH_TransactionNum);
				AssertEquals(testBatch.AH_ReceiptBatchNo, testBatch.AH_TransactionNum);
				AssertEquals(testBatch.AH_TransactionNum, testBatch.Transactions[0].AH_ReceiptBatchNo);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestOnSaveFailedWillResetTransactionNum()
		{
			try
			{
				Db.Connection.BeginTransaction();
				ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
				Factory.Save();
				DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
				DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
				try
				{
					Assert("Number should not be empty", !testBatch.AH_TransactionNum.IsEmpty);
					testBatch.OnSaved(true);
					Assert("Number should not be reset", !testBatch.AH_TransactionNum.IsEmpty);
					testBatch.OnSaved(false);
					Assert("Number should be reset", testBatch.AH_TransactionNum.IsEmpty);
				}
				catch
				{
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#region TestOnSaving_ReverseDepositBatchHasTheSamePostDateAsReverseReceipt

		public void TestOnSaving_ReverseDepositBatchHasTheSamePostDateAsReverseReceipt()
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();

			DepositBatch testDepositBatch = tempFactory.NewWithValidTestData<DepositBatch>();
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";
			tempFactory.Save();

			ARReceipt testReceipt = tempFactory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceipt.AH_ReceiptBatchNo = "00001580";
			tempFactory.Save();

			ReceiptReversing reversing = new ReceiptReversing(testReceipt);
			reversing.Reverse();
			ARReceipt reverseReceipt = testReceipt.ReverseTransaction as ARReceipt;
			reverseReceipt.AH_PostDate = ZDateTime.Now.AddDays(-2);
			tempFactory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testReceipt.RelatedDepositBatch.PK);
			DepositBatch reverseDepositBatch = tempFactory.LoadTop1<DepositBatch>(filter);
			AssertEquals("AH_PostDate", reverseReceipt.AH_PostDate, reverseDepositBatch.AH_PostDate);
		}

		#endregion

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			ARReceipt testARReceipt = Factory.New<ARReceipt>();
			testARReceipt.AH_ReceiptType = ReceiptTypes.Cash;
			testARReceipt.AH_TransactionType = TransactionTypes.Receipt;
			testARReceipt.AH_Ledger = LedgerTypes.AccountsReceivable;
			testARReceipt.AH_GB = GlbBranch.CurrentBranch.PK;
			testARReceipt.AH_AB = TestObjectCreator.USDBankAccount.PK;
			testARReceipt.AH_ExchangeRate = 0.7m;
			testARReceipt.AH_OSExTaxAmount = 100m;
			testARReceipt.AH_OSTaxAmount = 10m;
			testARReceipt.AH_OutstandingAmount = -157.15;

			AssertEquals(142.86m, testARReceipt.AH_LocalExTaxAmount);

			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			AssertEquals("OS Ex Tax Amount on Load", 110.00m, testBatch.AH_OSTotal);
			AssertEquals("OS Tax Amount on Load", 157.15m, testBatch.AH_InvoiceAmount);
		}

		public void TestIsSavedByFactory()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			testBatch.IsSelected = false;
			AssertEquals(false, testBatch.IsSavedByFactory);
			testBatch.IsSelected = true;
			AssertEquals(true, testBatch.IsSavedByFactory);

			DepositBatch testBatchSavedOne = Factory.Load<DepositBatch>(testBatch.PK);
			AssertEquals(true, testBatch.IsSavedByFactory);
		}

		public void TestIsSavedByFactoryReversingReceipt()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			testBatch.IsSelected = false;
			AssertEquals(false, testBatch.IsSavedByFactory);

			testBatch.ReversingReceipt = testReceipt;
			AssertEquals(true, testBatch.IsSavedByFactory);
		}

		public void TestReadOnlyFields()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(true, testBatch.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestInvertSigns()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(false, testBatch.InvertSigns_ForTestOnly);
		}

		public void TestLedger()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(Enterprise.ZArchitecture.Core.LedgerTypes.CashBook, testBatch.Ledger_ForTestOnly);
		}

		public void TestNumberFountain()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(AccountingNumberFountainWrapperFactory.Instance.BatchReceiptNo.GetType(), testBatch.NumberFountainForTransactionNumber_ForTestOnly.GetType());
		}

		public void TestTransactionType()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionTypes.ReceiptBatch, testBatch.TransactionType_ForTestOnly);
		}

		public void TestBusinessContext()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(BusinessContext.DepositBatch, testBatch.DocumentSupporter.BusinessContext);
		}

		public void TestDocBusinessObject()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			BusinessObject bizO = (BusinessObject)testBatch.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.DepositBatch, null)[0].WrappedObject;
			AssertEquals(testBatch.PK, bizO.PK);
		}

		public void TestIsCanncelledCalc()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			AssertEquals(2, testBatch.Transactions.Count);

			testReceipt.AH_IsCancelled = ZBool.True;
			((IMatching)testReceipt).CurrentMatchGroup.AddNew().AP_AH = testReceipt.PK; // to pass IsCancelled check

			testReceipt2.AH_IsCancelled = ZBool.True;
			((IMatching)testReceipt2).CurrentMatchGroup.AddNew().AP_AH = testReceipt2.PK; // to pass IsCancelled check

			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt);
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt2);

			Factory.Save();
			AssertEquals(ZBool.False, testBatch.IsCancelledCalc);

			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent2 = new DepositBatchParent(Factory);
			DepositBatch testBatch2 = testDepositBatchParent2.DepositBatchLines[0];
			testBatch2.AH_IsCancelled = ZBool.True;
			AssertEquals(1, testBatch2.Transactions.Count);
			AssertEquals(ZBool.True, testBatch2.IsCancelledCalc);

			ARReceipt testReceipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 600M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testBatch3 = Factory.LoadTop1<DepositBatch>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt4.AH_ReceiptBatchNo));
			AssertEquals(1, testBatch3.Transactions.Count);

			testReceipt4.AH_IsCancelled = ZBool.True;
			((IMatching)testReceipt4).CurrentMatchGroup.AddNew().AP_AH = testReceipt4.PK; // to pass IsCancelled check
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt4);
			testBatch3.AH_IsCancelled = ZBool.False;
			Factory.Save();

			AssertEquals(ZBool.True, testBatch3.IsCancelledCalc);

			ARReceipt testDirectReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt, ZArchitecture.Core.TransactionTypes.DirectReceipt, ZArchitecture.Core.LedgerTypes.CashBook, 600M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testBatch4 = Factory.LoadTop1<DepositBatch>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testDirectReceipt.AH_ReceiptBatchNo));
			AssertEquals(1, testBatch4.Transactions.Count);

			testDirectReceipt.AH_IsCancelled = ZBool.True;
			((IMatching)testDirectReceipt).CurrentMatchGroup.AddNew().AP_AH = testDirectReceipt.PK; // to pass IsCancelled check
			TestObjectCreator.SetupMatchLinkMatchDate(testDirectReceipt);
			testBatch4.AH_IsCancelled = ZBool.False;
			Factory.Save();

			AssertEquals(ZBool.True, testBatch4.IsCancelledCalc);
		}

		public void TestIsSelected()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			AssertEquals(2, testBatch.Transactions.Count);

			testBatch.IsSelected = false;
			AssertEquals(false, testBatch.Transactions[0].IsSelected);
			AssertEquals(false, testBatch.Transactions[1].IsSelected);

			testBatch.IsSelected = true;
			AssertEquals(true, testBatch.Transactions[0].IsSelected);
			AssertEquals(true, testBatch.Transactions[1].IsSelected);
		}

		public void TestIsSelectedReadOnly()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			AssertEquals(false, testBatch.IsInDatabase);
			AssertEquals(false, testBatch.IsSelectedInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, testBatch.IsInDatabase);
			AssertEquals(true, testBatch.IsSelectedInfo.ReadOnly);
		}

		public void TestBankDetails()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			testBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_Code, testBatch.BankCode);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_BankAddress, testBatch.BankAddress);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_BSB, testBatch.BankBSBNumber);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_AccountNum, testBatch.BankAccountNumber);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_Desc, testBatch.BankName);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency, testBatch.CurrencyCode);
		}

		public void TestLoadTransactions()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			testBatch.LoadTransactions(ZGuid.Empty);
			AssertEquals("Must return two transactions because Related Transaction is not setted", 2, testBatch.Transactions.Count);

			testBatch.RelatedTransactionPK = testReceipt.PK;
			testBatch.LoadTransactions(ZGuid.Empty);
			AssertEquals("Must return one transactions which equals to Related Transaction", 1, testBatch.Transactions.Count);

			testBatch.RelatedTransactionPK = ZGuid.NewZGuid();
			testBatch.LoadTransactions(ZGuid.Empty);
			AssertEquals("Must return zero transactions because transaction which equal to Related Transaction is not exist", 0, testBatch.Transactions.Count);
		}

		public void TestLoadTransactionsByReceiptsPKs()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			List<ZGuid> receiptPKs = new List<ZGuid>();
			receiptPKs.Add(testReceipt.PK);
			receiptPKs.Add(testReceipt2.PK);
			testBatch.LoadTransactions(receiptPKs);
			AssertEquals("Must return two transactions because Related Transaction is not setted", 2, testBatch.Transactions.Count);

			receiptPKs = new List<ZGuid>();
			receiptPKs.Add(testReceipt.PK);
			testBatch.LoadTransactions(receiptPKs);
			AssertEquals("Must return one transactions because Related Transaction is not setted", 1, testBatch.Transactions.Count);
			AssertEquals(testReceipt.PK, testBatch.Transactions[0].PK);

			receiptPKs = new List<ZGuid>();
			receiptPKs.Add(testReceipt2.PK);
			testBatch.LoadTransactions(receiptPKs);
			AssertEquals("Must return one transactions because Related Transaction is not setted", 1, testBatch.Transactions.Count);
			AssertEquals(testReceipt2.PK, testBatch.Transactions[0].PK);
		}

		public void TestSelectedTransactionAmounts()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.CreditCard, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.DirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 400M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt5 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.InterestReceived, ZArchitecture.Core.TransactionTypes.DirectReceipt, ZArchitecture.Core.LedgerTypes.CashBook, 500M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt6 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.EFT, ZArchitecture.Core.TransactionTypes.DirectReceipt, ZArchitecture.Core.LedgerTypes.CashBook, 600M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt7 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.eNettDirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 700M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			//IsSelected = false
			testBatch.IsSelected = false;
			AssertEquals(0M, testBatch.CashSelectedAmount);
			AssertEquals(0M, testBatch.ChequeSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(0M, testBatch.TotalDepositOSAmount);

			AssertEquals(100M, testBatch.CashNotSelectedAmount);
			AssertEquals(200M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(600M, testBatch.TotalDepositNotSelectedAmount);

			//IsSelected = true
			testBatch.IsSelected = true;
			AssertEquals(100M, testBatch.CashSelectedAmount);
			AssertEquals(200M, testBatch.ChequeSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(600M, testBatch.TotalDepositOSAmount);

			AssertEquals(0M, testBatch.CashNotSelectedAmount);
			AssertEquals(0M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(0M, testBatch.TotalDepositNotSelectedAmount);

			//Cash
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0M, testBatch.CashSelectedAmount);
			AssertEquals(200M, testBatch.ChequeSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(500M, testBatch.TotalDepositOSAmount);

			AssertEquals(100M, testBatch.CashNotSelectedAmount);
			AssertEquals(0M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(100M, testBatch.TotalDepositNotSelectedAmount);

			//Cheque
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0M, testBatch.CashSelectedAmount);
			AssertEquals(0M, testBatch.ChequeSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(300M, testBatch.TotalDepositOSAmount);

			AssertEquals(100M, testBatch.CashNotSelectedAmount);
			AssertEquals(200M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(300M, testBatch.TotalDepositNotSelectedAmount);

			//CreditCard
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0M, testBatch.CashSelectedAmount);
			AssertEquals(0M, testBatch.ChequeSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(0M, testBatch.TotalDepositOSAmount);

			AssertEquals(100M, testBatch.CashNotSelectedAmount);
			AssertEquals(200M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(600M, testBatch.TotalDepositNotSelectedAmount);

			//DirectCredit
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0M, testBatch.CashSelectedAmount);
			AssertEquals(0M, testBatch.ChequeSelectedAmount);
			AssertEquals(0M, testBatch.CreditCardSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditSelectedAmount);
			AssertEquals(0M, testBatch.TotalDepositOSAmount);

			AssertEquals(100M, testBatch.CashNotSelectedAmount);
			AssertEquals(200M, testBatch.ChequeNotSelectedAmount);
			AssertEquals(300M, testBatch.CreditCardNotSelectedAmount);
			AssertEquals(0M, testBatch.DirectCreditNotSelectedAmount);
			AssertEquals(600M, testBatch.TotalDepositNotSelectedAmount);
		}

		public void TestSelectedTransactionAmountWithForeignCurrency()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			testReceipt.AH_RX_NKTransactionCurrency = "USD";
			testReceipt.AH_ExchangeRate = 7;
			testReceipt.AH_OSTotal = 100M;
			testReceipt.AH_OutstandingAmount = 700M;
			testReceipt.AH_InvoiceAmount = 700M;

			var testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.USDBankAccount.PK);
			testReceipt2.AH_OSTotal = 200M;
			testReceipt2.AH_OutstandingAmount = 1400m;
			testReceipt2.AH_InvoiceAmount = 1400m;

			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);

			var testBatch1 = testDepositBatchParent.DepositBatchLines[0];
			testBatch1.IsSelected = true;
			AssertEquals("Should be based on local amount", -700m, testBatch1.CashSelectedAmount);

			var testBatch2 = testDepositBatchParent.DepositBatchLines[1];
			testBatch2.IsSelected = true;
			AssertEquals("Should be based on os amount", -200M, testBatch2.CashSelectedAmount);
		}

		public void TestSelectedTransactionCount()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.CreditCard, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.DirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 400M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt5 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.InterestReceived, ZArchitecture.Core.TransactionTypes.DirectReceipt, ZArchitecture.Core.LedgerTypes.CashBook, 500M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt6 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.EFT, ZArchitecture.Core.TransactionTypes.DirectReceipt, ZArchitecture.Core.LedgerTypes.CashBook, 600M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt7 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.eNettDirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 700M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			testBatch.IsSelected = false;
			AssertEquals(0, testBatch.CashTransactionSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(0, testBatch.TotalDepositCount);

			AssertEquals(1, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(3, testBatch.TotalDepositNotSelectedCount);

			testBatch.IsSelected = true;
			AssertEquals(1, testBatch.CashTransactionSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(3, testBatch.TotalDepositCount);

			AssertEquals(0, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(0, testBatch.TotalDepositNotSelectedCount);

			//Cash
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0, testBatch.CashTransactionSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(2, testBatch.TotalDepositCount);

			AssertEquals(1, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(1, testBatch.TotalDepositNotSelectedCount);

			//Cheque
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0, testBatch.CashTransactionSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(1, testBatch.TotalDepositCount);

			AssertEquals(1, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(2, testBatch.TotalDepositNotSelectedCount);

			//CreditCard
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0, testBatch.CashTransactionSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(0, testBatch.TotalDepositCount);

			AssertEquals(1, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(3, testBatch.TotalDepositNotSelectedCount);

			//DirectCredit
			foreach (DepositBatchTransactionLine line in testBatch.Transactions)
			{
				if (line.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit)
				{
					line.IsSelected = false;
					break;
				}
			}

			AssertEquals(0, testBatch.CashTransactionSelectedCount);
			AssertEquals(0, testBatch.ChequeTransactionSelectedCount);
			AssertEquals(0, testBatch.CreditCardTransactionSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionSelectedCount);
			AssertEquals(0, testBatch.TotalDepositCount);

			AssertEquals(1, testBatch.CashTransactionNotSelectedCount);
			AssertEquals(1, testBatch.ChequeTransactionNotSelectedCount);
			AssertEquals(1, testBatch.CreditCardTransactionNotSelectedCount);
			AssertEquals(0, testBatch.DirectCreditTransactionNotSelectedCount);
			AssertEquals(3, testBatch.TotalDepositNotSelectedCount);
		}

		public void TestReceiptReversingBatch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			Factory.Save();

			ReceiptReversing testRecRev = new ReceiptReversing(testReceipt);
			testRecRev.Reverse();
			Factory.Save();

			Assert(testReceipt.AH_IsCancelled);

			DepositBatch testSavedBatch = Factory.Load<DepositBatch>(testBatch.PK);
			AssertEquals(1, testSavedBatch.Transactions.Count);

			DepositBatchTransactionLine testLine = testSavedBatch.Transactions[0];

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testSavedBatch.AH_RX_NKTransactionCurrency);
			AssertEquals(100m, testSavedBatch.AH_OSTotal);
			AssertEquals(100m, testSavedBatch.AH_InvoiceAmount);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testSavedBatch.AH_PostDate.ToShortDateString());
			AssertEquals(ZDateTime.Today.ToShortDateString(), testSavedBatch.AH_InvoiceDate.ToShortDateString());
			AssertEquals(ZDateTime.Today.ToShortDateString(), testSavedBatch.AH_DueDate.ToShortDateString());
			AssertEquals(ZDateTime.Empty, testSavedBatch.AH_FullyPaidDate);

			AssertEquals(testReceipt.AH_AB, testSavedBatch.AH_AB);
			AssertEquals(testReceipt.AH_OH, testSavedBatch.AH_OH);

			AssertEquals(-100m, testLine.AH_OSTotal);
			AssertEquals(-100m, testLine.AH_InvoiceAmount);
		}

		public void TestCorrectSigns()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertEquals(-10M, testBatch.CorrectSigns_ForTestOnly(ZArchitecture.Core.TransactionTypes.Receipt, 10M));
			AssertEquals(10M, testBatch.CorrectSigns_ForTestOnly(ZArchitecture.Core.TransactionTypes.Payment, 10M));
		}

		public void TestBankChargeTypes_List()
		{
			DepositBatch testBatch = Factory.New<DepositBatch>();
			AssertNotNull("BankChargeTypes_List", testBatch.BankChargeTypes_List_ForTestOnly);
			AssertEquals("LookupEditType", OLookUpEditType.BankChargeTypes, testBatch.BankChargeTypes_List_ForTestOnly.LookupEditType);
		}

		public override void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties().Count", 0, ((DepositBatch)Header).GetWritableProperties_ForTestOnly().Count);
		}

		#region Base Override

		protected override Type TypeOfValidation
		{
			get { return typeof(DepositBatchValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 0M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch batch = Factory.New<DepositBatch>();
			batch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			batch.LoadTransactions(ZGuid.Empty);
			batch.IsSelected = true;
			return batch;
		}

		protected override void ForceSavingByFactoryIfApplicable(AccTransactionHeader header)
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);

			((DepositBatch)header).ReversingReceipt = testReceipt;
			base.ForceSavingByFactoryIfApplicable(header);
		}

		#endregion
	}
}
