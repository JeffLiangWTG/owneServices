using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow
{
	public class UpdatePkgStateAndRCNCustomsStatus : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update customs status for packages and rcns";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
---------------------------Pre-Transformation Check---------------------------------
IF EXISTS
(
	SELECT NULL
	FROM
		dbo.CusEntryNum
	WHERE
		CE_ParentTable IN ('PkgPackage', 'WhsItemReceiveConsignment') AND CE_EntryType = 'CRN' AND CE_Category = 'CUS'
)
BEGIN
------------------------------ Temporary Setup -------------------------------------
	CREATE TABLE #PKGView (
		WPS_KP_Package UNIQUEIDENTIFIER,
		WPS_WRC_TransitReceiveConsignment UNIQUEIDENTIFIER
		INDEX RC__WPS_KP_Package CLUSTERED (WPS_KP_Package ASC),
		INDEX RX__WPS_WRC_TransitReceiveConsignment (WPS_WRC_TransitReceiveConsignment ASC)
	);

	DECLARE @trueRegistryValue VARCHAR(MAX)
	SET @trueRegistryValue = '5400720075006500'

------------------------------ Filter PKGs --------------------------------------------
	;WITH WhsBranchPKsToExclude AS
	(
		SELECT 
			WW_GB_RelatedCompanyBranch 
		FROM 
			dbo.WhsWarehouse 
			JOIN dbo.StmData ON WW_GB_RelatedCompanyBranch = SD_Owner
		WHERE 
			WW_WarehouseType = 'TRW'
			AND SD_Name = 'ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation' AND CONVERT(VARCHAR(MAX), SD_BinaryValue, 2) = @trueRegistryValue
	)

	,PKGCustomEntryView AS
	(
		SELECT
			CE_ParentID,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CEN' THEN 1 ELSE 0 END), 0) AS PKG_CENCount,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CRN' THEN 1 ELSE 0 END), 0) AS PKG_CRNCount
		FROM
			dbo.CusEntryNum
		WHERE
			CE_ParentTable = 'PkgPackage' AND CE_Category = 'CUS'
		GROUP BY
			CE_ParentID
	),

	RCNCustomEntryView AS
	(
		SELECT
			CE_ParentID,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CEN' THEN 1 ELSE 0 END), 0) AS RCN_CENCount,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CRN' THEN 1 ELSE 0 END), 0) AS RCN_CRNCount
		FROM 
			dbo.CusEntryNum
		WHERE
			CE_ParentTable = 'WhsItemReceiveConsignment' AND CE_Category = 'CUS'
		GROUP BY
			CE_ParentID
	)

	INSERT INTO #PKGView
	SELECT
		WPS_KP_Package,
		WPS_WRC_TransitReceiveConsignment
	FROM 
		dbo.WhsItemPackageState
		JOIN dbo.WhsWarehouse ON WPS_WW_Warehouse = WW_PK
		OUTER APPLY
		(
			SELECT
				PKG_CENCount,
				PKG_CRNCount
			FROM
				PKGCustomEntryView PKG
			WHERE
				PKG.CE_ParentID = WPS_KP_Package
		) AS PKGCustomEntry
		OUTER APPLY
		(
			SELECT
				RCN_CENCount,
				RCN_CRNCount
			FROM
				RCNCustomEntryView RCN
			WHERE
				RCN.CE_ParentID = WPS_WRC_TransitReceiveConsignment
		) AS RCNCustomEntry
		CROSS APPLY
		(
			SELECT
				ISNULL(PKG_CENCount, 0) AS PKG_CENCountClean,
				ISNULL(PKG_CRNCount, 0) AS PKG_CRNCountClean,
				ISNULL(RCN_CENCount, 0) AS RCN_CENCountClean,
				ISNULL(RCN_CRNCount, 0) AS RCN_CRNCountClean
		) AS Temp
	WHERE
		WPS_UnitType IN ('PKG', 'PKL') AND WPS_WDH_TransitDispatchHeader IS NULL AND WPS_AdjustedOut = ''
		AND WPS_CustomsStatus IN ('CUS', 'NON', '')
		AND WW_WarehouseType = 'TRW'
		AND WW_GB_RelatedCompanyBranch NOT IN 
		(
			SELECT 
				WW_GB_RelatedCompanyBranch 
			FROM
				WhsBranchPKsToExclude
		)
		AND (Temp.PKG_CENCountClean + Temp.RCN_CENCountClean) = 0 AND (Temp.PKG_CRNCountClean + Temp.RCN_CRNCountClean) > 0

