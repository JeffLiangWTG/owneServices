using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms
{
	public abstract class ConvertDateTimeToDateTimeOffsetTransform : DataTransformation
	{
		const int BatchSize = 1000;

		protected ConvertDateTimeToDateTimeOffsetTransform()
		{
		}

		enum TransformationStatus
		{
			AddedColumn,
			PopulatingData,
			PopulatedData,
			Finished,
		}

		protected override void OnlinePreUpgradeTransform()
		{
			// All changes in below region are a temporary fix. The underlying error is being worked on in WI00723403 
			// Once the root issue with RefDatabase_RefUNLOCOUtcOffset is resolved, this will be removed.
#if DEBUG
			OnlinePreUpgradeTransform_TempFix();
#endif

			base.OnlinePreUpgradeTransform();

			var dateColumn = SourceColumn ?? (SchemaColumn)ColumnToConvert;
			var tableDescriptor = new DbObjectCreator.TableDescriptor(dateColumn.TableSchema.SqlSchemaName, dateColumn.TableName);

			if (
				// If the original column does not exist we cannot transform anything. If the offset function does not
				// exist, we will simply let the Schema Sync alter the type and make all datetimes offset of 0 (Utc).
				DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, dateColumn.Name)
				// if the column to convert is a DateTimeOffset or is now a computed column then transform ran already 
				&& !HasTransformationRanBasedOnSchema(dateColumn)
				&& OffsetConversionFunctionExists())
			{
				var statusName = GetStatusName(dateColumn.Name);
				var statusValue = ExtProperty.Database.Select(Db.Connection, statusName);
				var status = !string.IsNullOrEmpty(statusValue) && Enum.TryParse<TransformationStatus>(statusValue, out var result)
					? result
					: (TransformationStatus?)null;

				if (status != TransformationStatus.Finished)
				{
					CreateRefDatabaseSynonymIfDoesNotExist();

					AddTempOrNewColumnAndTransformDateTimesToOffset(dateColumn, tableDescriptor, statusName, status);
				}
			}

			bool OffsetConversionFunctionExists() => (bool)Db.Connection.ExecuteScalar("SELECT CAST(CASE WHEN OBJECT_ID('CalculateTimeZoneOffsetInMinutesFromRefUNLOCO', 'IF') IS NOT NULL THEN 1 ELSE 0 END as bit)");
		}

		protected virtual bool HasTransformationRanBasedOnSchema(SchemaColumn dateColumn)
			=> DbObjectCreator.GetColumnType(Db.Connection, dateColumn.TableName, dateColumn.Name).Equals("datetimeoffset", StringComparison.InvariantCultureIgnoreCase);

		static string GetStatusName(string columnName) => Invariant($"ConvertDateTimeToDateTimeOffsetTransform.{columnName}.Status");

		void CreateRefDatabaseSynonymIfDoesNotExist()
		{
			// Based on OdysseyGenerator.cs
			var sql = FormattableString.Invariant($@"
IF NOT EXISTS (SELECT name FROM sys.synonyms WHERE name = 'RefDatabase_RefUNLOCOUtcOffset')
BEGIN
	CREATE SYNONYM [dbo].[RefDatabase_RefUNLOCOUtcOffset] FOR [CW-RefDatabase].[dbo].[RefUNLOCOUtcOffsetTableView_V1]
END");
			Db.Connection.ExecuteNonQuery(sql);
		}

		void AddTempOrNewColumnAndTransformDateTimesToOffset(SchemaColumn dateColumn, DbObjectCreator.TableDescriptor tableDescriptor, string statusName, TransformationStatus? status)
		{
			var columnName = dateColumn.Name;
			var tableName = tableDescriptor.TableName;
			var schemaName = tableDescriptor.TableSchema;
			var columnNameToAdd = GetColumnNameToAdd();

			// set dummy default value if not nullable so that we don't need to change column nullability in Schema Sync (dashes are robust, 1994-01-01 works for any datetime type)
			var defaultValue = ColumnToConvert.IsNullable ? null : "'1994-01-01'";
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, tableDescriptor, columnNameToAdd, ColumnToConvert.SqlDbTypeDeclaration, defaultValue);

			if (status == null)
			{
				status = TransformationStatus.AddedColumn;
				ExtProperty.Database.Update(Db.Connection, statusName, status.ToString());
			}

			var timeZoneColumnName = Invariant($"{columnName}_WTG_TimeZoneUnloco");
			var timeZoneSubQuery = GetSubqueryForTimeZoneColumn(timeZoneColumnName);

			// need to add audit columns before supporting index as the supporting index will likely contain them.
			var auditColumnsExist = CreateAuditColumnsIfNecessary();

			CreateSyncTrigger(auditColumnsExist);

			var indexProvider = new TransformationIndexProvider(this);
			var index = GetSupportingIndex(indexProvider);
			if (index != null)
			{
				DbObjectCreator.CreateIndexIfNotExists(Db.Connection, tableName, index.IndexName, index.SQL_Create);
			}

			var fromTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{columnName}.FromTime";
			var toTimeName = $"ConvertDateTimeToDateTimeOffsetTransform.{columnName}.ToTime";
			if (status != TransformationStatus.PopulatedData)
			{
				var (fromTime, lowerBoundTime, upperBoundTime) = GetBoundaryTimes();
				if (upperBoundTime.HasValue)
				{
					if (status == TransformationStatus.AddedColumn)
					{
						status = TransformationStatus.PopulatingData;
						ExtProperty.Database.Update(Db.Connection, statusName, status.ToString());
					}

					DoTransform(fromTime, lowerBoundTime.Value, upperBoundTime.Value, auditColumnsExist);

					status = TransformationStatus.PopulatedData;
					ExtProperty.Database.Update(Db.Connection, statusName, status.ToString());

					ClearExtPropertyTable();
				}
			}

			index?.Drop(Db.Connection);
			ExtProperty.Database.Update(Db.Connection, statusName, nameof(TransformationStatus.Finished));

			void CreateSyncTrigger(bool auditColumnsExist)
			{
				var triggerName = GetTriggerName(dateColumn);

				var prefix = dateColumn.ColumnPrefix;
				var systemLastEditTimeColumnName = Invariant($"{prefix}_SystemLastEditTimeUtc");
				var systemLastEditUserColumnName = Invariant($"{prefix}_SystemLastEditUser");

				var updateAuditDataClause = auditColumnsExist
					? $@",
			tab.{systemLastEditTimeColumnName} = ISNULL({systemLastEditTimeColumnName}, GetUTCDate()),
			tab.{systemLastEditUserColumnName} = CASE WHEN tab.{systemLastEditUserColumnName} = '' THEN '~BP' ELSE tab.{systemLastEditUserColumnName} END"
					: "";

				if (!DbObjectCreator.TriggerExists(Db.Connection, tableName, triggerName))
				{
					var pkColumnName = dateColumn.TableSchema.PK.Name;
					Db.Connection.ExecuteNonQuery(Invariant($@"
CREATE TRIGGER {schemaName}.{triggerName}
	ON {schemaName}.{tableName}
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE({columnName})
	BEGIN
		UPDATE tab
		SET
			tab.{columnNameToAdd} = CASE WHEN tab.{columnName} IS NULL OR Offset IS NULL THEN tab.{columnName} ELSE TODATETIMEOFFSET({columnName}, Offset) END{updateAuditDataClause}
		FROM
			{schemaName}.{tableName} AS tab
			OUTER APPLY
			(
				{timeZoneSubQuery}
			) as {timeZoneColumnName}
			OUTER APPLY dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO({timeZoneColumnName}, {columnName}, 0)
		WHERE
			tab.{pkColumnName} IN (SELECT inserted.{pkColumnName} FROM inserted)
	END
END"));
				}
			}

			bool CreateAuditColumnsIfNecessary()
			{
				var prefix = dateColumn.ColumnPrefix;
				var systemLastEditTimeColumnName = Invariant($"{prefix}_SystemLastEditTimeUtc");
				var systemLastEditUserColumnName = Invariant($"{prefix}_SystemLastEditUser");
				if (!DbObjectCreator.ColumnExists(Db.Connection, tableDescriptor, systemLastEditTimeColumnName))
				{
					// do not create the audit columns if audit columns are not part of the this table's Schema
					var tableSchema = dateColumn.TableSchema;
					var systemLastEditUserSchemaColumn = tableSchema.GetSchemaColumn(systemLastEditUserColumnName);
					var systemLastEditTimeSchemaColumn = tableSchema.GetSchemaColumn(systemLastEditTimeColumnName);
					if (systemLastEditTimeSchemaColumn != null)
					{
						using (var manager = Db.Connection.BeginTransactionWithManager())
						{
							DbObjectCreator.CreateColumn(Db.Connection, tableDescriptor, systemLastEditUserColumnName, systemLastEditUserSchemaColumn.SqlDbTypeDeclaration, "''");
							DbObjectCreator.CreateColumn(Db.Connection, tableDescriptor, systemLastEditTimeColumnName, systemLastEditTimeSchemaColumn.SqlDbTypeDeclaration);

							manager.CommitTransaction();
						}

						return true;
					}
				}
				else
				{
					return true;
				}

				return false;
			}

			(DateTime? FromTime, DateTime? LowerBoundTime, DateTime? UpperBoundTime) GetBoundaryTimes()
			{
				DateTime? fromTime, lowerBoundTime, upperBoundTime;

				var upperBoundTimeString = ExtProperty.Database.Select(Db.Connection, toTimeName);
				var fromTimeString = ExtProperty.Database.Select(Db.Connection, fromTimeName);
				if (string.IsNullOrEmpty(upperBoundTimeString) || string.IsNullOrEmpty(fromTimeString))
				{
					fromTime = null;
					(lowerBoundTime, upperBoundTime) = GetLowerBoundAndUpperBoundTimeFromDatabase();
					if (upperBoundTime.HasValue)
					{
						ExtProperty.Database.Update(Db.Connection, toTimeName, SqlFormatInfo.ToSqlDateTimeString(upperBoundTime.Value));
					}
				}
				else
				{
					fromTime = SqlFormatInfo.TryParseFromSqlDateTime(fromTimeString, out var parsedFromTime) ? parsedFromTime : null;
					upperBoundTime = SqlFormatInfo.TryParseFromSqlDateTime(upperBoundTimeString, out var parsedToTime) ? parsedToTime : null;
					lowerBoundTime = fromTime;
				}

				return (fromTime, lowerBoundTime, upperBoundTime);
			}

			(DateTime? LowerBoundTime, DateTime? UpperBoundTime) GetLowerBoundAndUpperBoundTimeFromDatabase()
			{
				DateTime? lowerBoundTime;
				DateTime? upperBoundTime;

				var getTimeSql = Invariant($@"
SELECT
	MIN({columnName}) as FromTime,
	MAX({columnName}) as ToTime
FROM
	{schemaName}.{tableName}
WHERE
	{columnName} IS NOT NULL");

				using (var command = Db.Connection.Command(getTimeSql))
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					lowerBoundTime = reader["FromTime"] is DateTime fromDateTime ? fromDateTime : null;
					upperBoundTime = reader["ToTime"] is DateTime toDateTime ? toDateTime : null;
				}

				return (lowerBoundTime, upperBoundTime);
			}

			void DoTransform(DateTime? fromTime, DateTime lowerBoundTime, DateTime upperBoundTime, bool auditColumnsExist)
			{
				var logFullnessProvider = new CargoWise.Data.SqlServer.LogFullnessProvider();
				var backlogWaiter = new CargoWise.Data.SqlServer.BacklogWaiter(new[] { logFullnessProvider });
				var stopWatch = Stopwatch.StartNew();
				while (fromTime.GetValueOrDefault() < upperBoundTime)
				{
					var chunkToTime = GetNextToDateTime(fromTime ?? lowerBoundTime);
					chunkToTime = chunkToTime < upperBoundTime ? chunkToTime : upperBoundTime;

					var maxUpdatedTime = UpdateChunk(fromTime, chunkToTime, auditColumnsExist);
					var nextFromTime = maxUpdatedTime ?? chunkToTime;
					if (nextFromTime == fromTime)
					{
						throw new InvalidOperationException($"Transform {UserDescription} Update Chunk has returned the same last updated Time!");
					}

					fromTime = nextFromTime;

					if (stopWatch.Elapsed.TotalMinutes > 1)
					{
						var fromTimeString = SqlFormatInfo.ToSqlDateTimeString(fromTime.Value);
						ExtProperty.Database.Update(Db.Connection, fromTimeName, fromTimeString);
						manager.ShowInfoMessage($"Finished converting Offsets for {columnName} up to: {fromTimeString}.");

						backlogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) =>
						{
							manager.ShowInfoMessage("\t      waiting for backlog to clear, " + (success ? bytesBacklog.BacklogDescription : failureReason) + ". Please run the LBK service task to reduce log fullness.");
						});

						stopWatch.Restart();
					}
				}
			}

			DateTime? UpdateChunk(DateTime? fromTime, DateTime toTime, bool auditColumnsExist)
			{
				using (var transactionManager = PerformOffsetConversionAsTransaction ? Db.Connection.BeginTransactionWithManager() : null)
				using (var command = Db.Connection.Command(GetUpdateTempOffsetColumnSQL(auditColumnsExist, fromTime.HasValue)))
				{
					if (fromTime.HasValue)
					{
						command.AddParameter("@from", DateTimeTypeBeforeConversion, fromTime);
					}

					command.AddParameter("@to", DateTimeTypeBeforeConversion, toTime);

					DateTime? result = command.ExecuteScalar() is DateTime rawUpdatedTime ? rawUpdatedTime : null;
					transactionManager?.CommitTransaction();

					return result;
				}
			}

			string GetUpdateTempOffsetColumnSQL(bool auditColumnsExist, bool fromTimeHasValue)
			{
				var calculatedOffsetName = Invariant($"{columnName}_WTG_Offset");
				var prefix = dateColumn.ColumnPrefix;

				var fromTimeClause = fromTimeHasValue
					? Invariant($@"
			AND {columnName} > @from")
					: "";

				var auditUpdateClause = auditColumnsExist
					? Invariant($@"
	{prefix}_SystemLastEditTimeUtc = CASE WHEN ISNULL({calculatedOffsetName}Fast, {calculatedOffsetName}) IS NULL THEN {prefix}_SystemLastEditTimeUtc ELSE GetUtcDate() END,
	{prefix}_SystemLastEditUser = CASE WHEN ISNULL({calculatedOffsetName}Fast, {calculatedOffsetName}) IS NULL THEN {prefix}_SystemLastEditUser ELSE '~BP' END,")
					: "";

				return Invariant($@"
DECLARE	@maxUpdatedTime {DateTimeTypeBeforeConversion}
DECLARE @TOP bigint = {BatchSize}

;WITH
	DATA AS (
		SELECT TOP(@TOP) WITH TIES
			*
		FROM
			{schemaName}.{tableName}
		WHERE 1=1{fromTimeClause}
			AND {columnName} <= @to
			AND {columnName} IS NOT NULL
		ORDER BY
			{columnName}
		),
	DATAWITHMAX AS (
		SELECT
			DATA.*,
			MaxDateTime = MAX({columnName}) OVER()
		FROM
			DATA
		)
UPDATE DATAWITHMAX SET
	{columnNameToAdd} = COALESCE({calculatedOffsetName}Fast, {calculatedOffsetName}, {columnName}),{auditUpdateClause}
	@maxUpdatedTime = MaxDateTime
FROM
	DATAWITHMAX
	OUTER APPLY
	(
		{timeZoneSubQuery}
	) as {timeZoneColumnName}
	OUTER APPLY
	(
		SELECT
			TOP 1 {calculatedOffsetName}Fast
		FROM
			dbo.RefDatabase_RefUNLOCOUtcOffset
			CROSS APPLY (SELECT TODATETIMEOFFSET({columnName}, ISNULL(RLO_OffsetMinutesFromUtc, 0)) AS {calculatedOffsetName}Fast) AS {calculatedOffsetName}Fast
		WHERE
			RLO_RL_NKCode = {timeZoneColumnName}
			AND TODATETIMEOFFSET(RLO_StartTimeUtc, RLO_OffsetMinutesFromUtc) <= {columnName}
			AND TODATETIMEOFFSET(RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) > {columnName}
		ORDER BY
			{calculatedOffsetName}Fast DESC
	) AS {calculatedOffsetName}Fast
	OUTER APPLY
	(
		SELECT
			TODATETIMEOFFSET({columnName}, Offset) AS {calculatedOffsetName}
		FROM
			dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO({timeZoneColumnName}, {columnName}, 0)
		WHERE
			{calculatedOffsetName}Fast IS NULL
	) AS {calculatedOffsetName}
	OPTION(OPTIMIZE FOR (@TOP = 1), FORCE ORDER)

SELECT @maxUpdatedTime AS MaxUpdatedTime");
			}

			void ClearExtPropertyTable()
			{
				ExtProperty.Database.Delete(Db.Connection, fromTimeName);
				ExtProperty.Database.Delete(Db.Connection, toTimeName);
			}
		}

		string GetTriggerName(SchemaColumn dateColumn) => Invariant($"TG_{dateColumn.TableName}_Keep{dateColumn.Name}InSync");

		protected virtual bool PerformOffsetConversionAsTransaction => false;

		protected string GetColumnNameToAdd()
		{
			var columnName = ColumnToConvert.Name;
			return SourceColumn != null
				? columnName
				: GetTempColumnName(columnName);
		}

		protected string GetTempColumnName(string columnName) => Invariant($"{ColumnSynchroniser.ConvertDateTimeToDateTimeOffsetColumnPrefix}{columnName}");

		// Roughly a month at a time, override to reduce or increase Chunk Size consideration
		protected virtual DateTime GetNextToDateTime(DateTime currentFromDateTime) => currentFromDateTime.AddDays(30);

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			var dateColumn = SourceColumn ?? (SchemaColumn)ColumnToConvert;
			var columnName = dateColumn.Name;
			var tableName = dateColumn.TableName;
			var schemaName = dateColumn.TableSchema.SqlSchemaName;
			var columnNameToAdd = GetColumnNameToAdd();

			// column to add won't exist if transform never ran
			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(schemaName, tableName), columnNameToAdd))
			{
				ExtProperty.Database.Delete(Db.Connection, GetStatusName(columnName));

				if (SourceColumn != null)
				{
					DbObjectCreator.DropTriggerIfExists(Db.Connection, GetTriggerName(SourceColumn));
				}
			}
		}

		protected virtual SchemaDateTimeColumn SourceColumn => null;
		protected abstract SchemaDateTimeOffsetColumn ColumnToConvert { get; }
		protected abstract string GetSubqueryForTimeZoneColumn(string timeZoneColumnName);
		protected abstract IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider);
		protected abstract SqlDbType DateTimeTypeBeforeConversion { get; }

		// All changes in below region are a temporary fix. The underlying error is being worked on in WI00723403 
		// Once the root issue with RefDatabase_RefUNLOCOUtcOffset is resolved, this will be removed.
		#region TemporaryFix
