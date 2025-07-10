using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.Shared;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[TestsSubclassesOf(typeof(ConvertDateTimeToDateTimeOffsetTransform))]
	abstract class ConvertDateTimeToDateTimeOffsetTransformTestCase : DataTransformationTestCase
	{
		public void TestTempOrNewColumnIsCreated()
		{
			PrepareTestData();

			var column = GetDateTimeColumn();
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(GetCountSQL(column));
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var firstDateTimeSQL = $@"SELECT TOP 1 {column.Name} FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			// could be smalldatetime, datetime, datetime2 - simply check if the result is DateTime to know.
			AssertEquals("Precondition: The column is stored as a DateTime before converting to DateTimeOffset.", true, TestConnection.ExecuteScalar(firstDateTimeSQL) is DateTime);

			var transform = GetNewTestTransformationInstance();
			var indexCreationCommand = string.Empty;

			var indexProvider = new TransformationIndexProvider(transform);
			var supportingIndex = GetSupportingIndex(indexProvider);

			using (TestConnection.TrackExecutedCommands())
			{
				transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

				if (supportingIndex != null)
				{
					indexCreationCommand = TestConnection.ExecutedCommands.Single(c => c.Contains($"INDEX [{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}"));
				}
			}

			var transformedColumn = GetDateTimeOffsetColumnToConvert();
			var tempOrNewColumnName = GetTempOrNewColumnName();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempOrNewColumnName} should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempOrNewColumnName));
			AssertEquals($"Temp Column {tempOrNewColumnName} should be created with correct DateTimeOffset and precision.", "datetimeoffset", DbObjectCreator.GetColumnType(TestConnection, transformedColumn.TableName, tempOrNewColumnName));
			AssertEquals($"Temp Column {tempOrNewColumnName} should be created with correct DateTimeOffset and precision.", transformedColumn.Scale, GetScale(column.TableName, tempOrNewColumnName));

			var defaultConstraintName = DbObjectCreator.GenerateDefaultColumnConstraintName(transformedColumn.TableName, tempOrNewColumnName);
			var constraintExistsSQL = $"SELECT COUNT(*) WHERE OBJECT_ID('{tableDescriptor.TableSchema}.{defaultConstraintName}', 'D') IS NOT NULL";
			var constraintExists = (int)TestConnection.ExecuteScalar(constraintExistsSQL);
			AssertEquals("Default Constraint should exist if column is not nullable or not exist if column is nullable.", transformedColumn.IsNullable ? 0 : 1, constraintExists);

			if (!column.IsNullable)
			{
				AssertEquals("Default Constraint Definition should be correct.", "('1994-01-01')",
					TestConnection.ExecuteScalar($"SELECT definition FROM sys.default_constraints WHERE name = '{defaultConstraintName}'"));
			}

			var countOfConvertedSQL = $@"
