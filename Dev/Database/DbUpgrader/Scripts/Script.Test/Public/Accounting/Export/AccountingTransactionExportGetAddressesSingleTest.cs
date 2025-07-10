using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetAddressesSingle))]
	class AccountingTransactionExportGetAddressesSingleTest : DbCreateScriptTest
	{
		public void TestResultColumnsAreIdenticalToBatchVersion()
		{
			var singleResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetAddressesSingle '{Guid.NewGuid()}'");
			var batchResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetAddresses 'EDI', -1, NULL");
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
				"		INNER JOIN @HeaderPKs pks",		// Join on PKs table; replaced by WHERE in single version
				"			ON AH_PK = pks.PK",			// Join on PKs table; replaced by WHERE in single version
				"OPTION(RECOMPILE)",					// Recompile to ensure best query plan used in batch only.
			};
			var excludeLinesForSingle = new[]
			{
				"			WITH (FORCESEEK INDEX (PK_UC__AH_PK))",		// Index hints on all single versions to avoid poor plans.
				"		WHERE AH_PK = @TransactionHeaderPK",			// Replacement for PK table join.
			};

			var singleScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(ScriptToTest.Text, excludeLinesForSingle);
			var batchScriptCoreLinesToCompare = BatchAndSingleTestHelper.GetScriptLinesToCompare(new AccountingTransactionExportGetAddresses().Text, excludeLinesForBatch);

			AssertMultilineASCIIEquals("Core SELECT query of batch query must be identical to single version", batchScriptCoreLinesToCompare, singleScriptCoreLinesToCompare);
		}
	}
}

