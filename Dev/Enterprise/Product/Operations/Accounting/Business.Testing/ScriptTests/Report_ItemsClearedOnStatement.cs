using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ItemsClearedOnStatement : ScriptTest
	{
		[TestDate(2014, 07, 10)]
		[SuspendCriticalValidation]
		public void TestCorrectAmountSignsForDifferentTransactionTypes()
		{
			SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 140.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, 20.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, -30.0m, TestBank, Factory, "");

			var receiptBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -200.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 150.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, -320.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, 20.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, -50.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 600.0m, TestBank, Factory, "");

			Factory.Save();

			var results = RunScript(TypeOfCleared.DEPOSITS);
			var expectedResult = @"
Date                    TransactionType TransactionNum       ReceiptType Reference            BatchPayment                                                                                         Ledger BranchCode OSTotal
----------------------- --------------- -------------------- ----------- -------------------- ---------------------------------------------------------------------------------------------------- ------ ---------- ---------------------
2014-07-10 00:00:00     ORC             CBORC-320.0                                           Opening Receipt                                                                                      CB     BNE        320.00
2014-07-10 00:00:00     ORC             CBORC20.0                                             Opening Receipt                                                                                      CB     BNE        -20.00
2014-07-10 00:00:00     RCB             CBRCB0.0                                              Deposit Batch No. CBRCB0.0                                                                           CB     BNE        0.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("TypeOfCleared.DEPOSITS", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			results = RunScript(TypeOfCleared.PAYMENTS);
			expectedResult = @"
Date                    TransactionType TransactionNum       ReceiptType Reference            BatchPayment                                                                                         Ledger BranchCode OSTotal
----------------------- --------------- -------------------- ----------- -------------------- ---------------------------------------------------------------------------------------------------- ------ ---------- ---------------------
2014-07-10 00:00:00     PAY             APPAY140.0                                            NULL                                                                                                 AP     BNE        -140.00
2014-07-10 00:00:00     DPY             CBDPY-120.0                                                                                                                                                CB     BNE        -120.00
2014-07-10 00:00:00     DPY             CBDPY20.0                                                                                                                                                  CB     BNE        20.00
2014-07-10 00:00:00     OPY             CBOPY-50.0                                            NULL                                                                                                 CB     BNE        50.00
2014-07-10 00:00:00     OPY             CBOPY500.0                                            NULL                                                                                                 CB     BNE        -500.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("TypeOfCleared.PAYMENTS", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			results = RunScript(TypeOfCleared.TRANSFERS);
			expectedResult = @"
Date                    TransactionType TransactionNum       ReceiptType Reference            BatchPayment                                                                                         Ledger BranchCode OSTotal
----------------------- --------------- -------------------- ----------- -------------------- ---------------------------------------------------------------------------------------------------- ------ ---------- ---------------------
2014-07-10 00:00:00     TRF             CBTRF-30.0                                            Bank Transfer                                                                                        CB     BNE        -30.00
2014-07-10 00:00:00     TRF             CBTRF430.0                                            Bank Transfer                                                                                        CB     BNE        430.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("TypeOfCleared.TRANSFERS", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		[TestDate(2014, 07, 10)]
		[SuspendCriticalValidation]
		public void TestCorrectAmountSignsForPaymentDetailsAndBatch()
		{
			TestBank.AB_ShowDetailsOnDirectDebits = false;
			var paymentBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DDRBatch, -20.0m, TestBank, Factory, "");
			var paymentTransaction = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -20.0m, TestBank, Factory, paymentBatch.AH_TransactionNum);
			paymentBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;
			paymentTransaction.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			Factory.Save();

			TestBank.AB_ShowDetailsOnDirectDebits = true;
			paymentBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DDRBatch, -30.0m, TestBank, Factory, "");
			paymentTransaction = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -30.0m, TestBank, Factory, paymentBatch.AH_TransactionNum);
			paymentBatch.AH_ReceiptType = ReceiptTypes.NonRolledUpBatch;
			paymentTransaction.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var results = RunScript(TypeOfCleared.PAYMENTS);
			var expectedResult = @"
Date                    TransactionType TransactionNum       ReceiptType Reference            BatchPayment                                                                                         Ledger BranchCode OSTotal
----------------------- --------------- -------------------- ----------- -------------------- ---------------------------------------------------------------------------------------------------- ------ ---------- ---------------------
2014-07-10 00:00:00     DDB             CBDDB-20.0           DDR                              DDR Batch No. CBDDB-20.0                                                                             CB     BNE        20.00
2014-07-10 00:00:00     DPY             CBDPY-30.0           DDR                                                                                                                                   CB     BNE        -30.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("TypeOfCleared.PAYMENTS", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		#region Implementation

		AccBankAccount TestBank
		{
			get
			{
				if (testBank == null)
				{
					testBank = new BusinessObjectFactory().New<AccBankAccount>();

					testBank.AB_AccountNum = "Bank123";
					testBank.AB_GC = GlbCompany.CurrentCompany.PK;
					testBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					testBank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
					testBank.AB_LastStatementDate = ZDateTime.Now;
					testBank.AB_Code = "ABCBANK";
					testBank.Factory.Save();

					testBank = Factory.Load<AccBankAccount>(testBank.PK);
				}

				return testBank;
			}
		}
		AccBankAccount testBank;

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, AccBankAccount fBankAccount, BusinessObjectFactory factory, string receiptBatchNo)
		{
			AccTransactionHeader transaction = factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_OSTotal = localAmount;

			//Mandatory values
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_PostDate = ZDateTime.Now;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;

			//Others
			transaction.AH_TransactionNum = ledger + transactionType + localAmount;
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;
			transaction.AH_DateClearedInCashbook = TestBank.AB_LastStatementDate;

			return transaction;
		}

		DataTable RunScript(TypeOfCleared typeOfCleared)
		{
			return RunScript(TestBank, typeOfCleared);
		}

		DataTable RunScript(AccBankAccount bank, TypeOfCleared typeOfCleared)
		{
			var sql = string.Format(@"
						SELECT *
						FROM 
							Report_ItemsClearedOnStatement(
								'{0}' --@Bank 
								,'{1}' --@TypeOfCleared
							)
						ORDER BY TransactionNum",
					bank.PK
					,typeOfCleared.ToString()
				);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		enum TypeOfCleared
		{
			DEPOSITS,
			PAYMENTS,
			TRANSFERS
		}

		#endregion
	}
}

