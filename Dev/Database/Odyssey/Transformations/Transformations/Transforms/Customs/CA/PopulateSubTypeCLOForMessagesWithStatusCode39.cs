using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA
{
	public class PopulateSubTypeCLOForMessagesWithStatusCode39 : DataTransformation
	{
		public PopulateSubTypeCLOForMessagesWithStatusCode39()
		{
		}

		public override string UserDescription => "Populate Subtype CLO for messages with status code 39";
		const string StartDateFlag = "PopulateSubTypeCLOForMessagesWithStatusCode39_StartDate";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				var startDate = GetStartDate();

#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
				var today = DateTime.UtcNow;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule

				var chunkSize = 30;
				var totalUpdatedCount = 0;
				var stopWatch = Stopwatch.StartNew();

				while (startDate < today)
				{
					var endDate = startDate.AddDays(chunkSize);
					var updatedCount = ProcessBatch(startDate, endDate);
					totalUpdatedCount += updatedCount;

					SaveProgress(endDate, ref stopWatch, totalUpdatedCount, token);

					startDate = endDate;
				}

				FinalizeProcess(totalUpdatedCount);
			}

			DateTime GetStartDate()
			{
				var startDateFlag = ExtProperty.Database.Select(Db.Connection, StartDateFlag);
				if (string.IsNullOrEmpty(startDateFlag))
				{
					return new DateTime(2024, 10, 01);
				}

				if (SqlFormatInfo.TryParseFromSqlDateTime(startDateFlag, out DateTime startDate))
				{
					return startDate;
				}

				throw new InvalidOperationException("Failed to parse start date flag from ExtProperty.");
			}

			int ProcessBatch(DateTime startDate, DateTime endDate)
			{
				var sql = @"
DECLARE @CreateTimeTable Table(CreateTime DATETIME);

WITH CTE AS
(
	SELECT
		EM_MessageSubType, 
		EM_SystemLastEditTimeUtc, 
		EM_SystemLastEditUser, 
		EM_SystemCreateTimeUtc
	FROM 
		dbo.EDIMessage 
	WHERE
		EM_ApplicationCode = 'CAI' AND 
		EM_MessageType = 'CAD' AND 
		EM_ReceiveTransmit = 'RCV' AND 
		EM_Status = 'RCV' AND
		EM_MessageSubType = '' AND
		EM_MessageText LIKE '%<NameCode>39</NameCode>%' AND
		EM_SystemCreateTimeUtc > @StartDate AND
		EM_SystemCreateTimeUtc <= @EndDate
)

UPDATE
	CTE
SET
	EM_MessageSubType = 'CLO',
	EM_SystemLastEditTimeUtc = GETUTCDATE(),
	EM_SystemLastEditUser = '~BP'

SELECT @@ROWCOUNT";

				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddParameter("@StartDate", SqlDbType.DateTime, startDate);
					cmd.AddParameter("@EndDate", SqlDbType.DateTime, endDate);

					return (int)cmd.ExecuteScalar();
				}
			}

			void SaveProgress(DateTime endDate, ref Stopwatch stopWatch, int totalUpdatedCount, CancellationToken token)
			{
				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					var dateString = SqlFormatInfo.ToSqlDateTimeString(endDate);
					ExtProperty.Database.Update(Db.Connection, StartDateFlag, dateString);
					manager?.ShowInfoMessage($"Processed up to {endDate:yyyy-MM-dd}, total updated: {totalUpdatedCount}.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			void FinalizeProcess(int totalUpdatedCount)
			{
				manager?.ShowInfoMessage($"A total of {totalUpdatedCount} messages with status code 39 had their subType updated to CLO.");
				ExtProperty.Database.Delete(Db.Connection, StartDateFlag);
			}
		}
	}
}
