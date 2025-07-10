using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	public class UpdateBM_EntryDateFromTP5IE028Message : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update BM_EntryDate From TP5 028 Message.";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(EDIMessageSchema.Instance).Key(EDIMessageSchema.Constants.EM_ApplicationCode, EDIMessageSchema.Constants.EM_ReceiveTransmit, EDIMessageSchema.Constants.EM_Status, EDIMessageSchema.Constants.EM_MessageType, EDIMessageSchema.Constants.EM_MessageSubType)
					.Where(@"[EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND [EM_MessageType]='TP5' AND [EM_MessageSubType]='028'")
					.Include(EDIMessageSchema.Constants.EM_LinkUniqueID, EDIMessageSchema.Constants.EM_SystemCreateTimeUtc, EDIMessageSchema.Constants.EM_MessageData).GetInfo();
				indexProvider.New(CusInBondHeaderSchema.Instance).Key(CusInBondHeaderSchema.Constants.BH_HeaderType, CusInBondHeaderSchema.Constants.BH_ApplicationCode).Where(@"[BH_HeaderType]='D' AND [BH_ApplicationCode]='NC5'").GetInfo();
				return indexProvider;
			}
		}

		const string sql = @"
IF EXISTS (SELECT NULL FROM GlbCompany WHERE GC_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')) BEGIN

BEGIN TRY

WITH #TempTableProcessedCusInBondMoveHeader (MoveHeaderPK, EntryDate, RowNumber)
AS
(
	SELECT
	BM_PK,
	EntryDate,
	ROW_NUMBER() OVER (PARTITION BY BM_PK ORDER BY EM_SystemCreateTimeUtc DESC) AS RowNumber
	FROM (
		SELECT 
			BM_PK,
			EM_SystemCreateTimeUtc,
			ISNULL(XMLData.value('(/Message/MessageBody/*[local-name()=""CC028C""]/TransitOperation/declarationAcceptanceDate)[1]', 'NVARCHAR(50)'), '') AS EntryDate
		FROM (
				SELECT BM_PK,
				TRY_CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
				EM_SystemCreateTimeUtc
				FROM 
				dbo.EDIMessage
				INNER JOIN dbo.CusInBondHeader ON EM_LinkUniqueID = BH_PK 
				INNER JOIN CusInBondMoveHeader ON BM_BH = BH_PK
				WHERE EM_ApplicationCode = 'FRC' 
				AND EM_ReceiveTransmit = 'RCV' 
				AND EM_Status = 'PRS'
				AND EM_MessageType = 'TP5'
				AND EM_MessageSubType = '028'
				AND BH_HeaderType = 'D'
				AND BH_ApplicationCode = 'NC5'
				AND BM_EntryDate IS NULL
			) T1
		) T2
	WHERE EntryDate <> ''
)
UPDATE CusInBondMoveHeader
SET BM_SystemLastEditTimeUtc = GETUTCDATE(),
	BM_SystemLastEditUser = '~BP',
	BM_EntryDate = EntryDate
FROM CusInBondMoveHeader
JOIN #TempTableProcessedCusInBondMoveHeader ON BM_PK = MoveHeaderPK AND RowNumber=1;

END TRY
BEGIN CATCH  
	THROW
END CATCH

END
";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
