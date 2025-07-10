using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(csfn_APTransactionsDetail))]
	class csfn_APTransactionsDetailTest : DbCreateScriptTest
	{
		public void TestAPInvoiceFullyPaidInSubsequentPeriod()
		{
			Guid invoicePK = Guid.NewGuid();
			string invoiceDate = "2012-12-15 12:00:00";
			Guid paymentPK = Guid.NewGuid();
			string paymentDate = "2013-01-11 00:00:00";

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company)
	VALUES (NEWID(),201306,2013,'2012-12-01 00:00:00','2012-12-31 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company)
	VALUES (NEWID(),201307,2013,'2013-01-01 00:00:00','2013-01-31 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109',1,'AP INVOICE','{1}',-300.00,-30.00,-330.00,'NZD',1.000000000,'{1}','{3}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{2}','AP','PAY','00001093',1,'AP PAYMENT','{3}',330.00,0,330.00,'AUD',1.000000000,'{3}','{3}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), -300.00, 'M00001134', '{3}', '{0}')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), 300.00, 'M00001134', '{3}', '{2}')",
invoicePK, invoiceDate, paymentPK, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('201306', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2013-01-11 12:00:00')");
			AssertEquals("Result should have 1 row because the invoice is outstanding in period 201306", 1, result.Rows.Count);
			AssertEquals("AP Outstanding Balance", -330m, (decimal)result.Rows[0]["AH_Balance"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('201307', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2013-01-11 12:00:00')");
			AssertEquals("Result should have no rows because the invoice is fully paid in period 201307", 0, result.Rows.Count);
		}

		public void TestAH_BalanceAndAH_BalanceInLocalWithOtherTaxes_PartlyPaid()
		{
			Guid invoicePK = Guid.NewGuid();
			string invoiceDate = "2020-06-15 12:00:00";
			Guid paymentPK = Guid.NewGuid();
			string paymentDate = "2020-06-25 00:00:00";

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company)
	VALUES (NEWID(),202006,2020,'2020-06-01 00:00:00','2020-06-30 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109',1,'AP INVOICE','{1}',-1000.00,-100.00,-200.00,-2600.00,'USD',2.000000000,'{1}','',-500,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{2}','AP','INV','00001093',1,'AP PAYMENT','{3}',300,0,0,600,'USD',2.000000000,'{3}','',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), -300.00, 'M00001134', '{3}', '{0}')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), 300.00, 'M00001134', '{3}', '{2}')",
invoicePK, invoiceDate, paymentPK, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-07-15 00:00:00')");
			AssertEquals(-2000m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-1000m, result.Rows[0]["AH_BalanceInLocal"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-06-15 00:00:00')");
			AssertEquals(-1000m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-500m, result.Rows[0]["AH_BalanceInLocal"]);
		}

		public void TestAH_BalanceAndAH_BalanceInLocalWithOtherTaxes_Unpaid()
		{
			Guid invoicePK = Guid.NewGuid();
			string invoiceDate = "2020-06-15 12:00:00";
			Guid paymentPK = Guid.NewGuid();
			string paymentDate = "2020-06-25 00:00:00";

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company)
	VALUES (NEWID(),202006,2020,'2020-06-01 00:00:00','2020-06-30 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109',1,'AP INVOICE','{1}',-1000.00,-100.00,-200.00,-650.00,'USD',2.000000000,'{1}','',-1300,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')",
invoicePK, invoiceDate, paymentPK, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-07-15 00:00:00')");
			AssertEquals(-650m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-1300m, result.Rows[0]["AH_BalanceInLocal"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-06-15 00:00:00')");
			AssertEquals(-650m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-1300m, result.Rows[0]["AH_BalanceInLocal"]);
		}

		public void TestAH_BalanceAndAH_BalanceInLocalWithOtherTaxesWhenAH_ExchangeRateEqualOne()
		{
			Guid invoicePK = Guid.NewGuid();
			string invoiceDate = "2020-06-15 12:00:00";
			Guid paymentPK = Guid.NewGuid();
			string paymentDate = "2020-06-25 00:00:00";

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company)
	VALUES (NEWID(),202006,2020,'2020-06-01 00:00:00','2020-06-30 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109',1,'AP INVOICE','{1}',-1000.00,-100.00,-200.00,-1300.00,'AUD',1.000000000,'{1}','',-500,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{2}','AP','PAY','00001093',1,'AP PAYMENT','{3}',300,0,0,300,'AUD',1.000000000,'{3}','',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), -300.00, 'M00001134', '{3}', '{0}')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH)
	VALUES (NEWID(), 300.00, 'M00001134', '{3}', '{2}')",
invoicePK, invoiceDate, paymentPK, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-07-15 00:00:00')");
			AssertEquals(-1000m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-1000m, result.Rows[0]["AH_BalanceInLocal"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-06-15 00:00:00')");
			AssertEquals(-500m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-500m, result.Rows[0]["AH_BalanceInLocal"]);
		}
	}
}
