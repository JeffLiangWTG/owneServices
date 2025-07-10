using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class ReleasePackageIfNoAnyOpenVariance : DataTransformation
	{
		public override string UserDescription => "Release Package if no any open Variance.";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var sql = @"
BEGIN TRY

CREATE TABLE #NeedRelease (
    KHR_PK uniqueidentifier,
    KP_PK uniqueidentifier
);
CREATE NONCLUSTERED INDEX idx_NeedRelease_KHR_PK
ON #NeedRelease (KHR_PK);
CREATE NONCLUSTERED INDEX idx_NeedRelease_KP_PK 
ON #NeedRelease (KP_PK);

CREATE TABLE #LCCHolds (
    KHR_PK uniqueidentifier,
    KHR_KP_Package uniqueidentifier,
	KP_KP_TopHandlingUnitPackage uniqueidentifier,
	KP_KP_ParentPackage uniqueidentifier
);
CREATE NONCLUSTERED INDEX idx_LCCHolds_FilterAndJoin 
ON #LCCHolds (KP_KP_TopHandlingUnitPackage, KP_KP_ParentPackage, KHR_KP_Package);

INSERT INTO #LCCHolds
SELECT KHR_PK, KHR_KP_Package, KP_KP_TopHandlingUnitPackage, KP_KP_ParentPackage
FROM dbo.PkgPackageHold
JOIN dbo.PkgPackage ON KHR_KP_Package = KP_PK AND KP_IsHeld = 1
WHERE KHR_WHC_NKHoldCode = 'LCC' AND KHR_RemovedTime IS NULL

INSERT INTO #NeedRelease
SELECT DISTINCT KHR_PK, KHR_KP_Package
FROM #LCCHolds
WHERE KP_KP_TopHandlingUnitPackage IS NULL AND KP_KP_ParentPackage IS NULL
AND NOT EXISTS (
    SELECT Null
    FROM dbo.WhsItemCycleCountLocationVariance
	JOIN dbo.WhsItemPackageState ON WIV_WPS_PackageState = WPS_PK
    WHERE WPS_KP_Package = KHR_KP_Package
    AND WIV_Status = 'OPN'
);

INSERT INTO #NeedRelease
SELECT #LCCHolds.KHR_PK, #LCCHolds.KHR_KP_Package 
FROM #LCCHolds
JOIN #NeedRelease ON KP_KP_TopHandlingUnitPackage = #NeedRelease.KP_PK
WHERE KP_KP_TopHandlingUnitPackage IS NOT NULL


UPDATE dbo.PkgPackageHold
SET KHR_RemovedTime = GETDATE(),
    KHR_GS_NKRemovedBy = '~BP',
    KHR_SystemLastEditTimeUtc = GETDATE(),
    KHR_SystemLastEditUser = '~BP'
FROM dbo.PkgPackageHold
JOIN #NeedRelease filteredPackages ON PkgPackageHold.KHR_PK = filteredPackages.KHR_PK
WHERE KHR_RemovedTime is NULL;

UPDATE dbo.PkgPackage
SET KP_IsHeld = 0,
    KP_SystemLastEditTimeUtc = GETDATE(),
    KP_SystemLastEditUser = '~BP'
FROM dbo.PkgPackage
JOIN #NeedRelease filteredPackages ON PkgPackage.KP_PK = filteredPackages.KP_PK
WHERE KP_IsHeld = 1;

DROP TABLE #NeedRelease;
DROP TABLE #LCCHolds;

END TRY
BEGIN CATCH
DROP TABLE #NeedRelease;
DROP TABLE #LCCHolds;
THROW;
END CATCH";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
