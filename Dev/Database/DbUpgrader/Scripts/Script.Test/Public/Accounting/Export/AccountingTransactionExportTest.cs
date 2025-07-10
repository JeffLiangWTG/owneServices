using System;
using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.Export
{
	public class AccountingTransactionExportTest : TransactionedTestCase
	{
		public void TestGetDataIgnoreNullHeaderPK()
		{
			var org1Pk = Guid.NewGuid();
			var jobHeaderPk = Guid.NewGuid();
			var invoice1Pk = Guid.NewGuid();
			var invoice2Pk = Guid.NewGuid();
			var invoice3Pk = Guid.NewGuid();
			var invoiceLine1Pk = Guid.NewGuid();
			var invoiceLine2Pk = Guid.NewGuid();
			var accCashBasisVATPk = Guid.NewGuid();

			var insertSql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code)
VALUES
	('{org1Pk}', 'DDDABCSYD')

DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_Status,JH_OA_LocalChargesAddr, JH_OA_AgentCollectAddr)
VALUES ('{jobHeaderPk}', NEWID(), 'JS', '000000001', @CompanyPk, @BranchPK, @DepartmentPK, 'WRK', NULL, NULL)

--AccTransactionHeader

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoice1Pk}', 'INV001', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'AR', '{jobHeaderPk}', NULL)

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'ARV', 1, 'AH', '{invoice1Pk}', GETUTCDATE(), '~BP')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoice2Pk}', 'INV002', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'AR', '{jobHeaderPk}', NULL)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoice3Pk}', 'INV003', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'AR', '{jobHeaderPk}', NULL)

--AccTransactionLines AL_AH IS NULL

INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType)
VALUES ('{invoiceLine1Pk}', NULL, @BranchPK, @DepartmentPK, '2015-04-24 14:04:00', NULL, @CompanyPK, 'AJL')

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'ARV', 1, 'AL', '{invoiceLine1Pk}', GETUTCDATE(), '~BP')

--AccTransactionLines link to AccCashBasisVAT and AL_AH IS NULL

INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType)
VALUES ('{invoiceLine2Pk}', NULL, @BranchPK, @DepartmentPK, '2015-04-24 14:04:00', NULL, @CompanyPK, 'AJL')

INSERT INTO dbo.AccCashBasisVAT	(YC_PK, YC_PostDate, YC_MatchGroupNum, YC_TaxBaseAmount, YC_TaxAmount, YC_AL_TransactionLine, YC_GC,YC_SystemCreateTimeUtc, YC_SystemCreateUser)
VALUES	('{accCashBasisVATPk}', '2015-04-24 14:04:00', 0, 100, 100, '{invoiceLine2Pk}', @CompanyPK, '2015-04-24 14:04:00', 'ABC')

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'APS', 1, 'AL', '{accCashBasisVATPk}', GETUTCDATE(), '~BP')

-- only for AccountingTransactionExportGetExporterExemptionDocumentDetails

INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocType, EQ_DocPeriod, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID)
VALUES (NEWID(), '6789', '2015-5-5', '2015-5-5', 'EXV', 'PER', 'DBT', 'AU', 'OH', '{org1Pk}')
";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqls = new List<string>()
			{
				"EXEC AccountingTransactionExportGetAddresses 'EDI', 1, NULL",
				"EXEC AccountingTransactionExportGetExporterExemptionDocumentDetails 'EDI', 1, NULL",
				"EXEC AccountingTransactionExportGetHeaders 'EDI', 1, NULL",
				"EXEC AccountingTransactionExportGetSenderAddresses 'EDI', 1, NULL"
			};

			foreach (var sql in sqls)
			{
				TestConnection.ExecuteNonQuery($@"
UPDATE dbo.Acctransactionlines SET AL_AH = NULL, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' where AL_PK = '{invoiceLine1Pk}'
UPDATE dbo.Acctransactionlines SET AL_AH = NULL, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' where AL_PK = '{invoiceLine2Pk}'");
				var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
				AssertEquals($"{sql}: Should have rows", 1, result.Rows.Count);

				TestConnection.ExecuteNonQuery($@"
UPDATE dbo.Acctransactionlines SET AL_AH = '{invoice2Pk}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' where AL_PK = '{invoiceLine1Pk}'
UPDATE dbo.Acctransactionlines SET AL_AH = '{invoice3Pk}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' where AL_PK = '{invoiceLine2Pk}'");
				result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
				AssertEquals($"{sql}: should have rows", 3, result.Rows.Count);
			}
		}
	}
}
