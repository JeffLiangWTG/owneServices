using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	public class UpdateZG_ConfirmedGuaranteeAmountFromDeltaGMessage : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update ZG_ConfirmedGuaranteeAmount From DeltaG Message.";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(EDIMessageSchema.Instance).Key(EDIMessageSchema.Constants.EM_EI, EDIMessageSchema.Constants.EM_LinkUniqueID).Where(@"[EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND ([EM_MessageType] IN ('IMC', 'IMD', 'EXC', 'EXD'))").GetInfo();
				indexProvider.New(GenAddOnColumnSchema.Instance).Key(GenAddOnColumnSchema.Constants.XA_ParentID).Where(@"[XA_ParentTableCode]='JE' AND [XA_Name]='JE_DeltaMode' AND [XA_Data]='G1'").GetInfo();
				indexProvider.New(GenAddOnColumnSchema.Instance).Key(GenAddOnColumnSchema.Constants.XA_ParentID).Where(@"[XA_ParentTableCode]='JE' AND [XA_Name]='JE_DeltaMode' AND [XA_Data]='G2'").GetInfo();
				indexProvider.New(CusEntryHeaderSchema.Instance).Key(CusEntryHeaderSchema.Constants.PK, CusEntryHeaderSchema.Constants.CH_JE).Where(@"[CH_DataModel]='FR' AND [CH_EntryStatus]>='050'").GetInfo();
				indexProvider.New(CusEntryHeaderSchema.Instance).Key(CusEntryHeaderSchema.Constants.PK, CusEntryHeaderSchema.Constants.CH_JE).Where(@"[CH_DataModel]='FR' AND [CH_EntryStatus]>='130'").GetInfo();
				return indexProvider;
			}
		}

		const string sql = @"
IF EXISTS (SELECT NULL FROM GlbCompany WHERE GC_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')) BEGIN

BEGIN TRY

WITH #TempTableProcessedCusEntryHeader_G1 (EntryHeaderPK, ConfirmedGuaranteeAmount, RowNumber)
AS
(
	SELECT
	CH_PK,
	GuaranteeAmount,
	ROW_NUMBER() OVER (PARTITION BY CH_PK ORDER BY EM_SystemCreateTimeUtc DESC) AS RowNumber
	FROM (
		SELECT 
			CH_PK,
			EM_SystemCreateTimeUtc,
			ISNULL(XMLData.value('(/Message/ReponseDeclaration/ReponseDatas/Notification/LiquidationGen/montantcautionnable)[1]', 'NVARCHAR(19)'), '') AS GuaranteeAmount
		FROM (
				SELECT CH_PK,
				TRY_CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
				EM_SystemCreateTimeUtc
				FROM 
				dbo.EDIMessage JOIN dbo.EDIInterchange ON EM_EI=EI_PK
				INNER JOIN dbo.CusEntryHeader ON EM_LinkUniqueID=CH_PK 
				INNER JOIN dbo.GenAddOnColumn ON XA_ParentID = CH_JE
				WHERE EM_ApplicationCode = 'FRC' 
				AND EM_MessageType IN ('IMC', 'IMD', 'EXC', 'EXD') 
				AND EM_ReceiveTransmit = 'RCV' 
				AND EM_Status = 'PRS'
				AND CH_DataModel = 'FR' 
				AND CH_EntryStatus >= '050'
				AND (EI_InterchangeNum LIKE '%.ANTICIPE.%' OR
					EI_InterchangeNum LIKE '%.ANT.%' OR
					EI_InterchangeNum LIKE '%.EAV.%' OR
					EI_InterchangeNum LIKE '%.VALIDE.%' OR
					EI_InterchangeNum LIKE '%.VAL.%' OR
					EI_InterchangeNum LIKE '%.VLD.%')
				AND XA_ParentTableCode = 'JE'
				AND XA_Name = 'JE_DeltaMode'
				AND XA_Data = 'G1'
			) T1
		) T2
	WHERE GuaranteeAmount <> ''
)
UPDATE CusEntryHeader
SET CH_SystemLastEditTimeUtc = GETUTCDATE(),
	CH_SystemLastEditUser = '~BP',
	CH_AddInfo = (CASE CH_AddInfo WHEN '' THEN '' ELSE CH_AddInfo + '*' END) + 'ConfirmedGuaranteeAmount=' + ConfirmedGuaranteeAmount
FROM CusEntryHeader JOIN #TempTableProcessedCusEntryHeader_G1 ON CH_PK=EntryHeaderPK AND RowNumber=1;

WITH #TempTableProcessedCusEntryHeader_G2 (EntryHeaderPK, ConfirmedGuaranteeAmount, RowNumber)
AS
(
	SELECT
	CH_PK,
	GuaranteeAmount,
	ROW_NUMBER() OVER (PARTITION BY CH_PK ORDER BY EM_SystemCreateTimeUtc DESC) AS RowNumber
	FROM (
		SELECT 
			CH_PK,
			EM_SystemCreateTimeUtc,
			ISNULL(XMLData.value('(/Message/ReponseDeclaration/ReponseDatas/Notification/LiquidationGen/montantcautionnable)[1]', 'NVARCHAR(19)'), '') AS GuaranteeAmount
		FROM (
				SELECT CH_PK,
				TRY_CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
				EM_SystemCreateTimeUtc
				FROM 
				dbo.EDIMessage JOIN dbo.EDIInterchange ON EM_EI=EI_PK
				INNER JOIN dbo.CusEntryHeader ON EM_LinkUniqueID=CH_PK 
				INNER JOIN dbo.GenAddOnColumn ON XA_ParentID = CH_JE
				WHERE EM_ApplicationCode = 'FRC' 
				AND EM_MessageType IN ('IMC', 'IMD', 'EXC', 'EXD') 
				AND EM_ReceiveTransmit = 'RCV' 
				AND EM_Status = 'PRS'
				AND CH_DataModel = 'FR' 
				AND CH_EntryStatus >= '130' 
				AND EI_InterchangeNum LIKE '%.BAE COMPLETE.%'
				AND XA_ParentTableCode = 'JE'
				AND XA_Name = 'JE_DeltaMode'
				AND XA_Data = 'G2'
			) T1
		) T2
	WHERE GuaranteeAmount <> ''
)
UPDATE CusEntryHeader
SET CH_SystemLastEditTimeUtc = GETUTCDATE(),
	CH_SystemLastEditUser = '~BP',
	CH_AddInfo = (CASE CH_AddInfo WHEN '' THEN '' ELSE CH_AddInfo + '*' END) + 'ConfirmedGuaranteeAmount=' + ConfirmedGuaranteeAmount
FROM CusEntryHeader JOIN #TempTableProcessedCusEntryHeader_G2 ON CH_PK=EntryHeaderPK AND RowNumber=1

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
