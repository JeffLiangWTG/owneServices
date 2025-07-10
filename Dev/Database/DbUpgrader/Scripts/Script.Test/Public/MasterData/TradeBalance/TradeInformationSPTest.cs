using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData.TradeBalance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterData.TradeBalance.Testing
{
	[TestedType(typeof(TradeInformationSP))]
	class TradeInformationSPTest : DbCreateScriptTest
	{
		public void TestGetTradeInformationForCompany()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC,AH_SystemCreateUser)VALUES(newID(),'AR','DSC','00005013',1,'','DIRECT PAYMENT','01/05/2005 3:44:00:000PM','','01/05/2005 3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'01/05/2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'Test Ref 2',NULL,0,0,NULL,0,1.0000,0,1,'',0,'',0,0,0,0,0,NULL,'C3F842EF-3BE5-448C-BED3-0017B232C624',NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','E')");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC TradeInformationSP @ReportDate = '2005-01-31', @Company = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'");

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Balance should be 1", 1m, (decimal)result.Rows[0]["Balance"]) ;
		}
	}
}

