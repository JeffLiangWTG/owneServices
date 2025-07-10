using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;

public class UpdateEmptyStmNoteContextToDefaultValue : DataTransformation
{
	public override string UserDescription => "Update Empty StmNoteContext to default value";

	const string LastProcessedPKExtendedPropertyString = "ConvertEmptyStmNoteContextToDefaultValue.LastProcessedChunkPK";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		ProcessChunks(token);
		ProcessNullParentID(token);
	}

	void ProcessChunks(CancellationToken token)
	{
		var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedPKExtendedPropertyString);
		var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

		var totalCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, StmNoteSchema.Constants.TableName);

		var chunks = GuidChunker.GenerateChunks(100000, totalCount, lastProcessedPK);
		var chunksCount = Math.Ceiling(totalCount / 100000.0);
		var i = 0;

		var sql = @"UPDATE dbo.StmNote SET ST_NoteContext = '???', ST_SystemLastEditTimeUtc = GETUTCDATE(), ST_SystemLastEditUser = '~BP' WHERE ST_ParentID >= @StartGuid AND ST_ParentID <= @EndGuid AND LEN(ST_NoteContext) < 3 OPTION (MAXDOP 1)";
		var stopWatch = Stopwatch.StartNew();
		foreach (var chunk in chunks)
		{
			token.ThrowIfCancellationRequested();

			var startGuid = chunk.LowerBound;
			var endGuid = chunk.UpperBound;

			Db.Connection.ExecuteNonQuery(sql, cmd =>
			{
				cmd.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, startGuid);
				cmd.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, endGuid);
			});

			i++;
			if (stopWatch.Elapsed >= TimeSpan.FromMinutes(1) || token.IsCancellationRequested)
			{
				ExtProperty.Database.Update(Db.Connection, LastProcessedPKExtendedPropertyString, chunk.UpperBound.ToString());
				manager?.ShowInfoMessage($"Processed {(i / chunksCount).ToString("P2", CultureInfo.InvariantCulture)}.");
				stopWatch.Restart();
			}
		}
		manager?.ShowInfoMessage("Finished processing chunks");
		ExtProperty.Database.Delete(Db.Connection, LastProcessedPKExtendedPropertyString);
	}

	void ProcessNullParentID(CancellationToken token)
	{
		var sql = @"UPDATE TOP (1000) dbo.StmNote SET ST_NoteContext = '???', ST_SystemLastEditTimeUtc = GETUTCDATE(), ST_SystemLastEditUser = '~BP' WHERE ST_ParentID IS NULL AND LEN(ST_NoteContext) < 3 OPTION (MAXDOP 1) SELECT @@ROWCOUNT";
		int updateCount;
		var stopWatch = Stopwatch.StartNew();
		var updateTotalCount = 0;
		while ((updateCount = Db.Connection.ExecuteScalar<int>(sql)) >= 1000)
		{
			updateTotalCount += updateCount;
			if (stopWatch.Elapsed >= TimeSpan.FromMinutes(1) || token.IsCancellationRequested)
			{
				manager?.ShowInfoMessage($"Processed {updateTotalCount} StmNote records with null ST_ParentID.");
				stopWatch.Restart();
			}

			token.ThrowIfCancellationRequested();
		}

		updateTotalCount += updateCount;
		manager?.ShowInfoMessage($"Finished processing {updateTotalCount} StmNote records with null ST_ParentID.");
	}
}
