using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE
{
	public class MoveAddInfoCusEntryInstructionToCusGoodsLocation : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Move CusEntryInstruction AddInfo ZG_QualifierOfIdentification and ZG_LoadingPlaceCode to CusGoodsLocation";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(sql);
		}

		const string sql =
			""""
				BEGIN TRY

				IF NOT EXISTS (SELECT * FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'DE')
				RETURN

				UPDATE CusGoodsLocation
				SET CusGoodsLocation.CGL_Qualifier = QualifierAddInfo.value,
					CusGoodsLocation.CGL_AdditionalIdentifier = IIF(QualifierAddInfo.value = 'U', COALESCE(OrgAddress.OA_RL_NKRelatedPortCode, ''), COALESCE(LoadingPlaceAddInfo.value, '')),
					CusGoodsLocation.CGL_Type = CASE WHEN QualifierAddInfo.value = 'V' THEN 'A' WHEN QualifierAddInfo.value = 'Y' THEN 'B' ELSE 'D' END,
					CusGoodsLocation.CGL_SystemLastEditTimeUtc = GETUTCDATE(),
					CusGoodsLocation.CGL_SystemLastEditUser = '~BP'
				FROM dbo.CusGoodsLocation AS CusGoodsLocation
				INNER JOIN dbo.CusEntryInstruction AS CusEntryInstruction ON CusGoodsLocation.CGL_ParentID = CusEntryInstruction.CEI_PK
				LEFT JOIN dbo.JobDocAddress AS SupplierPickupAddress ON SupplierPickupAddress.E2_ParentID = CusEntryInstruction.CEI_JE
				LEFT JOIN dbo.OrgAddress AS OrgAddress ON SupplierPickupAddress.E2_OA_Address = OrgAddress.OA_PK
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CusEntryInstruction.CEI_AddInfo, 'QualifierOfIdentification') AS QualifierAddInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CusEntryInstruction.CEI_AddInfo, 'LoadingPlaceCode') AS LoadingPlaceAddInfo
				WHERE CEI_DataModel ='DE' AND QualifierAddInfo.value IS NOT NULL AND SupplierPickupAddress.E2_AddressType = 'SUG'

				INSERT INTO dbo.CusGoodsLocation(CGL_PK, CGL_ParentTableCode, CGL_ParentID, CGL_Qualifier, CGL_LocationUse, CGL_SystemCreateTimeUtc, CGL_SystemLastEditTimeUtc, CGL_SystemCreateUser, CGL_SystemLastEditUser, CGL_Type, CGL_AdditionalIdentifier)
				SELECT NEWID(), 'CEI', CEI_PK, QualifierAddInfo.value, 'CEI', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP',

				CASE QualifierAddInfo.value 
					WHEN 'V' THEN 'A' 
					WHEN 'Y' THEN 'B' 
					ELSE 'D' 
				END AS LocationType,

				IIF(QualifierAddInfo.value = 'U', COALESCE(OrgAddress.OA_RL_NKRelatedPortCode,''), COALESCE(LoadingPlaceAddInfo.value, ''))

				FROM CusEntryInstruction
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CusEntryInstruction.CEI_AddInfo, 'QualifierOfIdentification') AS QualifierAddInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CusEntryInstruction.CEI_AddInfo, 'LoadingPlaceCode') AS LoadingPlaceAddInfo
				LEFT JOIN dbo.JobDocAddress AS SupplierPickupAddress ON SupplierPickupAddress.E2_ParentID = CusEntryInstruction.CEI_JE
				LEFT JOIN dbo.OrgAddress AS OrgAddress ON SupplierPickupAddress.E2_OA_Address = OrgAddress.OA_PK
				WHERE
					CEI_DataModel ='DE' AND CEI_AddInfo <> '' AND QualifierAddInfo.value IS NOT NULL AND NOT EXISTS (SELECT * FROM dbo.CusGoodsLocation WHERE CGL_ParentID = CEI_PK) AND SupplierPickupAddress.E2_AddressType = 'SUG'

				UPDATE JobDocAddress
				SET JobDocAddress.E2_AddressType = 'LOC',
					JobDocAddress.E2_CompanyName = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_CompanyName, OrgHeader.OH_FullName),
					JobDocAddress.E2_Address1 = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Address1, OrgAddress.OA_Address1),
					JobDocAddress.E2_Address2 = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Address2, OrgAddress.OA_Address2),
					JobDocAddress.E2_City = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_City, OrgAddress.OA_City),
					JobDocAddress.E2_Postcode = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Postcode, OrgAddress.OA_PostCode),
					JobDocAddress.E2_RN_NKCountryCode = IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_RN_NKCountryCode, OrgAddress.OA_RN_NKCountryCode),
					JobDocAddress.E2_GeoLocation = SupplierPickupAddress.E2_GeoLocation,
					JobDocAddress.E2_SystemLastEditTimeUtc = GETUTCDATE(),
					JobDocAddress.E2_SystemLastEditUser = '~BP'
				FROM dbo.JobDocAddress AS JobDocAddress
				INNER JOIN dbo.CusGoodsLocation AS CusGoodsLocation ON JobDocAddress.E2_ParentID = CusGoodsLocation.CGL_PK
				INNER JOIN dbo.CusEntryInstruction AS CusEntryInstruction ON CusGoodsLocation.CGL_ParentID = CusEntryInstruction.CEI_PK
				LEFT JOIN dbo.JobDocAddress AS SupplierPickupAddress ON SupplierPickupAddress.E2_ParentID = CusEntryInstruction.CEI_JE
				LEFT JOIN dbo.OrgAddress AS OrgAddress ON SupplierPickupAddress.E2_OA_Address = OrgAddress.OA_PK
				LEFT JOIN dbo.OrgHeader AS OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK
				WHERE
					CEI_DataModel ='DE' AND CEI_AddInfo <> '' AND SupplierPickupAddress.E2_AddressType = 'SUG'

				INSERT INTO dbo.JobDocAddress(E2_PK, E2_AddressType, E2_ParentID, E2_ParentTableCode, E2_CompanyName, E2_Address1, E2_Address2, E2_City, E2_Postcode, E2_RN_NKCountryCode, E2_GeoLocation, E2_SystemCreateTimeUtc, E2_SystemLastEditTimeUtc, E2_SystemCreateUser, E2_SystemLastEditUser)
				SELECT NEWID(),
					'LOC',
					CusGoodsLocation.CGL_PK,
					'CGL',
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_CompanyName, OrgHeader.OH_FullName),
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Address1, OrgAddress.OA_Address1),
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Address2, OrgAddress.OA_Address2),
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_City, OrgAddress.OA_City),
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_Postcode, OrgAddress.OA_PostCode),
					IIF(SupplierPickupAddress.E2_AddressOverride = 1, SupplierPickupAddress.E2_RN_NKCountryCode, OrgAddress.OA_RN_NKCountryCode),
					SupplierPickupAddress.E2_GeoLocation,
					GETUTCDATE(),
					GETUTCDATE(),
					'~BP',
					'~BP'
				FROM CusEntryInstruction
					INNER JOIN dbo.CusGoodsLocation AS CusGoodsLocation ON CGL_ParentID = CEI_PK
					LEFT JOIN dbo.JobDocAddress AS SupplierPickupAddress ON SupplierPickupAddress.E2_ParentID = CusEntryInstruction.CEI_JE
					LEFT JOIN dbo.OrgAddress AS OrgAddress ON SupplierPickupAddress.E2_OA_Address = OrgAddress.OA_PK
					LEFT JOIN dbo.OrgHeader AS OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK
					CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CusEntryInstruction.CEI_AddInfo, 'QualifierOfIdentification') AS QualifierAddInfo
				WHERE
					CEI_DataModel ='DE' AND CEI_AddInfo <> '' AND SupplierPickupAddress.E2_AddressType = 'SUG' AND NOT EXISTS (SELECT * FROM dbo.JobDocAddress WHERE E2_ParentID = CusGoodsLocation.CGL_PK AND E2_AddressType='LOC') AND QualifierAddInfo.value IN ('Z', 'W')

				UPDATE dbo.CusEntryInstruction
				SET
					CEI_AddInfo = TRIM('* ' FROM (IIF(LoadingPlaceAddInfo.value  IS NOT NULL, REPLACE(QualifierAddInfo.AddInfoValue, 'LoadingPlaceCode=' + LoadingPlaceAddInfo.value, '') , QualifierAddInfo.AddInfoValue))),
					CEI_SystemLastEditTimeUtc = GETUTCDATE(),
					CEI_SystemLastEditUser = '~BP'
				FROM dbo.CusEntryInstruction
				CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(CEI_AddInfo, 'QualifierOfIdentification') QualifierAddInfo
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(CEI_AddInfo, 'LoadingPlaceCode') AS LoadingPlaceAddInfo
							WHERE
					CEI_DataModel ='DE' AND CEI_AddInfo <> ''

				END TRY
				BEGIN CATCH
					THROW;
				END CATCH;
			"""";

		public TransformationIndexProvider IndexProvider => BuildIndexProvider();

		TransformationIndexProvider BuildIndexProvider()
		{
			var indexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusEntryInstruction", "IX_MoveCusEntryInstructionAddInfoZG_QualifierOfIdentificationAndZG_LoadingPlaceCodeToCusGoodsLocation_CEI_DataModel")
				.Key(CusEntryInstructionSchema.CEI_DataModel.Name)
				.Include(CusEntryInstructionSchema.PK.Name, CusEntryInstructionSchema.CEI_AddInfo.Name)
				.Where("[CEI_DataModel]='DE' AND [CEI_AddInfo]<>''")
				.GetInfo();

			return new TransformationIndexProvider(this) { indexInfo.Yield() };
		}
	}
}
