using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdatePackageStateSecurityStatusWhenSendDispatchInstruction : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY

CREATE TABLE #StandaloneAndOverpackPkgs (
	PK UNIQUEIDENTIFIER,
	INDEX RC__PK CLUSTERED (PK ASC),
)

CREATE TABLE #TopHandlingUnits (
	PK UNIQUEIDENTIFIER
	INDEX RC__PK CLUSTERED (PK ASC),
)

CREATE TABLE #SecurityStatusForTopHUView (
	PK UNIQUEIDENTIFIER,
	SecurityStatus CHAR(3) COLLATE SQL_Latin1_General_CP1_CI_AS,
	INDEX RC__PK CLUSTERED (PK ASC),
	INDEX RC__SecurityStatus NONCLUSTERED (SecurityStatus ASC)
);

INSERT INTO #StandaloneAndOverpackPkgs (PK)
SELECT PK
FROM
(
	SELECT
		WPS_KP_Package AS PK
	FROM
		dbo.WhsItemPackageState
		JOIN dbo.WhsItemDispatchLoadList ON WPS_WDL_LoadList = WDL_PK
	WHERE 
		WPS_UnitType IN ('OVP', 'PKL', 'PKG')
		AND WPS_SecurityStatus NOT IN ('NOT', 'OVR')
		AND WPS_WDH_TransitDispatchHeader IS NULL
		AND WPS_AdjustedOut = ''
		AND WDL_TransportMode IN ('COU', 'RAI', 'ROA', 'SEA')

	UNION

	SELECT
		WPS_KP_Package AS PK
	FROM
		dbo.WhsItemPackageState
		JOIN WhsItemReceiveConsignment ON WPS_WRC_TransitReceiveConsignment = WRC_PK
		JOIN WhsItemDispatchConsignment ON WPS_WDC_TransitDispatchConsignment = WDC_PK
	WHERE
		WPS_UnitType IN ('OVP', 'PKL', 'PKG')
		AND WPS_SecurityStatus NOT IN ('NOT', 'OVR')
		AND WPS_WDH_TransitDispatchHeader IS NULL
		AND WPS_AdjustedOut = ''
		AND WRC_Direction IN ('', 'EXP')
		AND WDC_Direction IN ('IMP', 'DOM')
) AS AllTargetPackages

UPDATE dbo.WhsItemPackageState
SET WPS_SecurityStatus = 'NOT',
	WPS_SystemLastEditUser = '~BP',
	WPS_SystemLastEditTimeUtc = GETUTCDATE(),
	WPS_AutoVersion = (WPS_AutoVersion + 1) % 32768
