using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_Receivables_Receipt_AnalysisTest : ScriptTest
	{
		public void TestSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 10M, TestObjectCreator.AUDBankAccount.PK).AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 20M, TestObjectCreator.AUDBankAccount.PK).AH_OH = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 30M, TestObjectCreator.AUDBankAccount.PK).AH_OH = TestObjectCreator.LocalClient2.PK;
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select(string.Format("SettleGroupOH_PK = '{0}'", TestObjectCreator.ABIGAS.PK)).Length);
		}
		public void TestTransactionCategory()
		{
			DataTable dt = RunScript();
			AssertEquals("Before Saving Transactions", 0, dt.Rows.Count);

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("000123AR", TestObjectCreator.AUD, 1.00m, TestObjectCreator.Debtor);
			arInvoice1.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.Clearing;
			var receipt = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice1, ZDateTime.Today);
			receipt.AH_ReceiptType = ReceiptTypes.Cash;
			receipt.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			receipt.AH_OH = TestObjectCreator.AALSHI.PK;
			receipt.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.TransactionAlreadyPaid;

			Factory.Save();

			dt = RunScript();
			AssertEquals("After Saving Transactions", 1, dt.Rows.Count);
			Assert("TransactionCategory Columns Should be available", dt.Columns.Contains("TransactionCategory"));
			AssertEquals("Transaction Category of Invoice should be the Transaction Category for the report", Core.Constants.TransactionCategory.Codes.Clearing, dt.Rows[0]["TransactionCategory"].ToString());
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_Receivables_Receipt_Analysis(
'{0}'	--@Company
)  
",
			GlbCompany.CurrentCompany.PK
			));
		}
	}
}