#if DEBUG
		void OnlinePreUpgradeTransform_TempFix()
		{
			if ((bool)Db.Connection.ExecuteScalar("SELECT CAST(CASE WHEN OBJECT_ID('dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO', 'IF') IS NOT NULL THEN 1 ELSE 0 END as bit)"))
			{
				// it's possible the function is not bound properly because the synonym is wrong, we will need to test this
				var isOffsetFunctionIsBoundCorrectly = OffsetConversionFunctionIsBoundCorrectly();
				if (!isOffsetFunctionIsBoundCorrectly)
				{
					CreateNewTempFunctionForDateTimeOffsetCalculation();
				}
			}
		}

		bool OffsetConversionFunctionIsBoundCorrectly()
		{
			try
			{
				Db.Connection.ExecuteNonQuery("SELECT Offset FROM dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO('AUSYD', GETUTCDATE(), 1)");
				return true;
			}
			catch (SqlException ex) when (ex.Number == 5313)
			{
				return false;
			}
		}

		void CreateNewTempFunctionForDateTimeOffsetCalculation()
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery("DROP SYNONYM RefDatabase_RefUNLOCOUtcOffset");
				Db.Connection.ExecuteNonQuery(@"CREATE VIEW [dbo].[RefUNLOCOUtcOffsetTableView_Temp] AS
SELECT
	CAST(NULL AS uniqueidentifier) AS [RLO_PK],
	CAST(NULL AS varchar(5)) AS [RLO_RL_NKCode],
	CAST(NULL AS smalldatetime) AS [RLO_StartTimeUtc],
	CAST(NULL AS smalldatetime) AS [RLO_EndTimeUtc],
	CAST(NULL AS smallint) AS [RLO_OffsetMinutesFromUtc],
	CAST(NULL AS decimal(5,2)) AS [RLO_OffsetFromUtc]");
				Db.Connection.ExecuteNonQuery("CREATE SYNONYM RefDatabase_RefUNLOCOUtcOffset FOR [dbo].[RefUNLOCOUtcOffsetTableView_Temp]");

				manager.CommitTransaction();
			}
		}
#endif
		#endregion
	}
}
