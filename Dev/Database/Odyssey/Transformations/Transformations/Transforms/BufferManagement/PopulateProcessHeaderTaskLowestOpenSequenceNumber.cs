using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PopulateProcessHeaderTaskLowestOpenSequenceNumber : DataTransformation
	{
		const string LowWatermarkPropertyName = "PopulateProcessHeaderTaskLowestOpenSequenceNumber_LowWatermark";

		public override string UserDescription => "Populate FH_TaskLowestOpenSequenceNumber column for Process Headers";

		protected override void OfflinePreUpgradeTransform()
		{
			var lowWatermarkProperty = ExtProperty.Table.Select(
					Db.Connection,
					ProcessHeaderSchema.Constants.SqlSchemaName,
					ProcessHeaderSchema.Constants.TableName,
					LowWatermarkPropertyName);

			if (lowWatermarkProperty == null || !Guid.TryParse(lowWatermarkProperty, out var _))
			{
				ExtProperty.Table.Update(
					Db.Connection,
					ProcessHeaderSchema.Constants.SqlSchemaName,
					ProcessHeaderSchema.Constants.TableName,
					LowWatermarkPropertyName,
					new Guid().ToString()
				);
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var taskWhere = @"
P9_Status IN ('ASN', 'OPN', 'SUS', 'WRK')
AND P9_Type NOT IN ('EXC', 'MIL', 'TRG')
AND P9_ParentTableCode <> 'P0'";

			var sql = $@"
DECLARE @HeadersPKs TABLE (PK UNIQUEIDENTIFIER, ParentId UNIQUEIDENTIFIER);

INSERT INTO @HeadersPKs (PK, ParentId)
SELECT FH_PK, FH_ParentId
	FROM dbo.ProcessHeader
	WHERE FH_PK BETWEEN @from AND @to
	AND FH_Status = 'OPN'
	AND FH_TaskLowestOpenSequenceNumber = -1
	AND FH_FH_ParentHeader IS NOT NULL
	AND EXISTS(SELECT 1 FROM ProcessTasks WHERE P9_ParentID = FH_ParentId AND P9_FH_ProcessHeader = FH_PK AND {taskWhere});

WITH HeadersToUpdate AS (
	SELECT 
		PK,
		MIN(P9_Sequence) OVER (PARTITION BY PK) AS MinVal,
		ROW_NUMBER() OVER (PARTITION BY PK ORDER BY PK) AS RN
	FROM dbo.ProcessTasks WITH(INDEX(NR_RC__P9_ParentID))
	JOIN @HeadersPKs ON PK = P9_FH_ProcessHeader
	WHERE P9_ParentID = ParentId AND {taskWhere}
)
UPDATE dbo.ProcessHeader
SET FH_TaskLowestOpenSequenceNumber = MinVal
FROM HeadersToUpdate
JOIN dbo.ProcessHeader ON PK = FH_PK
WHERE RN = 1
OPTION (MAXDOP 1)
SELECT @@ROWCOUNT AS RowsUpdated
FROM @HeadersPKs; 
";
			var lowWatermarkProperty = ExtProperty.Table.Select(
					Db.Connection,
					ProcessHeaderSchema.Constants.SqlSchemaName,
					ProcessHeaderSchema.Constants.TableName,
					LowWatermarkPropertyName);

			if (lowWatermarkProperty == null)
			{
				return;
			}

			var lastPKUpdated = new Guid(lowWatermarkProperty);

			manager?.ShowInfoMessage("Started updating headers");

			foreach (var chunk in GuidChunker.GenerateChunks(1000, DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessHeaderSchema.Constants.TableName), lastPKUpdated))
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				using var cmd = Db.Connection.Command(sql);

				cmd.AddParameter("@from", SqlDbType.UniqueIdentifier, chunk.LowerBound);
				cmd.AddParameter("@to", SqlDbType.UniqueIdentifier, chunk.UpperBound);

				var result = (int)(cmd.ExecuteScalar() ?? 0);

				if (result > 0)
				{
					ExtProperty.Table.Update(
					Db.Connection,
					ProcessHeaderSchema.Constants.SqlSchemaName,
					ProcessHeaderSchema.Constants.TableName,
					LowWatermarkPropertyName,
					chunk.UpperBound.ToString());

					manager?.ShowInfoMessage($"{result} headers processed, last PK: {chunk.UpperBound}.");
				}
			}

			manager?.ShowInfoMessage("Finished updating headers");

			ExtProperty.Table.Delete(
				Db.Connection,
				ProcessHeaderSchema.Constants.SqlSchemaName,
				ProcessHeaderSchema.Constants.TableName,
				LowWatermarkPropertyName);
		}
	}
}
