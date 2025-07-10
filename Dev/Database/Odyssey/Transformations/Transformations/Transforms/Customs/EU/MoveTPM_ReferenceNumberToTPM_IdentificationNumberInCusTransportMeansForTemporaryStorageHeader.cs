using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

sealed class MoveTPM_ReferenceNumberToTPM_IdentificationNumberInCusTransportMeansForTemporaryStorageHeader : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Move TPM_ReferenceNumber to TPM_IdentificationNumber in CusTransportMeans for TemporaryStorageHeader";

	public TransformationIndexProvider IndexProvider => BuildIndexProvider();

	TransformationIndexProvider BuildIndexProvider()
	{
		var indexInfo = IndexInfo.Builder
			.New(Db.SqlDbOwnerSchema, "CusTransportMeans", "IX_MoveTPM_ReferenceNumberToTPM_IdentificationNumber_CusTransportMeansInTemporaryStorageHeader")
			.Key(CusTransportMeansSchema.TPM_ParentTableCode.Name, CusTransportMeansSchema.TPM_ReferenceNumber.Name)
			.Include(CusTransportMeansSchema.TPM_ParentID.Name)
			.Where("[TPM_ParentTableCode]='AMA' AND [TPM_ReferenceNumber]<>''")
			.GetInfo();

		return new TransformationIndexProvider(this) { indexInfo.Yield() };
	}

	protected override void OfflinePostUpgradeTransform()
	{
		const string sql = @"
	UPDATE dbo.CusTransportMeans
	SET
		TPM_IdentificationNumber = TPM_ReferenceNumber,
		TPM_ReferenceNumber = '',
		TPM_SystemLastEditTimeUtc = GETUTCDATE(),
		TPM_SystemLastEditUser = '~BP'
	FROM dbo.CusTransportMeans
	INNER JOIN dbo.AsycudaManifestHeader ON TPM_ParentID = AMA_PK
	WHERE TPM_ParentTableCode = 'AMA' AND TPM_ReferenceNumber <> '';
";

		Db.Connection.ExecuteNonQuery(sql);
	}
}
