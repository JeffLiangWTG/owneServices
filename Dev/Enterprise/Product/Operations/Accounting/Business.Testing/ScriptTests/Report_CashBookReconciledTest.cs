using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CashBookReconciledTest : ScriptTest
	{
		public void TestForeignPayTransactionsWithLocalAmount()
		{
			var payment1 = TestObjectCreator.CreateAPPayment(0.9m, -900m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.AALSHI.PK, TestObjectCreator.USDBankAccount.PK);
			payment1.AH_ChequeOrReference = "0001001";
			payment1.AH_RX_NKTransactionCurrency = "USD";
			payment1.AH_ExchangeRate = 0.9m;

			var payment2 = TestObjectCreator.CreateAPPayment(0.9m, 900m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
			payment2.AH_ChequeOrReference = "0001002";
			payment2.AH_RX_NKTransactionCurrency = "USD";
			payment2.AH_ExchangeRate = 0.9m;

			var payment3 = TestObjectCreator.CreateAPPayment(1m, 900m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
			payment3.AH_ChequeOrReference = "0001003";
			payment3.AH_RX_NKTransactionCurrency = "AUD";

			Factory.Save();

			var headers = new[] { "Reference", "OSDebit", "OSCredit" };
			var result = RunScript();
			var lines = new object[][]
						{
							new object[] {	"0001001",		900m,		DBNull.Value },
							new object[] {	"0001002",		DBNull.Value,			1000m },
							new object[] {	"0001003",		DBNull.Value,			900m },
						};
			AssertDataTableAllRowsByKeyColumns("Foreign Pay Transactions with Local Amount", result, headers, lines, headers);
		}

		DataTable RunScript()
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false);
		}

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo, bool outstandingWIP, bool outstandingACR)
		{
			string sql = string.Format("SELECT * FROM Report_CashBookReconciled('{0}')", GlbCompany.CurrentCompany.PK);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
