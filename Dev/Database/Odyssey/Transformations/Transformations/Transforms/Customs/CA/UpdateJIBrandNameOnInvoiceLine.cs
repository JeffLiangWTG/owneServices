using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateJIBrandNameOnInvoiceLine : DataTransformation
	{
		public override string UserDescription => "Update JI_BrandName On CA InvoiceLine";

		public UpdateJIBrandNameOnInvoiceLine()
			: this(5000)
		{
		}

		internal UpdateJIBrandNameOnInvoiceLine(int batchSize)
		{
			this.batchSize = batchSize;
		}
		readonly int batchSize;
		internal const string LastClusterKeyWaterMark = "UpdateJI_BrandNameOnInvoiceLine_LastClusterKey";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var totalUpdatedInvoiceLineNum = 0;
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, LastClusterKeyWaterMark), out var lastClusterKey))
				{
					lastClusterKey = GetMaxClusterKey();
				}

				while (lastClusterKey > 0)
				{
					manager?.ShowInfoMessage($"Batch start, cluster key end with [{lastClusterKey}].");
					var updatedLinesNum = ProcessBatch(lastClusterKey);
					manager?.ShowInfoMessage($"Updated lines [{updatedLinesNum}].");

					totalUpdatedInvoiceLineNum += updatedLinesNum;

					lastClusterKey = Math.Max(lastClusterKey - batchSize, 0);
					if (lastClusterKey > 0)
					{
						ExtProperty.Database.Update(Db.Connection, LastClusterKeyWaterMark, lastClusterKey.ToString());
					}
					token.ThrowIfCancellationRequested();
				}

				manager?.ShowInfoMessage($"updated invoice line [{totalUpdatedInvoiceLineNum}].");

				ExtProperty.Database.Delete(Db.Connection, LastClusterKeyWaterMark);
			}
		}

		int ProcessBatch(int lastClusterKey)
		{
			var sql =
"""
UPDATE
	dbo.JobComInvoiceLine
SET
	JI_BrandName = AddInfo.Value,
	JI_AddInfo = AddInfo.AddInfoValue,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP',
	JI_AutoVersion = (JI_AutoVersion + 1) % 32768
FROM
	dbo.JobComInvoiceLine
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'BrandName') AS AddInfo
WHERE
	JI_DataModel = 'CA'
	AND JI_AddInfo LIKE '%BrandName=%'
	AND JI_ClusterKey BETWEEN (@endClusterKey - @batchSize + 1) AND @endClusterKey
""";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				return cmd.ExecuteNonQuery();
			}
		}

		int GetMaxClusterKey()
		{
			return Db.Connection.ExecuteScalar<int>(
"""
SELECT ISNULL(MAX(JI_ClusterKey), 0) maxClusterKey
FROM dbo.JobComInvoiceLine WHERE JI_DataModel = 'CA'
""");
		}
	}
}
