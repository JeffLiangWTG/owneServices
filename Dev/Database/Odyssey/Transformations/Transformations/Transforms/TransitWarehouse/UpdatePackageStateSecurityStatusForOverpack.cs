using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdatePackageStateSecurityStatusForOverpack : DataTransformation
	{
		public override string UserDescription => "Update dbo.WhsItemPackageState Security Status for Overpack";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
SELECT WPS_PK,
	WPS_KP_Package,
	WPS_IsHighRisk,
	WPS_WW_Warehouse,
	WPS_WDL_LoadList,
	WPS_UnitType,
	WPS_IsHandlingUnit,
	WPS_SecurityStatus,
	WPS_IsHighRiskAuthorized,
	WPS_IsSecure,
	WPS_AdjustedOut,
	WPS_WDH_TransitDispatchHeader
	INTO #OvpPackages
	FROM dbo.WhsItemPackageState				
WHERE
	WPS_SecurityStatus = 'SCR'
	AND WPS_UnitType = 'OVP'
	AND WPS_AdjustedOut = '' AND WPS_WDH_TransitDispatchHeader IS NULL

;WITH SecurityStatusForOverpacksView AS
(
	SELECT
		KP_PK,
		WPS_IsHandlingUnit,
		KP_KP_TopHandlingUnitPackage,
		CASE 
			WHEN WPS_SecurityStatus = 'OVR' THEN 'OVR'
			WHEN WW_TransitSecurityProcessingRequired = 1 AND (WDL_TransportMode IS NULL OR WDL_TransportMode = '' OR WDL_TransportMode = 'AIR') THEN (
				CASE
					WHEN WPS_IsHighRisk = 0 AND OverpackHasValidScreening = 1 THEN 'SCR'
					WHEN WPS_IsHighRisk = 1 AND OverpackHasValidScreening = 1 AND WPS_IsHighRiskAuthorized = 1 THEN 'SCR'
					WHEN WPS_IsHighRisk = 1 AND OverpackHasValidScreening = 1 AND WPS_IsHighRiskAuthorized = 0 THEN 'HRN'
					WHEN WPS_IsHighRisk = 1 THEN 'HRS'
					WHEN WPS_IsSecure = 1 AND CONVERT(NVARCHAR(10), SD_BinaryValue) = 'False' THEN 'SEC'
					ELSE 'REQ'
				END
			)
			ELSE 'NOT'
		END AS SecurityStatus
	FROM
		#OvpPackages AS Overpacks
		JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package
		JOIN dbo.WhsWarehouse ON WW_PK = WPS_WW_Warehouse
		LEFT JOIN dbo.StmData ON SD_Owner = WW_GB_RelatedCompanyBranch AND SD_Name = 'MandatoryPackageScreeningForAirAndUnknownTransportMode'
		LEFT JOIN dbo.WhsItemDispatchLoadList ON WDL_PK = WPS_WDL_LoadList
		OUTER APPLY
		(
			SELECT
				MAX(KPD_PackedTime) AS OverpackLatestPackingTime
			FROM
				dbo.PkgPackageHandlingUnitDivot
			WHERE
				WPS_IsHandlingUnit = 1 AND WPS_UnitType = 'OVP'
				AND KPD_PackedTime IS NOT NULL AND KPD_UnpackedTime IS NULL
				AND KPD_KP_HandlingUnit = WPS_KP_Package
		) AS OverpackDivotView
		LEFT JOIN
		(
			SELECT
				WPS_KP_Package,
				FIRST_VALUE(KPS_Method) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LatestScreeningMethod,
				LEAD(KPS_Method) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LastSecondScreeningMethod,
				FIRST_VALUE(KPS_Time) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LatestScreeningTime,
				LEAD(KPS_Time) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LastSecondScreeningTime,
				FIRST_VALUE(KPS_Passed) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LatestScreeningResult,
				LEAD(KPS_Passed) OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS LastSecondScreeningResult,
				ROW_NUMBER() OVER (PARTITION BY KPS_KP_Package ORDER BY KPS_Time DESC) AS Rownumber
			FROM
				dbo.PkgPackageScreening
				JOIN #OvpPackages ON WPS_KP_Package = KPS_KP_Package
		) AS ScreeningResultView ON ScreeningResultView.WPS_KP_Package = Overpacks.WPS_KP_Package and ScreeningResultView.Rownumber = 1
		OUTER APPLY 
		(
			SELECT 
				CASE 
					WHEN WPS_IsHighRisk = 0 AND LatestScreeningResult = 1 AND (OverpackLatestPackingTime IS NULL OR LatestScreeningTime >= OverpackLatestPackingTime) THEN 1
					WHEN WPS_IsHighRisk = 1 AND LatestScreeningResult = 1 AND LastSecondScreeningResult = 1 AND LatestScreeningMethod != LastSecondScreeningMethod AND (OverpackLatestPackingTime IS NULL OR LastSecondScreeningTime >= OverpackLatestPackingTime) THEN 1
					ELSE 0
				END AS OverpackHasValidScreening
		) AS OverpackHasValidScreeningView
),
SecurityStatusForStandalonePackagesAndOverpacksView AS
(
	SELECT 
		KP_PK,
		WPS_IsHandlingUnit,
		KP_KP_TopHandlingUnitPackage,
		WPS_SecurityStatus AS SecurityStatus
	FROM dbo.WhsItemPackageState
	JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package
	WHERE WPS_WDH_TransitDispatchHeader IS NULL
		AND (WPS_IsHandlingUnit = 0 OR (WPS_UnitType = 'OVP' AND WPS_SecurityStatus != 'SCR'))
		AND WPS_AdjustedOut = ''
		AND EXISTS (SELECT NULL FROM SecurityStatusForOverpacksView WHERE SecurityStatusForOverpacksView.KP_KP_TopHandlingUnitPackage = KP_KP_TopHandlingUnitPackage)

	UNION ALL 

	SELECT 
		KP_PK, 
		WPS_IsHandlingUnit,
		KP_KP_TopHandlingUnitPackage,
		SecurityStatus 
	FROM SecurityStatusForOverpacksView
),
SecurityStatusForTopHandlingUnitView AS
(
	SELECT
		HUPackage.KP_PK,
		CASE
			WHEN HRS > 0 THEN 'HRS'
			WHEN HRN > 0 THEN 'HRN'
			WHEN REQ > 0 THEN 'REQ'
			WHEN _NOT > 0 THEN 'NOT'
			WHEN SEC > 0 THEN 'SEC'
			WHEN SCR > 0 THEN 'SCR'
			WHEN OVR > 0 THEN 'OVR'
			ELSE 'NOT'
		END AS SecurityStatus
	FROM
		dbo.PkgPackage HUPackage
		JOIN dbo.WhsItemPackageState HU ON HU.WPS_KP_Package = HUPackage.KP_PK
		CROSS APPLY (
			SELECT
				SUM(CASE WHEN SecurityStatus = 'REQ' THEN 1 ELSE 0 END) AS REQ,
				SUM(CASE WHEN SecurityStatus = 'NOT' THEN 1 ELSE 0 END) AS _NOT,
				SUM(CASE WHEN SecurityStatus = 'SEC' THEN 1 ELSE 0 END) AS SEC,
				SUM(CASE WHEN SecurityStatus = 'SCR' THEN 1 ELSE 0 END) AS SCR,
				SUM(CASE WHEN SecurityStatus = 'OVR' THEN 1 ELSE 0 END) AS OVR,
				SUM(CASE WHEN SecurityStatus = 'HRS' THEN 1 ELSE 0 END) AS HRS,
				SUM(CASE WHEN SecurityStatus = 'HRN' THEN 1 ELSE 0 END) AS HRN
			FROM
				SecurityStatusForStandalonePackagesAndOverpacksView ChildView
			WHERE 
				ChildView.KP_KP_TopHandlingUnitPackage = HUPackage.KP_PK
			GROUP BY ChildView.KP_KP_TopHandlingUnitPackage
		) AS InnerPackageSecurityStatusCountView
	WHERE HUPackage.KP_KP_TopHandlingUnitPackage IS NULL AND HU.WPS_IsHandlingUnit = 1 AND HU.WPS_UnitType != 'OVP' AND WPS_WDH_TransitDispatchHeader IS NULL
		AND EXISTS (SELECT NULL FROM SecurityStatusForOverpacksView WHERE SecurityStatusForOverpacksView.KP_KP_TopHandlingUnitPackage = HUPackage.KP_PK AND SecurityStatusForOverpacksView.SecurityStatus != 'SCR')
),
SecurityStatusForMidHandlingUnitView AS
(
	SELECT
		MidHUPackage.KP_PK,
		TopHUView.SecurityStatus
	FROM
		dbo.WhsItemPackageState MidHU
		JOIN dbo.PkgPackage MidHUPackage ON MidHUPackage.KP_PK = MidHU.WPS_KP_Package
		JOIN SecurityStatusForTopHandlingUnitView TopHUView ON TopHUView.KP_PK = MidHUPackage.KP_KP_TopHandlingUnitPackage
	WHERE MidHU.WPS_SecurityStatus != TopHUView.SecurityStatus AND MidHU.WPS_IsHandlingUnit = 1 AND MidHU.WPS_UnitType != 'OVP' AND WPS_WDH_TransitDispatchHeader IS NULL
),
SecurityStatusForAllPackagesView AS 
(
	SELECT KP_PK, SecurityStatus FROM SecurityStatusForOverpacksView
	UNION ALL 
	SELECT KP_PK, SecurityStatus FROM SecurityStatusForTopHandlingUnitView
	UNION ALL 
	SELECT KP_PK, SecurityStatus FROM SecurityStatusForMidHandlingUnitView
)

UPDATE
	dbo.WhsItemPackageState
SET WPS_SecurityStatus = SecurityStatus,
	WPS_SystemLastEditTimeUtc = GetUtcDate(),
	WPS_SystemLastEditUser = '~BP',
	WPS_AutoVersion = (WPS_AutoVersion + 1) % 32768
FROM 
	SecurityStatusForAllPackagesView
WHERE KP_PK = WhsItemPackageState.WPS_KP_Package AND SecurityStatus != WhsItemPackageState.WPS_SecurityStatus

drop TABLE #OvpPackages
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
