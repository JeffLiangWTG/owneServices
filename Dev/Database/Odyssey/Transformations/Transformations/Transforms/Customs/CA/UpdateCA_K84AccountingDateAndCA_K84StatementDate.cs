using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateCA_K84AccountingDateAndCA_K84StatementDate : DataTransformation
	{
		public override string UserDescription => "Online Update CA_K84AccountingDate And CA_K84StatementDate On IMP/LVS Declaration";

		public UpdateCA_K84AccountingDateAndCA_K84StatementDate()
			: this(100000)
		{
		}

		internal UpdateCA_K84AccountingDateAndCA_K84StatementDate(int batchSize)
		{
			this.batchSize = batchSize;
		}
		readonly int batchSize;
		internal const string LastClusterKeyWaterMark = "UpdateCA_K84AccountingDateAndCA_K84StatementDate_LastClusterKey";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var updatedDeclarationNum = 0;
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, LastClusterKeyWaterMark), out var lastClusterKey))
				{
					lastClusterKey = GetMaxClusterKey();
				}

				var stopWatch = Stopwatch.StartNew();
				while (lastClusterKey > 0)
				{
					var startClusterKey = GetStartClusterKey(lastClusterKey);
					var result = ProcessBatch(startClusterKey, lastClusterKey);
					updatedDeclarationNum += result;
					lastClusterKey = Math.Max(startClusterKey - 1, 0);

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						if (lastClusterKey > 0)
						{
							ExtProperty.Database.Update(Db.Connection, LastClusterKeyWaterMark, lastClusterKey.ToString());
						}
						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}

				manager?.ShowInfoMessage($"updated declarations [{updatedDeclarationNum}].");

				ExtProperty.Database.Delete(Db.Connection, LastClusterKeyWaterMark);

				var totalDeleteGenAddonColumnCount = 0;
				int deleteGenAddonColumnCount;
				do
				{
					deleteGenAddonColumnCount = Db.Connection.ExecuteNonQuery(DeleteGenAddonColumnQuery);
					totalDeleteGenAddonColumnCount += deleteGenAddonColumnCount;
					token.ThrowIfCancellationRequested();
				}
				while (deleteGenAddonColumnCount > 0);

				manager?.ShowInfoMessage($"Deleted GenAddonColumn(CA_K84AccountingDate) [{totalDeleteGenAddonColumnCount}].");
			}
		}

		int ProcessBatch(int startClusterKey, int lastClusterKey)
		{
			var sql =
"""
CREATE TABLE #DeclarationNeedsUpdate (
	ClusterKey INT NOT NULL,
	EntryReleaseDate SMALLDATETIME NOT NULL,
	K84AccountingDate SMALLDATETIME NULL,
	K84StatementDate SMALLDATETIME NULL
);

INSERT INTO #DeclarationNeedsUpdate(ClusterKey, EntryReleaseDate, K84AccountingDate, K84StatementDate)
SELECT 
    JE_ClusterKey, CH_EntryReleaseDate, JE_K84AccountingDate, JE_K84StatementDate 
FROM 
    dbo.CAJobDeclaration
JOIN dbo.CusEntryHeader ON CH_ClusterKey = JE_ClusterKey
JOIN dbo.CusEntryLine ON CL_CH = CH_PK AND CL_ClusterKey = CH_ClusterKey
LEFT JOIN dbo.CusEntryLineFee ON CF_CL = CL_PK AND CF_ClusterKey = CL_ClusterKey AND CF_Source = 'CUS' AND CF_ChargeType = 'TOT'
WHERE 
    JE_MessageType IN ('IMP', 'LVS') 
    AND JE_IsCancelled = 0
	AND JE_DataModel = 'CA'
    AND (JE_K84AccountingDate IS NULL OR JE_K84StatementDate IS NULL)
	AND JE_ClusterKey BETWEEN @startClusterKey AND @endClusterKey
    AND CH_MessageType = 'CAD' 
    AND CH_EntryReleaseDate IS NOT NULL
GROUP BY 
    JE_ClusterKey, CH_PK, CH_EntryReleaseDate, JE_K84AccountingDate, JE_K84StatementDate
HAVING SUM(ISNULL(CF_ChargeAmount, 0)) = 0;

UPDATE 
	dbo.JobDeclaration
SET JE_AddInfo = CONCAT(JE_AddInfo,
	IIF(K84AccountingDate IS NULL, CONCAT('*K84AccountingDate=', CONVERT(VARCHAR, EntryReleaseDate, 121)), ''),
	IIF(K84StatementDate IS NULL, CONCAT('*K84StatementDate=', CONVERT(VARCHAR, EntryReleaseDate, 121)), '')
	),
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP',
	JE_AutoVersion = (JE_AutoVersion + 1) % 32768
FROM
	dbo.JobDeclaration
	JOIN #DeclarationNeedsUpdate ON JE_ClusterKey = ClusterKey
SELECT @@ROWCOUNT;
""";
			int updatedDeclarationNum;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@startClusterKey", SqlDbType.Int, startClusterKey);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					updatedDeclarationNum = (int)reader[0];
				}
			}
			return updatedDeclarationNum;
		}

		int GetMaxClusterKey()
		{
			return Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(JE_ClusterKey), 0) maxClusterKey
FROM dbo.JobDeclaration WHERE JE_DataModel = 'CA'");
		}

		int GetStartClusterKey(int lastClusterKey)
		{
			var sql = """
SELECT ISNULL(MIN(DeclarationClusterKey.JE_ClusterKey), 0)
FROM
(
	SELECT TOP (@batchSize) JE_ClusterKey
	FROM dbo.JobDeclaration
	JOIN dbo.CusEntryHeader ON CH_ClusterKey = JE_ClusterKey
	JOIN dbo.CusEntryLine ON CL_CH = CH_PK AND CL_ClusterKey = CH_ClusterKey
	WHERE CH_MessageType = 'CAD'  AND JE_ClusterKey <= @endClusterKey
	ORDER BY JE_ClusterKey DESC) DeclarationClusterKey;
""";

			int startClusterKey;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				startClusterKey = (int)cmd.ExecuteScalar();
			}
			return startClusterKey;
		}

		string DeleteGenAddonColumnQuery =>
@"
DELETE TOP(1000)
	dbo.GenAddOnColumn
WHERE
	XA_Name = 'CA_K84AccountingDate'
	AND XA_ParentTableCode = 'JE'
OPTION (MAXDOP 1)
";
	}
}
