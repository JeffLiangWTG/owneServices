using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	[UseSnapshotProtection(skipTransaction: true)]
	sealed internal class RecreateIndexesForCommandIdInAuditDbCommandIdNotPartOfIndexTest : RecreateIndexesForCommandIdInAuditDbTest
	{
		protected override SchemaTableNamePair[] TestTables => new SchemaTableNamePair[] {
			new SchemaTableNamePair("Test", "TestTable")
		};

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($@"
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
)"
);

			CreateTableConfigIfNotExists();
			AddTestTableToTableConfig();

			TestConnection.ExecuteNonQuery(@"
				CREATE NONCLUSTERED INDEX [IX_TestTable_StartLsn] ON [Test].[TestTable]
				(
					[__$start_lsn] ASC,
					[__$seqval] ASC,
					[__$operation] ASC
				) WITH (DATA_COMPRESSION = PAGE)
			");
		}
	}
}