SELECT COUNT(*)
FROM {transformedColumn.TableName}
WHERE
	{transformedColumn.Name} IS NOT NULL
	AND CAST({tempOrNewColumnName} as DATETIME) = {column.Name}";
			AssertEquals("All rows should have been populated with offset column.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfConvertedSQL));

			string countOfAutoVersion = null;
			if (DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), $"{column.ColumnPrefix}_AutoVersion"))
			{
				countOfAutoVersion = $@"
SELECT COUNT(*)
FROM {transformedColumn.TableName}
WHERE
	{transformedColumn.Name} IS NOT NULL
	AND {transformedColumn.ColumnPrefix}_AutoVersion = 1";
				AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));
			}

			if (supportingIndex != null)
			{
				AssertEquals("Supporting Index should have correct definition.", supportingIndex.SQL_Create, indexCreationCommand);
				AssertEquals("Supporting Index for Transform should have been deleted by the time the online transform finishes.", false, DbObjectCreator.IndexExists(TestConnection, column.TableName, supportingIndex.IndexName));
			}

			var fromTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.FromTime";
			var toTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.ToTime";
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, fromTimeName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, toTimeName));

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals($"Temp Column {tempOrNewColumnName} should still be created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempOrNewColumnName));
			AssertEquals($"Temp Column {tempOrNewColumnName} should still have correct DateTimeOffset and precision.", "datetimeoffset", DbObjectCreator.GetColumnType(TestConnection, column.TableName, tempOrNewColumnName));
			AssertEquals($"Temp Column {tempOrNewColumnName} should still have correct DateTimeOffset and precision.", transformedColumn.Scale, GetScale(column.TableName, tempOrNewColumnName));

			if (countOfAutoVersion != null)
			{
				AssertEquals("Rows should have been updated once.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));
			}

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			if (GetSourceDateTimeColumn() != null)
			{
				AssertEquals($"New Column {tempOrNewColumnName} should exist.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempOrNewColumnName));
			}
			else
			{
				AssertEquals($"Temp Column {tempOrNewColumnName} should no longer exist.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempOrNewColumnName));
			}
			AssertEquals($"Existing Column {transformedColumn.Name} should have correct DateTimeOffset and precision.", "datetimeoffset", DbObjectCreator.GetColumnType(TestConnection, transformedColumn.TableName, transformedColumn.Name));
			AssertEquals($"Existing Column {transformedColumn.Name} should have correct DateTimeOffset and precision.", transformedColumn.Scale, GetScale(transformedColumn.TableName, transformedColumn.Name));
			AssertEquals("All rows that needed conversion have the Offset Column Populated.", countOfRowsToConvert, TestConnection.ExecuteScalar(GetCountSQL(transformedColumn)));
			AssertEquals("Default Constraint should not exist.", 0, (int)TestConnection.ExecuteScalar(constraintExistsSQL));

			string GetCountSQL(SchemaColumn countColumn) => $@"SELECT COUNT(*) FROM {countColumn.TableName} WHERE {countColumn.Name} IS NOT NULL";
		}

		public void TestIndexesWithTempColumnAreNotDropped()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var column = GetDateTimeColumn();
			var tempColumnName = GetTempOrNewColumnName();
			var schemaName = column.TableSchema.SqlSchemaName;
			var tableDescriptor = new DbObjectCreator.TableDescriptor(schemaName, column.TableName);
			AssertEquals($"Temp Column {tempColumnName} should get created.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));

			// pretend we have an index on the field being transformed
			TestConnection.ExecuteNonQuery($"CREATE NONCLUSTERED INDEX Test ON {schemaName}.{column.TableName} ([{column.Name}])");
			// pretend the online upgrade added an index matching the column but with the temp column
			TestConnection.ExecuteNonQuery($"CREATE NONCLUSTERED INDEX {IndexInfo.ONLINE_INDEX_PREFIX}Test ON {schemaName}.{column.TableName} ([{tempColumnName}])");

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			AssertEquals("Original Index on Existing Column should have been dropped.", false, DbObjectCreator.IndexExists(TestConnection, column.TableName, "Test"));
			AssertEquals("Online Index on Temp Column should not have been dropped even after rename.", true, DbObjectCreator.IndexExists(TestConnection, column.TableName, $"{IndexInfo.ONLINE_INDEX_PREFIX}Test"));
		}

		public void TestTransformWithoutCache()
		{
			TestConnection.ExecuteNonQuery("DELETE dbo.RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'SGSIN'");
			InsertDataWithThisTimeZone("SGSIN");

			var column = GetDateTimeColumn();
			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();

			var transformedColumn = GetDateTimeOffsetColumnToConvert();
			var countOfConvertedColumns = $@"
SELECT COUNT(*)
FROM {transformedColumn.TableName}
CROSS APPLY (SELECT CAST({transformedColumn.Name} AS DATETIME) AS UtcDateTime) AS UtcDateTime
WHERE
	{transformedColumn.Name} IS NOT NULL
	AND {transformedColumn.Name} != UtcDateTime
	AND {transformedColumn.Name} = TODATETIMEOFFSET(UtcDateTime, (SELECT Offset FROM dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO('SGSIN', UtcDateTime, 0)))";
			AssertEquals("All Rows should have been updated without using the cache.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfConvertedColumns));
		}

		protected abstract void InsertDataWithThisTimeZone(string timeZoneUnloco);

		public void TestTriggerIsCreatedCorrectly()
		{
			var column = GetDateTimeColumn();
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = $"TG_{column.TableName}_Keep{column.Name}InSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, column.TableName, triggerName));

			var expectedTriggerDefinition = GetExpectedTriggerDefinition(triggerName);
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, triggerName));

			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var firstDateTimeSQL = $@"SELECT TOP 1 {column.Name} FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			// could be smalldatetime, datetime, datetime2 - simply check if the result is DateTime to know.
			AssertEquals("Precondition: The column is stored as a DateTime before converting to DateTimeOffset.", true, TestConnection.ExecuteScalar(firstDateTimeSQL) is DateTime);

			var tempColumnName = GetTempOrNewColumnName();
			var countOfConvertedSQL = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND CAST({tempColumnName} as DATETIME) = {column.Name}";
			AssertEquals("All rows should have been populated with offset column from Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfConvertedSQL));

			if (DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), $"{column.ColumnPrefix}_AutoVersion"))
			{
				var countOfAutoVersion = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {column.ColumnPrefix}_AutoVersion = 1";
				AssertEquals("Rows should have been updated once via Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfAutoVersion));
			}

			var prefix = column.ColumnPrefix;
			var systemLastEditTimeColumnName = $"{prefix}_SystemLastEditTimeUtc";
			var systemLastEditUserColumnName = $"{prefix}_SystemLastEditUser";
			var updateSQL = $@"
UPDATE {column.TableName}
SET {column.Name} = DATEADD(day, 10, {column.Name}),
{systemLastEditTimeColumnName} = GetUTCDate(),
{systemLastEditUserColumnName} = '~BP'
WHERE {column.Name} IS NOT NULL";
			TestConnection.ExecuteNonQuery(updateSQL);
			AssertEquals("All rows should have offset column updated from Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfConvertedSQL));
		}

		public void TestTriggerAtTheEndOfOfflineTransform()
		{
			var column = GetDateTimeColumn();
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var triggerName = $"TG_{column.TableName}_Keep{column.Name}InSync";
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, column.TableName, triggerName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be deleted if adding a new datetimeoffset column.", GetSourceDateTimeColumn() != null, !DbObjectCreator.TriggerExists(TestConnection, column.TableName, triggerName));
		}

		protected string GetExpectedTriggerDefinition(string triggerName)
		{
			var column = GetDateTimeColumn();
			var schemaName = column.TableSchema.SqlSchemaName;
			var tempColumnName = GetTempOrNewColumnName();
			var timeZoneColumnName = $"{column.Name}_WTG_TimeZoneUnloco";
			var pkColumnName = column.TableSchema.PK.Name;

			var prefix = column.ColumnPrefix;
			var systemLastEditTimeColumnName = $"{prefix}_SystemLastEditTimeUtc";
			var systemLastEditUserColumnName = $"{prefix}_SystemLastEditUser";

			return $@"
CREATE TRIGGER {schemaName}.{triggerName}
	ON {schemaName}.{column.TableName}
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE({column.Name})
	BEGIN
		UPDATE tab
		SET
			tab.{tempColumnName} = CASE WHEN tab.{column.Name} IS NULL OR Offset IS NULL THEN tab.{column.Name} ELSE TODATETIMEOFFSET({column.Name}, Offset) END,
			tab.{systemLastEditTimeColumnName} = ISNULL({systemLastEditTimeColumnName}, GetUTCDate()),
			tab.{systemLastEditUserColumnName} = CASE WHEN tab.{systemLastEditUserColumnName} = '' THEN '~BP' ELSE tab.{systemLastEditUserColumnName} END
		FROM
			{schemaName}.{column.TableName} AS tab
			OUTER APPLY
			(
				{GetTimeZoneSubQuery(timeZoneColumnName)}
			) as {timeZoneColumnName}
			OUTER APPLY dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO({timeZoneColumnName}, {column.Name}, 0)
		WHERE
			tab.{pkColumnName} IN (SELECT inserted.{pkColumnName} FROM inserted)
	END
END";
		}

		public void TestTrigger_SettingToNullWhileOnlineShouldClearTempColumn()
		{
			var column = GetDateTimeColumn();
			if (column.IsNullable)
			{
				PrepareTestData();

				var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
				var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
				AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

				var transform = GetNewTestTransformationInstance();
				transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

				var tempColumnName = GetTempOrNewColumnName();
				var countOfConvertedSQL = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND CAST({tempColumnName} as DATETIME) = {column.Name}";
				AssertEquals("All rows should have been populated with offset column from Trigger.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfConvertedSQL));

				var prefix = column.ColumnPrefix;
				var systemLastEditTimeColumnName = $"{prefix}_SystemLastEditTimeUtc";
				var systemLastEditUserColumnName = $"{prefix}_SystemLastEditUser";
				var updateSQL = $@"
UPDATE {column.TableName}
SET
{column.Name} = NULL,
{systemLastEditTimeColumnName} = GetUTCDate(),
{systemLastEditUserColumnName} = '~BP'
WHERE
	{column.Name} IS NOT NULL";
				TestConnection.ExecuteNonQuery(updateSQL);

				var countOfNonNullTempColumn = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{tempColumnName} IS NOT NULL";
				AssertEquals("All temp column values should have been set to null.", 0, TestConnection.ExecuteScalar(countOfNonNullTempColumn));
			}
			else
			{
				Assert(true); // this test is only relevant for nullable columns
			}
		}

		public void TestTransformSetsLastEditTimeAndUser()
		{
			AssertTransformSetsLastEditTimeAndUser(dropAuditColumns: false);
		}

		public void TestTransformSetsLastEditTimeAndUser_CreatesAuditColumnsIfNecessary()
		{
			AssertTransformSetsLastEditTimeAndUser(dropAuditColumns: true);
		}

		void AssertTransformSetsLastEditTimeAndUser(bool dropAuditColumns)
		{
			var column = GetDateTimeColumn();
			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var timeColumName = $"{column.ColumnPrefix}_SystemLastEditTimeUtc";
			var userColumName = $"{column.ColumnPrefix}_SystemLastEditUser";
			if (dropAuditColumns)
			{
				var schema = column.TableSchema.SqlSchemaName;
				new DbColumnDependencyRemover(schema, column.TableName, timeColumName).DropRelateObjects(TestConnection);
				TestConnection.ExecuteNonQuery($"ALTER TABLE {schema}.{column.TableName} DROP COLUMN {timeColumName}");

				new DbColumnDependencyRemover(schema, column.TableName, userColumName).DropRelateObjects(TestConnection);
				TestConnection.ExecuteNonQuery($"ALTER TABLE {schema}.{column.TableName} DROP COLUMN {userColumName}");
			}
			else
			{
				var updateSQL = $@"
UPDATE {column.TableName}
SET
	{timeColumName} = '2024/01/01',
	{userColumName} = 'ZZZ'
WHERE
	{column.Name} IS NOT NULL";

				TestConnection.ExecuteNonQuery(updateSQL);
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tempColumnName = GetTempOrNewColumnName();
			var countOfRowsWithOffsetsSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND CAST({tempColumnName} as DATETIME) > {tempColumnName}";
			AssertEquals("Make sure all rows in 'PrepareTestData()' will be converted to a non zero offset.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithOffsetsSql));

			var countOfRowsWithUpdatedTimeAndUserSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {timeColumName} > DATEADD(day, -1, GetUtcDate())
	AND {userColumName} = '~BP'";
			AssertEquals("Last Edit Time & User should have been updated.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithUpdatedTimeAndUserSql));
		}

		public void TestTransformResumesWithFromAndToTimeSet()
		{
			var column = GetDateTimeColumn();
			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var maxValueSql = $@"
SELECT MAX({column.Name})
FROM {column.TableName}
WHERE {column.Name} IS NOT NULL";

			var maxValue = (DateTime)TestConnection.ExecuteScalar(maxValueSql);

			var fromTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.FromTime";
			var toTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.ToTime";
			ExtProperty.Database.Update(TestConnection, fromTimeName, SqlFormatInfo.ToSqlDateTimeString(maxValue));
			ExtProperty.Database.Update(TestConnection, toTimeName, SqlFormatInfo.ToSqlDateTimeString(maxValue));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var whereClause = column.IsNullable ? "IS NULL" : "= '1994-01-01'";
			var tempOrNewColumnName = GetTempOrNewColumnName();
			var countOfRowsWithNoTempColumnSetSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {tempOrNewColumnName} {whereClause}";
			AssertEquals("Since all rows have a time less than or equal to the current from time stored, nothing should get processed.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithNoTempColumnSetSql));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, fromTimeName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, toTimeName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			if (GetSourceDateTimeColumn() != null)
			{
				AssertEquals("New Column should be not Dropped.", true, DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), tempOrNewColumnName));
			}
			else
			{
				AssertEquals("Temp Column should be Dropped.", false, DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), tempOrNewColumnName));
			}
		}

		public void TestTransformResumesWithOnlyToTimeSet()
		{
			var column = GetDateTimeColumn();
			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var maxValueSql = $@"
SELECT MAX({column.Name})
FROM {column.TableName}
WHERE {column.Name} IS NOT NULL";

			var maxValue = (DateTime)TestConnection.ExecuteScalar(maxValueSql);

			var fromTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.FromTime";
			var toTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{column.Name}.ToTime";
			ExtProperty.Database.Update(TestConnection, toTimeName, SqlFormatInfo.ToSqlDateTimeString(maxValue));
			AssertNull("Precondition: Only To Time should be set.", ExtProperty.Database.Select(TestConnection, fromTimeName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var whereClause = column.IsNullable ? "IS NOT NULL" : "<> '1994-01-01'";
			var tempOrNewColumnName = GetTempOrNewColumnName();
			var countOfRowsWithNoTempColumnSetSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {tempOrNewColumnName} {whereClause}";
			AssertEquals("Since only ToTime was set, all rows should get processed.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithNoTempColumnSetSql));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, fromTimeName));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Database.Select(TestConnection, toTimeName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			if (GetSourceDateTimeColumn() != null)
			{
				AssertEquals("New Column should be not Dropped.", true, DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), tempOrNewColumnName));
			}
			else
			{
				AssertEquals("Temp Column should be Dropped.", false, DbObjectCreator.ColumnExists(TestConnection, new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName), tempOrNewColumnName));
			}
		}

		public void TestTransform_WhenDefaultDateTimeHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.Zero);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTransform_WhenDefaultDateTimeHasBeenSet_WithoutTransaction()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.Zero);
		}

		public void TestTransform_WhenDefaultDateTimePlusLessThanAMinuteHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.FromSeconds(59));
		}

		public void TestTransform_WhenDefaultDateTimePlusAMinuteHasBeenSet()
		{
			AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan.FromMinutes(1));
		}

		void AssertTestTransform_WhenDefaultDateTimeHasBeenSet(TimeSpan timeAdded)
		{
			var column = GetDateTimeColumn();

			using (var manager = TestConnection.IsInTransaction ? null : TestConnection.BeginTransactionWithManager())
			{
				PrepareTestData();
				manager?.CommitTransaction();
			}

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var prefix = column.ColumnPrefix;
			var systemLastEditTimeColumnName = $"{prefix}_SystemLastEditTimeUtc";
			var systemLastEditUserColumnName = $"{prefix}_SystemLastEditUser";

			using (var manager = TestConnection.IsInTransaction ? null : TestConnection.BeginTransactionWithManager())
			{
				TestConnection.ExecuteNonQuery($@"
UPDATE {column.TableName}
SET {column.Name} = DATEADD(second, {timeAdded.TotalSeconds}, CAST('' as {DateTimeTypeBeforeConversion})),
{systemLastEditTimeColumnName} = GetUTCDate(),
{systemLastEditUserColumnName} = '~BP'
WHERE {column.Name} IS NOT NULL");
				manager?.CommitTransaction();
			}

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tempColumnName = GetTempOrNewColumnName();

			var countOfRowsWithNoTempColumnSetSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {tempColumnName} < DATEADD(second, {timeAdded.TotalSeconds}, CAST('' as {DateTimeTypeBeforeConversion}))";
			AssertEquals("Since all rows have a time less than or equal to the current from time stored, nothing should get processed.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithNoTempColumnSetSql));
		}

		public void TestStatusExtendedProperty()
		{
			var statusName = GetStatusName();
			AssertNull("No Status stored yet.", ExtProperty.Database.Select(TestConnection, statusName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Status should be updated to Finished once Transform is complete.", "Finished", ExtProperty.Database.Select(TestConnection, statusName));

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Status be unchanged if it is Complete.", "Finished", ExtProperty.Database.Select(TestConnection, statusName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertNull("Status is removed after Offline Upgrade.", ExtProperty.Database.Select(TestConnection, statusName));
		}

		public void TestStatusExtendedProperty_WhenAlreadySetToPopulatedData()
		{
			var column = GetDateTimeColumn();
			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var statusName = GetStatusName();
			ExtProperty.Database.Update(TestConnection, statusName, "PopulatedData");

			var transform = GetNewTestTransformationInstance();
			var indexProvider = new TransformationIndexProvider(transform);
			var supportingIndex = GetSupportingIndex(indexProvider);
			if (supportingIndex != null)
			{
				TestConnection.ExecuteNonQuery(supportingIndex.SQL_Create);
			}

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tempColumnName = GetTempOrNewColumnName();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempColumnName} should be added even if nothing was transformed.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));
			AssertEquals("Status should be updated to Finished once Transform is complete.", "Finished", ExtProperty.Database.Select(TestConnection, statusName));

			var whereClause = column.IsNullable ? "IS NULL" : "= '1994-01-01'";
			var countOfRowsWithNoTempColumnSetSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {tempColumnName} {whereClause}";
			AssertEquals("Since status shows all data transformed, nothing should get processed.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithNoTempColumnSetSql));

			if (supportingIndex != null)
			{
				AssertEquals("Supporting Index for Transform should still be deleted when the online transform finishes.", false, DbObjectCreator.IndexExists(TestConnection, column.TableName, supportingIndex.IndexName));
			}
		}

		public void TestStatusExtendedProperty_WhenAlreadySetToFinished()
		{
			var statusName = GetStatusName();
			ExtProperty.Database.Update(TestConnection, statusName, "Finished");

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tempOrNewColumnName = GetTempOrNewColumnName();
			var column = GetDateTimeOffsetColumnToConvert();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempOrNewColumnName} will not be be added as transform has finished.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempOrNewColumnName));
			AssertEquals("Status should be updated to Finished once Transform is complete.", "Finished", ExtProperty.Database.Select(TestConnection, statusName));
		}

		public void TestTransformWithNothingToTransformWorks()
		{
			var column = GetDateTimeColumn();
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var tempColumnName = GetTempOrNewColumnName();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempColumnName} should be added even if nothing was transformed.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var templateDBName = SetupAndReturnTemplateDb(column);
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			if (GetSourceDateTimeColumn() != null)
			{
				AssertEquals($"New Column {tempColumnName} should exist.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));
			}
			else
			{
				AssertEquals($"Temp Column {tempColumnName} should get renamed even if nothing was transformed.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));
				AssertEquals($"Column {column.Name} should have its type updated.", "datetimeoffset", DbObjectCreator.GetColumnType(TestConnection, column.TableName, column.Name));
			}
		}

		public void TestTransformWithNoDateColumnWorks()
		{
			var column = GetDateTimeColumn();
			TestConnection.ExecuteNonQuery($"ALTER TABLE {column.TableName} DROP COLUMN {column.Name}");

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));

			var tempColumnName = GetTempOrNewColumnName();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempColumnName} should not get added.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));

			var statusName = GetStatusName();
			AssertNull("No Status is created if No Column Exists.", ExtProperty.Database.Select(TestConnection, statusName));
			AssertNoExceptionThrown(() =>
			{
				transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
				var templateDBName = SetupAndReturnTemplateDb(column);
				var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
				new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			});
		}

		public void TestTransformWorksWithoutFunction()
		{
			var column = GetDateTimeColumn();
			var dropSQL = UpgraderUtils.GetSchemaBoundObjectsToDropSql("dbo", "CalculateTimeZoneOffsetInMinutesFromRefUNLOCO");
			TestConnection.ExecuteNonQuery(dropSQL);
			TestConnection.ExecuteNonQuery("DROP FUNCTION dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO");
			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));

			var tempColumnName = GetTempOrNewColumnName();
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals($"Temp Column {tempColumnName} should not get added.", false, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, tempColumnName));
			AssertNoExceptionThrown(() =>
			{
				transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
				var templateDBName = SetupAndReturnTemplateDb(column);
				var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
				new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			});
		}

		public void TestTransformWithoutSynonym()
		{
			var column = GetDateTimeColumn();
			Db.Connection.ExecuteNonQuery($"DROP SYNONYM IF EXISTS dbo.RefDatabase_RefUNLOCOUtcOffset");
			CreateCalculateTimeZoneOffsetInMinutesFromRefUNLOCO_WithoutSynonym();

			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None));

			var synonymCheckSQL = $"SELECT COUNT(*) FROM sys.synonyms WHERE name = 'RefDatabase_RefUNLOCOUtcOffset'";
			AssertEquals("RefUNLOCO should exist after running the Transform.", 1, Db.Connection.ExecuteScalar(synonymCheckSQL));
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestOnlineTransformWorksWithoutTransaction()
		{
			using (var manager = TestConnection.BeginTransactionWithManager())
			{
				PrepareTestData();
				manager.CommitTransaction();
			}

			var column = GetDateTimeColumn();
			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		// All changes in below region are a temporary fix. The underlying error is being worked on in WI00723403 
		// Once the root issue with RefDatabase_RefUNLOCOUtcOffset is resolved, this will be removed.
		#region TestTransformWorksWithoutProperlyBoundFunction

		public void TestTransformWorksWithoutProperlyBoundFunction_Trigger()
		{
			var column = GetDateTimeColumn();
			PrepareTestData();

			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			TestConnection.ExecuteNonQuery("DROP SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset");
			TestConnection.ExecuteNonQuery("CREATE SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset FOR dbo.Missing");

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None));
			SetupTemplateDBAndRunColumnSync();

			var dateTimeOffsetColumn = GetDateTimeOffsetColumnToConvert();
			var countOfRowsWithOffsetsSql = $@"
