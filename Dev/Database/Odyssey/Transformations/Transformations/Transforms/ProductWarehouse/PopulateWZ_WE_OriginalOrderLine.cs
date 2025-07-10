using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class PopulateWZ_WE_OriginalOrderLine : DataTransformation
	{
		public override string UserDescription => "Populate WZ_WE_OriginalOrderLine column";
		const string UpdateTriggerName = "TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Update";
		const string InsertTriggerName = "TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Insert";
		const string LastProcessedChunkPKName = "PopulateWZ_WE_OriginalOrderLine.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsPickLineSchema.Constants.TableName))
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine, "uniqueidentifier");
				AddTriggers();
				DoTransform();
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
			DbObjectCreator.DropTriggerIfExists(Db.Connection, UpdateTriggerName);
			DbObjectCreator.DropTriggerIfExists(Db.Connection, InsertTriggerName);
		}

		static void AddTriggers()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsPickLineSchema.Constants.TableName, UpdateTriggerName))
			{
				var triggerSQL = @"
CREATE TRIGGER dbo.TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Update
	ON dbo.WhsPickLine
	FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WZ_WE_InventoryLine)
	BEGIN
		EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
		EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

		UPDATE wp
		SET
			wp.WZ_WE_OriginalOrderLine = i.WZ_WE_TransactionLine,
			wp.WZ_SystemLastEditUser = CASE WHEN wp.WZ_SystemLastEditUser <> '' THEN wp.WZ_SystemLastEditUser ELSE '~BP' END,
			wp.WZ_SystemLastEditTimeUtc = ISNULL(wp.WZ_SystemLastEditTimeUtc, GetUtcDate())
		FROM
			dbo.WhsPickLine wp
			JOIN inserted i ON i.WZ_WE_InventoryLine = wp.WZ_WE_TransactionLine
		WHERE
			i.WZ_WE_OriginalPickedInventoryLine IS NOT NULL

		EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
		EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
	END
END";
				Db.Connection.ExecuteNonQuery(triggerSQL);
			}

			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsPickLineSchema.Constants.TableName, InsertTriggerName))
			{
				var triggerSQL = @"
CREATE TRIGGER dbo.TG_WhsPickLine_KeepWZ_WE_OriginalOrderLineInSync_Insert
	ON dbo.WhsPickLine
	FOR INSERT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

	UPDATE wp
	SET
		wp.WZ_WE_OriginalOrderLine = op.WZ_WE_TransactionLine,
		wp.WZ_SystemLastEditUser = wp.WZ_SystemLastEditUser,
		wp.WZ_SystemLastEditTimeUtc = wp.WZ_SystemLastEditTimeUtc
	FROM
		dbo.WhsPickLine wp
		JOIN inserted i ON wp.WZ_PK = i.WZ_PK
		JOIN dbo.WhsDocketLine ON wp.WZ_WE_TransactionLine = WE_PK
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsPickLine op ON op.WZ_WE_InventoryLine = WE_PK
	WHERE
		WE_DocketLineType = 'TFR'
		AND WD_WP_ParentPickForTransfer IS NOT NULL

	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
END";
				Db.Connection.ExecuteNonQuery(triggerSQL);
			}
		}

		void DoTransform()
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var docketCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, docketCount, lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				UpdateChunk(chunk.LowerBound, chunk.UpperBound);

				if (stopWatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedChunkPKString = chunk.UpperBound.ToString();
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);

					stopWatch.Restart();
				}
			}
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			var sql = @"
BEGIN TRY
	DECLARE @PickLinesToCheck dbo.tvp_uniqueidentifier;
	
	INSERT INTO @PickLinesToCheck
	SELECT
		WZ_PK
	FROM
		dbo.WhsDocket WITH (INDEX (PK_UC__WD_PK))
		JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
		JOIN dbo.WhsPickLine ON WZ_WE_TransactionLine = WE_PK
	WHERE
		WD_PK BETWEEN @FromPK AND @ToPK
		AND WD_WP_ParentPickForTransfer IS NOT NULL
	OPTION (MAXDOP 1)

	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
	
	UPDATE TransferPickLine
	SET
		WZ_WE_OriginalOrderLine = WE_PK,
		WZ_SystemLastEditTimeUtc = GetUtcDate(),
		WZ_SystemLastEditUser = '~BP'
	FROM
		dbo.WhsPickLine TransferPickLine
		JOIN dbo.WhsPickLine OrderPickLine ON OrderPickLine.WZ_WE_InventoryLine = TransferPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsDocketLine OrderLine ON OrderPickLine.WZ_WE_TransactionLine = WE_PK
	WHERE
		TransferPickLine.WZ_PK IN (SELECT Value FROM @PickLinesToCheck) AND WE_DocketLineType = 'ORD' AND TransferPickLine.WZ_WE_OriginalOrderLine IS NULL
	OPTION (MAXDOP 1)
	
	UPDATE TransferPickLine
	SET
		WZ_WE_OriginalOrderLine = WE_PK,
		WZ_SystemLastEditTimeUtc = GetUtcDate(),
		WZ_SystemLastEditUser = '~BP'
	FROM
		dbo.WhsPickLine TransferPickLine
		JOIN dbo.WhsPickLine SecondPickLine ON SecondPickLine.WZ_WE_InventoryLine = TransferPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsPickLine OrderPickLine ON OrderPickLine.WZ_WE_InventoryLine = SecondPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsDocketLine OrderLine ON OrderPickLine.WZ_WE_TransactionLine = WE_PK
	WHERE
		TransferPickLine.WZ_PK IN (SELECT Value FROM @PickLinesToCheck) AND WE_DocketLineType = 'ORD' AND TransferPickLine.WZ_WE_OriginalOrderLine IS NULL
	OPTION (MAXDOP 1)
	
	UPDATE TransferPickLine
	SET
		WZ_WE_OriginalOrderLine = WE_PK,
		WZ_SystemLastEditTimeUtc = GetUtcDate(),
		WZ_SystemLastEditUser = '~BP'
	FROM
		dbo.WhsPickLine TransferPickLine
		JOIN dbo.WhsPickLine SecondPickLine ON SecondPickLine.WZ_WE_InventoryLine = TransferPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsPickLine ThirdPickLine ON ThirdPickLine.WZ_WE_InventoryLine = SecondPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsPickLine OrderPickLine ON OrderPickLine.WZ_WE_InventoryLine = ThirdPickLine.WZ_WE_TransactionLine
		JOIN dbo.WhsDocketLine OrderLine ON OrderPickLine.WZ_WE_TransactionLine = WE_PK
	WHERE
		TransferPickLine.WZ_PK IN (SELECT Value FROM @PickLinesToCheck) AND WE_DocketLineType = 'ORD' AND TransferPickLine.WZ_WE_OriginalOrderLine IS NULL
	OPTION (MAXDOP 1)

	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
	EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'
END TRY
BEGIN CATCH
	THROW
END CATCH";

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(
					sql,
					cmd =>
					{
						cmd.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
						cmd.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
					});
				transactionManager.CommitTransaction();
			}
		}
	}
}
