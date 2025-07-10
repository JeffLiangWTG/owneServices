using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class UpdateCusEntryHeaderStatusAndEntryStatusAccordingToStmALog : DataTransformation, ITransformationIndexProvider
	{
		public UpdateCusEntryHeaderStatusAndEntryStatusAccordingToStmALog()
		{
		}

		public override string UserDescription => "Update Status And Entry Status Of CAD CusEntryHeader According To StmALog.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				using (DataTransformationHelper.SuspendTriggerIfExists("TG_CusEntryHeader_UpdateAutoVersion", CusEntryHeaderSchema.Constants.TableName))
				{
					var script = @"
DECLARE @PKAndReference TABLE(PK UNIQUEIDENTIFIER, ClusterKey INT, Reference VARCHAR(3), RowNumber INT)

INSERT INTO @PKAndReference
SELECT 
	CH_PK, CH_ClusterKey, SL_Reference, ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PostedTimeUtc DESC) RowNumber
FROM
	dbo.CusEntryHeader
INNER JOIN dbo.StmALog ON SL_Parent = CH_PK AND SL_Table = 'CusEntryHeader'
WHERE SL_SE_NKEvent = 'CES' AND 
	SL_Reference IN ('39', '41', '200') AND 
	SL_PostedTimeUtc >= '2024-10-01' AND 
	CH_DataModel = 'CA' AND 
	CH_MessageType = 'CAD' AND
	CH_EntryStatus = '200'

UPDATE 
	dbo.CusEntryHeader
SET 
	CH_Status = Status, 
	CH_EntryStatus = EntryStatus, 
	CH_SystemLastEditUser = '~BP', 
	CH_SystemLastEditTimeUtc = GETUTCDATE(),
	CH_AutoVersion = (CH_AutoVersion + 1) % 32768
FROM 
	dbo.CusEntryHeader
INNER JOIN 
(
	SELECT 
		row2.PK, 
		row2.ClusterKey,
		CASE 
			WHEN row2.Reference = '39' THEN 'CLO' 
			WHEN row2.Reference = '41' THEN 'ERO' 
		END Status,
		row2.Reference EntryStatus
	FROM 
		@PKAndReference row1
	INNER JOIN @PKAndReference row2 ON row1.PK = row2.PK AND row1.ClusterKey = row2.ClusterKey
	WHERE 
		row1.rowNumber = 1 AND 
		row1.Reference = '200' AND 
		row2.rowNumber = 2 AND 
		row2.Reference in ('39', '41')
) A ON PK = CH_PK AND ClusterKey = CH_ClusterKey
";
					Db.Connection.ExecuteNonQuery(script);
				}
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryHeaderSchema.Instance)
				.Key(
					CusEntryHeaderSchema.Constants.CH_DataModel,
					CusEntryHeaderSchema.Constants.CH_MessageType,
					CusEntryHeaderSchema.Constants.CH_EntryStatus)
				.Include(CusEntryHeaderSchema.Constants.PK)
				.Where("[CH_DataModel]='CA' AND [CH_MessageType]='CAD' AND [CH_EntryStatus]='200'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
