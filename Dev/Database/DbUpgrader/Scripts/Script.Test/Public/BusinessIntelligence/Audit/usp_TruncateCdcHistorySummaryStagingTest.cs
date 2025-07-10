using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	internal class usp_TruncateCdcHistorySummaryStagingTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Audit } )]
		public void TestTruncateCdcHistorySummaryStaging()
		{
			using (var conn = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.AuditDatabaseName))
			{
				var sqlText = @"INSERT INTO [biadmin].[CdcHistorySummaryStaging] (TableID, Lsn, LsnPeriod) VALUES (1, 0x0, 2209)";
				conn.ExecuteNonQuery(sqlText);

				AssertEquals("CdcHistorySummaryStaging count before sproc execution", 1, GetCdcHistorySummaryStagingCount(conn));

				conn.ExecuteNonQuery("EXEC [biadmin].[usp_TruncateCdcHistorySummaryStaging]");

				AssertEquals("CdcHistorySummaryStaging count after sproc execution", 0, GetCdcHistorySummaryStagingCount(conn));
			}
		}

		int GetCdcHistorySummaryStagingCount(DbConnection conn)
		{
			return Convert.ToInt32(conn.ExecuteScalar("SELECT COUNT(*) FROM [biadmin].[CdcHistorySummaryStaging]"));
		}
	}

	[TestedType(typeof(usp_TruncateCdcHistorySummaryStaging))]
	internal class usp_TruncateCdcHistorySummaryStagingTransactionedTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

