using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.EventLog
{
	class UpdatePersonSensitiveInformationEventLogTransform : DataTransformation
	{
		public override string UserDescription => "Update person sensitive information event log";

		const string LastProcessedBatchEndDateName = "UpdatePersonSensitiveInformationEventLogTransform.LastProcessedBatchEndDate";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule", Justification = "EndDate does not need to be precise as this log issue was fixed in December 2023")]
		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var startDate = new DateTime(2018, 5, 1);
			var endDate = DateTime.UtcNow.AddHours(1); // Reserve the time required to execute sql to ensure that all event logs can be transformed.
			var interval = TimeSpan.FromDays(7);

			var lastProcessedBatchEndDateString = ExtProperty.Database.Select(Db.Connection, LastProcessedBatchEndDateName);
			if (string.IsNullOrEmpty(lastProcessedBatchEndDateString))
			{
				lastProcessedBatchEndDateString = SqlFormatInfo.ToSqlDateTimeString(startDate);
			}

			var stopWatch = Stopwatch.StartNew();
			SqlFormatInfo.TryParseFromSqlDateTime(lastProcessedBatchEndDateString, out var lastProcessedBatchEndDate);
			startDate = lastProcessedBatchEndDate;
			while (startDate < endDate)
			{
				var batchStartDate = startDate;
				var batchEndDate = startDate + interval;

				if (batchEndDate > endDate)
				{
					batchEndDate = endDate;
				}

				UpdateChunk(batchStartDate, batchEndDate);

				startDate += interval;
				lastProcessedBatchEndDate = batchEndDate;
				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedBatchEndDateString = SqlFormatInfo.ToSqlDateTimeString(lastProcessedBatchEndDate);
					ExtProperty.Database.Update(Db.Connection, LastProcessedBatchEndDateName, lastProcessedBatchEndDateString);
					manager.ShowInfoMessage($"Last processed batch endDate: {lastProcessedBatchEndDateString}.");

					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}
			ExtProperty.Database.Delete(Db.Connection, LastProcessedBatchEndDateName);
		}

		void UpdateChunk(DateTime batchStartDate, DateTime batchEndDate)
		{
			var updateStatement = @"
UPDATE dbo.StmALog 
SET SL_Reference = 
    CASE
        WHEN SL_Reference LIKE 'Changed PER_FullName from:%' THEN 'Changed PER_FullName.'
        WHEN SL_Reference LIKE 'Changed PER_LegalName from:%' THEN 'Changed PER_LegalName.'
        WHEN SL_Reference LIKE 'Changed PER_BirthDate from:%' THEN 'Changed PER_BirthDate.'
        WHEN SL_Reference LIKE 'Changed PER_EmailAddress from:%' THEN 'Changed PER_EmailAddress.'
        WHEN SL_Reference LIKE 'Changed PER_MobilePhone from:%' THEN 'Changed PER_MobilePhone.'
        WHEN SL_Reference LIKE 'Changed PER_HomePhone from:%' THEN 'Changed PER_HomePhone.'
        ELSE SL_Reference
    END
WHERE SL_PostedTimeUtc > @batchStartDate
AND SL_PostedTimeUtc <= @batchEndDate
AND SL_Table = 'GlbPerson'
AND SL_Reference LIKE 'Changed PER[_]%';";

			using (var cmd = Db.Connection.Command(updateStatement))
			{
				cmd.AddParameter("@batchStartDate", System.Data.SqlDbType.DateTime, batchStartDate);
				cmd.AddParameter("@batchEndDate", System.Data.SqlDbType.DateTime, batchEndDate);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
