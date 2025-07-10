using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdatePackageStateSecurityStatusForHuWhenScreeningIsBypassed : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Security Status for HU when screening is bypassed from dll";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
CREATE TABLE #CandidateTopHandlingUnits (
	PackagePK UNIQUEIDENTIFIER,
	INDEX RC__PackagePK CLUSTERED (PackagePK ASC)
);

CREATE TABLE #TopHuToBeExcluded (
	PackagePK UNIQUEIDENTIFIER,
	INDEX RC__PackagePK CLUSTERED (PackagePK ASC)
);

CREATE TABLE #TopHuToBeUpdated (
	PackagePK UNIQUEIDENTIFIER,
	INDEX RC__PackagePK CLUSTERED (PackagePK ASC)
);

INSERT INTO #CandidateTopHandlingUnits
SELECT
	HuPackageState.WPS_KP_Package AS PackagePK
FROM 
	dbo.WhsItemPackageState HuPackageState
	JOIN dbo.PkgPackage HuPkgPackage ON HuPkgPackage.KP_PK = HuPackageState.WPS_KP_Package
WHERE
	HuPackageState.WPS_UnitType IN ('HU', 'CNT', 'ULD')
	AND HuPackageState.WPS_SecurityStatus NOT IN ('OVR')
	AND HuPackageState.WPS_WDH_TransitDispatchHeader IS NULL
	AND HuPkgPackage.KP_KP_TopHandlingUnitPackage IS NULL

INSERT INTO #TopHuToBeExcluded
SELECT
	TopHuWithNonOVRChildPackage.KP_KP_TopHandlingUnitPackage
FROM 
	#CandidateTopHandlingUnits
	CROSS APPLY
	(
		SELECT TOP 1
		ChildPkgPackage.KP_KP_TopHandlingUnitPackage
		FROM 
			PkgPackage ChildPkgPackage
			JOIN dbo.WhsItemPackageState ChildPkgState ON ChildPkgState.WPS_KP_Package = ChildPkgPackage.KP_PK
		 WHERE 
			ChildPkgState.WPS_SecurityStatus != 'OVR'
			AND ChildPkgPackage.KP_KP_TopHandlingUnitPackage = #CandidateTopHandlingUnits.PackagePK
			AND ChildPkgState.WPS_UnitType IN ('OVP', 'PKL', 'PKG')
	)AS TopHuWithNonOVRChildPackage

INSERT INTO #TopHuToBeUpdated
SELECT
	#CandidateTopHandlingUnits.PackagePK
FROM
	#CandidateTopHandlingUnits
	CROSS APPLY
	(
		SELECT TOP 1
		ChildPkgPackage.KP_PK
		FROM 
			PkgPackage ChildPkgPackage
			JOIN WhsItemPackageState ChildPackageState ON ChildPackageState.WPS_KP_Package = ChildPkgPackage.KP_PK
		WHERE
			ChildPkgPackage.KP_KP_TopHandlingUnitPackage = #CandidateTopHandlingUnits.PackagePK
			AND ChildPackageState.WPS_UnitType IN ('OVP', 'PKL', 'PKG')
	)AS TopHuHasChildPackage
WHERE #CandidateTopHandlingUnits.PackagePK NOT IN
	(
		SELECT PackagePK FROM #TopHuToBeExcluded
	)

UPDATE dbo.WhsItemPackageState
SET 
	WPS_SecurityStatus = 'OVR',
	WPS_SystemLastEditUser = '~BP',
	WPS_SystemLastEditTimeUtc = GETUTCDATE(),
	WPS_AutoVersion = (WPS_AutoVersion + 1) % 32768
WHERE 
	WPS_KP_Package IN (SELECT PackagePK FROM #TopHuToBeUpdated)
	OR WPS_KP_Package IN (
		SELECT MidHuPkgState.WPS_KP_Package
		FROM dbo.WhsItemPackageState MidHuPkgState
		JOIN dbo.PkgPackage MidHuPkgPackage ON MidHuPkgState.WPS_KP_Package = MidHuPkgPackage.KP_PK
		WHERE MidHuPkgState.WPS_SecurityStatus != 'OVR'
			AND MidHuPkgState.WPS_UnitType IN ('HU', 'CNT', 'ULD')
			AND MidHuPkgPackage.KP_KP_TopHandlingUnitPackage IN (SELECT PackagePK FROM #TopHuToBeUpdated)
	);

DROP TABLE #TopHuToBeUpdated;
DROP TABLE #TopHuToBeExcluded;
DROP TABLE #CandidateTopHandlingUnits;
";
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsItemPackageState_UpdateAutoVersion", WhsItemPackageStateSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(PkgPackageSchema.Instance)
					.Key(PkgPackageSchema.Constants.PK)
					.Include(PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage)
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_KP_Package, WhsItemPackageStateSchema.Constants.WPS_UnitType])
					.Include([WhsItemPackageStateSchema.Constants.WPS_SecurityStatus, WhsItemPackageStateSchema.Constants.WPS_WDH_TransitDispatchHeader])
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_UnitType])
					.Include([WhsItemPackageStateSchema.Constants.WPS_KP_Package, WhsItemPackageStateSchema.Constants.WPS_SecurityStatus, WhsItemPackageStateSchema.Constants.WPS_WDH_TransitDispatchHeader])
					.GetInfo();
				
				return indexProvider;
			}
		}
	}
}
