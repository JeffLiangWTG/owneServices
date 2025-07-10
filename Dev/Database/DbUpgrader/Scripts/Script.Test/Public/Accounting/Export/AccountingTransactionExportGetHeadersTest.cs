using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetHeaders))]
	class AccountingTransactionExportGetHeadersTest : DbCreateScriptTest
	{
		public void TestAccountingTransactionExportGetHeaders_Contra()
		{
			var org1Pk = Guid.NewGuid();
			var contra1Pk = Guid.NewGuid();
			var contra2Pk = Guid.NewGuid();

			var insertSql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code)
VALUES
	('{org1Pk}', 'DDDABCSYD')

DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'
DECLARE @TransactionBelongsToGroupPK UNIQUEIDENTIFIER = 'F3099DFB-5048-4175-9DCA-6C0B12156886'

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_OA_InvoiceAddressOverride, AH_TransactionBelongsToGroup)
VALUES ('{contra1Pk}', 'CTR002', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'CTR', '{org1Pk}', 'AR', NULL, @TransactionBelongsToGroupPK)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_OA_InvoiceAddressOverride, AH_TransactionBelongsToGroup)
VALUES ('{contra2Pk}', 'CTR003', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'CTR', '{org1Pk}', 'AR', NULL, @TransactionBelongsToGroupPK)

";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetHeaders 'EDI', -1, '{contra1Pk}'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals($"{sql}: Should have rows", 2, result.Rows.Count);
		}

		public void TestAccountingTransactionExportGetHeaders_TaxBranch_IsNotNull()
		{
			var taxBranchPk = Guid.NewGuid();
			var transactionPk = Guid.NewGuid();
			var expectedTaxBranchCode = "TB1";
			var expectedTaxBranchName = "TaxBranch";

			var insertSql = $@"
DECLARE @OrgPK UNIQUEIDENTIFIER = '8AF9D5D6-562F-41CF-823B-59FBCF182D37'
DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC)
VALUES('{taxBranchPk}', '{expectedTaxBranchCode}', '{expectedTaxBranchName}', @CompanyPk)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_GB_TaxBranch)
VALUES ('{transactionPk}', '001', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', @OrgPK, 'AR', '{taxBranchPk}')";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetHeaders 'EDI', -1, '{transactionPk}'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals($"{sql}: Should have rows", 1, result.Rows.Count);

			AssertEquals(expectedTaxBranchCode, result.Rows[0]["TaxBranchCode"]);
			AssertEquals(expectedTaxBranchName, result.Rows[0]["TaxBranchName"]);
		}

		public void TestAccountingTransactionExportGetHeaders_TaxBranch_IsNull()
		{
			var transactionPk = Guid.NewGuid();

			var insertSql = $@"
DECLARE @OrgPK UNIQUEIDENTIFIER = '8AF9D5D6-562F-41CF-823B-59FBCF182D37'
DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_GB_TaxBranch)
VALUES ('{transactionPk}', '001', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', @OrgPK, 'AR', NULL)";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetHeaders 'EDI', -1, '{transactionPk}'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals($"{sql}: Should have rows", 1, result.Rows.Count);

			AssertEquals(DBNull.Value, result.Rows[0]["TaxBranchCode"]);
			AssertEquals(DBNull.Value, result.Rows[0]["TaxBranchName"]);
		}
	}
}

