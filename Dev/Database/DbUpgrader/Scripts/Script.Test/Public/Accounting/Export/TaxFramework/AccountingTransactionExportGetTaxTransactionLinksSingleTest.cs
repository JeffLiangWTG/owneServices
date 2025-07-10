using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export.TaxFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.TaxFramework.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetTaxTransactionLinksSingle))]
	class AccountingTransactionExportGetTaxTransactionLinksSingleTest : DbCreateScriptTest
	{
		public void TestResultColumnsAreIdenticalToBatchVersion()
		{
			var singleResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetTaxTransactionLinksSingle '{Guid.NewGuid()}'");
			var batchResult = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC AccountingTransactionExportGetTaxTransactionLinks 'EDI', -1");
			AssertSameSchema("Result schemas from batch and single export stored procs must be identical in column order, column name and column type. When making changes to one, you must make identical changes for the other.",
				batchResult,
				singleResult
			);
		}
	}
}

