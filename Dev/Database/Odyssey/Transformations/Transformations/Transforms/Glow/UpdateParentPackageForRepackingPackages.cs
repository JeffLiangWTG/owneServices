using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	public class UpdateParentPackageForRepackingPackages : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update KP_KP_ParentPackage for Repacking Packages";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
WITH childPkgs AS
(
	SELECT
		KP_Sequence,
		KP_KP_ParentPackage,
		KPD_KP_HandlingUnit,
		KP_SystemLastEditTimeUtc,
		KP_SystemLastEditUser,
		KP_AutoVersion
	FROM dbo.PkgPackage childPackage
	JOIN dbo.PkgPackageHandlingUnitDivot ON KP_PK = KPD_KP_Package
	JOIN dbo.WhsItemPackageState parentPackageState ON WPS_KP_Package = KPD_KP_HandlingUnit 
	WHERE 
		KPD_UnpackedTime IS NULL AND KP_KP_ParentPackage IS NULL AND parentPackageState.WPS_UnitType = 'OVP'
)

UPDATE
	childPkgs
SET
	KP_KP_ParentPackage = KPD_KP_HandlingUnit, 
	KP_Sequence = 0,
	KP_SystemLastEditTimeUtc = GetUtcDate(),
	KP_SystemLastEditUser = '~BP'
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key(WhsItemPackageStateSchema.Constants.WPS_UnitType)
					.Include(WhsItemPackageStateSchema.Constants.WPS_KP_Package)
					.Where("[WPS_UnitType]='OVP'")
					.GetInfo();

				indexProvider.New(PkgPackageHandlingUnitDivotSchema.Instance)
					.Key(PkgPackageHandlingUnitDivotSchema.Constants.KPD_KP_HandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime)
					.Include(PkgPackageHandlingUnitDivotSchema.Constants.KPD_PackedTime, PkgPackageHandlingUnitDivotSchema.Constants.KPD_KP_Package)
					.Where("[KPD_UnpackedTime] IS NULL")
					.GetInfo();

				indexProvider.New(PkgPackageSchema.Instance)
					.Key(PkgPackageSchema.Constants.KP_KP_ParentPackage)
					.Include(PkgPackageSchema.Constants.KP_Sequence, PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc, PkgPackageSchema.Constants.KP_SystemLastEditUser)
					.Where("[KP_KP_ParentPackage] IS NULL")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
