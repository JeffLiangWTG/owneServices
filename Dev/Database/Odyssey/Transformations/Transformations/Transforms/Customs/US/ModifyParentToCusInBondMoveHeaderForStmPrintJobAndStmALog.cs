using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog : DataTransformation, ITransformationIndexProvider
	{
		public ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog()
		{
		}

		public override string UserDescription => "Modify Parent to CusInBondMoveHeader for StmPrintJob and StmALog";

		const string FromTimeNameForStmALog = "ModifyParentToCusInBondMoveHeaderInStmALog.From";
		const string ToTimeNameForStmALog = "ModifyParentToCusInBondMoveHeaderInStmALog.To";
		const int BatchSize = 3000;

		protected override void OfflinePostUpgradeTransform()
		{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
			var toTime = DateTime.UtcNow;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
			ExtProperty.Database.Update(Db.Connection, ToTimeNameForStmALog, SqlFormatInfo.ToSqlDateTimeString(toTime));

			var sql = @"
UPDATE
	dbo.StmPrintJob
SET
	SP_ParentTableName = 'CusInBondMoveHeader',
	SP_SystemLastEditTimeUtc = GETUTCDATE(),
	SP_SystemLastEditUser = '~BP'
WHERE
	SP_ParentTableName = 'USInBondMoveHeader'
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(StmPrintJobSchema.Instance)
					.Key(StmPrintJobSchema.Constants.SP_ParentTableName)
					.Include(StmPrintJobSchema.Constants.SP_SystemLastEditTimeUtc)
					.Include(StmPrintJobSchema.Constants.SP_SystemLastEditUser)
					.Where("[SP_ParentTableName]='USInBondMoveHeader'")
					.GetInfo();

				return indexProvider;
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var upperBoundToTimeString = ExtProperty.Database.Select(Db.Connection, ToTimeNameForStmALog);
			if (upperBoundToTimeString != null)
			{
				SqlFormatInfo.TryParseFromSqlDateTime(upperBoundToTimeString, out var upperBoundToTime);

				var fromTimeString = ExtProperty.Database.Select(Db.Connection, FromTimeNameForStmALog)
					?? SqlFormatInfo.ToSqlDateTimeString(new DateTime(2024, 11, 01));
				SqlFormatInfo.TryParseFromSqlDateTime(fromTimeString, out var fromTime);

				var stopWatch = Stopwatch.StartNew();
				while (fromTime < upperBoundToTime)
				{
					var chunkToTime = fromTime.AddDays(1);
					chunkToTime = chunkToTime < upperBoundToTime ? chunkToTime : upperBoundToTime;

					var maxUpdatedTime = UpdateChunkForStmALog(fromTime, chunkToTime);
					fromTime = maxUpdatedTime ?? chunkToTime;

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						fromTimeString = SqlFormatInfo.ToSqlDateTimeString(fromTime);
						ExtProperty.Database.Update(Db.Connection, FromTimeNameForStmALog, fromTimeString);
						manager.ShowInfoMessage($"Finished processing StmALog up to {fromTimeString}.");

						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}

				ExtProperty.Database.Delete(Db.Connection, FromTimeNameForStmALog);
				ExtProperty.Database.Delete(Db.Connection, ToTimeNameForStmALog);
			}
		}

		DateTime? UpdateChunkForStmALog(DateTime fromTime, DateTime toTime)
		{
			var updateStatement = $@"
DECLARE	@maxUpdatedTime DATETIME

;WITH
	DATA AS (
		SELECT TOP({BatchSize}) WITH TIES
			*
		FROM
			dbo.StmALog WITH (FORCESEEK)
		WHERE 1=1
			AND SL_Table = 'USInBondMoveHeader'
			AND SL_PostedTimeUtc > @from
			AND SL_PostedTimeUtc <= @to
			AND SL_SE_NKEvent = 'DSN'
		ORDER BY
			SL_PostedTimeUtc
		)
UPDATE DATA SET
	SL_Table = 'CusInBondMoveHeader',
	@maxUpdatedTime = SL_PostedTimeUtc

SELECT @maxUpdatedTime AS MaxUpdatedTime
";
			using (var command = Db.Connection.Command(updateStatement))
			{
				command.AddParameter("@from", SqlDbType.DateTime, fromTime);
				command.AddParameter("@to", SqlDbType.DateTime, toTime);

				return command.ExecuteScalar() is DateTime rawUpdatedTime ? rawUpdatedTime : null;
			}
		}
	}
}
