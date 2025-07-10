using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class UpdateIM_CloseTimeUtcValueTransform : DataTransformation
	{
		public override string UserDescription => "Copy Incident Closed Event time to IM_CloseTimeUtc";

		const string sqlSchemaName = "dbo";
		const string tableName = "IncidentMain";
		const string lowWatermarkPropertyName = "UpdateIM_ClosedDateUtcValue_LowWatermark";

		protected virtual int BatchSize => 100;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, tableName))
			{
				manager?.ShowInfoMessage($"No {tableName} table");
				return;
			}

			Guid.TryParse(ExtProperty.Table.Select(
				Db.Connection,
				sqlSchemaName,
				tableName,
				lowWatermarkPropertyName), out var lastUpdatedIncidentPK);
			var sql = $@"
WITH IncidentCloseDateInfo (IncidentPK, CloseDate)
	AS (
		SELECT
			IM_PK AS IncidentPK,
			MAX(CASE WHEN IM_CloseTimeUtc IS NULL THEN NULL ELSE COALESCE(SL_EventTimeUtc, SL_PostedTimeUtc) END) AS CloseDate
		FROM dbo.IncidentMain
		INNER JOIN dbo.StmALog WITH(INDEX(NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime)) ON SL_Parent = IM_PK AND SL_SE_NKEvent = 'ICL' AND SL_IsEstimate='N' AND SL_IsCancelled = 'N'
		WHERE
			IM_PK BETWEEN @FromPK AND @ToPK
		GROUP BY IM_PK
	)
UPDATE dbo.IncidentMain
SET
	IM_CloseTimeUtc = IncidentCloseDateInfo.CloseDate,
	IM_SystemLastEditTimeUtc = GETUTCDATE(),
	IM_SystemLastEditUser = '~BP'
FROM dbo.IncidentMain
JOIN IncidentCloseDateInfo ON dbo.IncidentMain.IM_PK = IncidentCloseDateInfo.IncidentPK;

SELECT @@ROWCOUNT AS RowsUpdated;";

			var counter = 0;
			long totalRowsUpdated = 0;
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, tableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastUpdatedIncidentPK);

			manager?.ShowInfoMessage("Started copying ICL event time to IM_CloseTimeUtc");
			foreach (var chunk in chunks)
			{
				token.ThrowIfCancellationRequested();

				lastUpdatedIncidentPK = chunk.UpperBound;
				var rowsUpdated = 0;

				Db.Connection.RunInTransaction(() =>
				{
					using (var cmd = Db.Connection.Command(sql))
					{
						cmd.AddParameter("@FromPK", System.Data.SqlDbType.UniqueIdentifier, chunk.LowerBound);
						cmd.AddParameter("@ToPK", System.Data.SqlDbType.UniqueIdentifier, chunk.UpperBound);
						rowsUpdated = (int)cmd.ExecuteScalar();
						ExtProperty.Table.Update(
							Db.Connection,
							sqlSchemaName,
							tableName,
							lowWatermarkPropertyName,
							lastUpdatedIncidentPK.ToString());
					}
				}, doCommit: true);

				manager?.ShowInfoMessage($"Batch{counter}: {rowsUpdated} rows were updated");
				counter++;
				totalRowsUpdated += rowsUpdated;
			}

			ExtProperty.Table.Delete(
				Db.Connection,
				sqlSchemaName,
				tableName,
				lowWatermarkPropertyName);
			manager?.ShowInfoMessage($"Finished copying ICL event time to IM_CloseTimeUtc: {totalRowsUpdated} rows were updated");
		}
	}
}
