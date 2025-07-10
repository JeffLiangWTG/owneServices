using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	class csfn_APTransactionsDetailIntegrationTest : TransactionedTestCase
	{
		public void TestAH_BalanceAndAH_BalanceInLocalWithOtherTaxes_GCIsReciprocal()
		{
			Guid invoicePK = Guid.NewGuid();
			string invoiceDate = "2020-06-15 12:00:00";
			Guid paymentPK = Guid.NewGuid();
			string paymentDate = "2020-06-25 00:00:00";
			var factory = new BusinessObjectFactory();
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			factory.Save();
			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser)
	VALUES (NEWID(),202006,2020,'2020-06-01 00:00:00','2020-06-30 23:59:00',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC,AH_SystemCreateTimeUtc,AH_SystemCreateUser,AH_SystemLastEditTimeUtc,AH_SystemLastEditUser)
	VALUES ('{0}','AP','INV','00001109',1,'AP INVOICE','{1}',-1000.00,-100.00,-200.00,-650.00,'USD',2.000000000,'{1}','',-500,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',GetUtcDate(),'~BP', GetUtcDate(),'~BP')
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC,AH_SystemCreateTimeUtc,AH_SystemCreateUser,AH_SystemLastEditTimeUtc,AH_SystemLastEditUser)
	VALUES ('{2}','AP','INV','00001093',1,'AP PAYMENT','{3}',300,0,0,150,'USD',2.000000000,'{3}','',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',GetUtcDate(),'~BP',GetUtcDate(),'~BP')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH, AP_SystemCreateTimeUtc, AP_SystemCreateUser, AP_SystemLastEditTimeUtc, AP_SystemLastEditUser)
	VALUES (NEWID(), -300.00, 'M00001134', '{3}', '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.AccTransactionMatchLink (AP_PK, AP_Amount, AP_MatchGroupNum, AP_MatchDate, AP_AH, AP_SystemCreateTimeUtc, AP_SystemCreateUser, AP_SystemLastEditTimeUtc, AP_SystemLastEditUser)
	VALUES (NEWID(), 300.00, 'M00001134', '{3}', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
invoicePK, invoiceDate, paymentPK, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-07-15 00:00:00')");
			AssertEquals(-500m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-1000m, result.Rows[0]["AH_BalanceInLocal"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_APTransactionsDetail ('202006', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2020-06-15 00:00:00')");
			AssertEquals(-250m, result.Rows[0]["AH_Balance"]);
			AssertEquals(-500m, result.Rows[0]["AH_BalanceInLocal"]);
		}
	}
}
