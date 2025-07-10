using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

#region Test
namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ComplianceNumberAndSubTypeExceptions))]
	class Report_ComplianceNumberAndSubTypeExceptionsTest : DbCreateScriptTest
	{
		//[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;

			//AccComplianceSequence
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccComplianceSequence ( XD_PK, XD_Description, XD_Prefix, XD_MaxChargesPerTransaction, XD_RollupBehaviourWhenMaxExceeded, XD_SequenceClass, XD_GC_Company, XD_IsActive, XD_StartNumber, XD_EndNumber, XD_NextNumber, XD_MaximumNumberDigits, XD_Code, XD_AllocationLevel, XD_PrintingAuthorizationNumber, XD_NumberFormat) " +
				$" VALUES (NEWID(), 'APS TEST', 'APS', 40, '', 'APS','{companyPK}', 1, 1, 999999999, 333, 9, 'APS-20', 'COM', '', 'DEF')");

			var tempDate = new DateTime(2020, 01, 15);
			var tempPK = Guid.NewGuid();
			var tempDateSTR = tempDate.ToString("yyyyMMdd");
			var tempTransactionNumber = "TST00002";
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_ComplianceSubType,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)" +
				$"VALUES('{tempPK}','AP','PAY','{tempTransactionNumber}',1,'APS','{tempTransactionNumber}','DIRECT PAYMENT','{tempDateSTR}','','{tempDateSTR}',-30.0000,-1.0000,0.0000,-31.0000,'EUR',1.000000000,0,0,'{tempDateSTR}','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'{companyPK}')");
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType) VALUES (NEWID(), '{tempPK}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{tempDateSTR}','ED35AE14-20CD-40DC-B75A-257D20608257','{companyPK}', 'CST')");

			tempDate = new DateTime(2020, 02, 5);
			tempPK = Guid.NewGuid();
			tempDateSTR = tempDate.ToString("yyyyMMdd");
			tempTransactionNumber = "TST00003";
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_ComplianceSubType,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)" +
				$"VALUES('{tempPK}','AP','PAY','{tempTransactionNumber}',1,'APS','{tempTransactionNumber}','DIRECT PAYMENT','{tempDateSTR}','','{tempDateSTR}',-30.0000,-1.0000,0.0000,-31.0000,'EUR',1.000000000,0,0,'{tempDateSTR}','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'{companyPK}')");
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType) VALUES (NEWID(), '{tempPK}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{tempDateSTR}','ED35AE14-20CD-40DC-B75A-257D20608257','{companyPK}', 'CST')");

			tempDate = new DateTime(2020, 02, 15);
			tempPK = Guid.NewGuid();
			tempDateSTR = tempDate.ToString("yyyyMMdd");
			tempTransactionNumber = "TST00004";
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_ComplianceSubType,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)" +
				$"VALUES('{tempPK}','AP','PAY','{tempTransactionNumber}',1,'APS','{tempTransactionNumber}','DIRECT PAYMENT','{tempDateSTR}','','{tempDateSTR}',-30.0000,-1.0000,0.0000,-31.0000,'EUR',1.000000000,0,0,'{tempDateSTR}','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'{companyPK}')");
			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType) VALUES (NEWID(), '{tempPK}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{tempDateSTR}','ED35AE14-20CD-40DC-B75A-257D20608257','{companyPK}', 'CST')");

			for (int i = 1; i < 300; i++)
			{
				tempPK = Guid.NewGuid();
				tempDate = new DateTime(2020, 01, 01).AddDays(i);
				if ((i % 10) == 0)
				{
					tempDate = tempDate.AddDays(2);
				}
				tempDateSTR = tempDate.ToString("yyyyMMdd");
				tempTransactionNumber = string.Format("APS{0:000000000}", i);
				TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_ComplianceSubType,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)" +
					$"VALUES('{tempPK}','AR','INV','{tempTransactionNumber}',1,'APS','{tempTransactionNumber}','DIRECT PAYMENT','{tempDateSTR}','','{tempDateSTR}',-30.0000,-1.0000,0.0000,-31.0000,'EUR',1.000000000,0,0,'{tempDateSTR}','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'{companyPK}')");
				TestConnection.ExecuteNonQuery($"INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType) VALUES (NEWID(), '{tempPK}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{tempDateSTR}','ED35AE14-20CD-40DC-B75A-257D20608257','{companyPK}', 'CST')");
			}
			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader");
			AssertEquals("Waited lines", 302, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.vw_Report_ProgressiveComplianceNumberForEachSubType");
			AssertEquals("Waited lines", 299, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC Report_ComplianceNumberAndSubTypeExceptions  @CompanyPK = '{companyPK}', @DateFrom = '20020101', @DateTo = NULL, @PeriodFrom = NULL, @PeriodTo = NULL");
			AssertEquals("Waited lines", 58, result.Rows.Count);
		}
	}
}
#endregion
