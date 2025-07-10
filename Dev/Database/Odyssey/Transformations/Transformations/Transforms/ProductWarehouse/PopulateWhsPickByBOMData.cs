using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsPickByBOMData : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Populate data for Pick By BOM refactor.";

		const string FromTimeName = "PopulateWhsPickByBOMData.From";
		const string MaxToTimeName = "PopulateWhsPickByBOMData.MaxTo";
		const short DayInterval = 7;

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = GetUpdateSQL(isOfflinePhase: true);
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var fromTimeString = ExtProperty.Database.Select(Db.Connection, FromTimeName);
			var maxToTimeString = ExtProperty.Database.Select(Db.Connection, MaxToTimeName);
			DateTime? fromTime;
			DateTime? maxToTime;
			if (string.IsNullOrEmpty(fromTimeString) || string.IsNullOrEmpty(maxToTimeString))
			{
				(fromTime, maxToTime) = GetInitialTime();
			}
			else
			{
				SqlFormatInfo.TryParseFromSqlDateTime(fromTimeString, out var parsedFromTime);
				fromTime = parsedFromTime;
				SqlFormatInfo.TryParseFromSqlDateTime(maxToTimeString, out var parsedMaxToTime);
				maxToTime = parsedMaxToTime;
			}

			var loggingStopWatch = Stopwatch.StartNew();
			while (fromTime != null && fromTime < maxToTime)
			{
				var maxProcessedFinalizedDate = fromTime.Value.AddDays(DayInterval);
				UpdateChunk(fromTime.Value, maxProcessedFinalizedDate);
				fromTime = maxProcessedFinalizedDate;
				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, FromTimeName, SqlFormatInfo.ToSqlDateTimeString(maxProcessedFinalizedDate));
					manager?.ShowInfoMessage($@"Processed Picks with WP_FinalizedDateUtc up to {maxProcessedFinalizedDate}.");
					token.ThrowIfCancellationRequested();
					loggingStopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, FromTimeName);
			ExtProperty.Database.Delete(Db.Connection, MaxToTimeName);
		}

		(DateTime? FromTime, DateTime? ToTime) GetInitialTime()
		{
			DateTime? fromTime = null;
			DateTime? maxToTime = null;
			var getTimeSql = @"
SELECT
	DATEADD(MINUTE, -1, MIN(WP_FinalizedDateUtc)) AS MinFinalisedTime,
	DATEADD(MINUTE, 1, MAX(WP_FinalizedDateUtc)) AS MaxFinalisedTime
FROM
	dbo.WhsPick";
			using (var command = Db.Connection.Command(getTimeSql))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						fromTime = reader["MinFinalisedTime"] is DateTime minTime ? minTime : null;
						maxToTime = reader["MaxFinalisedTime"] is DateTime maxTime ? maxTime : null;
					}
				}
			}

			if (fromTime != null)
			{
				ExtProperty.Database.Update(Db.Connection, FromTimeName, SqlFormatInfo.ToSqlDateTimeString(fromTime.Value));
				ExtProperty.Database.Update(Db.Connection, MaxToTimeName, SqlFormatInfo.ToSqlDateTimeString(maxToTime.Value));
			}

			return (fromTime, maxToTime);
		}

		static void UpdateChunk(DateTime minFinalisedTime, DateTime maxFinalisedTime)
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			using (var command = Db.Connection.Command(GetUpdateSQL(isOfflinePhase: false)))
			{
				command.AddParameter("@MinFinalisedTime", SqlDbType.DateTime, minFinalisedTime);
				command.AddParameter("@MaxFinalisedTime", SqlDbType.DateTime, maxFinalisedTime);
				command.ExecuteNonQuery();
				manager.CommitTransaction();
			}
		}

		#region GetUpdateSQL

		static string BatchPickSQL => @"
SELECT
	DISTINCT WP_PK AS FinalisedPickPK
INTO
	#WhsFinalisedPick
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket WhsOrder ON WhsOrder.WD_WP = WP_PK
	JOIN dbo.WhsDocketLine ComponentLine ON ComponentLine.WE_WD = WD_PK
