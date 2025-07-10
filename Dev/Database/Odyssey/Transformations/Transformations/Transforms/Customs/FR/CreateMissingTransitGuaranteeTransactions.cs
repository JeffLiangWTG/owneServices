using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR
{
	sealed class CreateMissingTransitGuaranteeTransactions : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Create missing transit guarantee transactions.";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(EDIMessageSchema.Instance).Key(EDIMessageSchema.Constants.EM_LinkTable, EDIMessageSchema.Constants.EM_ApplicationCode, EDIMessageSchema.Constants.EM_ReceiveTransmit, EDIMessageSchema.Constants.EM_Status, EDIMessageSchema.Constants.EM_MessageType, EDIMessageSchema.Constants.EM_MessageSubType)
					.Where(@"[EM_LinkTable]='CusInBondHeader' AND [EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND [EM_MessageType]='TP5' AND [EM_MessageSubType]='029'")
					.Include(EDIMessageSchema.Constants.EM_LinkUniqueID, EDIMessageSchema.Constants.EM_MessageNum).GetInfo();

				indexProvider.New(CusPermitHeaderSchema.Instance).Key(CusPermitHeaderSchema.Constants.PK, CusPermitHeaderSchema.Constants.CPH_StartDate)
					.Where(@"[CPH_ApplicationCode]='GUA' AND [CPH_RN_NKCountryCode]='FR' AND [CPH_Type]='COD'")
					.Include(CusPermitHeaderSchema.Constants.CPH_Number, CusPermitHeaderSchema.Constants.CPH_OH_PermitHolder, CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode, CusPermitHeaderSchema.Constants.CPH_SubType).GetInfo();

				indexProvider.New(CusBondDetailSchema.Instance).Key(CusBondDetailSchema.Constants.PW_BondNumber, CusBondDetailSchema.Constants.PW_BondType, CusBondDetailSchema.Constants.PW_ParentID)
					.Include(CusBondDetailSchema.Constants.PW_ApplicationCode, CusBondDetailSchema.Constants.PW_BondAmount).GetInfo();

				indexProvider.New(CusInBondHeaderSchema.Instance).Key(CusInBondHeaderSchema.Constants.PK)
					.Include(CusInBondHeaderSchema.Constants.BH_ApplicationCode, CusInBondHeaderSchema.Constants.BH_GB).GetInfo();

				indexProvider.New(CusInBondMoveHeaderSchema.Instance).Key(CusInBondMoveHeaderSchema.Constants.BM_BH, CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus)
					.Include(CusInBondMoveHeaderSchema.Constants.BM_PaperlessInbondNum).GetInfo();
				return indexProvider;
			}
		}

		const string sql = @"
IF EXISTS(SELECT NULL FROM dbo.GLBCOMPANY WHERE GC_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL'))
BEGIN
	WITH TemporaryCusPermitHeader AS
	(
		SELECT CPH_PK, CPH_Number, CPH_OH_PermitHolder, CPH_SubType FROM
		(
			SELECT CPH_PK, CPH_Number, CPH_OH_PermitHolder, CPH_SubType, ROW_NUMBER() OVER (PARTITION BY CPH_Number, CPH_OH_PermitHolder, CPH_RN_NKCountryCode ORDER BY CPH_StartDate DESC) as RN
			FROM CusPermitHeader
			WHERE CPH_ApplicationCode = 'GUA'
			AND CPH_RN_NKCountryCode = 'FR'
			AND CPH_Type = 'COD'
		) CPH
		WHERE CPH.RN = 1
	),
	TemporaryCusBondDetail AS
	(
		SELECT PW_PK, PW_ParentID, PW_ApplicationCode, PW_BondAmount, CPH_PK as RelatedGuaranteePK
		FROM CusBondDetail
		INNER JOIN TemporaryCusPermitHeader ON CPH_Number = PW_BondNumber AND CPH_SubType = PW_BondType
		INNER JOIN CusInBondMoveHeader ON BM_PK = PW_ParentID
		INNER JOIN dbo.JobDocAddress ON E2_ParentTableCode = 'BH' AND E2_ParentID = BM_BH AND E2_AddressType = 'PRC'
		INNER JOIN OrgAddress ON OA_PK = E2_OA_Address AND OA_OH = CPH_OH_PermitHolder
	),
	ExistingNctsGuaranteeRecords AS 
	(
		SELECT CPL_Reference
		FROM TemporaryCusBondDetail 
		INNER JOIN dbo.CusPermitHeader ON CPH_PK = RelatedGuaranteePK
		INNER JOIN dbo.CusPermitLineTransaction ON CPL_CPH_PermitHeader = CPH_PK
		WHERE PW_BondAmount > 0
		AND PW_ApplicationCode = 'NCT'
		AND CPH_ApplicationCode = 'GUA'
		AND CPH_RN_NKCountryCode = 'FR'
		AND CPH_Type = 'COD'
	),
	TemporaryEDIMessage AS
	(
		SELECT EM_MessageNum, EM_LinkTable, EM_LinkUniqueID
		FROM (
			SELECT EM_MessageNum, EM_LinkTable, EM_LinkUniqueID, ROW_NUMBER() OVER (PARTITION BY EM_LinkUniqueID, EM_MessageSubType ORDER BY EM_SystemCreateTimeUtc) AS RN
			FROM EDIMessage
			WHERE EM_ReceiveTransmit = 'RCV' 
			AND EM_MessageSubType = '029' 
			AND EM_Status = 'PRS' 
			AND EM_ApplicationCode = 'FRC' 
			AND EM_MessageType = 'TP5'
		) EM
		WHERE EM.RN = 1
	)

	INSERT INTO dbo.CusPermitLineTransaction
		(CPL_PK, CPL_CPH_PermitHeader, CPL_AppId, CPL_TransactionType, CPL_TransactionDate, CPL_Reference, CPL_TranValue, CPL_Comment, CPL_TransactionStatus, CPL_TransactionCategory, CPL_IsAggregated, CPL_SystemCreateTimeUtc, CPL_SystemCreateUser, CPL_SystemLastEditTimeUtc, CPL_SystemLastEditUser)
	SELECT
		NEWID(), RelatedGuaranteePK, EM_MessageNum, 'TRA', CE_IssueDate, BM_PaperlessInbondNum,  -PW_BondAmount, 'NCTS Departure ' + BM_PaperlessInbondNum, 'CON', 'CUM', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'
	FROM
		((dbo.CusInBondHeader
		INNER JOIN dbo.GlbBranch ON BH_GB = GB_PK AND GB_RN_NKCountryCode IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL') AND BH_ApplicationCode = 'NC5')
		INNER JOIN dbo.CusInBondMoveHeader ON BM_BH = BH_PK AND BM_CustomsStatus = 'REL') 
		INNER JOIN TemporaryEDIMessage ON EM_LinkTable = 'CusInBondHeader' AND EM_LinkUniqueID = BH_PK
		INNER JOIN dbo.CusEntryNum ON CE_ParentTable = 'CusInBondHeader' AND CE_ParentID = BH_PK AND CE_EntryType = 'MRN'
		INNER JOIN TemporaryCusBondDetail ON PW_ParentID = BM_PK AND PW_ApplicationCode = 'NCT' 
	WHERE 
	PW_BondAmount > 0
	AND BM_PaperlessInbondNum NOT IN 
	(
		SELECT distinct CPL_Reference
		FROM ExistingNctsGuaranteeRecords
	)
END";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
