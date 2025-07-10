using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class RemoveDuplicatedCSRForConsol : DataTransformation
	{
		public override string UserDescription => "Remove duplicated CSR reference numbers for consols.";

		const string LastProcessedChunkPKName = "RemoveDuplicatedCSRForConsol.JK_PK.LastProcessedChunkPKName";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, JobConsolSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

			var loggingStopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				ProcessBatch(chunk.LowerBound, chunk.UpperBound);

				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
				{
					manager?.ShowInfoMessage($"RemoveDuplicatedCSRForConsol: Last processed batch chunk: {chunk.UpperBound}.");
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());

					loggingStopWatch.Restart();
				}

				token.ThrowIfCancellationRequested();
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		#region Implementation

		void ProcessBatch(Guid startGuid, Guid endGuid)
		{
			var sql = $@"
WITH CTE AS
(
	SELECT
		CE_PK,
		ROW_NUMBER() OVER (PARTITION BY cen.CE_ParentID, CE_EntryNum ORDER BY CE_SystemCreateTimeUTC) RN
	FROM dbo.CusEntryNum cen
	WHERE
		cen.CE_ParentTable = 'JobConsol'
		AND cen.CE_Category = 'OTH'
		AND cen.CE_EntryType = 'CSR'
		AND cen.CE_EntryIsSystemGenerated = 1
		AND cen.CE_SystemCreateTimeUtc > '2020-06-12 00:00:00'
		AND cen.CE_ParentID BETWEEN @startGuid AND @endGuid
)
DELETE FROM CTE
WHERE RN > 1
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@startGuid", SqlDbType.UniqueIdentifier, startGuid);
				command.AddParameter("@endGuid", SqlDbType.UniqueIdentifier, endGuid);

				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
