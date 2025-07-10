using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	internal class PopulateSourceForCusEntryLineFee : DataTransformation
	{
		readonly int batchSize;
		const string ExtPropertyLastProcessedKey = "PopulateSourceForCusEntryLineFee_LastProcessedKey";

		public PopulateSourceForCusEntryLineFee()
			: this(5000)
		{
		}

		internal PopulateSourceForCusEntryLineFee(int batchSize)
		{
			this.batchSize = batchSize;
		}

		public override string UserDescription => "Populate Source For Entry Line Fee";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (ExistCACompany)
			{
				if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, ExtPropertyLastProcessedKey), out var lastClusterKey))
				{
					lastClusterKey = QueryMaxClusterKey();
				}
				var stopWatch = Stopwatch.StartNew();
				var totalUpdatedNum = 0;
				var lastClusterKeyForLog = lastClusterKey;

				while (lastClusterKey >= 0)
				{
					var startClusterKey = Math.Max(0, lastClusterKey - batchSize);
					var updatedNum = ProcessBatch(startClusterKey, lastClusterKey);
					totalUpdatedNum += updatedNum;
					lastClusterKey = startClusterKey - 1;

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						ExtProperty.Database.Update(Db.Connection, ExtPropertyLastProcessedKey, lastClusterKey.ToString());
						ShowInfoMessage($"Processed batch key range: {startClusterKey}-{lastClusterKeyForLog}, {totalUpdatedNum} records updated.");
						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}

				ShowInfoMessage($"Processing completed, {totalUpdatedNum} records updated.");
				ExtProperty.Database.Delete(Db.Connection, ExtPropertyLastProcessedKey);
			}
		}

		int ProcessBatch(int startClusterKey, int endClusterKey)
		{
			var sql = $@"
UPDATE cf
SET CF_Source = 'CW1'
	, CF_SystemLastEditTimeUtc = GETUTCDATE()
	, CF_SystemLastEditUser = '~BP'
FROM dbo.CusEntryLineFee cf
WHERE CF_ClusterKey BETWEEN @startClusterKey AND @endClusterKey
    AND CF_Source IS NULL
    AND EXISTS (
        SELECT 1
        FROM dbo.CusEntryLine
        INNER JOIN dbo.CusEntryHeader
            ON CH_PK = CL_CH
            AND CH_ClusterKey = CL_ClusterKey
            AND CH_MessageType IN ('B3C', 'CAD')
            AND CH_DataModel = 'CA'
        WHERE CL_ClusterKey = cf.CF_ClusterKey
            AND CL_PK = cf.CF_CL
    )

SELECT @@ROWCOUNT
";
			return Db.Connection.ExecuteScalar<int>(sql, cmd =>
			{
				cmd.AddParameter("@startClusterKey", SqlDbType.Int, startClusterKey);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, endClusterKey);
			});
		}

		int QueryMaxClusterKey() => Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(CH_ClusterKey), 0) AS maxClusterKey
FROM dbo.CusEntryHeader
WHERE CH_MessageType IN ('B3C', 'CAD')
    AND CH_DataModel = 'CA';");

		bool ExistCACompany => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);
	}
}
