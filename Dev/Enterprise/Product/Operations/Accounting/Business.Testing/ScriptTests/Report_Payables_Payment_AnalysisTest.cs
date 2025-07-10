using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_Payables_Payment_AnalysisTest : ScriptTest
	{
		public void TestSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 10M, TestObjectCreator.AALSHI);
			var payment = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice);
			payment.AH_ReceiptType = ReceiptTypes.Cash;
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_OH = TestObjectCreator.AALSHI.PK;

			TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 20M, TestObjectCreator.AUDBankAccount.PK).AH_OH = TestObjectCreator.Creditor1.PK;
			TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 30M, TestObjectCreator.AUDBankAccount.PK).AH_OH = TestObjectCreator.Creditor2.PK;

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			var company = GlbCompany.CurrentCompany.PK;

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has AALSHI as settlemet code", 3, resultForSettlementGroup.Select(string.Format("SettleGroupOH_PK = '{0}'", TestObjectCreator.AALSHI.PK)).Length);

			//couldn't get it to come up as yes, i think this is enough though
			AssertEquals("must have FullMatched column", "NO", resultForSettlementGroup.Rows[0]["FullMatched"]);
			AssertEquals("must have FullMatched column", "NO", resultForSettlementGroup.Rows[1]["FullMatched"]);
			AssertEquals("must have FullMatched column", "YES", resultForSettlementGroup.Rows[2]["FullMatched"]);
		}

		public void TestTransactionCategory()
		{
			DataTable dt = RunScript();
			AssertEquals("Before Saving Transactions", 0, dt.Rows.Count);

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 10M, TestObjectCreator.AALSHI);
			invoice.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.Clearing;
			var payment = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice);
			payment.AH_ReceiptType = ReceiptTypes.Cash;
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_OH = TestObjectCreator.AALSHI.PK;
			payment.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.TransactionAlreadyPaid;

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
FROM Report_Payables_Payment_Analysis(
'{0}'	--@Company
) ORDER BY LocalEquiv 
",
			GlbCompany.CurrentCompany.PK
			));
		}
	}
}


