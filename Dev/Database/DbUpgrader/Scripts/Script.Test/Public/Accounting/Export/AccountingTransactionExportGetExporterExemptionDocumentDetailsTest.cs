using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetExporterExemptionDocumentDetails))]
	class AccountingTransactionExportGetExporterExemptionDocumentDetailsTest : DbCreateScriptTest
	{
		[TestDate(2018, 2, 10)]
		public void TestAccountingTransactionExportGetExporterExemptionDocumentDetails()
		{
			var insertSql = @"
			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @OrgPK2 UNIQUEIDENTIFIER = 'BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK2, 'TESTORG2');

			DECLARE @JobReq1 UNIQUEIDENTIFIER = '7803F853-B05A-4091-B603-985567C2D373';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq1, '1234A', '2018-02-01 07:05:01 +10:00', '2018-02-09', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq1, 'COSTA RICA EXV DOCUMENT TYPE', 'Compras Autorizadas')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq1, 'ISSUING AUTHORITY NAME', 'Bureau of Meteorology')

			DECLARE @JobReq2 UNIQUEIDENTIFIER = '73B57E80-57F5-4614-BFB8-95976815A927';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq2, '1234B', '2018-02-09 07:05:02 -05:00', '2018-02-19', 'Special Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq2, 'COSTA RICA EXV DOCUMENT TYPE', 'Compras Autorizadas')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq2, 'ISSUING AUTHORITY NAME', 'Bureau of Meteorology')

			DECLARE @JobReq3 UNIQUEIDENTIFIER = 'ED543C81-B122-4F6C-88CA-F8D8D59774BE';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '2345', '2018-02-10 07:05:03 +08:00', '2018-02-19', 'EXV', 'PER', 'DBT', 'NZ', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '3456', '2018-02-10 07:05:04 +08:00', '2018-02-19', 'EXV', 'PER', 'CRT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq3, '4567', '2018-02-10 07:05:05 +08:00', '2018-02-19', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (newid(), '5678', '2018-02-10 07:05:06 +08:00', '2018-02-19', 'DEC', 'PER', 'DBT', 'AU', 'OH', @OrgPK1)
			
			DECLARE @JobReq6 UNIQUEIDENTIFIER = '817BF967-3D22-41D5-B9B6-176CA1F670F1';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq6, '6789', '2018-02-10 07:05:07 +08:00', '2018-02-19', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK2)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq6, 'COSTA RICA EXV DOCUMENT TYPE', 'Zonas Francas')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq6, 'ISSUING AUTHORITY NAME', 'Department of Education')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq6, 'Direction', 'IMP')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq6, 'COMPANY CODE', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')

			DECLARE @JobReq7 UNIQUEIDENTIFIER = '1CE229C1-C87B-4D17-A9E2-B281132FE594';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq7, '7890', '2018-02-10 07:05:08 +08:00', '2018-02-19', 'Notes', 'EXV', 'PER', 'DBT', 'AU', 'OH', @OrgPK2)
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq7, 'COSTA RICA EXV DOCUMENT TYPE', 'Zonas Francas Second')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq7, 'ISSUING AUTHORITY NAME', 'Department of Education Bis')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq7, 'Direction', 'IMP')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq7, 'COMPANY CODE', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')

			DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC';
			DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB';
			DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491';

			DECLARE @TransacionPK1 UNIQUEIDENTIFIER = 'D0D65D63-9C27-4CCB-9120-321703A017D3';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK1, 'AR', 'INV', 1, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-01', '2018-02-01', @OrgPK1)

			DECLARE @TransacionPK2 UNIQUEIDENTIFIER = '0C0E056E-DA87-48E4-84DC-5DED651C7D1D';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK2, 'AR', 'INV', 2, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-10', '2018-02-10', @OrgPK1)

			DECLARE @TransacionPK3 UNIQUEIDENTIFIER = '43A20A9A-BCB9-4B80-B427-98EE1321FE48';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK3, 'AP', 'INV', 3, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-10', '2018-02-10', @OrgPK1)

			DECLARE @TransacionPK4 UNIQUEIDENTIFIER = '5FCF43AB-57B0-4C02-BA71-21E9BDA50918';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK4, 'AR', 'PAY', 4, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-10', '2018-02-10', @OrgPK1)

			DECLARE @TransacionPK5 UNIQUEIDENTIFIER = '71D7C559-5369-43CD-AF00-F0497057173F';
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_OH) VALUES (@TransacionPK5, 'AR', 'INV', 5, @CompanyPK, @BranchPK, @DepartmentPK, '2018-02-10', '2018-02-19', @OrgPK2)

			INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK1, GETUTCDATE(), '~BP' UNION ALL
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK2, GETUTCDATE(), '~BP' UNION ALL
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK3, GETUTCDATE(), '~BP' UNION ALL
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK4, GETUTCDATE(), '~BP' UNION ALL
			SELECT newid(), 'ARV', 1, 'AH', @TransacionPK5, GETUTCDATE(), '~BP'
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "EXEC AccountingTransactionExportGetExporterExemptionDocumentDetails 'EDI', 1, NULL"; //AR invoice, AH_GB has a branch org proxy

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var expectedResult = @"
AH_PK                                EQ_PK                                EQ_DocNumber    EQ_DocCategory EQ_DocType RT_Desc                                            EQ_DocUsage EQ_DocPeriod EQ_DateReceived         EQ_ValidToDate          EQ_RN_NKRelatedCountry RN_Desc                             EQ_DocumentNotes                                                                                                                                                                                         EQ_CreditControlDoc EQ_OriginalDocRequired EQ_RcvFromCustomsBroker EQ_ReturnToShipper      EQ_SntToCustomsBroker   D0_AttribName                       D0_AttribValue
------------------------------------ ------------------------------------ --------------- -------------- ---------- -------------------------------------------------- ----------- ------------ ----------------------- ----------------------- ---------------------- ----------------------------------- -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------- ---------------------- ----------------------- ----------------------- ----------------------- ----------------------------------- --------------------------------------------------------------------------------
d0d65d63-9c27-4ccb-9120-321703a017d3 7803f853-b05a-4091-b603-985567c2d373 1234A                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-01 07:05:01 +10:00 2018-02-09 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Compras Autorizadas
d0d65d63-9c27-4ccb-9120-321703a017d3 7803f853-b05a-4091-b603-985567c2d373 1234A                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-01 07:05:01 +10:00 2018-02-09 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Bureau of Meteorology
0c0e056e-da87-48e4-84dc-5ded651c7d1d 73b57e80-57f5-4614-bfb8-95976815a927 1234B                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-09 07:05:02 -05:00 2018-02-19 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Compras Autorizadas
0c0e056e-da87-48e4-84dc-5ded651c7d1d 73b57e80-57f5-4614-bfb8-95976815a927 1234B                          EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-09 07:05:02 -05:00 2018-02-19 00:00:00.000 AU                     Australia                           Special Notes                                                                                                                                                                                            0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Bureau of Meteorology
0c0e056e-da87-48e4-84dc-5ded651c7d1d ed543c81-b122-4f6c-88ca-f8d8d59774be 4567                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:05 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    NULL                                NULL
71d7c559-5369-43cd-af00-f0497057173f 817bf967-3d22-41d5-b9b6-176ca1f670f1 6789                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:07 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    COMPANY CODE                        EDI
71d7c559-5369-43cd-af00-f0497057173f 817bf967-3d22-41d5-b9b6-176ca1f670f1 6789                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:07 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Zonas Francas
71d7c559-5369-43cd-af00-f0497057173f 817bf967-3d22-41d5-b9b6-176ca1f670f1 6789                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:07 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    Direction                           IMP
71d7c559-5369-43cd-af00-f0497057173f 817bf967-3d22-41d5-b9b6-176ca1f670f1 6789                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:07 +08:00 2018-02-19 00:00:00.000 AU                     Australia                                                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Department of Education
71d7c559-5369-43cd-af00-f0497057173f 1ce229c1-c87b-4d17-a9e2-b281132fe594 7890                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:08 +08:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    COMPANY CODE                        EDI
71d7c559-5369-43cd-af00-f0497057173f 1ce229c1-c87b-4d17-a9e2-b281132fe594 7890                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:08 +08:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    COSTA RICA EXV DOCUMENT TYPE        Zonas Francas Second
71d7c559-5369-43cd-af00-f0497057173f 1ce229c1-c87b-4d17-a9e2-b281132fe594 7890                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:08 +08:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    Direction                           IMP
71d7c559-5369-43cd-af00-f0497057173f 1ce229c1-c87b-4d17-a9e2-b281132fe594 7890                           EXV        VAT/GST Exporter Exemption (AR and AP Invoicing)   DBT         PER          2018-02-10 07:05:08 +08:00 2018-02-19 00:00:00.000 AU                     Australia                           Notes                                                                                                                                                                                                    0                   0                      NULL                    NULL                    NULL                    ISSUING AUTHORITY NAME              Department of Education Bis
";

			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("13 records should be returned", result, expectedResult, new List<string>() { "" });
		}
	}
}
