using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CashBookDepositBatchListingReport : ScriptTest
	{
		[TestDate(2020, 03, 02)]
		public void TestTotalAmountWithReciept()
		{
			var testBank1 = TestObjectCreator.USDBankAccount;
			var testBank2 = TestObjectCreator.AUDBankAccount;

			SetReceiptAndBatchTransactions(testBank1, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -700m, -100m, "Test1", "111");
			SetReceiptAndBatchTransactions(testBank2, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -7m, -1m, "Test2", "222");

			var results = RunScript("111","222");
			var expectedResult = @"
AH_RECEIPTBATCHNO   AB_CODE   AH_TRANSACTIONTYPE   AH_TRANSACTIONNUM   AH_RECEIPTTYPE   AH_CHEQUEORREFERENCE   AH_LEDGER   GB_CODE   AH_INVOICEDATE   AH_POSTDATE   Amount   RX_CODE   AH_DESC   BATCHDATE
--- --- --- --- --- --- --- --- --- --- --- --- --- ---
111   ZHSBCUSD   REC   ARREC-700      Test1   AR   BNE   2020-03-02 00:00:00   2020-03-02 00:00:00   100.00   USD      2020-03-02 00:00:00
222   ZHSBCAUD   REC   ARREC-7      Test2   AR   BNE   2020-03-02 00:00:00   2020-03-02 00:00:00   7.00   AUD      2020-03-02 00:00:00
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
			AccTransactionHeader transaction = factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

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

		DataTable RunScript(ZString batchFrom, ZString batchTo)
		{
			var sql = string.Format(@"
						SELECT
							AH_RECEIPTBATCHNO
							,AB_CODE
							,AH_TRANSACTIONTYPE
							,AH_TRANSACTIONNUM
							,AH_RECEIPTTYPE
							,AH_CHEQUEORREFERENCE
							,AH_LEDGER
							,GB_CODE
							,AH_INVOICEDATE
							,AH_POSTDATE
							,Amount
							,RX_CODE
							,AH_DESC
							,BATCHDATE
						FROM 
							Report_CashBookDepositBatchListingReport(
								'{0}' --@Company
								,'{1}' --@BATCHFROM
								,'{2}'  --@BATCHTO
							)
						ORDER BY AH_RECEIPTBATCHNO",
					GlbCompany.CurrentCompany.PK
					, batchFrom
					, batchTo
				);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}
