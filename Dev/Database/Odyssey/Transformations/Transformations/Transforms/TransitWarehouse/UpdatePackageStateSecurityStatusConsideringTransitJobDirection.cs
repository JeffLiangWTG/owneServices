using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdatePackageStateSecurityStatusConsideringTransitJobDirection : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Package Security Status considering consignment direction";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
CREATE TABLE #StandaloneAndOverpackPkgs (
	PK UNIQUEIDENTIFIER,
	UnitType VARCHAR(3),
	INDEX RC__PK CLUSTERED (PK ASC),
	INDEX RC__UnitType NONCLUSTERED (UnitType ASC),
)

;CREATE TABLE #CandidateTopHandlingUnits (
	PK UNIQUEIDENTIFIER
	INDEX RC__PK CLUSTERED (PK ASC),
)

;CREATE TABLE #TopHandlingUnits (
	PK UNIQUEIDENTIFIER
	INDEX RC__KP_KP_TopHandlingUnitPackage CLUSTERED (PK ASC)
)

;WITH ValidReceiveConsignments AS (
	SELECT WRC_PK
	FROM dbo.WhsItemReceiveConsignment 
	WHERE WRC_Direction IN ('IMP', 'DOM')
),

ValidDispatchConsignments AS (
	SELECT WDC_PK
	FROM dbo.WhsItemDispatchConsignment 
	WHERE WDC_Direction IN ('IMP', 'DOM')
)

INSERT INTO #StandaloneAndOverpackPkgs
SELECT
	WPS_KP_Package AS PK,
	WPS_UnitType AS UnitType
FROM
	dbo.WhsItemPackageState
	JOIN dbo.WhsWarehouse ON WPS_WW_Warehouse = WW_PK
	LEFT JOIN ValidReceiveConsignments ON WPS_WRC_TransitReceiveConsignment = WRC_PK
	LEFT JOIN ValidDispatchConsignments ON WPS_WDC_TransitDispatchConsignment = WDC_PK
WHERE 
	WPS_UnitType IN ('OVP', 'PKL', 'PKG')
	AND WPS_SecurityStatus NOT IN ('OVR', 'NOT')
	AND (WPS_WDH_TransitDispatchHeader IS NULL OR WPS_Status = 'FLO')
	AND WPS_AdjustedOut = ''
	AND WW_TransitSecurityProcessingRequired = 1
	AND (WPS_WRC_TransitReceiveConsignment IS NULL OR WRC_PK IS NOT NULL)
	AND (WPS_WDC_TransitDispatchConsignment IS NULL OR WDC_PK IS NOT NULL)

;INSERT INTO #CandidateTopHandlingUnits
SELECT
	WPS_KP_Package AS PK
FROM
	dbo.WhsItemPackageState
WHERE
	WPS_KP_Package IN
	(
		SELECT
			KP_KP_TopHandlingUnitPackage
		FROM
			#StandaloneAndOverpackPkgs
			JOIN dbo.PkgPackage ON PK = KP_PK
	)
	AND WPS_IsHandlingUnit = 1 AND WPS_UnitType <> 'OVP' AND WPS_SecurityStatus NOT IN ('OVR', 'NOT')

;INSERT INTO #TopHandlingUnits
SELECT
	PK 
FROM
	#CandidateTopHandlingUnits hu
WHERE NOT EXISTS (
	SELECT 1
	FROM
		dbo.WhsItemPackageState wps
		JOIN dbo.PkgPackage pkg ON wps.WPS_KP_Package = pkg.KP_PK
		LEFT JOIN #StandaloneAndOverpackPkgs ON pkg.KP_PK = PK 
	WHERE
		pkg.KP_KP_TopHandlingUnitPackage = hu.PK
		AND wps.WPS_UnitType IN ('OVP', 'PKL', 'PKG')
		AND wps.WPS_SecurityStatus IN ('HRS', 'HRN', 'REQ')
		AND PK IS NULL
)

;WITH MidHandlingUnits AS
(
	SELECT
		WPS_KP_Package AS PK
	FROM
		dbo.WhsItemPackageState
		JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
	WHERE
		KP_KP_TopHandlingUnitPackage IN
		(
			SELECT
				PK
			FROM
				#TopHandlingUnits
		)
		AND WPS_IsHandlingUnit = 1 AND WPS_UnitType <> 'OVP' AND WPS_SecurityStatus NOT IN ('OVR', 'NOT')
),

AllPackages AS (
	SELECT PK FROM #StandaloneAndOverpackPkgs
	UNION
	SELECT PK FROM #TopHandlingUnits
	UNION
	SELECT PK FROM MidHandlingUnits
)

UPDATE dbo.WhsItemPackageState
SET
	WPS_SecurityStatus = 'NOT',
	WPS_SystemLastEditUser = '~BP',
	WPS_SystemLastEditTimeUtc = GETUTCDATE(),
	WPS_AutoVersion = (WPS_AutoVersion + 1) % 32768
FROM AllPackages
WHERE 
	AllPackages.PK = WPS_KP_Package;

;DROP TABLE #StandaloneAndOverpackPkgs
;DROP TABLE #CandidateTopHandlingUnits
;DROP TABLE #TopHandlingUnits
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
				indexProvider.New(WhsItemReceiveConsignmentSchema.Instance)
					.Key(WhsItemReceiveConsignmentSchema.Constants.WRC_Direction)
					.Include(WhsItemReceiveConsignmentSchema.Constants.PK)
					.GetInfo();

				indexProvider.New(WhsItemDispatchConsignmentSchema.Instance)
					.Key(WhsItemDispatchConsignmentSchema.Constants.WDC_Direction)
					.Include(WhsItemDispatchConsignmentSchema.Constants.PK)
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_AdjustedOut, WhsItemPackageStateSchema.Constants.WPS_UnitType, WhsItemPackageStateSchema.Constants.WPS_SecurityStatus])
					.Include([WhsItemPackageStateSchema.Constants.WPS_KP_Package,
						WhsItemPackageStateSchema.Constants.WPS_WDH_TransitDispatchHeader,
						WhsItemPackageStateSchema.Constants.WPS_Status,
						WhsItemPackageStateSchema.Constants.WPS_WDC_TransitDispatchConsignment,
						WhsItemPackageStateSchema.Constants.WPS_WRC_TransitReceiveConsignment,
						WhsItemPackageStateSchema.Constants.WPS_WW_Warehouse])
					.Where($"(([{WhsItemPackageStateSchema.Constants.WPS_UnitType}] IN ('OVP', 'PKL', 'PKG')) AND [{WhsItemPackageStateSchema.Constants.WPS_AdjustedOut}]='')")
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_IsHandlingUnit, WhsItemPackageStateSchema.Constants.WPS_UnitType, WhsItemPackageStateSchema.Constants.WPS_SecurityStatus])
					.Include(WhsItemPackageStateSchema.Constants.WPS_KP_Package)
					.Where($"([{WhsItemPackageStateSchema.Constants.WPS_IsHandlingUnit}]=(1))")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
