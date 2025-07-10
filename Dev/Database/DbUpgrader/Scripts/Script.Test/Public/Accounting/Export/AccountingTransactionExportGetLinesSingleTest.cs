using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetLinesSingle))]
	class AccountingTransactionExportGetLinesSingleTest : DbCreateScriptTest
	{
		public void TestResultColumnsAreIdenticalToBatchVersion()
		{
			var singleResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetLinesSingle '{Guid.NewGuid()}', '2021-01-01'");
			var batchResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetLines 'EDI', -1, '2021-01-01', NULL");
			AssertSameSchema("Result schemas from batch and single export stored procs must be identical in column order, column name and column type. When making changes to one, you must make identical changes for the other.", batchResult, singleResult);
		}

		public void TestNoOptionRecompile()
		{
			AssertNotContains("Single versions of export queries should not contain OPTION (RECOMPILE), to improve performance when executed more frequently", "OPTION (RECOMPILE)", ScriptToTest.Text, ignoreCase: true);
		}

		public void TestNoTableVariableDeclaration()
		{
			AssertNotContains("Single versions of export queries should not declare a table variable, to ensure a query plan specific to single transaction use case", "DECLARE @LinePKs TABLE", ScriptToTest.Text, ignoreCase: true);
		}

		public void TestCoreQueryLogicIsSameAsBatchVersion()
		{
			// Each line excluded from the comparison must be listed exactly (case and whitespace sensitive)
			// And a comment justifying why it is excluded.
			var excludeLinesForBatch = new[]
			{
				"INNER JOIN @LinePKs pks",		// Join on PKs table; replaced by WHERE in single version
				"	ON AL_PK = pks.PK",			// Join on PKs table; replaced by WHERE in single version
				"	IsCashBasisVATOnly,",		// Column from table variable
				"	BatchSequence,",			// Column from table variable
				"	OriginalBatchNumber,",		// Column from table variable
				"	OriginalBatchSequence,",	// Column from table variable
				"OPTION(RECOMPILE)",			// Recompile to ensure best query plan used in batch only.
			};
			var excludeLinesForSingle = new[]
			{
				"	WITH (INDEX (FK_RX__AL_AH))",					// Index hints on all single versions to avoid poor plans.
				"	CONVERT(bit, 0) AS IsCashBasisVATOnly,",		// Hard coded column from table variable; required for correctness.
				"	CONVERT(int, NULL) AS BatchSequence,",			// Hard coded column from table variable; required for correctness.
				"	CONVERT(int, NULL) AS OriginalBatchNumber,",	// Hard coded column from table variable; required for correctness.
				"	CONVERT(int, NULL) AS OriginalBatchSequence,",	// Hard coded column from table variable; required for correctness.
				"AND AL_AH = @TransactionHeaderPK",					// Replacement for PK table join.
				"	WITH (FORCESEEK INDEX (PK_UX__JH_PK))",			// Force seeks on JobHeader to avoid slow plans when invoice with 10k lines queried.
				"	WITH (FORCESEEK INDEX (PK_UC__OH_PK))",			// Force seeks on OrgHeader to avoid slow plans when invoice with 10k lines queried.
				"	WITH (FORCESEEK INDEX (FK_UC__OB_GC_OB_OH))",	// Force seeks on OrgCompanyData to avoid slow plans when invoice with 10k lines queried.
				"OPTION (MAXDOP 1)",								// Force non-parallel plan to avoid slow plans when invoice with 10k lines queried.
			};

			var singleScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(ScriptToTest.Text, excludeLinesForSingle);
			var batchScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(new AccountingTransactionExportGetLines().Text, excludeLinesForBatch);

			AssertMultilineASCIIEquals("Core SELECT query of batch query must be identical to single version", batchScriptCoreLinesToCompare, singleScriptCoreLinesToCompare);
		}

		public void TestAccountingTransactionExportGetLinesSingle_TaxBranch_IsNotNull()
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
DECLARE @GLHeaderPK UNIQUEIDENTIFIER = '010038A8-6368-4050-9586-D71D52EDEDF0'

INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC)
VALUES('{taxBranchPk}', '{expectedTaxBranchCode}', '{expectedTaxBranchName}', @CompanyPk)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger)
VALUES ('{transactionPk}', '001', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', @OrgPK, 'AR')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType, AL_GB_TaxBranch)
VALUES(NEWID(), '{transactionPk}', @GLHeaderPK, @BranchPK, @CompanyPK, @DepartmentPK, 'REV', '{taxBranchPk}')";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetLinesSingle '{transactionPk}', '2015-5-5'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals($"{sql}: Should have rows", 1, result.Rows.Count);

			AssertEquals(expectedTaxBranchCode, result.Rows[0]["TaxBranchCode"]);
			AssertEquals(expectedTaxBranchName, result.Rows[0]["TaxBranchName"]);
		}

		public void TestAccountingTransactionExportGetLinesSingle_TaxBranch_IsNull()
		{
			var transactionPk = Guid.NewGuid();

			var insertSql = $@"
DECLARE @OrgPK UNIQUEIDENTIFIER = '8AF9D5D6-562F-41CF-823B-59FBCF182D37'
DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'
DECLARE @GLHeaderPK UNIQUEIDENTIFIER = '010038A8-6368-4050-9586-D71D52EDEDF0'

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger)
VALUES ('{transactionPk}', '001', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', @OrgPK, 'AR')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_AG, AL_GB, AL_GC, AL_GE, AL_LineType, AL_GB_TaxBranch)
VALUES(NEWID(), '{transactionPk}', @GLHeaderPK, @BranchPK, @CompanyPK, @DepartmentPK, 'REV', NULL)";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetLinesSingle '{transactionPk}', '2015-5-5'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals($"{sql}: Should have rows", 1, result.Rows.Count);

			AssertEquals(DBNull.Value, result.Rows[0]["TaxBranchCode"]);
			AssertEquals(DBNull.Value, result.Rows[0]["TaxBranchName"]);
		}
	}
}