WHERE WPS_KP_Package IN (SELECT PK FROM #StandaloneAndOverpackPkgs)

INSERT INTO #TopHandlingUnits
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
	AND WPS_UnitType != 'OVP'

INSERT INTO #SecurityStatusForTopHUView
SELECT
	PK,
	CASE
		WHEN HRS > 0 THEN 'HRS'
		WHEN HRN > 0 THEN 'HRN'
		WHEN REQ > 0 THEN 'REQ'
		ELSE 'NOT'
	END AS SecurityStatus
FROM
	#TopHandlingUnits
	CROSS APPLY (
		SELECT
			SUM(CASE WHEN WPS_SecurityStatus = 'REQ' THEN 1 ELSE 0 END) AS REQ,
			SUM(CASE WHEN WPS_SecurityStatus = 'HRS' THEN 1 ELSE 0 END) AS HRS,
			SUM(CASE WHEN WPS_SecurityStatus = 'HRN' THEN 1 ELSE 0 END) AS HRN
		FROM
			dbo.WhsItemPackageState ChildPackageState
			JOIN dbo.PkgPackage ChildPackage ON ChildPackage.KP_PK = ChildPackageState.WPS_KP_Package
			JOIN #TopHandlingUnits ON ChildPackage.KP_KP_TopHandlingUnitPackage = #TopHandlingUnits.PK
		GROUP BY ChildPackage.KP_KP_TopHandlingUnitPackage
	) AS InnerPackageSecurityStatusCountView

;WITH SecurityStatusForMidHUView AS
(
	SELECT
		MidHU.WPS_KP_Package AS PK,
		TopHUView.SecurityStatus
	FROM
		dbo.WhsItemPackageState MidHU
		JOIN dbo.PkgPackage ON MidHU.WPS_KP_Package = KP_PK
		JOIN #SecurityStatusForTopHUView TopHUView ON TopHUView.PK = KP_KP_TopHandlingUnitPackage
	WHERE 
		MidHU.WPS_SecurityStatus != TopHUView.SecurityStatus
		AND MidHU.WPS_IsHandlingUnit = 1
		AND MidHU.WPS_UnitType != 'OVP'
),

TopAndMidHUPackages AS (
	SELECT PK, SecurityStatus FROM #SecurityStatusForTopHUView
	UNION ALL
	SELECT PK, SecurityStatus FROM SecurityStatusForMidHUView
)

UPDATE dbo.WhsItemPackageState
SET
	WPS_SecurityStatus = SecurityStatus,
	WPS_SystemLastEditUser = '~BP',
	WPS_SystemLastEditTimeUtc = GETUTCDATE(),
	WPS_AutoVersion = (WPS_AutoVersion + 1) % 32768
FROM
	TopAndMidHUPackages
WHERE 
	TopAndMidHUPackages.PK = WPS_KP_Package
	AND WPS_SecurityStatus != SecurityStatus

;DROP TABLE #StandaloneAndOverpackPkgs
;DROP TABLE #TopHandlingUnits
;DROP TABLE #SecurityStatusForTopHUView;

END TRY
BEGIN CATCH
	THROW
END CATCH
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

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_KP_Package])
					.Include([WhsItemPackageStateSchema.Constants.WPS_UnitType, WhsItemPackageStateSchema.Constants.WPS_SecurityStatus])
					.GetInfo();

				indexProvider.New(WhsItemDispatchLoadListSchema.Instance)
					.Key([WhsItemDispatchLoadListSchema.Constants.PK, WhsItemDispatchLoadListSchema.Constants.WDL_TransportMode])
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key([WhsItemPackageStateSchema.Constants.WPS_UnitType, WhsItemPackageStateSchema.Constants.WPS_SecurityStatus, WhsItemPackageStateSchema.Constants.WPS_WDH_TransitDispatchHeader, WhsItemPackageStateSchema.Constants.WPS_AdjustedOut])
					.Include([WhsItemPackageStateSchema.Constants.WPS_KP_Package, WhsItemPackageStateSchema.Constants.WPS_WDL_LoadList, WhsItemPackageStateSchema.Constants.WPS_WRC_TransitReceiveConsignment, WhsItemPackageStateSchema.Constants.WPS_WDC_TransitDispatchConsignment])
					.Where($"(([{WhsItemPackageStateSchema.Constants.WPS_UnitType}] IN ('OVP', 'PKL', 'PKG')) AND [{WhsItemPackageStateSchema.Constants.WPS_AdjustedOut}]='' AND [{WhsItemPackageStateSchema.Constants.WPS_WDH_TransitDispatchHeader}] IS NULL AND [{WhsItemPackageStateSchema.Constants.WPS_SecurityStatus}]<>'NOT' AND [{WhsItemPackageStateSchema.Constants.WPS_SecurityStatus}]<>'OVR')")
					.GetInfo();

				indexProvider.New(WhsItemReceiveConsignmentSchema.Instance)
					.Key([WhsItemReceiveConsignmentSchema.Constants.PK, WhsItemReceiveConsignmentSchema.Constants.WRC_Direction])
					.GetInfo();

				indexProvider.New(WhsItemDispatchConsignmentSchema.Instance)
					.Key([WhsItemDispatchConsignmentSchema.Constants.PK, WhsItemDispatchConsignmentSchema.Constants.WDC_Direction])
					.GetInfo();

				indexProvider.New(PkgPackageSchema.Instance)
					.Key([PkgPackageSchema.Constants.PK])
					.Include([PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage])
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
