using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(NetAPAgedBase))]
	class NetAPAgedBaseTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)VALUES(newID(),'AP','DSC','00005013',1,'','DIRECT PAYMENT','May 25 2005  3:44:00:000PM','','May 25 2005  3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'May 25 2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,-31.0000,0,'Y','',0,'',0,0,0,0,0,NULL,'1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5',NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '200605', 'May 01 2006  3:44:00:000PM','May 31 2006  3:44:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from NetAPAgedBase (200605,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','FDD429D2-648C-4895-8F9F-06E90DED2BE5','','')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
		}
	}
}

