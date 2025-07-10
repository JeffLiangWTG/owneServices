using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Transformations.Transforms.PopulateAuditColumns
{
	public abstract class BasePostOnPopulateAuditColumns<T> : DataTransformation where T : ITableSchema
	{
		public override string UserDescription => $"Populate audit columns for {TableName} from dbo.StmALog";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			using (new AuditDetailsAreNotMissingTriggerSuspender(TableSchema))
			{
				var loggingStopWatch = new Stopwatch();
				loggingStopWatch.Start();
				var totalCount = Db.Connection.ExecuteScalar<long>($"SELECT SUM(rows) FROM sys.partitions WHERE object_id = OBJECT_ID('{TableSchema.SqlSchemaName}.{TableName}') AND index_id in (0, 1)");
				var totalUpdatedCount = 0;
				var startingPoint = GetStartingPointFromTableExtProperty();

				while (true)
				{
					var (updatedCount, endPoint) = PopulateBatch(startingPoint);

					if (updatedCount == 0)
					{
						ClearExtPropertyTable();
						return;
					}

					totalUpdatedCount += updatedCount;
					if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 2)
					{
						manager.ShowInfoMessage($"Finished processing {totalUpdatedCount:N0} item(s) of {totalCount:N0} in {TableName}.");
						loggingStopWatch.Restart();
						SetNextStartingPointToTableExtProperty(endPoint);
						token.ThrowIfCancellationRequested();
					}
					startingPoint = endPoint;
				}
			}
		}

		(int updatedCount, Guid ranToPK) PopulateBatch(Guid runFrom)
		{
			var sqlUpdate = $@"
DECLARE @batchSize BIGINT = {BatchSize};
DECLARE @ranToPK UNIQUEIDENTIFIER;

WITH
	ChunkOfTableToUpdate AS (
		SELECT TOP (@batchSize) WITH TIES
			{PKColumn.Name}
			, {CreateTimeColumn.Name}
			, {CreateUserColumn.Name}
			, {LastEditTimeColumn.Name}
			, {LastEditUserColumn.Name}
		FROM
			{TableSchema.SqlSchemaName}.{TableName}
		WHERE 1=1 AND (
			{CreateTimeColumn.Name} IS NULL AND {CreateUserColumn.Name} = ''
			OR {LastEditTimeColumn.Name} IS NULL AND {LastEditUserColumn.Name} = ''
		) AND (
			{PKColumn.Name} > @runFrom
		)
		ORDER BY
			{PKColumn.Name}
	),
	ChunkOfTableToUpdateWithMaxPK AS (
		SELECT *, MaxPK = MAX({PKColumn.Name}) OVER() FROM ChunkOfTableToUpdate
	),
	ALog AS (
		SELECT
			SL_Parent
			, CreateTime = MIN(IIF(SL_SE_NKEvent = 'ADD', SL_PostedTimeUtc, NULL))
			, CreateUser = MIN(IIF(SL_SE_NKEvent = 'ADD', SL_GS_NKUser, NULL))
			, LastEditTime = MAX(SL_PostedTimeUtc)
			, LastEditUser = RIGHT(MAX(CONVERT(char(23), SL_PostedTimeUtc, 121) + SL_GS_NKUser), 3)
		FROM
			dbo.StmALog
		WITH (INDEX(NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime))
		WHERE 1=1
			AND SL_Table='{SL_TableName}'
			AND SL_SE_NKEvent IN ('ADD', 'EDT')
		GROUP BY
			SL_Parent
	)

UPDATE ChunkOfTableToUpdateWithMaxPK SET
	{CreateTimeColumn.Name} = COALESCE({CreateTimeColumn.Name}, CreateTime)
	, {CreateUserColumn.Name} = COALESCE(NULLIF({CreateUserColumn.Name},''), NULLIF(CreateUser,''), '')
	, {LastEditTimeColumn.Name} = COALESCE({LastEditTimeColumn.Name}, LastEditTime)
	, {LastEditUserColumn.Name} = COALESCE(NULLIF({LastEditUserColumn.Name},''), NULLIF(LastEditUser,''), '')
	, @ranToPK = MaxPK
FROM
	ChunkOfTableToUpdateWithMaxPK
LEFT JOIN
	ALog ON SL_Parent = {PKColumn.Name}
OPTION (OPTIMIZE FOR (@batchSize = 1));

SELECT @@ROWCOUNT AS UpdatedCount, @ranToPK AS RanToPK";    // SQL query not to show.

			using (var command = Db.Connection.Command(sqlUpdate))
			{
				command.AddParameter("@runFrom", SqlDbType.UniqueIdentifier, runFrom);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					return (
						reader["UpdatedCount"] is DBNull ? 0 : (int)reader["UpdatedCount"],
						reader["RanToPK"] is DBNull ? Guid.Empty : (Guid)reader["RanToPK"]
					);
				}
			}
		}

		#region Markings

		const string ProcessMarkExtPropertyTableKey = "PostOnPopulateAuditColumnStartingPoint";   // ExtProperty storage key.

		Guid GetStartingPointFromTableExtProperty()
		{
			var extProperty = ExtProperty.Table.Select(Db.Connection, TableSchema.SqlSchemaName, TableName, ProcessMarkExtPropertyTableKey);
			return extProperty == null ? Guid.Empty : new Guid(extProperty);
		}

		void SetNextStartingPointToTableExtProperty(Guid endPoint)
		{
			ExtProperty.Table.Update(Db.Connection, TableSchema.SqlSchemaName, TableName, ProcessMarkExtPropertyTableKey, endPoint.ToString());
		}

		void ClearExtPropertyTable()
		{
			ExtProperty.Table.Delete(Db.Connection, TableSchema.SqlSchemaName, TableName, ProcessMarkExtPropertyTableKey);
		}

		#endregion

		public virtual int BatchSize => 1000;

		#region Table Schema

		string TableName => TableSchema.TableName;

		protected virtual string SL_TableName => TableName;

		SchemaPKColumn PKColumn => TableSchema.PK;

		SchemaDateTimeColumn CreateTimeColumn => createTimeColumn ?? (createTimeColumn = TableSchema.All.OfType<SchemaDateTimeColumn>().FirstOrDefault(c => c.Name.Equals(ColumnNamePrefix + "_SystemCreateTimeUtc")));
		SchemaDateTimeColumn createTimeColumn;

		SchemaStringColumn CreateUserColumn => createUserColumn ?? (createUserColumn = TableSchema.All.OfType<SchemaStringColumn>().FirstOrDefault(c => c.Name.Equals(ColumnNamePrefix + "_SystemCreateUser")));
		SchemaStringColumn createUserColumn;

		SchemaDateTimeColumn LastEditTimeColumn => lastEditTimeColumn ?? (lastEditTimeColumn = TableSchema.All.OfType<SchemaDateTimeColumn>().FirstOrDefault(c => c.Name.Equals(ColumnNamePrefix + "_SystemLastEditTimeUtc")));
		SchemaDateTimeColumn lastEditTimeColumn;

		SchemaStringColumn LastEditUserColumn => lastEditUserColumn ?? (lastEditUserColumn = TableSchema.All.OfType<SchemaStringColumn>().FirstOrDefault(c => c.Name.Equals(ColumnNamePrefix + "_SystemLastEditUser")));
		SchemaStringColumn lastEditUserColumn;

		string ColumnNamePrefix => columnNamePrefix ?? (columnNamePrefix = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetColumnNamePrefix(TableName));
		string columnNamePrefix;

		public ITableSchema TableSchema => tableSchema ?? (tableSchema = (ITableSchema)typeof(T).GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null));
		ITableSchema tableSchema;

		#endregion

		#region AuditDetailsAreNotMissing Trigger suspender

		class AuditDetailsAreNotMissingTriggerSuspender : DisposableObject
		{
			public AuditDetailsAreNotMissingTriggerSuspender(ITableSchema tableSchema)
			{
				TableName = tableSchema.TableName;
				Db.Connection.ExecuteNonQuery($@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{AuditDetailsAreNotMissingTriggerName}')
BEGIN
	DISABLE TRIGGER {AuditDetailsAreNotMissingTriggerName} ON {TableName}
END");
			}

			protected override void Dispose(bool isDisposing)
			{
				base.Dispose(isDisposing);
				Db.Connection.ExecuteNonQuery($@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{AuditDetailsAreNotMissingTriggerName}')
BEGIN
	ENABLE TRIGGER {AuditDetailsAreNotMissingTriggerName} ON {TableName}
END");
			}

			string TableName { get; }

			string AuditDetailsAreNotMissingTriggerName => $@"TG_{TableName}_AuditDetailsAreNotMissing_Update";
		}

		#endregion

	}
}
