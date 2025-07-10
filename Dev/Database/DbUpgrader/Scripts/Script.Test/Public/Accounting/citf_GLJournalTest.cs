using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(citf_GLJournal))]
	class citf_GLJournalTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)VALUES('7CC1FE49-9E8C-420C-8299-9522DB4856D8','AR','AJL','00005013',1,'','DIRECT PAYMENT','May 25 2005  3:44:00:000PM','','May 25 2005  3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'May 25 2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType) VALUES (NEWID(), '7CC1FE49-9E8C-420C-8299-9522DB4856D8', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', 'May 25 2005  3:44:00:000PM','ED35AE14-20CD-40DC-B75A-257D20608257','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'AJL')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM citf_GLJournal(NULL, '{TestDbHelper.DefaultCompanyPK}')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
		}

		public void TestContainUnits()
		{
			var testDbHelper = new TestDbHelper(TestConnection);
			var companyPK = testDbHelper.InsertCompany("TST", "CHA", "CNY", "CA", false, false);
			var branchPK = testDbHelper.InsertBranch("CCB", companyPK);
			var departmentPK = testDbHelper.InsertDepartment("CCD");
			var plAccountPK = testDbHelper.InsertGLAccount("PLAcc", "Test PLAccount");
			var noteAccountPK = testDbHelper.InsertGLAccount("NoteAcc", "Test NoteAccount", "NTE", "KWH");
			var today = DateTime.Today;

			var transactionHeaderPK1 = testDbHelper.InsertTransactionHeader("AP", "GJL", "001", 0M, DateTime.Today, branchPK, departmentPK);
			testDbHelper.InsertTransactionLine(transactionHeaderPK1, null, null, plAccountPK, branchPK, departmentPK, null, -1m, "CST", DateTime.Today, today);
			var transactionHeaderPK2 = testDbHelper.InsertTransactionHeader("AP", "NJL", "002", 0M, DateTime.Today, branchPK, departmentPK);
			testDbHelper.InsertTransactionLine(transactionHeaderPK2, null, null, noteAccountPK, branchPK, departmentPK, null, -2m, "NJL", DateTime.Today, today);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM citf_GLJournal(NULL, '{TestDbHelper.DefaultCompanyPK}')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			var rowselected = result.Select("GLAccount = 'PLAcc' AND TransactionNumber = '001'");
			AssertEquals("Result should have 1 row", 1, rowselected.Length);
			AssertEquals("Title is not expected", "GJL - 001 - AP GJL 001 - Post To: 0", rowselected[0]["Title"].ToString());
			AssertEquals("Title is not expected", "GJL", rowselected[0]["TransactionType"].ToString());
			AssertEquals("Title is not expected", "", rowselected[0]["Units"].ToString());

			rowselected = result.Select("GLAccount = 'NoteAcc' AND TransactionNumber = '002'");
			AssertEquals("Result should have 1 row", 1, rowselected.Length);
			AssertEquals("Title is not expected", "NJL - 002 - AP NJL 002 - Post To: 0", rowselected[0]["Title"].ToString());
			AssertEquals("Title is not expected", "NJL", rowselected[0]["TransactionType"].ToString());
			AssertEquals("Title is not expected", "KWH", rowselected[0]["Units"].ToString());
		}
	}
}

