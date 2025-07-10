using System;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	[UseSnapshotProtection(skipTransaction: true)]
	abstract class RecreateIndexesForCommandIdInAuditDbTest : AuditDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RecreateIndexesForAuditDb();

		protected const string TestAuditDbName = "TestDb_Audit";

		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection());
			}
		}
		DbConnection testConnection;

		protected abstract SchemaTableNamePair[] TestTables { get; }

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

			CreateTableConfigIfNotExists();
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

		protected override void AssertTransformationResults()
		{
			CheckCommandIdColumnExists();
			CheckIndexExists();
			CheckIndexDataCompression();
			CheckIndexMaintenanceTaskRescheduled();
		}

		void CheckCommandIdColumnExists()
		{
			var sqlText = string.Format("IF EXISTS (select null from sys.columns where name = '__$command_id' and object_id in ({0})) SELECT 1 ELSE SELECT 0",
				string.Join(",", TestTables.Select(s => $"OBJECT_ID('{s.SchemaName}.{s.TableName}')")));
			Assert("__$command_id column was not created.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		void CheckIndexExists()
		{
			var sqlText = string.Format("IF EXISTS (select null from sys.indexes where name like 'IX_%_StartLsn' and object_id in ({0})) SELECT 1 ELSE SELECT 0",
				string.Join(",", TestTables.Select(s => $"OBJECT_ID('{s.SchemaName}.{s.TableName}')")));
			Assert("IX_TestTable_StartLsn index was not created.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		void CheckIndexDataCompression()
		{
			var sqlText = string.Format("IF EXISTS (SELECT null FROM sys.partitions p JOIN sys.indexes i ON p.object_id = i.object_id AND i.index_id = p.index_id WHERE i.name in ({0}) AND p.data_compression_desc = 'PAGE') SELECT 1 ELSE SELECT 0",
				string.Join(",", TestTables.Select(s => $"'IX_{s.TableName}_StartLsn'")));
			Assert("IX_TestTable_StartLsn has incorrect data compression.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		void CheckIndexMaintenanceTaskRescheduled()
		{
			var sqlText = $"SELECT ParamValue FROM biadmin.MasterState WHERE ParamName = N'{BiConstants.LastIndexRebuildUtcDt}'";
			AssertGreaterThanOrEqualTo("Index Maintenance task should be rescheduled.", Convert.ToDateTime(TestConnection.ExecuteScalar(sqlText)), DateTime.UtcNow.AddMinutes(-1));
		}

		protected void CreateTableConfigIfNotExists()
		{
			TestConnection.ExecuteNonQuery(@"

				IF NOT EXISTS (
					SELECT * FROM sys.tables
					WHERE name = 'TableConfiguration' AND schema_id = SCHEMA_ID('biadmin'))
				BEGIN
					CREATE TABLE [biadmin].[TableConfiguration]
					(
						[SourceSchemaName] [nvarchar](MAX) NOT NULL,
						[SourceTableName] [nvarchar](MAX) NOT NULL,
						[PkName] [nvarchar](MAX) NULL,
						[TableColumnList] [nvarchar](MAX) NOT NULL,
					)
				END
			");
		}

		protected void AddTestTableToTableConfig()
		{
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO [biadmin].[TableConfiguration]
				(SourceSchemaName, SourceTableName, PkName, TableColumnList)
				VALUES 
				('Test', 'TestTable', 'Test_PK', '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,Test_PK');
			");
		}
	}
}