SELECT COUNT(*)
FROM {dateTimeOffsetColumn.TableName}
WHERE
	{dateTimeOffsetColumn.Name} IS NOT NULL
	AND CAST({dateTimeOffsetColumn.Name} as DATETIME) > {dateTimeOffsetColumn.Name}";
			AssertEquals("Make sure all rows in 'PrepareTestData()' will be converted to a non zero offset.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithOffsetsSql));
		}

		public void TestTriggerWorksWithoutProperlyBoundFunction()
		{
			// We suspect the audit triggers here to allow this test to pass. This test began to break as of WI00539677.
			// The newly created trigger is only modifying values in a temporary column of the table, and hence should not modify the audit columns
			// When the temporary column replaces the existing column, the audit columns will be updated accordingly.
			SuspendAuditTriggers();

			var column = GetDateTimeColumn();
			TestConnection.ExecuteNonQuery("DROP SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset");
			TestConnection.ExecuteNonQuery("CREATE SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset FOR dbo.Missing");

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));

			PrepareTestData();
			var countSQL = $@"SELECT COUNT(*) FROM {column.TableName} WHERE {column.Name} IS NOT NULL";
			var countOfRowsToConvert = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("Precondition: We have some DateTime Column's to convert.", true, countOfRowsToConvert > 0);

			var whereClause = column.IsNullable ? "IS NOT NULL" : "<> '1994-01-01'";
			var tempColumnName = GetTempOrNewColumnName();
			var countOfRowsWithOffsetsSql = $@"
