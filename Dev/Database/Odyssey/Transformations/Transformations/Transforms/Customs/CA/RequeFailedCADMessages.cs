using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA
{
	public class RequeFailedCADMessages : DataTransformation
	{
		public RequeFailedCADMessages()
			: this(5000)
		{
		}

		internal RequeFailedCADMessages(int batchSize)
		{
			this.batchSize = batchSize;
		}

		public override string UserDescription => "Requeue Failed CAD Messages";

		readonly int batchSize;
		internal const string MinClusterKeyWaterMark = "RequeFailedCADMessages_MinClusterKey";
		internal const string MaxClusterKeyWaterMark = "RequeFailedCADMessages_MaxClusterKey";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var (minClusterKey, maxClusterKey) = (0, 0);
			if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, MaxClusterKeyWaterMark), out maxClusterKey))
			{
				(minClusterKey, maxClusterKey) = GetClusterKeyRange();
				ExtProperty.Database.Update(Db.Connection, MinClusterKeyWaterMark, minClusterKey.ToString());
				ExtProperty.Database.Update(Db.Connection, MaxClusterKeyWaterMark, maxClusterKey.ToString());
			}
			else
			{
				int.TryParse(ExtProperty.Database.Select(Db.Connection, MinClusterKeyWaterMark), out minClusterKey);
			}

			while (maxClusterKey >= minClusterKey)
			{
				var startClusterKey = maxClusterKey - batchSize + 1;
				startClusterKey = startClusterKey > minClusterKey ? startClusterKey : minClusterKey;
				manager?.ShowInfoMessage($"Batch processing start from cluster key [{startClusterKey}] to [{maxClusterKey}].");

				using (var cmd = Db.Connection.Command(UpdateSQL))
				{
					cmd.AddParameter("@startClusterKey", SqlDbType.Int, startClusterKey);
					cmd.AddParameter("@endClusterKey", SqlDbType.Int, maxClusterKey);
					cmd.ExecuteNonQuery();
				}

				maxClusterKey = startClusterKey - 1;
				ExtProperty.Database.Update(Db.Connection, MaxClusterKeyWaterMark, maxClusterKey.ToString());
				manager?.ShowInfoMessage($"Max cluster key updated to [{maxClusterKey}].");

				token.ThrowIfCancellationRequested();
			}

			ExtProperty.Database.Delete(Db.Connection, MinClusterKeyWaterMark);
			ExtProperty.Database.Delete(Db.Connection, MaxClusterKeyWaterMark);
		}

		static (int, int) GetClusterKeyRange()
		{
			var (minClusterKey, maxClusterKey) = (0, 0);
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				var sql = @"
SELECT	ISNULL(MIN(CH_ClusterKey), 0) AS MinClusterKey
	,	ISNULL(MAX(CH_ClusterKey), 0) AS MaxClusterKey
FROM	dbo.CusEntryHeader
INNER JOIN dbo.JobDeclaration ON JE_ClusterKey = CH_ClusterKey
WHERE	CH_MessageType = 'CAD'
AND		JE_DataModel = 'CA'
AND		JE_MessageType IN ('IMP', 'LVS')";

				using (var cmd = Db.Connection.Command(sql))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						minClusterKey = reader.GetInt32(0);
						maxClusterKey = reader.GetInt32(1);
					}
				}
			}
			return (minClusterKey, maxClusterKey);
		}

		const string UpdateSQL = @"
UPDATE	dbo.EDIMessage
SET		EM_Status = 'QUE'
, EM_SystemLastEditTimeUtc = GETUTCDATE()
, EM_SystemLastEditUser = '~BP'
WHERE	EM_PK IN (
	SELECT	EM_PK
	FROM	(
		SELECT	ROW_NUMBER() OVER(PARTITION BY CH_PK ORDER BY EM_SystemCreateTimeUtc DESC) AS RN
		, EM_PK
		, EM_ReceiveTransmit
		, EM_ApplicationCode
		, EM_MessageType
		, EM_Status
		FROM	dbo.EDIMessage
		INNER JOIN dbo.CusEntryHeader ON CH_PK = EM_LinkUniqueID
		WHERE	CH_DataModel = 'CA'
		AND		CH_MessageType = 'CAD'
		AND		CH_ClusterKey BETWEEN @startClusterKey AND @endClusterKey
	) AS CAMessages
	WHERE	RN = 1
	AND		EM_ReceiveTransmit = 'RCV'
	AND		EM_ApplicationCode = 'CAI'
	AND		EM_MessageType = 'CAD'
	AND		EM_Status = 'FAL'
)
";
	}
}
