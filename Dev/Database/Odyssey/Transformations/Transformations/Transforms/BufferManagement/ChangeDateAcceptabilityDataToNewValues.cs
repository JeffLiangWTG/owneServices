using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	class ChangeDateAcceptabilityDataToNewValues : DataTransformation
	{
		public override string UserDescription => "Transform the existing Date Acceptability values into the new Deadline types";

		const string LowerBoundName = "@LowerBound";
		const string UpperBoundName = "@UpperBound";

		const string LastProcessedPropertyName = "LastProcessedGuidForChangeDateAcceptabilityDataToNewValues";
		const string TableSizePropertyName = "TableSizeForChangeDateAcceptabilityDataToNewValues";

		const int BatchSize = 5000;
		const int TimeoutMins = 1;

		string DropIndexSql => $@"DROP INDEX IF EXISTS [_WTG__{UserDescription}_1] ON [dbo].[ProcessHeader];";

		string UpdateSqlForNullParentIds => $@"
UPDATE [dbo].[ProcessHeader]
SET
	[FH_SystemLastEditTimeUtc] = GETUTCDATE(),
	[FH_SystemLastEditUser] = '~BP',
	[FH_DeadlineType] = CASE
		WHEN [FH_DateAcceptability] IN ('_|_', '/|_', '¯|_') THEN 'HRD'
		ELSE 'SFT'
	END
WHERE
	FH_ParentId IS NULL
	AND [FH_DeadlineType] = '' AND [FH_DateAcceptability] IN ('_|_', '/|_', '¯|_', '¯|¯', '¯|\', '/|¯', '/|\', '_|¯', '_|\')
OPTION (MAXDOP 1)";

		string UpdateSqlMain => $@"
UPDATE [dbo].[ProcessHeader]
SET
	[FH_SystemLastEditTimeUtc] = GETUTCDATE(),
	[FH_SystemLastEditUser] = '~BP',
	[FH_DeadlineType] = CASE
		WHEN [FH_DateAcceptability] IN ('_|_', '/|_', '¯|_') THEN 'HRD'
		ELSE 'SFT'
	END
WHERE
	FH_ParentId BETWEEN {LowerBoundName} AND {UpperBoundName}
	AND [FH_DeadlineType] = '' AND [FH_DateAcceptability] IN ('_|_', '/|_', '¯|_', '¯|¯', '¯|\', '/|¯', '/|\', '_|¯', '_|\')
OPTION (MAXDOP 1)";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			Db.Connection.ExecuteNonQuery(DropIndexSql);

			using (var cmd = Db.Connection.Command(UpdateSqlForNullParentIds))
			{
				cmd.ExecuteNonQuery();
			}
			manager?.ShowInfoMessage($"Processes all records with no FH_ParentId");

			var tableSize = ExtProperty.Table.Select(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, TableSizePropertyName);
			var lastProcessedGuidString = ExtProperty.Table.Select(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, LastProcessedPropertyName);

			if (string.IsNullOrEmpty(tableSize))
			{
				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessHeaderSchema.Constants.TableName);
				if (rowCount == 0)
				{
					return;
				}
				else
				{
					tableSize = rowCount.ToString();
					ExtProperty.Table.Update(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, TableSizePropertyName, tableSize);
				}
			}

			var count = long.Parse(tableSize);
			Guid? lastProcessedGuid = string.IsNullOrEmpty(lastProcessedGuidString) ? null : Guid.Parse(lastProcessedGuidString);
			foreach (var chunk in GuidChunker.GenerateChunks(BatchSize, count, lastProcessedGuid))
			{
				var stopWatch = Stopwatch.StartNew();

				using (var cmd = Db.Connection.Command(UpdateSqlMain))
				{
					cmd.AddParameterBasedOnDbColumn(LowerBoundName, chunk.LowerBound, ProcessHeaderSchema.FH_ParentId);
					cmd.AddParameterBasedOnDbColumn(UpperBoundName, chunk.UpperBound, ProcessHeaderSchema.FH_ParentId);

					cmd.ExecuteNonQuery();
				}

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > TimeoutMins)
				{
					lastProcessedGuidString = chunk.UpperBound.ToString();
					ExtProperty.Table.Update(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, LastProcessedPropertyName, lastProcessedGuidString);
					manager?.ShowInfoMessage($"FH_ParentId of last processed record: {lastProcessedGuidString}, total no of records: {tableSize}.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ExtProperty.Table.Delete(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, LastProcessedPropertyName);
			ExtProperty.Table.Delete(Db.Connection, ProcessHeaderSchema.Constants.SqlSchemaName, ProcessHeaderSchema.Constants.TableName, TableSizePropertyName);
		}
	}
}
