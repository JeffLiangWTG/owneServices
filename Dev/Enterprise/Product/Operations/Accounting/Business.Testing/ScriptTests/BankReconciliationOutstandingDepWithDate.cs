using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class BankReconciliationOutstandingDepWithDate : ScriptTest
	{
		[TestDate(2014, 07, 10)]
		public void TestStatements()
		{
			var statementTypes = new BusinessObjectFactory().New<Statement>().Lookups.AS_Type_List;
			var amountDifference = 4m;
			var amount = 10;
			foreach (CodeDescriptionPair pair in statementTypes)
			{
				SetStatement("DR", pair.Code, amount, "TEST1", TestBank, Factory);
				SetStatement("CR", pair.Code, amount + amountDifference, "TEST2", TestBank, Factory);
				amount += 10;
			}
			Factory.Save();

			var results = RunScript(TestBank);
			var expectedResult = @"
Source    Date                    TransactionType TransactionNum       ReceiptType Reference                 Amount1               StatementAmount       BatchPayment                                                                                         Ledger BranchCode
--------- ----------------------- --------------- -------------------- ----------- ------------------------- --------------------- --------------------- ---------------------------------------------------------------------------------------------------- ------ ----------
STATEMENT 2014-07-10 00:00:00                                          AMF         TEST2                     0.00                  104.00
STATEMENT 2014-07-10 00:00:00                                          BDP         TEST2                     0.00                  124.00
STATEMENT 2014-07-10 00:00:00                                          BDT         TEST2                     0.00                  114.00
STATEMENT 2014-07-10 00:00:00                                          DCR         TEST1                     0.00                  -30.00
STATEMENT 2014-07-10 00:00:00                                          DCR         TEST2                     0.00                  34.00
STATEMENT 2014-07-10 00:00:00                                          INR         TEST2                     0.00                  144.00
STATEMENT 2014-07-10 00:00:00                                          INT         TEST2                     0.00                  134.00
STATEMENT 2014-07-10 00:00:00                                          MSF         TEST2                     0.00                  184.00
STATEMENT 2014-07-10 00:00:00                                          MSR         TEST2                     0.00                  174.00
STATEMENT 2014-07-10 00:00:00                                          PPY         TEST2                     0.00                  154.00
STATEMENT 2014-07-10 00:00:00                                          RCB         TEST1                     0.00                  -80.00
STATEMENT 2014-07-10 00:00:00                                          RCB         TEST2                     0.00                  84.00
STATEMENT 2014-07-10 00:00:00                                          STD         TEST2                     0.00                  164.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		[TestDate(2020, 03, 02)]
		public void TestTotalAmountWithReciept()
		{
			SetReceiptAndBatchTransactions(TestBank, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -700m, -100m, "Test1", "Test111");

			var results = RunScript(TestBank);
			var expectedResult = @"
Source    Date                    TransactionType TransactionNum       ReceiptType Reference                 Amount1               StatementAmount       BatchPayment                                                                                         Ledger BranchCode
--------- ----------------------- --------------- -------------------- ----------- ------------------------- --------------------- --------------------- ---------------------------------------------------------------------------------------------------- ------ ----------
CASHBOOK    2020-03-02 00:00:00                    RCB Test111        Test1                 700.00               0.00       Deposit Batch No. Test111                                                                                         CB BNE 
";

			AssertTableAsTextFromSQLServerManagenentStudio("", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());

			var testBank1 = TestObjectCreator.USDBankAccount;
			SetReceiptAndBatchTransactions(testBank1, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -7m, -1m, "Test2", "Test222");

			results = RunScript(testBank1);
			expectedResult = @"
Source    Date                    TransactionType TransactionNum       ReceiptType Reference                 Amount1               StatementAmount       BatchPayment                                                                                         Ledger BranchCode
--------- ----------------------- --------------- -------------------- ----------- ------------------------- --------------------- --------------------- ---------------------------------------------------------------------------------------------------- ------ ----------
CASHBOOK    2020-03-02 00:00:00                    RCB Test222        Test2                 1.00               0.00       Deposit Batch No. Test222                                                                                         CB BNE 
";
			AssertTableAsTextFromSQLServerManagenentStudio("", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		void SetReceiptAndBatchTransactions(AccBankAccount bank, string receiptLedger, string receiptTransactionType, decimal localAmount, decimal osAmount, string reference, string batchNumber)
		{
			var receipt = SetTransaction(receiptLedger, receiptTransactionType, localAmount, osAmount, bank, Factory, batchNumber);
			receipt.AH_ChequeOrReference = reference;
			var batch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, -70.0m, -10m, bank, Factory, "");
			batch.AH_TransactionNum = batchNumber;
			batch.AH_PostDate = ZDateTime.Today;
			batch.AH_ChequeOrReference = reference;

			Factory.Save();
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, decimal osAmount, AccBankAccount fBankAccount, BusinessObjectFactory factory, string receiptBatchNo)
		{
			var transaction = factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_OSTotal = osAmount;
			transaction.AH_InvoiceDate = ZDateTime.Today;
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transaction.AH_TransactionNum = ledger + transactionType + localAmount;
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;

			return transaction;
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
					testBank.AB_Code = "ABCBANK";
					testBank.Factory.Save();

					testBank = Factory.Load<AccBankAccount>(testBank.PK);
				}

				return testBank;
			}
		}
		AccBankAccount testBank;

		Statement SetStatement(string debitCredit, string type, decimal amount, string reference, AccBankAccount fBankAccount, BusinessObjectFactory factory)
		{
			Statement testStatment = factory.New(typeof(Statement)) as Statement;

			testStatment.AS_AB = fBankAccount.PK;
			testStatment.AS_DebitCredit = debitCredit;
			testStatment.AS_Type = type;
			testStatment.AS_Amount = amount;
			testStatment.AS_ChequeOrReference = reference;
			testStatment.AS_StatementDate = ZDateTime.Now;

			return testStatment;
		}

		DataTable RunScript(AccBankAccount bankAccount)
		{
			return RunScript(bankAccount, ZDateTime.Now.AddMinutes(1), ZDateTime.Now.AddMinutes(1));
		}

		DataTable RunScript(AccBankAccount bank, ZDateTime reconcileDate, ZDateTime satementDate)
		{
			var sql = string.Format(@"
						SELECT
							Source
							,Date
							,TransactionType
							,TransactionNum
							,ReceiptType
							,Reference
							,Amount1
							,StatementAmount
							,BatchPayment
							,Ledger
							,BranchCode
						FROM 
							BankReconciliationOutstandingDepWithDate(
								'{0}' --@Bank 
								,'{1}' --@Company
								,'{2}' --@ReconcileDate
								,'{3}'  --@StatementDate
							)
						ORDER BY ReceiptType, Reference",
					bank.PK
					,GlbCompany.CurrentCompany.PK
					,reconcileDate.ToISO8601String()
					,satementDate.ToISO8601String()
				);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}

