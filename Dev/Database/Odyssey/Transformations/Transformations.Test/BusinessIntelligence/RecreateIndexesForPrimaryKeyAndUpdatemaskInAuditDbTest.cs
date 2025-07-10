using System;
using System.Linq;
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
	abstract class RecreateIndexesForPrimaryKeyAndUpdateMaskInAuditDbTest : AuditDataTransformationTestCase
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
				);
			");

			TestConnection.ExecuteNonQuery(@"
				IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TableConfiguration' AND schema_id = SCHEMA_ID('biadmin'))
				BEGIN
					CREATE TABLE [biadmin].[TableConfiguration]
					(
						[SourceSchemaName] [nvarchar](MAX) NOT NULL,
						[SourceTableName] [nvarchar](MAX) NOT NULL,
						[PkName] [nvarchar](MAX) NOT NULL,
						[TableColumnList] [nvarchar](MAX) NOT NULL,
					)
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

		protected override void AssertTransformationResults()
		{
			CheckUpdateMaskAndPrimaryKeyColumnExists();
			CheckIndexExists();
			CheckIndexDataCompression();
			CheckUpdateMaskAndPrimaryKeyIncluded();
			CheckIndexMaintenanceTaskRescheduled();
		}

		void CheckUpdateMaskAndPrimaryKeyColumnExists()
		{
			var sqlText = string.Format("IF EXISTS (select null from sys.columns where name = '__$update_mask' and object_id in ({0})) SELECT 1 ELSE SELECT 0",
				string.Join(",", TestTables.Select(s => $"OBJECT_ID('{s.SchemaName}.{s.TableName}')")));
			Assert("__$update_mask column was not created.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));

			sqlText = string.Format("IF EXISTS (select null from sys.columns where name = 'Test_PK' and object_id in ({0})) SELECT 1 ELSE SELECT 0",
				string.Join(",", TestTables.Select(s => $"OBJECT_ID('{s.SchemaName}.{s.TableName}')")));
			Assert("Test_PK column was not created.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
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
			var sqlText = "SELECT ParamValue FROM biadmin.MasterState WHERE ParamName = N'LAST_INDEX_REBUILD_UTC_DT'";
			AssertGreaterThanOrEqualTo("Index Maintenance task should be rescheduled.", Convert.ToDateTime(TestConnection.ExecuteScalar(sqlText)), DateTime.UtcNow.AddMinutes(-1));
		}

		void CheckUpdateMaskAndPrimaryKeyIncluded()
		{
			var sqlText = @"
				SELECT
					SourceSchemaName AS SchemaName,
					SourceTableName AS TableName,
					PkName
				INTO #TABLE_LIST
				FROM biadmin.TableConfiguration;

				IF EXISTS(
					SELECT NULL
					From
						sys.tables t
					LEFT JOIN
						sys.indexes i ON t.object_id = i.object_id AND i.type = 2 AND i.name = 'IX_' + t.name + '_StartLsn'
					LEFT JOIN
						sys.schemas s ON s.schema_id = t.schema_id
					LEFT JOIN 
						#TABLE_LIST config ON config.SchemaName = s.name AND config.TableName = t.name 
					LEFT JOIN 
						sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1		
					LEFT JOIN 
						sys.columns c ON c.object_id = i.object_id  AND c.column_id = ic.column_id AND (c.name = '__$update_mask' OR c.name = config.PkName)
					WHERE 
						schema_name(t.schema_id) <> 'biadmin' 
						AND c.name is NULL
				)
					SELECT 0
				ELSE
					SELECT 1;

				DROP TABLE #TABLE_LIST;";
			Assert("__$update_mask or Test_PK is not include column. ", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
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
	}
}
