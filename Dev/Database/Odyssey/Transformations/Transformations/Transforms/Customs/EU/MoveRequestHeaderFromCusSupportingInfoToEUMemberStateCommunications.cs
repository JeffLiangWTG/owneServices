using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	class MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Move RequestHeader from CusSupportingInfo to EUMemberStateCommunications";

		public TransformationIndexProvider IndexProvider => BuildIndexProvider();

		TransformationIndexProvider BuildIndexProvider()
		{
			var asycudaIndexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "AsycudaManifestHeader", "IX_MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications_AsycudaManifestHeader")
				.Key(AsycudaManifestHeaderSchema.AMA_ManifestType.Name)
				.Include(AsycudaManifestHeaderSchema.PK.Name, AsycudaManifestHeaderSchema.AMA_ClusterKey.Name)
				.Where("[AMA_ManifestType]='ENS'")
				.GetInfo();

			var cusSupportingInfoIndexInfo = IndexInfo.Builder
				.New(Db.SqlDbOwnerSchema, "CusSupportingInfo", "IX_MoveRequestHeaderFromCusSupportingInfoToEUMemberStateCommunications_CusSupportingInfo")
				.Key(CusSupportingInfoSchema.CSI_Type.Name, CusSupportingInfoSchema.CSI_ParentTableCode.Name)
				.Include(CusSupportingInfoSchema.PK.Name, CusSupportingInfoSchema.CSI_Code.Name, CusSupportingInfoSchema.CSI_SubType.Name, CusSupportingInfoSchema.CSI_Description.Name, CusSupportingInfoSchema.CSI_ReferenceNumber.Name, CusSupportingInfoSchema.CSI_AdditionalDescription.Name, CusSupportingInfoSchema.CSI_ReferenceNumber2.Name, CusSupportingInfoSchema.CSI_RN_NKCountryCode.Name, CusSupportingInfoSchema.CSI_Status.Name, CusSupportingInfoSchema.CSI_UnitOfQuantity2.Name)
				.Where("[CSI_Type]='RQH' AND [CSI_ParentTableCode]='AMA'")
				.GetInfo();

			return new TransformationIndexProvider(this) { asycudaIndexInfo.Yield(), cusSupportingInfoIndexInfo.Yield() };
		}

		const string SQL = @"
BEGIN TRY
    DECLARE @timeStamp DATETIME = GETUTCDATE();

    SELECT 
        CSI_PK,
        CSI_Code,
        CSI_SubType,
        CSI_Description,
        CSI_ReferenceNumber,
        CSI_AdditionalDescription,
        CSI_ReferenceNumber2,
        CSI_RN_NKCountryCode,
        CSI_Status,
        CSI_UnitOfQuantity2,
        AMA_ClusterKey,
        AMA_PK
    INTO #RequestHeaders
    FROM dbo.CusSupportingInfo AS csi
    INNER JOIN dbo.AsycudaManifestHeader AS amh ON csi.CSI_ParentID = amh.AMA_PK
    WHERE csi.CSI_Type = 'RQH'
      AND csi.CSI_ParentTableCode = 'AMA'
      AND amh.AMA_ManifestType = 'ENS';

    INSERT INTO dbo.EUMemberStateCommunication (
        EUS_PK, 
        EUS_Identifier, 
        EUS_Type, 
        EUS_MessageElement, 
        EUS_HouseBillNumber, 
        EUS_ScreeningMethod, 
        EUS_TransportDocumentType,
        EUS_MemberState,
        EUS_Status,
        EUS_IncludeScreeningDetails,
        EUS_ClusterKey,
        EUS_ParentID,
        EUS_ParentTableCode,
        EUS_SystemCreateUser,
        EUS_SystemLastEditUser,
        EUS_SystemCreateTimeUtc,
        EUS_SystemLastEditTimeUtc
    )
    SELECT 
        NEWID(), 
        CSI_Code, 
        CSI_SubType, 
        CSI_Description, 
        CSI_ReferenceNumber, 
        CSI_AdditionalDescription, 
        CSI_ReferenceNumber2, 
        CSI_RN_NKCountryCode, 
        CSI_Status,
        CASE WHEN CSI_UnitOfQuantity2 = 'Y' THEN 1 ELSE 0 END,
        AMA_ClusterKey,
        AMA_PK,
        'AMA', '~BP', '~BP', @timeStamp, @timeStamp
    FROM #RequestHeaders;

    DELETE csi
    FROM dbo.CusSupportingInfo csi
    INNER JOIN #RequestHeaders rh ON csi.CSI_PK = rh.CSI_PK;

    DROP TABLE #RequestHeaders;

END TRY
BEGIN CATCH
    THROW;
END CATCH;";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(SQL);
		}
	}
}
