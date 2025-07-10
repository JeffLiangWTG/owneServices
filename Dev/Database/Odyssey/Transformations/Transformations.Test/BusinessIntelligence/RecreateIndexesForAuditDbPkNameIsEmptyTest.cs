using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	internal class RecreateIndexesForAuditDbPkNameIsEmptyTest : RecreateIndexesForCommandIdInAuditDbTest
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

			TestConnection.ExecuteNonQuery(@"
				CREATE NONCLUSTERED INDEX [IX_TestTable_StartLsn] ON [Test].[TestTable]
				(
					[__$start_lsn] ASC,
					[__$seqval] ASC,
					[__$operation] ASC
				) WITH (DATA_COMPRESSION = PAGE)
			");
		}

		public void TestGetListofTableToRecreateIndexDoesNotContainPkNameIsNull()
		{
			PrepareTestData();
			using (((ICurrentDbControl)TestConnection).UseDatabase(TestAuditDbName))
			{
				TestConnection.ExecuteNonQuery(@"
					INSERT INTO [biadmin].[TableConfiguration]
					(SourceSchemaName, SourceTableName, PkName, TableColumnList)
					VALUES 
					('Test', 'TestTable', NULL, '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,TT_PK');
				");

				var recreateIndexes = new RecreateIndexesForAuditDb();
				var tablesListForRecreateIndex = recreateIndexes.GetListofTableToRecreateIndex(TestConnection);
				var tablesWithTestTables = tablesListForRecreateIndex.Where(x => x.TableName.Equals("TestTable"));
				Assert(!tablesWithTestTables.Any());
			}
		}

		public void TestRecreateIndexesForAuditTablesWithPkNameIsNull()
		{
			PrepareTestData();
			using (((ICurrentDbControl)TestConnection).UseDatabase(TestAuditDbName))
			{
				TestConnection.ExecuteNonQuery(@"
					INSERT INTO [biadmin].[TableConfiguration]
					(SourceSchemaName, SourceTableName, PkName, TableColumnList)
					VALUES 
					('Test', 'TestTable', NULL, '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,TT_PK');
				");

				var recreateIndexes = new RecreateIndexesForAuditDb();
				try
				{
					recreateIndexes.RunAuditTransformation(TestConnection, TransformationSection.OnlinePreUpgrade, CancellationToken.None);
				}
				catch (SqlException ex)
				{
					Fail("Primary key name is empty for table TestTable not filter\n" + ex.Message);
				}
				Assert("Primary key name is empty for table TestTable filter", true);
			}
		}
	}
}
