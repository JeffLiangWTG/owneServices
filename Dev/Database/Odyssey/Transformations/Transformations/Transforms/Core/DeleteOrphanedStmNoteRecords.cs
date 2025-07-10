using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	abstract class DeleteOrphanedStmNoteRecords : DataTransformation
	{
		protected abstract string ParentTableName { get; }
		protected abstract string ParentPKColumnName { get; }
		protected string LastProcessedChunkPKName => "DeleteOrphanedStmNoteFrom" + ParentTableName + "LastProcessedChunkPKName";
		protected string TotalProcessedRowCountName => "DeleteOrphanedStmNoteFrom" + ParentTableName + "TotalProcessedRowCountName";

		public override string UserDescription => "Delete Orphaned StmNote From " + ParentTableName;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var sqlText = @$"DELETE dbo.StmNote
FROM dbo.StmNote LEFT JOIN dbo.{ParentTableName} ON ST_ParentID = {ParentPKColumnName}
WHERE ST_ParentID BETWEEN @lowerBound AND @upperBound
AND ST_Table = '{ParentTableName}'
AND {ParentPKColumnName} IS NULL

SELECT @@ROWCOUNT
";
			var totalProcessedRowCount = long.TryParse(ExtProperty.Database.Select(Db.Connection, TotalProcessedRowCountName), out var parsedNum) ? parsedNum : 0;

			var approximateRowCount = Math.Max(1, DataUtils.GetApproximateRowCountForTable(Db.Connection, StmNoteSchema.Constants.TableName));
			var stopWatch = Stopwatch.StartNew();

			foreach (var chunk in GuidChunker.GenerateChunks(20000, approximateRowCount, lastProcessedPK))
			{
				if (stopWatch.Elapsed >= TimeSpan.FromMinutes(1) || token.IsCancellationRequested)
				{
					LogTotalProcessedRowCount(totalProcessedRowCount);
					stopWatch.Restart();
				}

				token.ThrowIfCancellationRequested();
				totalProcessedRowCount += Db.Connection.ExecuteScalar<int>(sqlText, cmd =>
				{
					cmd.AddParameter("@lowerBound", SqlDbType.UniqueIdentifier, chunk.LowerBound);
					cmd.AddParameter("@upperBound", SqlDbType.UniqueIdentifier, chunk.UpperBound);
				});

				ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());
				ExtProperty.Database.Update(Db.Connection, TotalProcessedRowCountName, totalProcessedRowCount.ToString());
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
			ExtProperty.Database.Delete(Db.Connection, TotalProcessedRowCountName);
			LogTotalProcessedRowCount(totalProcessedRowCount);
		}

		void LogTotalProcessedRowCount(long count)
		{
			manager?.ShowInfoMessage($"Finished deleting {count} orphaned StmNote records from {ParentTableName} in total.");
		}
	}
}
