using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ReceiptListing : ScriptTest
	{
		[TestDate(2020, 03, 02)]
		public void TestTotalAmountWithReciept()
		{
			var testBank1 = TestObjectCreator.USDBankAccount;
			var testBank2 = TestObjectCreator.AUDBankAccount;

			SetReceiptTransactions(testBank1, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -700m, -100m, "Test1");
			SetReceiptTransactions(testBank2, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -7m, -1m, "Test2");

			var results = RunScript(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var expectedResult = @"
Cheque   InvoiceDate   IsCancelled   PostDate   Ledger   Type   ReceiptPaymentType   Number    Description   Currency   Amount
--- --- --- --- --- --- --- --- --- --- ---
Test1   2020-03-02 00:00:00   0   2020-03-02 00:00:00   AR   REC      ARREC-700         100.00
Test2   2020-03-02 00:00:00   0   2020-03-02 00:00:00   AR   REC      ARREC-7        7.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		void SetReceiptTransactions(AccBankAccount bank, string receiptLedger, string receiptTransactionType, decimal localAmount, decimal osAmount, string reference)
		{
			var receipt = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			receipt.AH_Ledger = receiptLedger;
			receipt.AH_TransactionType = receiptTransactionType;
			receipt.AH_InvoiceAmount = localAmount;
			receipt.AH_OutstandingAmount = localAmount;
			receipt.AH_OSTotal = osAmount;
			receipt.AH_InvoiceDate = ZDateTime.Today;
			receipt.AH_PostDate = ZDateTime.Today;
			receipt.AH_GC = GlbCompany.CurrentCompany.PK;
			receipt.AH_GB = GlbBranch.CurrentBranch.PK;
			receipt.AH_GE = GlbDepartment.CurrentDepartment.PK;
			receipt.AH_TransactionNum = receiptLedger + receiptTransactionType + localAmount;
			receipt.AH_AB = bank.PK;
			receipt.AH_ChequeOrReference = reference;
			Factory.Save();
		}

		#region Implementation

		DataTable RunScript(ZDateTime postDateFrom, ZDateTime postDateTo)
		{
			var sql = string.Format(@"
						SELECT
							Cheque
							,InvoiceDate
							,IsCancelled
							,PostDate
							,Ledger
							,Type
							,ReceiptPaymentType
							,Number
							,Description
							,Currency
							,Amount
						FROM 
							Report_ReceiptListing(
								'{0}' --@Company
								,'{1}' --@ReconcileDate
								,'{2}'  --@StatementDate
							)
						Order By Cheque",
					GlbCompany.CurrentCompany.PK
					, postDateFrom.ToISO8601String()
					, postDateTo.ToISO8601String()
				);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}
