using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	[UseSnapshotProtection(skipTransaction: true)]
	sealed class RecreateIndexesForPrimaryKeyAndUpdateMaskInAuditDbIndexNotExistTest : RecreateIndexesForPrimaryKeyAndUpdateMaskInAuditDbTest
	{
		protected override SchemaTableNamePair[] TestTables => new SchemaTableNamePair[] {
			new SchemaTableNamePair("Test", "TestTable")
		};

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(@"
CREATE SCHEMA [Test]

CREATE TABLE [Test].[TestTable]
(
	[__$start_lsn] [binary](10) NOT NULL,
	[__$seqval] [binary](10) NOT NULL,
	[__$operation] [int] NOT NULL,
	[__$update_mask] [varbinary](128) NOT NULL,
	[__$lsn_period] [smallint] NOT NULL,
	[__$command_id] [int] NOT NULL,
	[Test_PK] [uniqueidentifier] NULL
)");

			CreateTableConfigIfNotExists();
			AddTestTableToTableConfig();
		}
	}
}
