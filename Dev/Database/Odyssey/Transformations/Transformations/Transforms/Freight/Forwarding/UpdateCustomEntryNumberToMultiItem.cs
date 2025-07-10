using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class UpdateCustomEntryNumberToMultiItem : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Custom Entry Number To Multi Item";

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobDocsAndCartageSchema.Instance)
					.Key(JobDocsAndCartageSchema.Constants.JP_ParentID)
					.Where($"[{JobDocsAndCartageSchema.Constants.JP_DeliveryCartageCompleted}] IS NULL AND [{JobDocsAndCartageSchema.Constants.JP_ParentTableCode}]='JS'")
					.GetInfo();
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DROP TABLE IF EXISTS #SplitEntries;

WITH EligibleShipments AS (
	SELECT JS_PK
	FROM dbo.JobShipment
	LEFT JOIN dbo.JobDocsAndCartage ON JS_PK = JP_ParentID AND JP_ParentTableCode = 'JS'
	WHERE JP_DeliveryCartageCompleted IS NULL
)
SELECT
	CE_PK AS OriginalCE_PK,
    CE_ParentID,
    CE_ParentTable,
    splitNum.value AS CE_EntryNum,
    CE_EntryType,
    CE_EntryLineReference,
    CE_EntryStatus,
    CE_Category,
    CE_RN_NKCountryCode,
    CE_EntryIsSystemGenerated,
    CE_IsValid,
	ROW_NUMBER() OVER (PARTITION BY CE_PK ORDER BY VALUE) AS splitIndex
INTO #SplitEntries
FROM dbo.CusEntryNum
JOIN EligibleShipments ON CE_ParentID = JS_PK
CROSS APPLY STRING_SPLIT(CE_EntryNum, ',') AS splitNum
WHERE CE_Category = 'CUS'
    AND CHARINDEX(',', CE_EntryNum) > 0
	AND TRIM(splitNum.VALUE) <> ''

UPDATE dbo.CusEntryNum
SET 
	CE_EntryNum = SE.CE_EntryNum,
	CE_IssueDate = NULL,
	CE_ExpiryDate = NULL,
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = 'E'
FROM #SplitEntries SE
WHERE CE_PK = OriginalCE_PK
	AND SE.splitIndex = 1;

INSERT INTO dbo.CusEntryNum (
    CE_PK,
    CE_ParentID,
    CE_ParentTable,
    CE_EntryNum,
    CE_EntryType,
    CE_EntryLineReference,
    CE_EntryStatus,
    CE_Category,
    CE_IssueDate,
    CE_RN_NKCountryCode,
    CE_ExpiryDate,
    CE_EntryIsSystemGenerated,
    CE_IsValid,
    CE_SystemCreateTimeUtc,
    CE_SystemCreateUser,
    CE_SystemLastEditTimeUtc,
    CE_SystemLastEditUser
)
SELECT
	NEWID() AS CE_PK,
    CE_ParentID,
    CE_ParentTable,
    CE_EntryNum,
    CE_EntryType,
    CE_EntryLineReference,
    CE_EntryStatus,
    CE_Category,
    NULL AS CE_IssueDate,
    CE_RN_NKCountryCode,
    NULL AS CE_ExpiryDate,
    CE_EntryIsSystemGenerated,
    CE_IsValid,
    GETUTCDATE() AS CE_SystemCreateTimeUtc,
    'E' AS CE_SystemCreateUser,
    GETUTCDATE() AS CE_SystemLastEditTimeUtc,
    'E' AS CE_SystemLastEditUser
FROM #SplitEntries
WHERE splitIndex > 1;

DROP TABLE #SplitEntries;
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
