using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class ReferenceDbUpgraderGenericTest : TransactionedTestCase
	{
		public void TestFixColumnCollation()
		{
			// Create table with different collation columns
			string createTableSql = @"
				CREATE TABLE [TestFixColumnCollation!Table!]
				(
					Col1 varchar(10) COLLATE Latin1_General_CI_AS NULL,
					Col2 char(2) COLLATE Latin1_General_BIN NOT NULL
				)";
			TestConnection.ExecuteNonQuery(createTableSql);

			// Assert collation before
			DbSchemaChangeTest.AssertColumnCollation(TestConnection, "TestFixColumnCollation!Table!", "Col1", "Latin1_General_CI_AS");
			DbSchemaChangeTest.AssertColumnCollation(TestConnection, "TestFixColumnCollation!Table!", "Col2", "Latin1_General_BIN");

			// Run ref db upgrade
			var refDbUpgrader = new ReferenceDbUpgraderForAfterUpgStepTesting(upgradeContext, TestConnection);
			refDbUpgrader.CreateAndUpgradeIfRequired();

			// Assert collation afterwards
			DbSchemaChangeTest.AssertColumnCollation(TestConnection, "TestFixColumnCollation!Table!", "Col1", Db.DatabaseCollation);
			DbSchemaChangeTest.AssertColumnCollation(TestConnection, "TestFixColumnCollation!Table!", "Col2", Db.DatabaseCollation);
		}

		public void TestColumnsSchemaRequirementsEnsuredInCaseOfConstraintColumnMismatch()
		{
			// Create table with different collation columns
			string createTableSql = @"
				CREATE TABLE dbo.Table1 (Col1 int, Col2 char(1));
				ALTER TABLE dbo.Table1 ADD CONSTRAINT DF_Table1_Col1 DEFAULT 0 FOR Col2;";
			TestConnection.ExecuteNonQuery(createTableSql);

			// Run EnsureColumnSchemaRequirements
			var refDbUpgrader = new ReferenceDbUpgraderForAfterUpgStepTesting(upgradeContext, TestConnection);
			refDbUpgrader.EnsureColumnSchemaRequirementsExposed(TestConnection);

			// Assert no incorrect constraints exist
			string assertSql = @"
				SELECT COUNT(*)
				FROM
					  sys.tables tab
					  INNER JOIN sys.columns col ON col.object_id = tab.object_id
					  INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
					  INNER JOIN sys.default_constraints def
							ON def.name = 'DF_' + tab.name + '_' + col.name
							AND (def.parent_object_id <> tab.object_id OR def.parent_column_id <> col.column_id)
				WHERE
					  tab.is_ms_shipped = 0
				;";
			AssertEquals(0, TestConnection.ExecuteScalar(assertSql));
		}

		public void TestAnyForeignKeyHasAnIndex()
		{
			string sqlText = $@"
			SELECT
				OBJECT_NAME(ForeignKeyColumn.parent_object_id) as TableName,
				COL_NAME(ForeignKeyColumn.parent_object_id, ForeignKeyColumn.parent_column_id) as ColumnName,
				OBJECT_NAME(ForeignKeyColumn.constraint_object_id) as ForeignKeyName,
				OBJECT_NAME(ForeignKeyColumn.referenced_object_id) as ReferencedTableName,
				COL_NAME(ForeignKeyColumn.referenced_object_id, ForeignKeyColumn.referenced_column_id) as ReferencedColumnName,
				Indexes.name as IndexName 
			FROM sys.foreign_key_columns ForeignKeyColumn
			-----should be the first column of some index
			LEFT join sys.index_columns IndexColumn on IndexColumn.object_id=ForeignKeyColumn.parent_object_id and IndexColumn.column_id=ForeignKeyColumn.parent_column_id and IndexColumn.index_column_id=1
			LEFT join sys.indexes Indexes on Indexes.object_id=IndexColumn.object_id and Indexes.index_id=IndexColumn.index_id
			WHERE IndexColumn.object_id is null
			ORDER BY
				OBJECT_NAME(ForeignKeyColumn.parent_object_id),
				COL_NAME(ForeignKeyColumn.parent_object_id, ForeignKeyColumn.parent_column_id)
";

			var messageBuilder = new StringBuilder();
			foreach (var databaseName in TestConnection.GetDatabases(DatabaseType.ExclusiveRefOrSharedRef))
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(databaseName))
				{
					using (var reader = TestConnection.Command(sqlText).ExecuteReader())
					{
						var builder = new StringBuilder();
						while (reader.Read())
						{
							var tableName = reader["TableName"].ToString();
							var columnName = reader["ColumnName"].ToString();
							var columnIsExecluded = TableForeignKeyColumnIgnoreList.ContainsKey(tableName) && TableForeignKeyColumnIgnoreList[tableName].Contains(columnName);
							if (!columnIsExecluded)
							{
								builder.AppendLine(tableName);
								builder.AppendLine(columnName);
							}
						}
						if (builder.Length != 0)
						{
							string assertMsg = string.Format("The following foreign key columns in '{0}' don't have an index:\r\n\r\n", databaseName);
							messageBuilder.AppendLine(assertMsg + builder.ToString());
						}
					}
				}
			}
			Assert(messageBuilder.ToString(), messageBuilder.Length == 0);
		}

		readonly Dictionary<string, string[]> TableForeignKeyColumnIgnoreList = new Dictionary<string, string[]>()
		{
			{ "RefCusCodeListAttributeName", new string[] { "ZXE_ZZK_NKCodeType" , "ZXE_ZZK_NKCodeTypeForValueList" } },// Foreign key will be delete soon
		};

		public void TestSynonymDroppedWhenPointingObjectsDeletedFromRefDb()
		{
			var refDbName = RefDbTableNameResolver.DefaultSingleRefDbName;
			Assert($"'{refDbName}' database should exist.", TestConnection.DatabaseExists(refDbName));

			var workflowLoggerMock = new Mock<IUpgradeTaskWorkflowLogger>();

			var refDbUpgrader = new ReferenceDbUpgraderForSynonymTesting(upgradeContext, TestConnection, workflowLoggerMock.Object, refDbName);

			using (((ICurrentDbControl)TestConnection).UseDatabase(refDbName))
			{
				TestConnection.ExecuteNonQuery("CREATE TABLE dbo.[SynonymTest-Table] (Col1 BIT)");
				TestConnection.ExecuteNonQuery("CREATE VIEW dbo.[SynonymTest-View] AS SELECT Col1 = null");
				TestConnection.ExecuteNonQuery("CREATE PROCEDURE dbo.[SynonymTest-Proc] AS SELECT null");
				TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-InlineFunc]() RETURNS TABLE AS RETURN SELECT Col1 = null");
				TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-TableFunc]() RETURNS @Table TABLE (Col1 BIT) AS BEGIN RETURN END");
				TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-ScalarFunc]() RETURNS BIT AS BEGIN RETURN null END");
			}

			refDbUpgrader.SynchroniseSynonyms();

			CombineAssertions("Synonyms exist", () =>
			{
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-Table", expected: true);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-View", expected: true);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-Proc", expected: true);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-InlineFunc", expected: true);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-TableFunc", expected: true);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-ScalarFunc", expected: true);
			});

			using (((ICurrentDbControl)TestConnection).UseDatabase(refDbName))
			{
				TestConnection.ExecuteNonQuery("DROP TABLE dbo.[SynonymTest-Table]");
				TestConnection.ExecuteNonQuery("DROP VIEW dbo.[SynonymTest-View]");
				TestConnection.ExecuteNonQuery("DROP PROCEDURE dbo.[SynonymTest-Proc]");
				TestConnection.ExecuteNonQuery("DROP FUNCTION dbo.[SynonymTest-InlineFunc]");
				TestConnection.ExecuteNonQuery("DROP FUNCTION dbo.[SynonymTest-TableFunc]");
				TestConnection.ExecuteNonQuery("DROP FUNCTION dbo.[SynonymTest-ScalarFunc]");
			}

			refDbUpgrader.SynchroniseSynonyms();
			CombineAssertions("After Synchronising Synonyms dropped when pointing to invalid objects", () =>
			{
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-Table", expected: false);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-View", expected: false);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-Proc", expected: false);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-InlineFunc", expected: false);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-TableFunc", expected: false);
				AssertSynonymExists($"{refDbUpgrader.SynonymPrefix}SynonymTest-ScalarFunc", expected: false);
			});
		}

		public void TestSynonymsAddedForSupportedDbObjects()
		{
			var xxRefDbUpgrader = new ReferenceDbUpgraderForAfterUpgStepTesting(upgradeContext, TestConnection);

			TestConnection.ExecuteNonQuery("CREATE TABLE dbo.[SynonymTest-Table] (Col1 BIT)");
			TestConnection.ExecuteNonQuery("CREATE VIEW dbo.[SynonymTest-View] AS SELECT Col1 = null");
			TestConnection.ExecuteNonQuery("CREATE PROCEDURE dbo.[SynonymTest-Proc] AS SELECT null");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-InlineFunc]() RETURNS TABLE AS RETURN SELECT Col1 = null");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-TableFunc]() RETURNS @Table TABLE (Col1 BIT) AS BEGIN RETURN END");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION dbo.[SynonymTest-ScalarFunc]() RETURNS BIT AS BEGIN RETURN null END");

			CombineAssertions("Before Synchronising Synonyms", () =>
			{
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-Table", expected: false);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-View", expected: false);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-Proc", expected: false);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-InlineFunc", expected: false);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-TableFunc", expected: false);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-ScalarFunc", expected: false);
			});

			xxRefDbUpgrader.SynchroniseSynonyms();

			CombineAssertions("After Synchronising Synonyms", () =>
			{
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-Table", expected: true);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-View", expected: true);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-Proc", expected: true);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-InlineFunc", expected: true);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-TableFunc", expected: true);
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-ScalarFunc", expected: true);
			});
		}

		public void TestCreateSynonymForIS_MS_ShippedObject()
		{
			var xxRefDbUpgrader = new ReferenceDbUpgraderForAfterUpgStepTesting(upgradeContext, TestConnection);
			var connection = Db.NewAdminConnection();
			try
			{
				connection.ExecuteNonQuery("CREATE TABLE dbo.[SynonymTest-Table] (Col1 BIT)");
				connection.ExecuteNonQuery("exec sys.sp_ms_marksystemobject [SynonymTest-Table]");
				xxRefDbUpgrader.SynchroniseSynonyms();
				AssertSynonymExists($"{xxRefDbUpgrader.SynonymPrefix}SynonymTest-Table", true);
			}
			finally
			{
				connection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo.[SynonymTest-Table]");
				connection.Dispose();
			}
		}

		void AssertSynonymExists(string synonymName, bool expected)
		{
			var actual = TestConnection.Exists($"FROM sys.synonyms WHERE name = '{synonymName}'");
			AssertEquals($"[{synonymName}] synonym exists", expected, actual);
		}

		protected override void SetUp()
		{
			upgradeContext = new Mock<IUpgradeContext>().Object;
			base.SetUp();
		}

		IUpgradeContext upgradeContext;

		class ReferenceDbUpgraderForSynonymTesting : ReferenceDbUpgrader
		{
			public ReferenceDbUpgraderForSynonymTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger, string refDbName)
				: base(upgradeContext, upgradeConnection, logger)
			{
				dbPreparationStrategy = new RefDbPreparationStrategyForSynonymTesting(refDbName);
			}

			public override string ReferenceName => null;

			public override RefDbTypeEnum DatabaseType => RefDbTypeEnum.Enterprise;

			public override string CountryCode => "XX";

			public override int LatestVersion => 1;

			protected override void DoDataUpgrade(DbConnection conn, int versionBeforeUpgrade)
			{
			}

			class RefDbPreparationStrategyForSynonymTesting : IRefDbPreparationStrategy
			{
				public RefDbPreparationStrategyForSynonymTesting(string refDbName)
				{
					RefDbName = refDbName;
				}
				public string RefDbName { get; }

				public int GetVersionFromDatabase()
				{
					return 1;
				}

				public void PrepareAndUpgradeDatabase(Action<DbConnection> performUpgradeTasksCallback)
				{
				}
			}
		}

		sealed class ReferenceDbUpgraderForAfterUpgStepTesting : ReferenceDbUpgrader
		{
			public ReferenceDbUpgraderForAfterUpgStepTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection)
				: base(upgradeContext, upgradeConnection, new UpgradeTaskWorkflowLoggerTestClass())
			{
				dbPreparationStrategy = new RefDbPreparationStrategyInTheMainDbForTesting(RefDbTypeEnum.Enterprise, refDbCountry: "XX", upgradeConnection);
			}

			public override int LatestVersion
			{
				get { return 1; }
			}

			protected override void DoDataUpgrade(DbConnection conn, int versionBeforeUpgrade)
			{
			}

			public void EnsureColumnSchemaRequirementsExposed(DbConnection conn)
			{
				base.EnsureColumnSchemaRequirements(conn);
			}

			public override string ReferenceName
			{
				get { return CountryCode + " TEST"; }
			}

			public override RefDbTypeEnum DatabaseType
			{
				get { return (dbPreparationStrategy as RefDbPreparationStrategyInTheMainDbForTesting).RefDbType; }
			}

			public override string CountryCode
			{
				get { return (dbPreparationStrategy as RefDbPreparationStrategyInTheMainDbForTesting).RefDbCountry; }
			}

			class RefDbPreparationStrategyInTheMainDbForTesting : StandardRefDbPreparationStrategy
			{
				public RefDbPreparationStrategyInTheMainDbForTesting(RefDbTypeEnum refDbType, string refDbCountry, DbConnection upgradeConnection)
					: base(Db.DatabaseName, refDbType, refDbCountry, upgradeConnection)
				{
				}

				protected override string GetLatestVersionDatabaseName()
				{
					return mainDbName;
				}

				public RefDbTypeEnum RefDbType { get { return this.refDbType; } }
				public string RefDbCountry { get { return this.refDbCountry; } }
			}
		}
	}
}
