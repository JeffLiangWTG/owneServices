using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE
{
	sealed class InsertNctsPhase5DepartureRegisteredCusEntryNum : DataTransformation, ITransformationIndexProvider
	{
		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(EDIMessageSchema.Instance)
					.Key(EDIMessageSchema.Constants.EM_MessageType, EDIMessageSchema.Constants.EM_ApplicationCode, EDIMessageSchema.Constants.EM_Status)
					.Where("([EM_MessageType] IN ('028', '043')) AND [EM_ApplicationCode]='IEN' AND [EM_Status]='PRS'")
					.GetInfo();
				return indexProvider;
			}
		}

		public override string UserDescription => "Insert Registered CusEntryNum for IE NCTS Departure Reports";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
				INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_EntryLineReference, CE_IssueDate, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
					SELECT NEWID(), BH_PK, 'CusInBondHeader', BM_PaperlessInbondNum, 'CUS', 'REG', 'TD-' + OH_Code, EM_SystemCreateTimeUtc, 'IE', CURRENT_TIMESTAMP, '~BP', CURRENT_TIMESTAMP, '~BP'
				FROM dbo.EDIMessage
					JOIN dbo.CusInBondMoveHeader ON EM_LinkTable = 'CusInBondMoveHeader' AND EM_LinkUniqueID = BM_PK
					JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'D'
					JOIN dbo.JobDocAddress ON E2_ParentTableCode = 'BH' AND E2_ParentID = BH_PK AND E2_AddressType = 'PRC'
					JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK
					JOIN dbo.OrgHeader ON OA_OH = OH_PK
				WHERE EM_MessageType = '028'
				  AND EM_ApplicationCode = 'IEN'
				  AND EM_Status = 'PRS'
				  AND NOT EXISTS(SELECT NULL FROM dbo.CusEntryNum WHERE CE_ParentID = BH_PK AND CE_EntryType = 'REG' AND CE_RN_NKCountryCode = 'IE');";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
