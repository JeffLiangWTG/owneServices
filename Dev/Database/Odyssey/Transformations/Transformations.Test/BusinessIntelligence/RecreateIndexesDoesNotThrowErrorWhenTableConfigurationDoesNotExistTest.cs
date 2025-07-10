using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	[UseSnapshotProtection(skipTransaction: true)]
	sealed class RecreateIndexesDoesNotThrowErrorWhenTableConfigurationDoesNotExistTest : AuditDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RecreateIndexesForAuditDb();
		const string TestAuditDbName = "TestDb_Audit";

		DbConnection testConnection;
		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection());
			}
		}

		protected override void AssertTransformationResults()
		{
			CheckIndexDoesNotExist();
		}

		void CheckIndexDoesNotExist()
		{
			var sqlText = "IF EXISTS (select null from sys.indexes where name like 'IX_%_StartLsn' and object_id in (OBJECT_ID('Test.TestTable'))) SELECT 1 ELSE SELECT 0";
			Assert("IX_TestTable_StartLsn index should not be created.", !Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery($"IF EXISTS (SELECT null FROM sys.databases WHERE name = '{TestAuditDbName}') DROP DATABASE [{TestAuditDbName}]; CREATE DATABASE [{TestAuditDbName}]");
			((ICurrentDbControl)TestConnection).UseDatabase(TestAuditDbName);

			TestConnection.ExecuteNonQuery(@"
				CREATE SCHEMA [biadmin]

				CREATE TABLE [biadmin].[MasterState]
				(
					[ParamID] [int] IDENTITY(1,1) NOT NULL,
					[ParamName] [nvarchar](MAX) NULL,
					[ParamValue] [nvarchar](MAX) NULL,
				)
			");

			TestConnection.ExecuteNonQuery(@"
				IF EXISTS (
					SELECT * FROM sys.tables
					WHERE name = 'TableConfiguration' AND schema_id = SCHEMA_ID('biadmin')
				)
				BEGIN
					DROP TABLE biadmin.TableConfiguration
				END
			");
		}

		protected override void TearDown()
		{
			((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb);
			TestConnection.ExecuteNonQuery($"DROP DATABASE [{TestAuditDbName}]");

			if (testConnection != null)
			{
				testConnection.Dispose();
				testConnection = null;
			}

			base.TearDown();
		}

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
				)"
			);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetListofTableToRecreateIndexDoesNotContainTablesThatAreNotGoingToBeTrackedByCdc()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				TestConnection.ExecuteNonQuery(@"
					INSERT INTO [biadmin].[TableConfiguration]
					(SourceSchemaName, SourceTableName, PkName, TableColumnList)
					VALUES 
					('Test', 'TestTable', 'TT_PK', '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,TT_PK'),
					('Test', 'TestTable2', NULL, '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,TT_PK');
				");

				var recreateIndexes = new RecreateIndexesForAuditDb();
				var tablesListForRecreateIndex = recreateIndexes.GetListofTableToRecreateIndex(TestConnection);
				var tablesWithEmptyPrimaryKey = tablesListForRecreateIndex.Where(x => x.PrimaryKeyName == null);
				Assert(!tablesWithEmptyPrimaryKey.Any());
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOutgoingAuditTableDoesNotCauseDbUpgradeToFail()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				//Create a dummy audit table that has the mandatory columns of a normal Audit table(no rows).
				TestConnection.ExecuteNonQuery(@"
					CREATE TABLE [dbo].[TestTable]
					(
						__$start_lsn BINARY(10) NOT NULL,
						__$seqval BINARY(10) NOT NULL,
						__$operation INT NOT NULL,
						__$update_mask BINARY(10) NOT NULL,
						__$command_id INT NOT NULL,
					);
				");

				//This is a table in the database that isn't in the config.
				TestConnection.ExecuteNonQuery("DELETE FROM [biadmin].[TableConfiguration] WHERE SourceTableName = 'TestTable'"); // Should affect 0 rows anyway

				//If an upgrade succeeds without error, this demonstrates that the defect is fixed.
				var recreateIndexes = new RecreateIndexesForAuditDb();
				AssertNoExceptionThrown(() => recreateIndexes.RunAuditTransformation(TestConnection, TransformationSection.OnlinePreUpgrade, CancellationToken.None));
			}
		}
	}
}
