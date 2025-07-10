using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetExporterExemptionDocumentDetailsSingle))]
	class AccountingTransactionExportGetExporterExemptionDocumentDetailsSingleTest : DbCreateScriptTest
	{
		public void TestResultColumnsAreIdenticalToBatchVersion()
		{
			var singleResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetExporterExemptionDocumentDetailsSingle '{Guid.NewGuid()}'");
			var batchResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetExporterExemptionDocumentDetails 'EDI', -1, NULL");
			AssertSameSchema("Result schemas from batch and single export stored procs must be identical in column order, column name and column type. When making changes to one, you must make identical changes for the other.",
				batchResult,
				singleResult
			);
		}

		public void TestNoOptionRecompile()
		{
			AssertNotContains("Single versions of export queries should not contain OPTION (RECOMPILE), to improve performance when executed more frequently",
				"OPTION (RECOMPILE)",
				ScriptToTest.Text,
				ignoreCase: true
			);
		}

		public void TestNoTableVariableDeclaration()
		{
			AssertNotContains("Single versions of export queries should not declare a table variable, to ensure a query plan specific to single transaction use case",
				"DECLARE @HeaderPKs TABLE",
				ScriptToTest.Text,
				ignoreCase: true
			);
		}

		public void TestCoreQueryLogicIsSameAsBatchVersion()
		{
			// Each line excluded from the comparison must be listed exactly (case and whitespace sensitive)
			// And a comment justifying why it is excluded.
			var excludeLinesForBatch = new[]
			{
				"	INNER JOIN @HeaderPKs pks ON AH_PK = pks.PK",		// Join on PKs table; replaced by WHERE in single version
				"OPTION (RECOMPILE)",									// Recompile to ensure best query plan used in batch only.
			};
			var excludeLinesForSingle = new[]
			{
				"		WITH (FORCESEEK INDEX (PK_UC__AH_PK))",		// Index hints on all single versions to avoid poor plans.
				"	AND AH_PK = @TransactionHeaderPK",				// Replacement for PK table join.
			};

			var singleScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(ScriptToTest.Text, excludeLinesForSingle);
			var batchScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(new AccountingTransactionExportGetExporterExemptionDocumentDetails().Text, excludeLinesForBatch);

			AssertMultilineASCIIEquals("Core SELECT query of batch query must be identical to single version", batchScriptCoreLinesToCompare, singleScriptCoreLinesToCompare);
		}

		[TestDate(2018, 2, 10)]
		public void TestAccountingTransactionExportGetExporterExemptionDocumentDetailsSingle()
		{
			var insertSql = @"
			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');

			DECLARE @JobReq1 UNIQUEIDENTIFIER = '7803F853-B05A-4091-B603-985567C2D373';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq1, '1234A', '2018-02-01 07:05:01 +10:00', '2018-02-10', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq1, 'COSTA RICA EXV DOCUMENT TYPE', 'Compras Autorizadas')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq1, 'ISSUING AUTHORITY NAME', 'Bureau of Meteorology')

			DECLARE @JobReq2 UNIQUEIDENTIFIER = '73B57E80-57F5-4614-BFB8-95976815A927';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq2, '1234B', '2018-02-09 07:05:02 -05:00', '2018-02-19', 'Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq2, 'COSTA RICA EXV DOCUMENT TYPE', 'Compras Autorizadas')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq2, 'ISSUING AUTHORITY NAME', 'Bureau of Meteorology')

			DECLARE @JobReq3 UNIQUEIDENTIFIER = 'ED543C81-B122-4F6C-88CA-F8D8D59774BE';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '2345', '2018-02-10 07:05:03 +08:00', '2018-02-19', 'EXV', 'PER', 'DBT', 'NZ', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '3456', '2018-02-10 07:05:04 +08:00', '2018-02-19', 'EXV', 'PER', 'CRT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq3, '4567', '2018-02-10 07:05:05 +08:00', '2018-02-19', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '5678', '2018-02-10 07:05:06 +08:00', '2018-02-19', 'DEC', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)

			DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC';
			DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB';
			DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491';

			DECLARE @TransacionPK1 UNIQUEIDENTIFIER = 'D0D65D63-9C27-4CCB-9120-321703A017D3';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK1, 'AR', 'INV', 101, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-10', '2018-02-10', @OrgPK1)

			INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK1, GETUTCDATE(), '~BP'
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "EXEC AccountingTransactionExportGetExporterExemptionDocumentDetailsSingle 'd0d65d63-9c27-4ccb-9120-321703a017d3' "; //AR invoice, AH_GB has a branch org proxy

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var expectedResult = @"
AH_PK                                EQ_PK                                EQ_DocNumber    EQ_DocCategory EQ_DocType RT_Desc                                            EQ_DocUsage EQ_DocPeriod EQ_DateReceived         EQ_ValidToDate          EQ_RN_NKRelatedCountry RN_Desc                             EQ_DocumentNotes                                                                                                                                                                                         EQ_CreditControlDoc EQ_OriginalDocRequired EQ_RcvFromCustomsBroker EQ_ReturnToShipper      EQ_SntToCustomsBroker   D0_AttribName                       D0_AttribValue
------------------------------------ ------------------------------------ --------------- -------------- ---------- -------------------------------------------------- ----------- ------------ ----------------------- ----------------------- ---------------------- ----------------------------------- -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------- ---------------------- ----------------------- ----------------------- ----------------------- ----------------------------------- --------------------------------------------------------------------------------
d0d65d63-9c27-4ccb-9120-321703a017d3 7803f853-b05a-4091-b603-985567c2d373 1234A                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-01 07:05:01 +10:00 2018-02-10 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Compras Autorizadas
d0d65d63-9c27-4ccb-9120-321703a017d3 7803f853-b05a-4091-b603-985567c2d373 1234A                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-01 07:05:01 +10:00 2018-02-10 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Bureau of Meteorology
d0d65d63-9c27-4ccb-9120-321703a017d3 73b57e80-57f5-4614-bfb8-95976815a927 1234B                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-09 07:05:02 -05:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Compras Autorizadas
d0d65d63-9c27-4ccb-9120-321703a017d3 73b57e80-57f5-4614-bfb8-95976815a927 1234B                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-09 07:05:02 -05:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Bureau of Meteorology
d0d65d63-9c27-4ccb-9120-321703a017d3 ed543c81-b122-4f6c-88ca-f8d8d59774be 4567                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:05 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    NULL                                NULL
";

			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("5 records should be returned", result, expectedResult, new List<string>() { "" });
		}
	}
}
