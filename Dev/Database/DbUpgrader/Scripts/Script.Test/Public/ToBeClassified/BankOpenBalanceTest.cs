using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(BankOpenBalance))]
	class BankOpenBalanceTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			Guid bankAccountPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC) VALUES ('{0}', 'BANKCDE', 'AUD', '{1}', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", bankAccountPK, GetFirstGLAccount()));

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.AccTransactionHeader 
(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_OH, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_PostDate,
	AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_InvoiceAmount, AH_OSTotal)
VALUES (newID(), 'AP','PAY','00005013', '1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{0}', 'May 25 2005  3:44:00:000PM', 'May 25 2005  3:44:00:000PM',
	'USD', 0.500000000, -100.0000, -50.0000)", bankAccountPK));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from BankOpenBalance('{0}', 'May 31 2005', '')", bankAccountPK));
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", (string)result.Rows[0]["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", 100m, (decimal)result.Rows[0]["AccumulatedBankOSAmount"]);
			AssertEquals("AllOpeningPaymentsAndReceiptsOSAmount", DBNull.Value, result.Rows[0]["AllOpeningPaymentsAndReceiptsOSAmount"]);
			AssertEquals("AllOpeningPaymentsAndReceiptsLocalAmount", DBNull.Value, result.Rows[0]["AllOpeningPaymentsAndReceiptsLocalAmount"]);
			AssertEquals("BankOpenBalance", 0m, (decimal)result.Rows[0]["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, (decimal)result.Rows[0]["LocalBankOpenBalance"]);
		}

		Guid GetFirstGLAccount()
		{
			string sQL = "SELECT TOP 1 AG_PK FROM dbo.AccGLHeader";
			return (Guid)TestConnection.ExecuteScalar(sQL);
		}
	}
}

