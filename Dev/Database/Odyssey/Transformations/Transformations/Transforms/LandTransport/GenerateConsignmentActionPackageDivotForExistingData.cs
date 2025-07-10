using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.LandTransport
{
	class GenerateConsignmentActionPackageDivotForExistingData : DataTransformation
	{
		public override string UserDescription => "Generate missing action package divots for consignments";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var sql = @"
INSERT INTO dbo.DtbConsignmentActionPackageDivot
	(LTP_PK, LTP_AutoVersion, LTP_LTA_ConsignmentAction, LTP_KP_Package, LTP_PackageQuantity, LTP_SystemCreateTimeUtc, LTP_SystemCreateUser, LTP_SystemLastEditTimeUtc, LTP_SystemLastEditUser)
	SELECT
		NEWID(),
		0,
		LTA_PK,
		KP_PK,
		KP_PackageQty,
		GETUTCDATE(),
		'~BP',
		GETUTCDATE(),
		'~BP'
	FROM dbo.DtbConsignmentAction
		JOIN dbo.DtbConsignmentAddress ON LTA_LTS_ConsignmentAddress = LTS_PK
		JOIN dbo.PkgPackageJob ON KJ_ParentID = LTS_LTC_Consignment
		JOIN dbo.PkgPackage ON KP_KJ_ParentPackageJob = KJ_PK
	WHERE
		NOT EXISTS (
        SELECT 1 
        FROM dbo.DtbConsignmentActionPackageDivot d 
        WHERE LTP_LTA_ConsignmentAction = LTA_PK
	)
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