------------------------ Transform PKGs -------------------------------
	IF EXISTS
	(
		SELECT NULL
		FROM
			#PKGView
	)
	BEGIN
		;UPDATE
			dbo.WhsItemPackageState
		SET
			WPS_CustomsStatus = 'CLR',
			WPS_SystemLastEditTimeUtc = GETUTCDATE(),
			WPS_SystemLastEditUser = '~BP'
		WHERE
			WPS_KP_Package IN (SELECT WPS_KP_Package FROM #PKGView)

---------------------------- Filter RCNs --------------------------------------
		;WITH #PKGViewToUpdateRCN AS
		(
		SELECT
			WPS_WRC_TransitReceiveConsignment,
			COUNT(WPS_KP_Package) AS PKG_Count,
			ISNULL(SUM(CASE WHEN WPS_CustomsStatus = 'CLR' THEN 1 ELSE 0 END), 0) AS PKG_CLRCount,
			ISNULL(SUM(CASE WHEN WPS_CustomsStatus = 'CUS' THEN 1 ELSE 0 END), 0) AS PKG_CUSCount,
			ISNULL(SUM(CASE WHEN WPS_CustomsStatus = 'PAN' THEN 1 ELSE 0 END), 0) AS PKG_PANCount,
			ISNULL(SUM(CASE WHEN (WPS_CustomsStatus = 'NON' OR WPS_CustomsStatus = '') THEN 1 ELSE 0 END), 0) AS PKG_NONCount,
			MIN(CAST(WW_IsCustomsControlled AS INT)) AS IsCustomsControlled,
			MIN(CAST(WW_IsPortAuthorityControlled AS INT)) AS IsPortControlled
		FROM
			dbo.WhsItemPackageState
			JOIN dbo.WhsWarehouse ON WPS_WW_Warehouse = WW_PK
		WHERE
			WPS_WRC_TransitReceiveConsignment IN (SELECT WPS_WRC_TransitReceiveConsignment FROM #PKGView)
			AND WPS_UnitType IN ('PKG', 'PKL') AND WPS_WDH_TransitDispatchHeader IS NULL AND WPS_AdjustedOut = ''
		GROUP BY
			WPS_WRC_TransitReceiveConsignment
		)

		,RCNCustomEntryView AS
		(
		SELECT
			CE_ParentID,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'PAN' THEN 1 ELSE 0 END), 0) AS RCN_PANCount,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CEN' THEN 1 ELSE 0 END), 0) AS RCN_CENCount,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'CRN' THEN 1 ELSE 0 END), 0) AS RCN_CRNCount,
			ISNULL(SUM(CASE WHEN CE_EntryType = 'PAN' AND CE_EntryStatus = 'CLR' THEN 1 ELSE 0 END), 0) AS RCN_PANClearedCount
		FROM
			dbo.CusEntryNum
		WHERE
			CE_ParentTable = 'WhsItemReceiveConsignment' AND CE_Category IN ('CUS', 'PRT')
			AND CE_ParentID IN (SELECT WPS_WRC_TransitReceiveConsignment FROM #PKGViewToUpdateRCN)
		GROUP BY
			CE_ParentID
		)

		,RCNCustomStatusView AS
		(
		SELECT
			WPS_WRC_TransitReceiveConsignment,
			CASE
				WHEN ((Temp.RCN_PANCount > 0 AND Temp.RCN_PANClearedCount = Temp.RCN_PANCount) AND (Temp.RCN_CRNCount > 0 AND Temp.RCN_CENCount <= Temp.RCN_CRNCount)) OR (Temp.PKG_Count > 0 AND Temp.PKG_Count = Temp.PKG_CLRCount) THEN 'CLR'
				WHEN IsCustomsControlled = 0 AND IsPortControlled = 0 AND Temp.RCN_PANCount = 0 AND Temp.RCN_CENCount = 0 AND Temp.RCN_CRNCount = 0 AND (Temp.PKG_Count = 0 OR Temp.PKG_Count = Temp.PKG_NONCount) THEN 'NON'
				WHEN (IsCustomsControlled = 1 AND Temp.RCN_CRNCount = 0) OR (Temp.RCN_CENCount > 0 AND Temp.RCN_CENCount > Temp.RCN_CRNCount) OR (Temp.PKG_CUSCount > 0) THEN 'CUS'
				WHEN (IsPortControlled = 1 AND Temp.RCN_PANCount = 0) OR (Temp.RCN_PANCount > 0 AND Temp.RCN_PANCount > Temp.RCN_PANClearedCount) OR (Temp.PKG_PANCount > 0) THEN 'PAN'
				ELSE 'CLR'
			END AS CustomStatus
		FROM 
			#PKGViewToUpdateRCN
			OUTER APPLY
			(
				SELECT
					CE_ParentID,
					RCN_PANCount,
					RCN_CENCount,
					RCN_CRNCount,
					RCN_PANClearedCount
				FROM 
					RCNCustomEntryView
				WHERE
					CE_ParentID = WPS_WRC_TransitReceiveConsignment
			) AS RCNCustomEntry
			CROSS APPLY 
			(
				SELECT 
					ISNULL(PKG_Count, 0) AS PKG_Count,
					ISNULL(PKG_CLRCount, 0) AS PKG_CLRCount,
					ISNULL(PKG_PANCount, 0) AS PKG_PANCount,
					ISNULL(PKG_NONCount, 0) AS PKG_NONCount,
					ISNULL(PKG_CUSCount, 0) AS PKG_CUSCount,
					ISNULL(RCN_PANCount, 0) AS RCN_PANCount,
					ISNULL(RCN_CENCount, 0) AS RCN_CENCount,
					ISNULL(RCN_CRNCount, 0) AS RCN_CRNCount,
					ISNULL(RCN_PANClearedCount, 0) AS RCN_PANClearedCount
			) AS Temp
		)

------------------------------------ Transform RCNs ----------------------------------------
		UPDATE
			dbo.WhsItemReceiveConsignment
		SET
			WRC_CustomsStatus = StatusView.CustomStatus,
			WRC_SystemLastEditTimeUtc = GETUTCDATE(),
			WRC_SystemLastEditUser = '~BP'
		FROM
			RCNCustomStatusView AS StatusView
		WHERE 
			StatusView.WPS_WRC_TransitReceiveConsignment = WhsItemReceiveConsignment.WRC_PK 
			AND StatusView.CustomStatus != WhsItemReceiveConsignment.WRC_CustomsStatus

-------------------------------------- Filter Handling Units --------------------------
		;WITH TopHUView AS
		(
		SELECT
			KP_KP_TopHandlingUnitPackage
		FROM
			#PKGView
			JOIN dbo.PkgPackage ON KP_PK = WPS_KP_package
		WHERE
			KP_KP_TopHandlingUnitPackage IS NOT NULL
		)

		,TopHUCustomStatusView AS
		(
		SELECT
			KP_PK,
			CASE
				WHEN Child_Count = 0 OR Child_Count = PKG_NONCount Then 'NON'
				WHEN PKG_CUSCount > 0 Then 'CUS'
				WHEN PKG_PANCount > 0 Then 'PAN'
				ELSE 'CLR'
			END AS CustomStatus
		FROM
			dbo.PkgPackage HUPackage
			JOIN dbo.WhsItemPackageState HU ON HU.WPS_KP_Package = HUPackage.KP_PK
			CROSS APPLY 
			(
				SELECT
					COUNT(KP_PK) AS Child_Count,
					ISNULL(SUM(CASE WHEN WPS_CustomsStatus='CLR' THEN 1 ELSE 0 END), 0) AS PKG_CLRCount,
					ISNULL(SUM(CASE WHEN WPS_CustomsStatus='CUS' THEN 1 ELSE 0 END), 0) AS PKG_CUSCount,
					ISNULL(SUM(CASE WHEN WPS_CustomsStatus='PAN' THEN 1 ELSE 0 END), 0) AS PKG_PANCount,
					ISNULL(SUM(CASE WHEN (WPS_CustomsStatus='NON' OR WPS_CustomsStatus='') THEN 1 ELSE 0 END), 0) AS PKG_NONCount
				FROM
					dbo.PkgPackage ChildPackage
					JOIN dbo.WhsItemPackageState Child ON Child.WPS_KP_Package = ChildPackage.KP_PK
				WHERE
					ChildPackage.KP_KP_TopHandlingUnitPackage = HUPackage.KP_PK
					AND Child.WPS_WDH_TransitDispatchHeader IS NULL AND Child.WPS_IsHandlingUnit = 0 AND Child.WPS_AdjustedOut = ''
			) AS InnerPackageCustomStatusCountView
		WHERE
			HUPackage.KP_KP_TopHandlingUnitPackage IS NULL
			AND HU.WPS_IsHandlingUnit = 1 AND HU.WPS_WDH_TransitDispatchHeader IS NULL AND HU.WPS_AdjustedOut = ''
			AND HUPackage.KP_PK IN (SELECT KP_KP_TopHandlingUnitPackage FROM TopHUView)
		)

		,MidHUCustomStatusView AS
		(
		SELECT
			MidHUPackage.KP_PK,
			TopHUView.CustomStatus
		FROM
			dbo.WhsItemPackageState MidHU
			JOIN dbo.PkgPackage MidHUPackage ON MidHUPackage.KP_PK = MidHU.WPS_KP_Package
			JOIN TopHUCustomStatusView TopHUView ON TopHUView.KP_PK = MidHUPackage.KP_KP_TopHandlingUnitPackage
		WHERE MidHU.WPS_CustomsStatus != TopHUView.CustomStatus AND MidHU.WPS_IsHandlingUnit = 1 AND MidHU.WPS_WDH_TransitDispatchHeader IS NULL AND MidHU.WPS_AdjustedOut = ''
		)

		,AllHUCustomStatusView AS
		(
		SELECT KP_PK, CustomStatus FROM TopHUCustomStatusView
		UNION ALL
		SELECT KP_PK, CustomStatus FROM MidHUCustomStatusView
		)

-------------------------------- Transform Handling Units ----------------------------
		UPDATE
			dbo.WhsItemPackageState
		SET
			WPS_CustomsStatus = AllHUCustomStatusView.CustomStatus,
			WPS_SystemLastEditTimeUtc = GETUTCDATE(),
			WPS_SystemLastEditUser = '~BP'
		FROM
			AllHUCustomStatusView
		WHERE 
			AllHUCustomStatusView.KP_PK = WhsItemPackageState.WPS_KP_Package 
			AND AllHUCustomStatusView.CustomStatus != WhsItemPackageState.WPS_CustomsStatus
	END
-------------------------- Cleanup--------------------------------------
	;DROP TABLE #PKGView
END
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusEntryNumSchema.Instance)
					.Key(new string[] { CusEntryNumSchema.Constants.CE_ParentTable, CusEntryNumSchema.Constants.CE_Category })
					.GetInfo();

				indexProvider.New(WhsItemPackageStateSchema.Instance)
					.Key(WhsItemPackageStateSchema.Constants.WPS_CustomsStatus)
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
