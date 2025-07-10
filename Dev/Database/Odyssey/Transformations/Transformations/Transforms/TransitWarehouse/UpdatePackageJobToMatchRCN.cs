using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdatePackageJobToMatchRCN : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Package Job to Match RCN.";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(UpdateRCNJobSQL);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key(WhsItemPackageStateSchema.Constants.WPS_SystemLastEditTimeUtc)
					.Include(WhsItemPackageStateSchema.Constants.WPS_KP_Package, WhsItemPackageStateSchema.Constants.WPS_WRC_TransitReceiveConsignment)
					.GetInfo();
				indexProvider.New(PkgPackageSchema.Instance)
					.Key(PkgPackageSchema.Constants.PK)
					.Include(PkgPackageSchema.Constants.KP_KPH_PackageHeader, "KP_AutoVersion")
					.GetInfo();
				indexProvider.New(PkgPackageJobSchema.Instance)
					.Key(PkgPackageJobSchema.Constants.KJ_ParentTableCode, PkgPackageJobSchema.Constants.KJ_ParentID)
					.Where($"[{PkgPackageJobSchema.Constants.KJ_ParentTableCode}]='WRC'")
					.GetInfo();
				return indexProvider;
			}
		}

		const string UpdateRCNJobSQL =
@"
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TG_PkgPackage_UpdateAutoVersion' AND parent_id = OBJECT_ID('dbo.PkgPackage'))
BEGIN
	DISABLE TRIGGER dbo.TG_PkgPackage_UpdateAutoVersion ON dbo.PkgPackage
END;

DROP TABLE IF EXISTS #AffectedPackages
CREATE TABLE #AffectedPackages (KP_PK UNIQUEIDENTIFIER NOT NULL, KJ_PK UNIQUEIDENTIFIER NOT NULL, KPH_PK UNIQUEIDENTIFIER NULL, KPH_PackageID nvarchar(46))
CREATE NONCLUSTERED INDEX IX_KPH_PK ON #AffectedPackages (KPH_PK);
INSERT INTO	#AffectedPackages (KP_PK, KJ_PK, KPH_PK)
	SELECT KP_PK, KJ_PK, KP_KPH_PackageHeader
	FROM
		dbo.PkgPackage
		JOIN dbo.WhsItemPackageState ON WPS_KP_Package = KP_PK
		JOIN dbo.PkgPackageJob ON KJ_ParentID = WPS_WRC_TransitReceiveConsignment
	WHERE
		WPS_SystemLastEditTimeUtc >= '2024-07-10'
		AND KJ_PK != KP_KJ_ParentPackageJob
		AND KJ_ParentTableCode = 'WRC'

UPDATE #AffectedPackages
SET KPH_PackageID = h.KPH_PackageID
	FROM #AffectedPackages
		LEFT JOIN dbo.PkgPackageHeader h ON h.KPH_PK = #AffectedPackages.KPH_PK

DROP TABLE IF EXISTS #ValidPackages
CREATE TABLE #ValidPackages (KP_PK UNIQUEIDENTIFIER NOT NULL, KJ_PK UNIQUEIDENTIFIER NOT NULL, KPH_PK UNIQUEIDENTIFIER NULL)
CREATE CLUSTERED INDEX IX_KP_PK ON #ValidPackages (KP_PK);
INSERT INTO	#ValidPackages (KP_PK, KJ_PK, KPH_PK)
	SELECT KP_PK, KJ_PK, KPH_PK
	FROM #AffectedPackages p1
	WHERE NOT EXISTS
	(
		SELECT NULL
		FROM #AffectedPackages p2
		WHERE p1.KP_PK <> p2.KP_PK AND p1.KJ_PK = p2.KJ_PK AND p1.KPH_PackageID = p2.KPH_PackageID
	)

DROP TABLE IF EXISTS #DuplicatedPackages
CREATE TABLE #DuplicatedPackages (KP_PK UNIQUEIDENTIFIER NOT NULL)
CREATE CLUSTERED INDEX IX_KP_PK ON #DuplicatedPackages (KP_PK);
INSERT INTO	#DuplicatedPackages (KP_PK)
	SELECT DISTINCT #ValidPackages.KP_PK
	FROM #ValidPackages
		JOIN dbo.PkgPackageHeader validHeader1 ON validHeader1.KPH_PK = #ValidPackages.KPH_PK
		JOIN dbo.PkgPackageHeader duplicateHeader1 ON duplicateHeader1.KPH_PK <> validHeader1.KPH_PK AND duplicateHeader1.KPH_PackageID = validHeader1.KPH_PackageID
		JOIN
		(
			SELECT
				KP_KPH_PackageHeader AS Header,
				KP_KJ_ParentPackageJob AS PackageJob
			FROM dbo.PkgPackage

			UNION ALL

			SELECT
				KPJ_KPH_PackageHeader AS Header,
				KPJ_KJ_PackageJob AS PackageJob
			FROM dbo.PkgPackageJobPackageHeaderPivot
		) AS Items ON Header = duplicateHeader1.KPH_PK AND PackageJob = #ValidPackages.KJ_PK

UPDATE
	source
SET
	KP_KJ_ParentPackageJob = #ValidPackages.KJ_PK,
	KP_SystemLastEditTimeUtc = GetUtcDate(),
	KP_SystemLastEditUser = '~BP',
	KP_AutoVersion = (source.KP_AutoVersion + 1) % 32768
FROM
	dbo.PkgPackage AS source
	JOIN #ValidPackages ON #ValidPackages.KP_PK = source.KP_PK
	LEFT JOIN #DuplicatedPackages ON #DuplicatedPackages.KP_PK = source.KP_PK
WHERE #DuplicatedPackages.KP_PK IS NULL;

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TG_PkgPackage_UpdateAutoVersion' AND parent_id = OBJECT_ID('dbo.PkgPackage'))
BEGIN
	ENABLE TRIGGER dbo.TG_PkgPackage_UpdateAutoVersion ON dbo.PkgPackage
END;
";
	}
}
