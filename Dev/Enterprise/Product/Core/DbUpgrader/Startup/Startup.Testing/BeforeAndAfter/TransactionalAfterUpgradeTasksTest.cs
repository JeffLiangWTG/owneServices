using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Schema.Synchronisers;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	class TransactionalAfterUpgradeTasksTest : TestCase
	{
		class TransactionalAfterUpgradeTasksForTest : TransactionalAfterUpgradeTasks
		{
			public TransactionalAfterUpgradeTasksForTest(AdminConnection connection, IUpgradeTaskWorkflowControl logger, bool isRunningSchemaOrScriptUpgrade)
				: base(connection, logger, isRunningSchemaOrScriptUpgrade)
			{ }

			public void EnsureCdcConfiguration_Exposed()
			{
				EnsureCdcConfiguration();
			}

			public void EnsureAutoVersionTriggers_Exposed()
			{
				EnsureAutoVersionTriggers("dbo");
			}

			public void RemoveRegistryIndex_Exposed()
			{
				RemoveRegistryIndex();
			}
		}

		[UseSnapshotProtection]
		public void TestAllAutoVersionTableHaveTrigger()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var logger = new Mock<IUpgradeTaskWorkflowControl>();
				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(testConnection, logger.Object, true);
				afterUpgradeTasks.EnsureAutoVersionTriggers_Exposed();

				var tables = new List<(string schema, string name, string code, string definition)>();
				testConnection.ExecuteReader(
$@"SELECT
	SchemaName		= schema_name(tab.schema_id),
	TabName			= tab.name,
	TabCode			= SUBSTRING(col.name, 1, CHARINDEX('_', col.name) - 1),
	TrgDefinition	= def.definition
FROM
	sys.tables					AS tab
	JOIN sys.columns			AS col ON col.object_id = tab.object_id
	LEFT JOIN sys.triggers		AS trg ON trg.parent_id = col.object_id
		AND trg.name = CONCAT(N'TG_', tab.name, N'_UpdateAutoVersion')
	LEFT JOIN sys.sql_modules	AS def ON def.object_id = trg.object_id
WHERE
	tab.is_ms_shipped = 0
	AND col.name LIKE N'%[_]AutoVersion'",
					(reader) => tables.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? string.Empty : reader.GetString(3))));

				var tablesNeedToCheckColumnValues = tables.Where(tbl => AutoVersionTriggerFactory.ColumnsThatDontUpdateAutoVersion.ContainsKey($"{tbl.schema}.{tbl.name}")).ToList();

				foreach (var (schema, name, code, definition) in tables)
				{
					if (AutoVersionTriggerFactory.ColumnsThatDontUpdateAutoVersion.ContainsKey($"{schema}.{name}"))
					{
						var columnsToCheck = GetColumnsToCheck(schema, name, testConnection);
						var expectedTriggerDefinition = AutoVersionTriggerThatChecksColumnValues(schema, name, code, columnsToCheck);
						AssertEquals($"table {name} should have an autoversion trigger. Expected Column order: {string.Join(", ", columnsToCheck)}", expectedTriggerDefinition, definition);
					}
					else
					{
						var expectedTriggerDefinition = AutoVersionTrigger(schema, name, code);
						AssertEquals($"table {name} should have an autoversion trigger.", expectedTriggerDefinition, definition);
					}
				}
			}
		}

		static string[] GetColumnsToCheck(string schemaName, string tableName, DbConnection testConnection)
		{
			var sql =
@"SELECT
	ColName = col.name
FROM
	sys.tables					AS tab
	INNER JOIN sys.columns		AS col ON col.object_id = tab.object_id
WHERE
	tab.name = @tableName
	AND tab.schema_id = SCHEMA_ID(@schemaName)
ORDER BY
	col.name ASC";

			var result = new List<string>();
			testConnection.ExecuteReader(
				sql,
				(dbCommand) =>
				{
					dbCommand.AddParameter("schemaName", SqlDbType.NVarChar, 128, schemaName);
					dbCommand.AddParameter("tableName", SqlDbType.NVarChar, 128, tableName);
				},
				(reader) => result.Add(reader.GetString(0)));

			return result.Except(AutoVersionTriggerFactory.ColumnsThatDontUpdateAutoVersion[$"{schemaName}.{tableName}"]).ToArray();
		}

		static string AutoVersionTrigger(string schemaName, string tableName, string tableCode)
		{
			return $@"CREATE TRIGGER {schemaName}.TG_{tableName}_UpdateAutoVersion
	ON {schemaName.QuoteName()}.{tableName.QuoteName()}
	AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON
	UPDATE tab SET tab.{tableCode}_AutoVersion = (tab.{tableCode}_AutoVersion + 1) % 32768
	FROM {schemaName.QuoteName()}.{tableName.QuoteName()} AS tab
	INNER JOIN INSERTED AS ins ON ins.{tableCode}_PK = tab.{tableCode}_PK;
END";
		}

		static string AutoVersionTriggerThatChecksColumnValues(string schemaName, string tableName, string tableCode, string[] columns)
		{
			return $@"CREATE TRIGGER {schemaName}.TG_{tableName}_UpdateAutoVersion
	ON {schemaName.QuoteName()}.{tableName.QuoteName()}
	AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON
	UPDATE tab SET tab.{tableCode}_AutoVersion = (tab.{tableCode}_AutoVersion + 1) % 32768
	FROM {schemaName.QuoteName()}.{tableName.QuoteName()} AS tab
	INNER JOIN INSERTED AS ins ON ins.{tableCode}_PK = tab.{tableCode}_PK
	INNER JOIN DELETED AS del ON del.{tableCode}_PK = tab.{tableCode}_PK
	WHERE
		{string.Join(" OR\r\n\t\t", columns.Select(col => $"ins.[{col}] <> del.[{col}]"))}
END";
		}

		[UseSnapshotProtection]
		public void TestAllNoneAutoVersionTableHaveNoTrigger()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				testConnection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTableWithoutAutoVersion] ([KO_PK] [UNIQUEIDENTIFIER] NOT NULL);");
				testConnection.ExecuteNonQuery("CREATE TRIGGER [dbo].[TG_TestTableWithoutAutoVersion_UpdateAutoVersion] ON [dbo].[TestTableWithoutAutoVersion] AFTER UPDATE AS RETURN");

				var logger = new Mock<IUpgradeTaskWorkflowControl>();
				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(testConnection, logger.Object, true);

				Assert(DataUtils.ObjectExists(testConnection, "[dbo].[TG_TestTableWithoutAutoVersion_UpdateAutoVersion]"));
				afterUpgradeTasks.EnsureAutoVersionTriggers_Exposed();
				Assert("None-AutoVersion tables should not have an AutoVersion trigger.", !DataUtils.ObjectExists(testConnection, "[dbo].[TG_TestTableWithoutAutoVersion_UpdateAutoVersion]"));
			}
		}

		[UseSnapshotProtection]
		public void TestAutoVersionTriggerDoesNotUpdatedByExcludedColumns()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var staffPk = Guid.NewGuid();

				var insertSql = $@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
	('{staffPk}', 'GS1', 'Staff1', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbEmploymentHistory
	(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_SystemCreateTimeUtc, GEH_SystemLastEditTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditUser)
VALUES
	(NEWID(), '{staffPk}', '2020-01-01 01:00:00 +01:00', 'JobTitle0', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), '{staffPk}', '2020-02-01 11:00:00 +07:00', 'JobTitle1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), '{staffPk}', '2020-03-01 03:00:00 +11:00', 'JobTitle2', GETUTCDATE(), GETUTCDATE(), 'E', 'E')";

				testConnection.ExecuteNonQuery(insertSql);

				var autoVersionsBeforeEdit = new List<int>();
				testConnection.ExecuteReader(
$@"SELECT
	GEH_AutoVersion
FROM
	dbo.GlbEmploymentHistory
WHERE
	GEH_GS_Staff = '{staffPk}'
ORDER BY GEH_EffectiveDate DESC
",
					(reader) => autoVersionsBeforeEdit.Add((reader.GetInt16(0))));

				testConnection.ExecuteNonQuery("UPDATE dbo.GlbEmploymentHistory SET GEH_EffectiveDate = '2020-03-01 04:00:00 +00:00' WHERE GEH_JobTitle = 'JobTitle2'");

				var autoVersionsAfterEdit = new List<int>();
				testConnection.ExecuteReader(
$@"SELECT
	GEH_AutoVersion
FROM
	dbo.GlbEmploymentHistory
WHERE
	GEH_GS_Staff = '{staffPk}'
ORDER BY GEH_EffectiveDate DESC
",
					(reader) => autoVersionsAfterEdit.Add((reader.GetInt16(0))));

				var autoEffectiveEndDatesAfterEdit = new List<object>();
				testConnection.ExecuteReader(
$@"SELECT
	GEH_AutoEffectiveEndDate
FROM
	dbo.GlbEmploymentHistory
WHERE
	GEH_GS_Staff = '{staffPk}'
ORDER BY GEH_EffectiveDate DESC
",
					(reader) => autoEffectiveEndDatesAfterEdit.Add((reader.GetValue(0))));

				AssertContainsExactElementsInExactOrder(new int[] { 0, 0, 0 }, autoVersionsBeforeEdit);
				AssertContainsExactElementsInExactOrder(new int[] { 1, 0, 0 }, autoVersionsAfterEdit);
				AssertContainsExactElementsInExactOrder(
					new object[] {
						DBNull.Value,
						new DateTimeOffset(2020, 3, 1, 4, 0, 0, new TimeSpan(0, 0, 0)),
						new DateTimeOffset(2020, 2, 1, 11, 0, 0, new TimeSpan(7, 0, 0)),
					}, autoEffectiveEndDatesAfterEdit);
			}
		}

		[ExpectNoExceptions]
		public void TestLastEditAuditInfoMustBeUpdatedTriggerSynchronizerIsCalled()
		{
			// Arrange
			using (var testConnection = Db.NewAdminConnection())
			using (new DisposableAction(
				() => testConnection.BeginTransaction(),
				() => testConnection.RollbackTransaction()))
			{
				var logger = new Mock<IUpgradeTaskWorkflowControl>();
				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(testConnection, logger.Object, false);

				// Act
				afterUpgradeTasks.RunTasks();

				// Assert
				logger.Verify(l => l.StartNonEstimatedTask(It.Is<string>(x => x.StartsWith("Check autoversion trigger for schema: "))), Times.AtLeastOnce);
				logger.Verify(l => l.StartNonEstimatedTask(It.Is<string>(x => x.StartsWith("Check audit trigger for schema: "))), Times.AtLeastOnce);
				logger.Verify(l => l.StartNonEstimatedTask("Synchronising SystemLastEditAuditInfo update triggers"), Times.Once);
				logger.VerifyNoOtherCalls();
			}
		}

		public void TestLastEditAuditInfoMustBeUpdatedTriggerWorkWellWithAutoVersionTrigger_NoExceptionRaised()
		{
			// Arrange
			var pk = Guid.NewGuid();

			using (var testConnection = Db.NewAdminConnection())
			using (new DisposableAction(
				() => testConnection.BeginTransaction(),
				() => testConnection.RollbackTransaction()))
			{
				var tabDef = new TableDefinitionBuilder("dbo", "TestTableName", "TTS")
					.AddPk()
					.AddData()
					.AddSystemCreateTimeUtc()
					.AddSystemCreateUser()
					.AddSystemLastEditTimeUtc()
					.AddSystemLastEditUser()
					.AddAutoVersion()
					.Build();
				testConnection.ExecuteNonQuery(tabDef);

				testConnection.ExecuteNonQuery(@"
INSERT INTO TestTableName (
	TTS_PK
	, TTS_Data
	, TTS_SystemCreateTimeUtc
	, TTS_SystemCreateUser
	, TTS_SystemLastEditTimeUtc
	, TTS_SystemLastEditUser
) VALUES (
	@pk
	, 1
	, GETDATE()
	, 'ABC'
	, GETDATE()
	, 'ABC'
)", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));

				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(
					testConnection,
					Mock.Of<IUpgradeTaskWorkflowControl>(),
					false);
				afterUpgradeTasks.RunTasks();

				var originalAutoVersion = testConnection.ExecuteScalar<short>(@"
SELECT TTS_AutoVersion
FROM TestTableName
WHERE TTS_PK = @pk
", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));

				// Act
				// Assert
				AssertNoExceptionThrown(() =>
				{
					testConnection.ExecuteNonQuery(@"
UPDATE TestTableName
SET
	TTS_SystemLastEditTimeUtc = DATEADD(minute, 1, GETDATE())
	, TTS_SystemLastEditUser = 'ABC'
WHERE TTS_PK = @pk
", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));
				});

				var currentAutoVersion = testConnection.ExecuteScalar<short>(@"
SELECT TTS_AutoVersion
FROM TestTableName
WHERE TTS_PK = @pk
", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));
				AssertEquals("AutoVersion increases after update", 1, currentAutoVersion - originalAutoVersion);
			}
		}

		public void TestLastEditAuditInfoMustBeUpdatedTriggerWorkWellWithAutoVersionTrigger_ExceptionRaised()
		{
			// Arrange
			var pk = Guid.NewGuid();

			using (var testConnection = Db.NewAdminConnection())
			using (new DisposableAction(
				() => testConnection.BeginTransaction(),
				() => testConnection.RollbackTransaction()))
			{
				var tabDef = new TableDefinitionBuilder("dbo", "TestTableName", "TTS")
					.AddPk()
					.AddData()
					.AddSystemCreateTimeUtc()
					.AddSystemCreateUser()
					.AddSystemLastEditTimeUtc()
					.AddSystemLastEditUser()
					.AddAutoVersion()
					.Build();
				testConnection.ExecuteNonQuery(tabDef);

				testConnection.ExecuteNonQuery(@"
INSERT INTO TestTableName (
	TTS_PK
	, TTS_Data
	, TTS_SystemCreateTimeUtc
	, TTS_SystemCreateUser
	, TTS_SystemLastEditTimeUtc
	, TTS_SystemLastEditUser
) VALUES (
	@pk
	, 1
	, GETDATE()
	, 'ABC'
	, GETDATE()
	, 'ABC'
)", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));

				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(
					testConnection,
					Mock.Of<IUpgradeTaskWorkflowControl>(),
					false);
				afterUpgradeTasks.RunTasks();

				// Act
				// Assert
				AssertExceptionThrown<SqlException>(
					"Error is raised in update trigger",
					@"Attempt to update without [TTS_SystemLastEditTimeUtc] for [TestTableName].
The transaction ended in the trigger. The batch has been aborted.",
					() =>
				{
					testConnection.ExecuteNonQuery(@"
UPDATE TestTableName
SET
	TTS_SystemLastEditUser = 'ABC'
WHERE TTS_PK = @pk
", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));
				});
			}
		}

		[UseSnapshotProtection]
		public void TestRemoveRegistryIndex()
		{
			// Arrange
			using (var testConnection = Db.NewAdminConnection())
			{
				const string insertIfNotExists = @"
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue)
SELECT NEWID(), 'RegistryIndex', CONVERT(varbinary(max), '<empty />')
WHERE NOT EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'RegistryIndex')";

				testConnection.ExecuteNonQuery(insertIfNotExists);
				var afterUpgradeTasks = new TransactionalAfterUpgradeTasksForTest(testConnection, null, false);

				AssertEquals("Precondition: RegistryIndex exists", 1, testConnection.ExecuteScalar("SELECT count(*) FROM dbo.StmData WHERE SD_Name = 'RegistryIndex'"));

				// Act
				afterUpgradeTasks.RemoveRegistryIndex_Exposed();

				// Assert
				AssertEquals("RegistryIndex should be deleted", 0, testConnection.ExecuteScalar("SELECT count(*) FROM dbo.StmData WHERE SD_Name = 'RegistryIndex'"));
			}
		}
	}
}