WHERE
	WP_FinalizedDateUtc >= @MinFinalisedTime
	AND WP_FinalizedDateUtc < @MaxFinalisedTime
	AND ComponentLine.WE_DocketLineType = 'ORD'
	AND ComponentLine.WE_WE_ParentDocketLine IS NOT NULL
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsDocket ReceiveFromPick
		WHERE
			ReceiveFromPick.WD_WP_ParentPickForReceive = WP_PK
	)
";

		static string DisableTriggersSQL => @"
DISABLE TRIGGER TG_WhsDocketLine_LocationIsInCorrectWarehouse ON dbo.WhsDocketLine;
DISABLE TRIGGER TG_WhsPickLine_PreventReleaseCapturingIfNotOnAllocatedOrder ON dbo.WhsPickLine;
";

		static string EnableTriggersSQL => @"
ENABLE TRIGGER TG_WhsDocketLine_LocationIsInCorrectWarehouse ON dbo.WhsDocketLine;
ENABLE TRIGGER TG_WhsPickLine_PreventReleaseCapturingIfNotOnAllocatedOrder ON dbo.WhsPickLine;
";

		static string GetUpdateSQL(bool isOfflinePhase)
		{
			var joinAndWhereClause = isOfflinePhase ? @"
	WHERE
		WD_DocketType = 'ORD'
		AND ComponentLine.WE_WE_ParentDocketLine IS NOT NULL
		AND WP_PickStatus <> 'FIN'
		AND WP_PickStatus <> 'CAN'
		AND NOT EXISTS
		(
			SELECT
				NULL
			FROM
				dbo.WhsDocket ReceiveFromPick
			WHERE
				ReceiveFromPick.WD_WP_ParentPickForReceive = WP_PK
		)
" : @"WHERE
		ComponentLine.WE_WE_ParentDocketLine IS NOT NULL
		AND WP_PK IN (SELECT FinalisedPickPK FROM #WhsFinalisedPick)
	OPTION (MAXDOP 1)";

			var maxDOP = isOfflinePhase ? "" : "OPTION (MAXDOP 1)";

			return @$"
BEGIN TRY
	{(isOfflinePhase ? "" : BatchPickSQL)}

	-- select component lines on picks
	SELECT
		WD_OH_Client,
		WD_WW_Whs,
		WD_WP,
		ComponentLine.WE_PK AS ComponentOrderLinePK,
		ComponentLine.WE_WE_ParentDocketLine AS OrderKitLinePK,
		WP_SystemCreateTimeUtc AS ArrivalDate,
		WP_FinalizedDateUtc,
		WP_SystemLastEditUser,
		WP_PickNo,
		ParentLine.WE_OP AS KitProduct,
		ComponentLine.WE_OP AS ComponentProduct,
		OE_F3_NKPackType AS ComponentPackType,
		OE_ComponentQty AS BOMComponentQty,
		OP_StockKeepingUnit AS ComponentSKU,
		FIRST_VALUE(NEWID()) OVER (PARTITION BY WD_OH_Client, WD_WP ORDER BY WD_WP) AS NewReceivePK,
		FIRST_VALUE(NEWID()) OVER (PARTITION BY ComponentLine.WE_WE_ParentDocketLine ORDER BY ComponentLine.WE_WE_ParentDocketLine) AS NewReceiveLinePK
	INTO #WhsOrderComponentLines
	FROM
		dbo.WhsDocket WhsOrder
		JOIN dbo.WhsPick ON WP_PK = WhsOrder.WD_WP
		JOIN dbo.WhsDocketLine ComponentLine ON ComponentLine.WE_WD = WD_PK
		JOIN dbo.WhsDocketLine ParentLine ON ParentLine.WE_PK = ComponentLine.WE_WE_ParentDocketLine
		JOIN dbo.OrgPartBOM ON OE_OP_MainProduct = ParentLine.WE_OP AND OE_OP_Component = ComponentLine.WE_OP AND OE_F3_NKPackType = ComponentLine.WE_F3_NKPackType AND OE_IsValid = 1
		JOIN dbo.OrgSupplierPart ON OP_PK = OE_OP_Component
		{joinAndWhereClause}

	IF @@ROWCOUNT > 0
	BEGIN
		EXEC dbo.SuspendTrigger TG_WhsDocketLine_StockOnHandIsBalanced
		EXEC dbo.SuspendTrigger TG_WhsDocketLine_StockOnHandIsBalanced_Insert
		EXEC dbo.SuspendTrigger TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert
		EXEC dbo.SuspendTrigger TG_WhsPickLine_TransactionAndPickedQtyIsCorrect
		EXEC dbo.SuspendTrigger TG_PreventOverReduceOfStockViaInventoryLine
		EXEC dbo.SuspendTrigger TG_WhsOrder_EnsureDDLIsEnteredOnPick
		EXEC dbo.SuspendTrigger TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised
		EXEC dbo.SuspendTrigger TG_PreventOverfillLocationWithUnitsCapacity;

		{(isOfflinePhase ? DisableTriggersSQL : "")}

		-- pick line & outbound transfer line info
		SELECT
			WD_OH_Client,
			WD_WW_Whs,
			WD_WP,
			ComponentOrderLinePK,
			OrderKitLinePK,
			ArrivalDate,
			WP_FinalizedDateUtc,
			WP_SystemLastEditUser,
			WP_PickNo,
			KitProduct,
			ComponentProduct,
			ComponentPackType,
			BOMComponentQty,
			ComponentSKU,
			NewReceivePK,
			NewReceiveLinePK,
			SUM(PickLineQty) OVER (PARTITION BY ComponentOrderLinePK) AS PickLineQuantity,
			MIN(CASE WHEN FinalisedTime IS NOT NULL THEN 1 ELSE 0 END) OVER (PARTITION BY OrderKitLinePK) AS IsFullyStaged,
			MAX(PickedTime) OVER (PARTITION BY OrderKitLinePK) AS LastPickedTime,
			FIRST_VALUE(Picker) OVER (PARTITION BY OrderKitLinePK ORDER BY PickedTime DESC, Picker ASC, PickLocation ASC, ToLocation ASC, FinalisedTime DESC) AS LastPickedBy,
			FIRST_VALUE(PickLocation) OVER (PARTITION BY OrderKitLinePK ORDER BY PickedTime DESC, Picker ASC, PickLocation ASC, ToLocation ASC, FinalisedTime DESC) AS LastPickedLocation,
			FIRST_VALUE(ToLocation) OVER (PARTITION BY OrderKitLinePK ORDER BY PickedTime DESC, Picker ASC, PickLocation ASC, ToLocation ASC, FinalisedTime DESC) AS LastTransferToLocation,
			MAX(FinalisedTime) OVER (PARTITION BY OrderKitLinePK) AS LastTranserLineFinalisedTime,
			OutboundTransferPK,
			OutboundTransferLinePK
		INTO #WhsOrderComponentPickLine
		FROM
			(
				SELECT
					ComponentLines.*,
					TransactionPickLine.WZ_Units AS PickLineQty,
					OutboundPickLine.WZ_PickedDateTime AS PickedTime,
					OutboundPickLine.WZ_GS_NKAssignedTo AS Picker,
					OutboundTransferLine.WE_FinalisedDate AS FinalisedTime,
					OutboundTransferLine.WE_WL_TransferFrom AS PickLocation,
					OutboundTransferLine.WE_WL AS ToLocation,
					OutboundTransferLine.WE_WD AS OutboundTransferPK,
					OutboundTransferLine.WE_PK AS OutboundTransferLinePK
				FROM
					#WhsOrderComponentLines ComponentLines
					LEFT JOIN dbo.WhsPickLine TransactionPickLine ON TransactionPickLine.WZ_WE_TransactionLine = ComponentOrderLinePK
					LEFT JOIN dbo.WhsPickLine OutboundPickLine ON OutboundPickLine.WZ_WE_TransactionLine = TransactionPickLine.WZ_WE_InventoryLine
					LEFT JOIN dbo.WhsDocketLine OutboundTransferLine ON OutboundTransferLine.WE_PK = TransactionPickLine.WZ_WE_InventoryLine AND OutboundTransferLine.WE_PK = OutboundPickLine.WZ_WE_TransactionLine
					LEFT JOIN dbo.WhsDocketLine OriginalInventoryLine ON OriginalInventoryLine.WE_PK = TransactionPickLine.WZ_WE_InventoryLine
				WHERE
					TransactionPickLine.WZ_WE_OriginalPickedInventoryLine IS NOT NULL

				UNION ALL

				SELECT
					ComponentLines.*,
					TransactionPickLine.WZ_Units AS PickLineQty,
					TransactionPickLine.WZ_PickedDateTime AS PickedTime,
					TransactionPickLine.WZ_GS_NKAssignedTo AS Picker,
					NULL AS FinalisedTime,
					InventoryLine.WE_WL AS PickLocation,
					NULL AS ToLocation,
					NULL AS OutboundTransferPK,
					NULL AS OutboundTransferLinePK
				FROM
					#WhsOrderComponentLines ComponentLines
					LEFT JOIN dbo.WhsPickLine TransactionPickLine ON TransactionPickLine.WZ_WE_TransactionLine = ComponentOrderLinePK
					LEFT JOIN dbo.WhsDocketLine InventoryLine ON InventoryLine.WE_PK = TransactionPickLine.WZ_WE_InventoryLine
				WHERE
					TransactionPickLine.WZ_WE_OriginalPickedInventoryLine IS NULL
			) PickLineInfo
		{maxDOP}

		CREATE NONCLUSTERED INDEX [NR_RX__OutboundTransferLinePK] ON [#WhsOrderComponentPickLine] ([OutboundTransferLinePK] ASC)
		INCLUDE ( [LastPickedLocation] )

		CREATE NONCLUSTERED INDEX [NR_RX__IsFullyStaged] ON [#WhsOrderComponentPickLine] ([IsFullyStaged] ASC)
		INCLUDE ( [LastPickedLocation], [OutboundTransferLinePK] )
		WHERE IsFullyStaged = 1

		-- append picking info aggregated by kits
		SELECT
			ComponentLine.*,
			PartBOMQuantity,
			MIN(ISNULL(PickLineQuantity, 0) / PartBOMQuantity) OVER (PARTITION BY OrderKitLinePK) AS MinKitQuantity
		INTO #WhsOrderComponentLinesWithPickingInfo
		FROM
			(
				SELECT
					ComponentOrderLinePK,
					OrderKitLinePK,
					WD_OH_Client,
					WD_WW_Whs,
					WD_WP,
					WP_SystemLastEditUser,
					NewReceivePK,
					NewReceiveLinePK,
					OutboundTransferPK,
					KitProduct,
					LastPickedBy,
					ArrivalDate,
					WP_FinalizedDateUtc,
					WP_PickNo,
					LastPickedLocation,
					LastTransferToLocation,
					LastTranserLineFinalisedTime,
					IsFullyStaged,
					PickLineQuantity,
					LastPickedTime,
					ComponentProduct,
					ComponentPackType,
					ComponentSKU,
					BOMComponentQty
				FROM
				(
					SELECT
						ComponentOrderLinePK,
						OrderKitLinePK,
						WD_OH_Client,
						WD_WW_Whs,
						WD_WP,
						WP_SystemLastEditUser,
						NewReceivePK,
						NewReceiveLinePK,
						KitProduct,
						LastPickedBy,
						ArrivalDate,
						WP_FinalizedDateUtc,
						WP_PickNo,
						LastPickedLocation,
						LastTransferToLocation,
						LastTranserLineFinalisedTime,
						IsFullyStaged,
						ISNULL(PickLineQuantity, 0) AS PickLineQuantity,
						LastPickedTime,
						ComponentProduct,
						ComponentPackType,
						ComponentSKU,
						BOMComponentQty,
						MAX(OutboundTransferPK) OVER (PARTITION BY ComponentOrderLinePK ORDER BY ComponentOrderLinePK) AS OutboundTransferPK,
						ROW_NUMBER() OVER (PARTITION BY ComponentOrderLinePK ORDER BY ComponentOrderLinePK) AS TheRowNumber
					FROM
						#WhsOrderComponentPickLine
				) GroupedComponents
				WHERE
					TheRowNumber = 1
			) ComponentLine
			CROSS APPLY dbo.OrgSupplierPartUnitsConverter(ComponentProduct, ComponentPackType, ComponentSKU) AS UnitsConverter
			CROSS APPLY
			(
				SELECT BOMComponentQty * IIF(UnitsConverter.ConversionFactor = 0, 1, UnitsConverter.ConversionFactor) AS PartBOMQuantity
			) PartBOMQuantity
		{maxDOP}

		-- select kit lines
		SELECT
			OrderKitLinePK,
			WD_OH_Client,
			WD_WW_Whs,
			WD_WP,
			WP_SystemLastEditUser,
			ArrivalDate,
			WP_FinalizedDateUtc,
			WP_PickNo,
			NewReceivePK,
			NewReceiveLinePK,
			OutboundTransferPK,
			CASE WHEN IsFullyStaged = 1 THEN NEWID() END AS NewTransferLinePK,
			KitProduct,
			OP_StockKeepingUnit,
			MinKitQuantity AS TransactionQuantity,
			IsFullyStaged,
			CASE WHEN WP_FinalizedDateUtc IS NULL THEN CASE WHEN IsFullyStaged = 1 THEN 0 ELSE MinKitQuantity END ELSE 0 END AS StockOnHand,
			CASE WHEN WP_FinalizedDateUtc IS NULL THEN CASE WHEN IsFullyStaged = 1 THEN 'PUT' ELSE 'PND' END ELSE 'AVL' END AS ReceiveLineInventoryStatus,
			CASE WHEN WP_FinalizedDateUtc IS NULL THEN CASE WHEN IsFullyStaged = 1 THEN 'PFU' ELSE '' END ELSE 'FIN' END AS ReceiveLineStatus,
			CASE WHEN WP_FinalizedDateUtc IS NULL THEN '' ELSE WP_SystemLastEditUser END AS ReceiveFinalisedBy,
			WP_FinalizedDateUtc AS ReceiveFinalisedDate,
			CASE WHEN IsFullyStaged = 1 OR WP_FinalizedDateUtc IS NOT NULL THEN LastPickedLocation END AS LastPickedLocation,
			CASE WHEN IsFullyStaged = 1 THEN LastTransferToLocation END AS LastTransferToLocation,
			CASE WHEN IsFullyStaged = 1 THEN LastPickedBy END AS LastPickedBy,
			CASE WHEN IsFullyStaged = 1 THEN LastTranserLineFinalisedTime END AS LastTranserLineFinalisedTime
		INTO #WhsOrderKitLines
		FROM
		(
			SELECT
				*
			FROM
			(
				SELECT
					OrderKitLinePK,
					WD_OH_Client,
					WD_WW_Whs,
					WD_WP,
					WP_SystemLastEditUser,
					NewReceivePK,
					NewReceiveLinePK,
					KitProduct,
					MinKitQuantity,
					LastPickedBy,
					ArrivalDate,
					WP_FinalizedDateUtc,
					WP_PickNo,
					LastPickedLocation,
					LastTransferToLocation,
					LastTranserLineFinalisedTime,
					IsFullyStaged,
					MAX(OutboundTransferPK) OVER (PARTITION BY OrderKitLinePK ORDER BY OrderKitLinePK) AS OutboundTransferPK,
					ROW_NUMBER() OVER (PARTITION BY OrderKitLinePK ORDER BY OrderKitLinePK) AS TheRowNumber
				FROM
					#WhsOrderComponentLinesWithPickingInfo
				WHERE
					MinKitQuantity > 0
			) GroupedKits
			WHERE
				TheRowNumber = 1
		) GroupedByParentLine
		JOIN dbo.OrgSupplierPart ON OP_PK = KitProduct
		{maxDOP}

		CREATE CLUSTERED INDEX [NR_RC__IsFullyStaged] ON [#WhsOrderKitLines] ([IsFullyStaged] ASC)

		-- create Receive
		INSERT INTO dbo.WhsDocket
		(
			WD_PK,
			WD_OH_Client,
			WD_WW_Whs,
			WD_WP_ParentPickForReceive,
			WD_BookingDate,
			WD_ArrivalDate,
			WD_FinalisedDate,
			WD_UnloadCompletedTime,
			WD_DocketStatus,
			WD_GS_NKFinalizedBy,
			WD_DocketType,
			WD_DocketSubType,
			WD_DocketID,
			WD_ExternalReference,
			WD_SystemCreateTimeUtc,
			WD_SystemCreateUser,
			WD_SystemLastEditTimeUtc,
			WD_SystemLastEditUser
		)
		SELECT
			NewReceivePK,
			WD_OH_Client,
			WD_WW_Whs,
			WD_WP,
			ArrivalDate,
			ArrivalDate,
			ReceiveFinalisedDate,
			ReceiveFinalisedDate,
			ReceiveStatus,
			ReceiveFinalisedBy,
			'INW',
			'REC',
			DocketID,
			DocketID,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
		(
			SELECT
				*
			FROM
			(
				SELECT
					NewReceivePK,
					WD_OH_Client,
					WD_WW_Whs,
					WD_WP,
					ArrivalDate,
					WP_PickNo AS DocketID,
					CASE WHEN WP_FinalizedDateUtc IS NULL THEN 'ENT' ELSE 'FIN' END AS ReceiveStatus,
					ReceiveFinalisedDate,
					ReceiveFinalisedBy,
					ROW_NUMBER() OVER (PARTITION BY NewReceivePK, WD_OH_Client, WD_WW_Whs, WD_WP ORDER BY NewReceivePK) AS TheRowNumber
				FROM
					#WhsOrderKitLines
			) GroupedLines
			WHERE
				TheRowNumber = 1
		) KitLines
		{maxDOP}

		-- create Receive Line for Kits
		INSERT INTO dbo.WhsDocketLine
		(
			WE_PK,
			WE_WD,
			WE_OP,
			WE_TransactionQuantity,
			WE_ClientOrderedUnits,
			WE_StockOnHand,
			WE_F3_NKPackType,
			WE_DocketLineType,
			WE_WE_OriginalDocketLineForRating,
			WE_CurrentInventoryStatus,
			WE_OriginalInventoryStatus,
			WE_DocketLineStatus,
			WE_WL,
			WE_FinalisedDate,
			WE_AdjustmentArrivalDate,
			WE_UnloadedTime,
			WE_GS_NKUnloadedBy,
			WE_SystemCreateTimeUtc,
			WE_SystemCreateUser,
			WE_SystemLastEditTimeUtc,
			WE_SystemLastEditUser
		)
		SELECT
			NewReceiveLinePK,
			NewReceivePK,
			KitProduct,
			TransactionQuantity,
			TransactionQuantity,
			StockOnHand,
			OP_StockKeepingUnit,
			'INW',
			NewReceiveLinePK,
			ReceiveLineInventoryStatus,
			ReceiveLineInventoryStatus,
			ReceiveLineStatus,
			LastPickedLocation,
			ReceiveFinalisedDate,
			CASE WHEN IsFullyStaged = 1 OR WP_FinalizedDateUtc IS NOT NULL THEN ArrivalDate END,
			CASE WHEN IsFullyStaged = 1 THEN LastTranserLineFinalisedTime END,
			CASE WHEN IsFullyStaged = 1 THEN LastPickedBy ELSE '' END,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
			#WhsOrderKitLines
		{maxDOP}
		
		-- create Outbound Transfer Line for Kits
		INSERT INTO dbo.WhsDocketLine
		(
			WE_PK,
			WE_WD,
			WE_OP,
			WE_TransactionQuantity,
			WE_StockOnHand,
			WE_F3_NKPackType,
			WE_DocketLineType,
			WE_WE_OriginalDocketLineForRating,
			WE_CurrentInventoryStatus,
			WE_OriginalInventoryStatus,
			WE_DocketLineStatus,
			WE_WL_TransferFrom,
			WE_WL,
			WE_FinalisedDate,
			WE_AdjustmentArrivalDate,
			WE_PutawayTime,
			WE_GS_NKPutawayBy,
			WE_SystemCreateTimeUtc,
			WE_SystemCreateUser,
			WE_SystemLastEditTimeUtc,
			WE_SystemLastEditUser
		)
		SELECT
			NewTransferLinePK,
			OutboundTransferPK,
			KitProduct,
			TransactionQuantity,
			CASE WHEN WP_FinalizedDateUtc IS NULL THEN TransactionQuantity ELSE 0 END,
			OP_StockKeepingUnit,
			'TFR',
			NewReceiveLinePK,
			'STA',
			'AVL',
			'FIN',
			LastPickedLocation,
			LastTransferToLocation,
			LastTranserLineFinalisedTime,
			ArrivalDate,
			LastTranserLineFinalisedTime,
			LastPickedBy,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
			#WhsOrderKitLines
		WHERE
			IsFullyStaged = 1
		{maxDOP}

		-- pick line for the kit line
		INSERT INTO dbo.WhsPickLine
		(
			WZ_PK,
			WZ_Units,
			WZ_WE_InventoryLine,
			WZ_WE_OriginalPickedInventoryLine,
			WZ_WE_TransactionLine,
			WZ_PickedDateTime,
			WZ_GS_NKAssignedTo,
			WZ_SystemCreateTimeUtc,
			WZ_SystemCreateUser,
			WZ_SystemLastEditTimeUtc,
			WZ_SystemLastEditUser
		)
		SELECT
			NEWID(),
			TransactionQuantity,
			CASE WHEN IsFullyStaged = 1 THEN NewTransferLinePK ELSE NewReceiveLinePK END,
			CASE WHEN IsFullyStaged = 1 THEN NewReceiveLinePK END,
			OrderKitLinePK,
			WP_FinalizedDateUtc,
			CASE WHEN WP_FinalizedDateUtc IS NOT NULL THEN WP_SystemLastEditUser ELSE '' END,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
			#WhsOrderKitLines
		{maxDOP}

		-- pick line for the transfer line
		INSERT INTO dbo.WhsPickLine
		(
			WZ_PK,
			WZ_Units,
			WZ_WE_InventoryLine,
			WZ_WE_TransactionLine,
			WZ_WE_OriginalOrderLine,
			WZ_PickedDateTime,
			WZ_GS_NKAssignedTo,
			WZ_SystemCreateTimeUtc,
			WZ_SystemCreateUser,
			WZ_SystemLastEditTimeUtc,
			WZ_SystemLastEditUser
		)
		SELECT
			NEWID(),
			TransactionQuantity,
			NewReceiveLinePK,
			NewTransferLinePK,
			OrderKitLinePK,
			LastTranserLineFinalisedTime,
			LastPickedBy,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
			#WhsOrderKitLines
		WHERE
			IsFullyStaged = 1
		{maxDOP}

		INSERT INTO dbo.WhsBOMInventoryPivot
		(
			WIP_PK,
			WIP_ComponentQuantity,
			WIP_WE_ComponentLine,
			WIP_WE_InventoryLine,
			WIP_SystemCreateTimeUtc,
			WIP_SystemCreateUser,
			WIP_SystemLastEditTimeUtc,
			WIP_SystemLastEditUser
		)
		SELECT
			NEWID(),
			MinKitQuantity * PartBOMQuantity,
			ComponentOrderLinePK,
			NewReceiveLinePK,
			GetUtcDate(),
			'~BP',
			GetUtcDate(),
			'~BP'
		FROM
			#WhsOrderComponentLinesWithPickingInfo
		WHERE
			MinKitQuantity > 0
		{maxDOP}

		IF (SELECT TOP 1 1 FROM #WhsOrderComponentPickLine WHERE IsFullyStaged = 1) IS NOT NULL
		BEGIN
			UPDATE
				OutboundTransferLine
			SET
				WE_WL = LastPickedLocation,
				WE_StockOnHand = 0,
				WE_SystemLastEditTimeUtc = GetUtcDate(),
				WE_SystemLastEditUser = '~BP'
			FROM
				dbo.WhsDocketLine OutboundTransferLine
				JOIN #WhsOrderComponentPickLine ON OutboundTransferLinePK = OutboundTransferLine.WE_PK
			WHERE
				IsFullyStaged = 1
			{maxDOP}
		END

		IF (SELECT TOP 1 1 FROM #WhsOrderKitLines WHERE IsFullyStaged = 1) IS NOT NULL
		BEGIN
			UPDATE
				TransactionPickLine
			SET
				WZ_PickedDateTime = LastTranserLineFinalisedTime,
				WZ_GS_NKAssignedTo = LastPickedBy,
				WZ_SystemLastEditTimeUtc = GetUtcDate(),
				WZ_SystemLastEditUser = '~BP'
			FROM
				#WhsOrderKitLines
				JOIN dbo.WhsDocketLine ComponentLine ON ComponentLine.WE_WE_ParentDocketLine = OrderKitLinePK
				JOIN dbo.WhsPickLine TransactionPickLine ON TransactionPickLine.WZ_WE_TransactionLine = ComponentLine.WE_PK
			WHERE
				IsFullyStaged = 1
				AND WZ_PickedDateTime IS NULL
			{maxDOP}
		END

		EXEC dbo.ResumeTrigger TG_WhsDocketLine_StockOnHandIsBalanced
		EXEC dbo.ResumeTrigger TG_WhsDocketLine_StockOnHandIsBalanced_Insert
		EXEC dbo.ResumeTrigger TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert
		EXEC dbo.ResumeTrigger TG_WhsPickLine_TransactionAndPickedQtyIsCorrect
		EXEC dbo.ResumeTrigger TG_PreventOverReduceOfStockViaInventoryLine
		EXEC dbo.ResumeTrigger TG_WhsOrder_EnsureDDLIsEnteredOnPick
		EXEC dbo.ResumeTrigger TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised
		EXEC dbo.ResumeTrigger TG_PreventOverfillLocationWithUnitsCapacity;

		{(isOfflinePhase ? EnableTriggersSQL : "")}

		DECLARE @PKs dbo.TVP_uniqueidentifier;
		DECLARE @TransactionLinePKs dbo.TVP_uniqueidentifier;
		DECLARE @UpdatedPKs dbo.TVP_uniqueidentifier;

		INSERT INTO @UpdatedPKs
		SELECT
			OutboundTransferLinePK
		FROM
			#WhsOrderComponentPickLine
		WHERE
			IsFullyStaged = 1
		
		INSERT INTO @PKs
		SELECT
			Value
		FROM
			@UpdatedPKs

		INSERT INTO @PKs
		SELECT
			NewReceiveLinePK
		FROM
			#WhsOrderKitLines

		INSERT INTO @TransactionLinePKs
		SELECT
			NewTransferLinePK
		FROM
			#WhsOrderKitLines
		WHERE
			IsFullyStaged = 1

		INSERT INTO @PKs
		SELECT
			Value
		FROM
			@TransactionLinePKs

		EXEC WhsCheckStockOnHandIsBalanced @PKs
		EXEC WhsCheckTransactionAndPickQtyIsCorrect_ForInsert @TransactionLinePKs
		EXEC WhsCheckTotalUnits_V3 @UpdatedPKs, 0

		DROP TABLE #WhsOrderComponentPickLine
		DROP TABLE #WhsOrderComponentLinesWithPickingInfo
		DROP TABLE #WhsOrderKitLines
	END

	DROP TABLE #WhsOrderComponentLines
	{(isOfflinePhase ? "" : "DROP TABLE #WhsFinalisedPick;")}
END TRY
BEGIN CATCH
	THROW
END CATCH
";
		}

		#endregion

		#region IndexProvider

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(WhsPickSchema.Instance)
					.Key(WhsPickSchema.Constants.WP_PickStatus)
					.Include(WhsPickSchema.Constants.PK, WhsPickSchema.Constants.WP_PickNo, WhsPickSchema.Constants.WP_FinalizedDateUtc, WhsPickSchema.Constants.WP_SystemCreateTimeUtc, WhsPickSchema.Constants.WP_SystemLastEditUser)
					.Where("[WP_PickStatus]<>'FIN' AND [WP_PickStatus]<>'CAN'")
					.GetInfo();

				indexProvider.New(WhsPickLineSchema.Instance)
					.Key(WhsPickLineSchema.Constants.WZ_WE_TransactionLine)
					.Include(WhsPickLineSchema.Constants.WZ_PickedDateTime, WhsPickLineSchema.Constants.WZ_GS_NKAssignedTo, WhsPickLineSchema.Constants.WZ_Units, WhsPickLineSchema.Constants.WZ_WE_InventoryLine, WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine)
					.GetInfo();

				indexProvider.New(WhsDocketLineSchema.Instance)
					.Key(WhsDocketLineSchema.Constants.PK)
					.Include(WhsDocketLineSchema.Constants.WE_DocketLineStatus, WhsDocketLineSchema.Constants.WE_WD, WhsDocketLineSchema.Constants.WE_WL, WhsDocketLineSchema.Constants.WE_WL_TransferFrom, WhsDocketLineSchema.Constants.WE_FinalisedDate)
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion
	}
}
