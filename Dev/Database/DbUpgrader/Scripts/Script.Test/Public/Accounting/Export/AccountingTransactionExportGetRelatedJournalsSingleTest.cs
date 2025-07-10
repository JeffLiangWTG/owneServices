using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetRelatedJournalsSingle))]
	class AccountingTransactionExportGetRelatedJournalsSingleTest : DbCreateScriptTest
	{
		public void TestNoOptionRecompile()
		{
			AssertNotContains("There is no OPTION (RECOMPILE) statement",
				"OPTION (RECOMPILE)",
				ScriptToTest.Text,
				ignoreCase: true
			);
		}

		public void TestNoTableVariableDeclaration()
		{
			AssertNotContains("There isn't any type of DECLARE",
				"DECLARE @HeaderPKs TABLE",
				ScriptToTest.Text,
				ignoreCase: true
			);
		}

		public void TestHasTransactionHeaderPKParameter()
		{
			AssertContains("There must be the TransactionHeaderPK",
				"@TransactionHeaderPK UNIQUEIDENTIFIER",
				ScriptToTest.Text,
			ignoreCase: true
			);
		}

		public void TestHasCorrectFilterParameters()
		{
			AssertContains("AH_TransactionBelongsToGroup must be equals to TransactionHeaderPK",
				"WHERE header.AH_TransactionBelongsToGroup = @TransactionHeaderPK",
				ScriptToTest.Text,
			ignoreCase: true);

			AssertContains("There must be the filters for Apportionments and Multiple Installments",
				"AND ((AH_Ledger = 'AR' AND AH_TransactionCategory IN ('CLJ', 'INJ') AND AH_TransactionType = 'JNL') OR (AH_Ledger = 'GL' AND AH_TransactionType = 'GJL'))",
				ScriptToTest.Text,
			ignoreCase: true
			);
		}

		public void TestAccountingTransactionExportGetRelatedJournalsSingle()
		{
			var insertSql = @"
DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');
DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC';
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB';
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491';
DECLARE @TransacionPK1 UNIQUEIDENTIFIER = 'D0D65D63-9C27-4CCB-9120-321703A017D3';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK1, 'AR', 'INV', 101, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-26', '2023-02-26', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionCategory, AH_TransactionType, AH_TransactionNum, AH_TransactionBelongsToGroup, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES ('13a1ea1e-38e7-403d-a69f-2b090747902b', 'AR', 'CLJ', 'JNL', 102, @TransacionPK1, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-20', '2023-02-20', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionCategory, AH_TransactionType, AH_TransactionNum, AH_TransactionBelongsToGroup, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES ('ebfcc4c1-ca24-49b9-8e63-3837ed6ddb82', 'AR', 'INJ', 'JNL', 103, @TransacionPK1, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-21', '2023-02-21', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionBelongsToGroup, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES ('db161d54-5c95-4de7-b338-404c9edfd36e', 'GL', 'GJL', 104, @TransacionPK1, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-22', '2023-02-22', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionBelongsToGroup, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES ('75932754-8cc6-4671-9f78-5de848652086', 'GL', 'GJL', 105, @TransacionPK1, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-23', '2023-02-23', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionCategory, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (NEWID(), 'AR', 'CLJ', 'JNL', 106, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-22', '2023-02-22', @OrgPK1);
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (NEWID(), 'GL', 'GJL', 107, @CompanyPK, @BranchPK, @DepartmentPK, '2023-02-23', '2023-02-23', @OrgPK1);
";
			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "EXEC AccountingTransactionExportGetRelatedJournalsSingle 'D0D65D63-9C27-4CCB-9120-321703A017D3' ";

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var expectedResult = @"
AH_PK                                AH_Ledger                            AH_TransactionCategory AH_TransactionType AH_DueDate AH_ExchangeRate                                    AH_FullyPaidDate AG_AccountNum AG_Description          AH_LocalTotal           Local_RX_Code          Local_RX_Desc                       OH_Code                                                                                                                                                                                                  OS_RX_Code          OS_RX_Desc             AH_IsCancelled          AH_OSTotal              AH_OutstandingAmount    AH_PostDate                         AH_InvoiceDate         AH_Desc                        AH_TransactionBelongsToGroup
------------------------------------ ------------------------------------ ---------------------- ------------------ ---------- -------------------------------------------------- ---------------- ------------- ----------------------- ----------------------- ---------------------- ----------------------------------- -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------- ---------------------- ----------------------- ----------------------- ----------------------- ----------------------------------- ---------------------- ------------------------------ ----------------------------
13a1ea1e-38e7-403d-a69f-2b090747902b AR                                   CLJ                    JNL                NULL       1.00                                               NULL             NULL          NULL                    0.00                    AUD                    Australian Dollar                   TESTORG1                                                                                                                                                                                                 NULL                NULL                   0                       0.00                    0.00                    2023-02-20 00:00:00.000             2023-02-20 00:00:00.000                                d0d65d63-9c27-4ccb-9120-321703a017d3
ebfcc4c1-ca24-49b9-8e63-3837ed6ddb82 AR                                   INJ                    JNL                NULL       1.00                                               NULL             NULL          NULL                    0.00                    AUD                    Australian Dollar                   TESTORG1                                                                                                                                                                                                 NULL                NULL                   0                       0.00                    0.00                    2023-02-21 00:00:00.000             2023-02-21 00:00:00.000                                d0d65d63-9c27-4ccb-9120-321703a017d3
db161d54-5c95-4de7-b338-404c9edfd36e GL                                                          GJL                NULL       1.00                                               NULL             NULL          NULL                    0.00                    AUD                    Australian Dollar                   TESTORG1                                                                                                                                                                                                 NULL                NULL                   0                       0.00                    0.00                    2023-02-22 00:00:00.000             2023-02-22 00:00:00.000                                d0d65d63-9c27-4ccb-9120-321703a017d3
75932754-8cc6-4671-9f78-5de848652086 GL                                                          GJL                NULL       1.00                                               NULL             NULL          NULL                    0.00                    AUD                    Australian Dollar                   TESTORG1                                                                                                                                                                                                 NULL                NULL                   0                       0.00                    0.00                    2023-02-23 00:00:00.000             2023-02-23 00:00:00.000                                d0d65d63-9c27-4ccb-9120-321703a017d3
";

			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("4 records should be returned", result, expectedResult, new List<string>() { "" });
		}
	}
}

