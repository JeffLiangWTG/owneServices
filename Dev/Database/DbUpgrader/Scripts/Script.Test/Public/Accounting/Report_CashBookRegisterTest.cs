using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_CashBookRegister))]
	class Report_CashBookRegisterTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var bankAccountPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC) VALUES ('{0}', 'BANKCDE', 'AUD', '{1}', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", bankAccountPK, GetFirstGLAccount()));

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.AccTransactionHeader 
(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_OH, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate,
	AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal)
VALUES (newID(), 'AP','PAY','00005013', '1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{0}', 'May 25 2005  3:44:00:000PM',
	'USD', 0.500000000, -100.0000, -50.0000)", bankAccountPK));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from Report_CashBookRegister('{0}', 'May 20 2005', 'May 31 2005', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", bankAccountPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", (string)result.Rows[0]["BankCode"]);
			AssertEquals("TransactionFilter", "PAY, DPY, OPY", (string)result.Rows[0]["TransactionFilter"]);
			AssertEquals("TransactionType", "PAY", (string)result.Rows[0]["TransactionType"]);
			AssertEquals("TransactionNumber", "00005013", (string)result.Rows[0]["TransactionNumber"]);
			AssertEquals("OSDebit", 100m, (decimal)result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[0]["OSCredit"]);
			AssertEquals("OSRealTotal", 100m, (decimal)result.Rows[0]["OSRealTotal"]);
			AssertEquals("ExchangeRate", 1m, (decimal)result.Rows[0]["ExchangeRate"]);
			AssertEquals("LocalDebit", 100m, (decimal)result.Rows[0]["LocalDebit"]);
			AssertEquals("LocalCredit", DBNull.Value, result.Rows[0]["LocalCredit"]);
			AssertEquals("LocalRealAmount", 100m, (decimal)result.Rows[0]["LocalRealAmount"]);

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.AccTransactionHeader 
(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_OH, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_PostDate, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal, AH_ReceiptBatchNo)
VALUES (newID(), 'AR','REC','00005014', '1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{0}', 'May 19 2005  3:45:00:000PM', 'May 19 2005  3:45:00:000PM', 'USD', 0.500000000, -2000.0000, -1000.0000, '00005015')", bankAccountPK));
			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.AccTransactionHeader 
(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_OH, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_PostDate,
	AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal)
VALUES (newID(), 'CB','RCB','00005015', '1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{0}', 'May 20 2005  3:50:00:000PM', 'May 21 2005  3:50:00:000PM', 'AUD', 0.500000000, -2000.0000, -2000.0000)", bankAccountPK));

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from Report_CashBookRegister('{0}', 'May 20 2005', 'May 31 2005', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') where TransactionFilter = 'RCB, REC, DRC, ORC' and SortColumn = '1900-01-02'", bankAccountPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("OSDebit", DBNull.Value, result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", 2000m, (decimal)result.Rows[0]["OSCredit"]);
			AssertEquals("OSRealTotal", -2000m, (decimal)result.Rows[0]["OSRealTotal"]);
			AssertEquals("ExchangeRate", 0m, (decimal)result.Rows[0]["ExchangeRate"]);
			AssertEquals("LocalDebit", DBNull.Value, result.Rows[0]["LocalDebit"]);
			AssertEquals("LocalCredit", 2000m, (decimal)result.Rows[0]["LocalCredit"]);
			AssertEquals("LocalRealAmount", -2000m, (decimal)result.Rows[0]["LocalRealAmount"]);
		}

		public void TestDoNotOverflowWithBigSummedAmount()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");
			helper.InsertAccPeriod(2018, 06, TestDbHelper.DefaultCompanyPK);

			var postDateTime = new DateTime(2018, 06, 25);

			var glAccount1 = helper.InsertGLAccount("TEST1", "TEST1");
			var bankAccountPK1 = helper.InsertBankAccount("ZBA", glAccount1);
			var headerPK1 = helper.InsertTransactionHeader("AR", "REC", "001", 560000000000000M, postDateTime, currentBranch, currentDep, bankAccountPK1, postToGL: false);
			var headerPK2 = helper.InsertTransactionHeader("AR", "REC", "002", 700000000000000M, postDateTime, currentBranch, currentDep, bankAccountPK1, postToGL: false);

			var sqlQuery = "SELECT * FROM Report_CashBookRegister(@BankPK, @FromDate, @ToDate, @Company)";

			using (var cmd = Db.Connection.Command(sqlQuery))
			{
				cmd.AddParameter("@BankPK", System.Data.SqlDbType.UniqueIdentifier, bankAccountPK1);
				cmd.AddParameter("@FromDate", System.Data.SqlDbType.DateTime, postDateTime.AddDays(-1));
				cmd.AddParameter("@ToDate", System.Data.SqlDbType.DateTime, postDateTime.AddDays(1));
				cmd.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);

				DataTable result = null;

				AssertNoExceptionThrown(() => result = DataUtils.GetDataTableFromCommand(cmd));
				AssertNotNull(result);
				AssertEquals("Result should have one row", 1, result.Rows.Count);
				AssertEquals("OSCredit", 1260000000000000M, result.Rows[0]["OSCredit"]);
				AssertEquals("OSRealTotal", -1260000000000000M, result.Rows[0]["OSRealTotal"]);
				AssertEquals("LocalCredit", 1260000000000000M, result.Rows[0]["LocalCredit"]);
				AssertEquals("LocalRealAmount", -1260000000000000M, result.Rows[0]["LocalRealAmount"]);
			}
		}

		public void TestTransactionBelongsToGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var localCompany = helper.InsertCompany("DAU", "AUD DEMO", "AUD", "AU", false, false);
			var localBank = helper.InsertBankAccount("A01AUD", helper.GLAccountPK1, currency: "AUD");
			var postDate = DateTime.Now;
			var pk1 = helper.InsertTransactionHeader("CB", "EXX", "000001", 100m, postDate,
							category: "REA",
							companyPK: localCompany,
							bankAccountPK: localBank,
							exchangeRate: 1m,
							osAmount: 100m);
			var pk2 = helper.InsertTransactionHeader("CB", "TRF", "000001", 100m, postDate,
							companyPK: localCompany,
							bankAccountPK: localBank,
							exchangeRate: 1m,
							osAmount: 100m);
			var pk3 = helper.InsertTransactionHeader("CB", "DPY", "000001", 100m, postDate,
							companyPK: localCompany,
							bankAccountPK: localBank,
							exchangeRate: 1m,
							osAmount: 100m);
			var belongTo = Guid.NewGuid();
			var setBelongToSql = $"UPDATE AccTransactionHeader SET AH_TransactionBelongsToGroup='{belongTo}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK in ('{pk1}','{pk2}','{pk3}')";
			TestConnection.ExecuteNonQuery(setBelongToSql);

			var sqlQuery = "SELECT * FROM Report_CashBookRegister(@BankPK, @FromDate, @ToDate, @Company)";
			using (var cmd = Db.Connection.Command(sqlQuery))
			{
				cmd.AddParameter("@BankPK", SqlDbType.UniqueIdentifier, localBank);
				cmd.AddParameter("@FromDate", SqlDbType.DateTime, postDate.AddDays(-1));
				cmd.AddParameter("@ToDate", SqlDbType.DateTime, postDate.AddDays(1));
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, localCompany);

				DataTable result = null;

				AssertNoExceptionThrown(() => result = DataUtils.GetDataTableFromCommand(cmd));
				AssertNotNull(result);
				AssertEquals("Result should have one row", 3, result.Rows.Count);

				var transactionDPY = result.Select($"TransactionType='DPY'");
				var transactionEXX = result.Select($"TransactionType='EXX'");
				var transactionTRF = result.Select($"TransactionType='TRF'");

				AssertType<DBNull>(transactionDPY[0]["AH_TransactionBelongsToGroup"]);
				AssertEquals(belongTo, transactionEXX[0]["AH_TransactionBelongsToGroup"]);
				AssertEquals(belongTo, transactionTRF[0]["AH_TransactionBelongsToGroup"]);
			}
		}

		Guid GetFirstGLAccount()
		{
			string sQL = "SELECT TOP 1 AG_PK FROM dbo.AccGLHeader";
			return (Guid)TestConnection.ExecuteScalar(sQL);
		}
	}
}

