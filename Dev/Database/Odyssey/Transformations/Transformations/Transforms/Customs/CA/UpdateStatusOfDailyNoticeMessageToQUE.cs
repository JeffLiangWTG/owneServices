using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class UpdateStatusOfDailyNoticeMessageToQUE : DataTransformation
	{
		public UpdateStatusOfDailyNoticeMessageToQUE()
			: this(5000)
		{
		}

		internal UpdateStatusOfDailyNoticeMessageToQUE(int batchSize)
		{
			this.batchSize = batchSize;
		}
		readonly int batchSize;
		const string MaxCreateTimeUtcFlag = "UpdateStatusOfDailyNoticeMessageToQUE_CreateTimeUtc";

		public override string UserDescription => "Update The Status Of Daily Notice Message To QUE.";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				if (!DateTime.TryParse(ExtProperty.Database.Select(Db.Connection, MaxCreateTimeUtcFlag), out var maxCreateTimeUtc))
				{
					#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
					maxCreateTimeUtc = DateTime.UtcNow;
					#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
				}

				var totalUpdatedCount = 0;
				var updatedCount = 0;
				do
				{
					var sql = $@"
DECLARE @CreateTimeTable Table(CreateTime DATETIME);

WITH CTE AS
(
	SELECT TOP {batchSize}
		EM_Status, 
		EM_SystemLastEditTimeUtc, 
		EM_SystemLastEditUser, 
		EM_SystemCreateTimeUtc
	FROM 
		dbo.EDIMessage 
	WHERE
		EM_ApplicationCode = 'CAC' AND 
		EM_MessageType = 'DN' AND 
		EM_ReceiveTransmit = 'RCV' AND 
		EM_Status = 'RCV' AND 
		EM_SystemCreateTimeUtc >= '2024-10-21' AND
		EM_SystemCreateTimeUtc <= @MaxCreateTimeUtc
	ORDER BY EM_SystemCreateTimeUtc DESC
)

UPDATE
	CTE
SET
	EM_Status = 'QUE',
	EM_SystemLastEditTimeUtc = GETUTCDATE(),
	EM_SystemLastEditUser = '~BP'
OUTPUT
	deleted.EM_SystemCreateTimeUtc
INTO
	@CreateTimeTable

SELECT @@ROWCOUNT, MIN(CreateTime) FROM @CreateTimeTable";

					using (var cmd = Db.Connection.Command(sql))
					{
						cmd.AddParameter("@MaxCreateTimeUtc", SqlDbType.DateTime, maxCreateTimeUtc);
						using (var reader = cmd.ExecuteReader())
						{
							reader.Read();
							updatedCount = (int)reader[0];
							if (updatedCount != 0)
							{
								maxCreateTimeUtc = (DateTime)reader[1];
							}
						}
					}

					totalUpdatedCount += updatedCount;
					ExtProperty.Database.Update(Db.Connection, MaxCreateTimeUtcFlag, maxCreateTimeUtc.ToString("yyyy-MM-dd HH:mm:ss.fff"));
					token.ThrowIfCancellationRequested();
				}
				while (updatedCount == batchSize);

				manager?.ShowInfoMessage($"A total of {totalUpdatedCount} daily notice messages were updated to QUE status.");
				ExtProperty.Database.Delete(Db.Connection, MaxCreateTimeUtcFlag);
			}
		}
	}
}