SELECT COUNT(*)
FROM {column.TableName}
WHERE
	{column.Name} IS NOT NULL
	AND {column.Name} <> {tempColumnName}
	AND {tempColumnName} {whereClause}";
			AssertEquals("Make sure Trigger converted value to a non zero offset.", countOfRowsToConvert, TestConnection.ExecuteScalar(countOfRowsWithOffsetsSql));
		}

		public void TestTransformWorksWithoutProperlyBoundFunction_ChecksIfTempFunctionExists()
		{
			TestConnection.ExecuteNonQuery("DROP SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset");
			TestConnection.ExecuteNonQuery("CREATE SYNONYM dbo.RefDatabase_RefUNLOCOUtcOffset FOR dbo.Missing");
			TestConnection.ExecuteNonQuery(@"
CREATE FUNCTION CalculateTimeZoneOffsetInMinutesFromRefUNLOCO_Temp
(
	@UNLOCO CHAR(5),
	@utctime DATETIME,
	@OnlyUseCache bit
)
RETURNS TABLE AS RETURN
SELECT NULL AS Offset");

			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None));
		}

		void SuspendAuditTriggers()
		{
			// This code is based on DBConnection.AuditTriggerSuspension.ExecuteSetSessionContext
			var connection = TestConnection as IDbConnectionInternals;
			using var command = connection.InternalDbConnection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "sys.sp_set_session_context";
			command.Transaction = connection.InternalDbTransaction;

			var auditGuardTriggerName = "Suspend_System_Audit_Columns_Guard";
			var sessionState = "ALL";

			AddParameter("@Key", DbType.String, 128, auditGuardTriggerName);
			AddParameter("@Value", DbType.Object, 8016, sessionState);

			_ = command.ExecuteNonQuery();

			void AddParameter(string name, DbType type, int size, object value)
			{
				var param = command.CreateParameter();
				param.ParameterName = name;
				param.DbType = type;
				param.Size = size;
				param.Value = value;

				_ = command.Parameters.Add(param);
			}
		}

		#endregion

		string SetupAndReturnTemplateDb(SchemaColumn column)
		{
			if (templateDbCreator_DoNotSet != null)
			{
				throw new InvalidOperationException("Cannot create Template DB Twice!");
			}

			var templateDBName = UpgUtils.GetTemplateDbName();
			templateDbCreator_DoNotSet = new TablePreSynchroniserTestTemplateDbCreator(templateDBName, column.TableName,
				column.TableSchema.All
				.Select(c => $"{c.Name} {c.SqlDbTypeDeclaration} {(c.IsNullable ? "NULL" : "NOT NULL")}")
				.Concat(TableHasAutoVersion ? new[] { $"{column.ColumnPrefix}_AutoVersion smallint NOT NULL DEFAULT 0" } : Enumerable.Empty<string>()));
			templateDbCreator_DoNotSet.CreateDropExisting();
			TablePreSynchroniser.CreatePreAddDb_ForTest();

			return templateDBName;
		}

		protected abstract bool TableHasAutoVersion { get; }

		protected void SetupTemplateDBAndRunColumnSync()
		{
			if (templateDbCreator_DoNotSet == null)
			{
				var templateDBName = SetupAndReturnTemplateDb(GetDateTimeOffsetColumnToConvert());
				var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
				new ColumnSynchroniser(TestConnection, loggerMock.Object, TestConnection.CurrentDatabase, templateDBName).DropAlterAndAddColumns();
			}
		}

		byte GetScale(string tableName, string columnName)
		{
			var sql = @"
SELECT TOP 1 col.scale FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
WHERE tab.name = @tableName AND col.name = @columnName";

			using (var dbCommand = TestConnection.Command(sql))
			{
				dbCommand.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				dbCommand.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				return (byte)dbCommand.ExecuteScalar();
			}
		}

		string GetTempOrNewColumnName() => GetSourceDateTimeColumn() != null
			? GetDateTimeOffsetColumnToConvert().Name
			: $"_DTO_{GetDateTimeOffsetColumnToConvert().Name}";

		string GetStatusName() => $"ConvertDateTimeToDateTimeOffsetTransform.{GetDateTimeColumn().Name}.Status";

		SchemaColumn GetDateTimeColumn() => GetSourceDateTimeColumn() ?? (SchemaColumn)GetDateTimeOffsetColumnToConvert();
		protected abstract SchemaDateTimeOffsetColumn GetDateTimeOffsetColumnToConvert();
		protected virtual SchemaDateTimeColumn GetSourceDateTimeColumn() => null;
		protected abstract string GetTimeZoneSubQuery(string timeZoneColumnName);
		protected abstract IndexInfo GetSupportingIndex(TransformationIndexProvider provider);
		protected abstract SqlDbType DateTimeTypeBeforeConversion { get; }

		protected override void SetUp()
		{
			base.SetUp();
			RevertSchemaToOriginalStatePriorToUpgrade();
		}

		protected virtual void RevertSchemaToOriginalStatePriorToUpgrade()
		{
			var dateTimeColumn = GetDateTimeOffsetColumnToConvert();
			var nullClause = dateTimeColumn.IsNullable ? "NULL" : "NOT NULL";
			new DbColumnDependencyRemover(dateTimeColumn.TableName, dateTimeColumn.Name).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery($"ALTER TABLE {dateTimeColumn.TableName} ALTER COLUMN {dateTimeColumn.Name} {DateTimeTypeBeforeConversion} {nullClause}");
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			// Drop can only happen once transaction rolled back.
			if (templateDbCreator_DoNotSet != null)
			{
				TablePreSynchroniser.DropPreAddDb_ForTest();
				templateDbCreator_DoNotSet.Drop();
			}
			base.OnAfterBaseTestCaseRunBare();
		}

		TablePreSynchroniserTestTemplateDbCreator templateDbCreator_DoNotSet;

		void CreateCalculateTimeZoneOffsetInMinutesFromRefUNLOCO_WithoutSynonym()
		{
			Db.Connection.ExecuteNonQuery("DROP FUNCTION CalculateTimeZoneOffsetInMinutesFromRefUNLOCO");
			Db.Connection.ExecuteNonQuery(@"CREATE FUNCTION CalculateTimeZoneOffsetInMinutesFromRefUNLOCO
(
	@UNLOCO CHAR(5),
	@utctime DATETIME,
	@OnlyUseCache bit
)
RETURNS TABLE WITH SCHEMABINDING AS RETURN
WITH
	Step_1 AS (
			SELECT
				_year = YEAR(@utctime)
			Where @OnlyUseCache = 0
		)
	, Step_2 AS (
			SELECT _year
				, dstPK     = dstZone.R2_PK
				, stdOffset = stdZone.R2_OffsetMinutesFromUTC
				, dstOffset = dstZone.R2_OffsetMinutesFromUTC
			FROM
				Step_1
				CROSS JOIN dbo.RefUNLOCO
				JOIN dbo.RefTimeZoneSet              ON R3_PK = RL_R3
				LEFT JOIN dbo.RefTimeZone AS stdZone ON stdZone.R2_PK = R3_R2_StandardZone
				LEFT JOIN dbo.RefTimeZone AS dstZone ON dstZone.R2_PK = R3_R2_DaylightSavingZone
			WHERE
				RL_Code = @UNLOCO
				and @OnlyUseCache = 0
		)
	, Step_3 AS (
		SELECT stdOffset, dstOffset, 
			DaylightSavingTimeStart1 = CASE WHEN DaylightSavingTimeStart1 <= @utctime THEN DaylightSavingTimeStart1 ELSE '1900-01-01' END, 
			DaylightSavingTimeStart2 = CASE WHEN DaylightSavingTimeStart2 <= @utctime THEN DaylightSavingTimeStart2 ELSE '1900-01-01' END, 
			DaylightSavingTimeEnd1 = CASE WHEN DaylightSavingTimeEnd1 > @utctime THEN DaylightSavingTimeEnd1 ELSE '2079-06-06' END,
			DaylightSavingTimeEnd2 = CASE WHEN DaylightSavingTimeEnd2 > @utctime THEN DaylightSavingTimeEnd2 ELSE '2079-06-06' END,
			StartOrEndRule
		FROM (
			SELECT stdOffset, dstOffset,
				DaylightSavingTimeStart1 = v1.Value,
				DaylightSavingTimeStart2 = v2.Value,
				DaylightSavingTimeEnd1 = v3.Value,
				DaylightSavingTimeEnd2 = v4.Value,
				StartOrEndRule = rtzr.R4_StartOrEndRule
			FROM
				Step_2
				LEFT JOIN dbo.RefTimeZoneRule rtzr ON
					dstPK is NOT NULL
					AND R4_R2 = dstPK
					AND R4_FromYear <= _year
				LEFT JOIN dbo.RefTimeZoneRule rtzr1 ON
					dstPK is NOT NULL
					AND rtzr1.R4_R2 = dstPK
					AND (rtzr1.R4_ToYear = 0 OR rtzr1.R4_ToYear >= _year)
				CROSS APPLY dbo.CalculateDaylightSavingTime(_year, rtzr.R4_ToYear, rtzr.R4_FromYear, rtzr.R4_DaylightSavingDayWeekDate, rtzr.R4_DaylightSavingDate, rtzr.R4_DaylightSavingDayName, rtzr.R4_DaylightSavingMonth, rtzr.R4_DaylightSavingDayCount, rtzr.R4_TypeOfTime, rtzr.R4_StartOrEndRule, stdOffset*-1, dstOffset*-1) v1
				CROSS APPLY dbo.CalculateDaylightSavingTime(_year - 1, rtzr.R4_ToYear, rtzr.R4_FromYear, rtzr.R4_DaylightSavingDayWeekDate, rtzr.R4_DaylightSavingDate, rtzr.R4_DaylightSavingDayName, rtzr.R4_DaylightSavingMonth, rtzr.R4_DaylightSavingDayCount, rtzr.R4_TypeOfTime, rtzr.R4_StartOrEndRule, stdOffset*-1, dstOffset*-1) v2
				CROSS APPLY dbo.CalculateDaylightSavingTime(_year, rtzr1.R4_ToYear, rtzr1.R4_FromYear, rtzr1.R4_DaylightSavingDayWeekDate, rtzr1.R4_DaylightSavingDate, rtzr1.R4_DaylightSavingDayName, rtzr1.R4_DaylightSavingMonth, rtzr1.R4_DaylightSavingDayCount, rtzr1.R4_TypeOfTime, rtzr1.R4_StartOrEndRule, stdOffset*-1, dstOffset*-1) v3
				CROSS APPLY dbo.CalculateDaylightSavingTime(_year+1, rtzr1.R4_ToYear, rtzr1.R4_FromYear, rtzr1.R4_DaylightSavingDayWeekDate, rtzr1.R4_DaylightSavingDate, rtzr1.R4_DaylightSavingDayName, rtzr1.R4_DaylightSavingMonth, rtzr1.R4_DaylightSavingDayCount, rtzr1.R4_TypeOfTime, rtzr1.R4_StartOrEndRule, stdOffset*-1, dstOffset*-1) v4
		) DaylightSavingTime
	)
	,Step_4 AS (
			SELECT stdOffset, dstOffset,
				DaylightSavingTimeStart =   CASE WHEN DaylightSavingTimeStart1 > DaylightSavingTimeStart2 THEN DaylightSavingTimeStart1  ELSE DaylightSavingTimeStart2  END
				, DaylightSavingTimeEnd = CASE WHEN DaylightSavingTimeEnd1 > DaylightSavingTimeEnd2 THEN DaylightSavingTimeEnd2 ELSE DaylightSavingTimeEnd1 END
				, StartOrEndRule
			FROM
				Step_3
	)
	, Step_5 AS (
		SELECT stdOffset, dstOffset, DaylightSavingTimeStart, StartOrEndRule,
			DaylightSavingTimeEnd = (SELECT MIN(DaylightSavingTimeEnd) FROM Step_4)
		FROM
			(SELECT stdOffset, dstOffset, ROW_NUMBER () OVER (ORDER BY DaylightSavingTimeStart desc) AS StartOrd,
				DaylightSavingTimeStart,
				DaylightSavingTimeEnd,
				StartOrEndRule
			FROM
				Step_4
			) DayLightPeriod
			WHERE StartOrd = 1
		)
Select TOP 1
		Offset,
		DstStart,
		DstEnd
FROM (
		SELECT Top 1
			Offset = RLO_OffsetMinutesFromUtc,
			DstStart = RLO_StartTimeUtc,
			DstEnd = RLO_EndTimeUtc,
			SortOrder = 0
		FROM dbo.RefUNLOCOUtcOffset
		WHERE RLO_RL_NKCode = @UNLOCO
		AND RLO_StartTimeUtc <= @utctime
		AND RLO_EndTimeUtc > @utctime
		UNION ALL
		SELECT Offset = CASE
			WHEN DaylightSavingTimeStart = '1900-01-01' THEN stdOffset
			WHEN DaylightSavingTimeEnd = '2079-06-06' THEN stdOffset
			WHEN StartOrEndRule = 'STA' THEN dstOffSet ELSE stdOffset END,
			DstStart = DaylightSavingTimeStart,
			DstEnd = DaylightSavingTimeEnd,
			SortOrder = 1
		FROM Step_5
		Where @OnlyUseCache = 0
) combined
Order by SortOrder
");
		}
	}

	class ConvertDateTimeToDateTimeOffsetTransformTest : TransactionedTestCase
	{
		public void TestTriggerWorksWithoutTimeZone()
		{
			new DbColumnDependencyRemover(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.Z0_DateTimeOffset).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE DummyBizo ALTER COLUMN Z0_DateTimeOffset datetime NULL");

			var transformation = new DummyTransformationClass();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			new DummyBizo
			{
				Z0_DateTimeOffset = new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero)
			}.InsertAsDateTimeForColumn(d => d.Z0_DateTimeOffset).Post(TestConnection);

			AssertEquals(new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero), TestConnection.ExecuteScalar("SELECT _DTO_Z0_DateTimeOffset FROM DummyBizo"));
		}

		public void TestTransformWorksWithoutTimeZoneAndWithoutAuditColumns()
		{
			new DbColumnDependencyRemover(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.Z0_DateTimeOffset).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE DummyBizo ALTER COLUMN Z0_DateTimeOffset datetime NULL");

			new DummyBizo
			{
				Z0_DateTimeOffset = new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero)
			}.InsertAsDateTimeForColumn(d => d.Z0_DateTimeOffset).Post(TestConnection);

			var transformation = new DummyTransformationClass();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals(new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero), TestConnection.ExecuteScalar("SELECT _DTO_Z0_DateTimeOffset FROM DummyBizo"));
		}

		public void TestTransformWorksWithNewColumns()
		{
			new DbColumnDependencyRemover(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.Z0_DateTimeOffset).DropRelateObjects(TestConnection);
			TestConnection.ExecuteNonQuery("ALTER TABLE DummyBizo DROP COLUMN Z0_DateTimeOffset");

			new DummyBizo
			{
				Z0_Date = new DateTime(2024, 1, 2, 3, 4, 6)
			}.Insert(TestConnection);

			var transformation = new DummyWithNewColumnTransformationClass();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals(new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero), TestConnection.ExecuteScalar("SELECT Z0_DateTimeOffset FROM DummyBizo"));

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			var column = DummyBizoSchema.Z0_DateTimeOffset;
			var tableDescriptor = new DbObjectCreator.TableDescriptor(column.TableSchema.SqlSchemaName, column.TableName);
			AssertEquals("New Column still exists.", true, DbObjectCreator.ColumnExists(TestConnection, tableDescriptor, "Z0_DateTimeOffset"));
		}

		class DummyBizo : SQLDataObject<DummyBizo>
		{
			public string Z0_Code { get; set; }
			public DateTime? Z0_Date { get; set; }
			public DateTimeOffset? Z0_DateTimeOffset { get; set; }
		}

		class DummyTransformationClass : ConvertDateTimeToDateTimeOffsetTransform
		{
			public override string UserDescription => "Test";

			protected override SchemaDateTimeOffsetColumn ColumnToConvert => DummyBizoSchema.Z0_DateTimeOffset;

			protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

			protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
			{
				return $"SELECT 'AUSYD' as {timeZoneColumnName} WHERE 1 = 0";
			}

			protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider) => null;
		}

		class DummyWithNewColumnTransformationClass : ConvertDateTimeToDateTimeOffsetTransform
		{
			public override string UserDescription => "Test";

			protected override SchemaDateTimeOffsetColumn ColumnToConvert => DummyBizoSchema.Z0_DateTimeOffset;

			protected override SchemaDateTimeColumn SourceColumn => DummyBizoSchema.Z0_Date;

			protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.DateTime;

			protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
			{
				return $"SELECT 'AUSYD' as {timeZoneColumnName} WHERE 1 = 0";
			}

			protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider) => null;
		}
	}
}
