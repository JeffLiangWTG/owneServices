using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	public class SplitHVC_ReleaseStatusByImportAndExport : DataTransformation
	{
		public override string UserDescription => "Split HVC_ReleaseStatus into HVC_ImportReleaseStatus And HVC_ExportReleaseStatus";

		const string LastProcessedHVLVConsignmentPK = "SplitHVC_ReleaseStatusByImportAndExport.LastProcessedConsignmentPK";
		const string SyncConsignmentTriggerName = "TG_HVLVConsignment_KeepReleaseStatusesInSync";
		const string HVC_ReleaseStatusColumnName = "HVC_ReleaseStatus";
		const string HVI_ReleaseStatusColumnName = "HVI_ReleaseStatus";

		bool SourceColumnsExist => DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, HVLVConsignmentSchema.Constants.TableName)
			&& DbObjectCreator.ColumnExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVC_ReleaseStatusColumnName)
			&& DbObjectCreator.ColumnExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ExportCustomsClearanceStatus)
			&& DbObjectCreator.ColumnExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ImportCustomsClearanceStatus)
			&& DbObjectCreator.TableExists(Db.Connection, HVLVItemSchema.Constants.TableName)
			&& DbObjectCreator.ColumnExists(Db.Connection, HVLVItemSchema.Constants.TableName, HVI_ReleaseStatusColumnName);

		protected override void OnlinePreUpgradeTransform()
		{
			if (SourceColumnsExist)
			{
				CreateNewColumnsAndSyncTriggerIfNeeded();

				var chunkingOperation = new GuidChunkingOperation(manager, 1000, GetApproximateRowCount(), UpdateImportAndExportReleaseStatusInBatches, LastProcessedHVLVConsignmentPK);
				chunkingOperation.DoChunking();
			}
		}

		static long GetApproximateRowCount()
		{
			var rowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, HVLVConsignmentSchema.Constants.TableName);
			if (rowCountApprox == 0)
			{
				rowCountApprox = 1;
			}

			return rowCountApprox;
		}

		void CreateNewColumnsAndSyncTriggerIfNeeded()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ImportReleaseStatus, "CHAR(3)", "'NON'");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ImportReleaseStatus, "CHAR(1)", "'N'");

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ExportReleaseStatus, "CHAR(3)", "'NON'");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVItemSchema.Constants.TableName, HVLVItemSchema.Constants.HVI_ExportReleaseStatus, "CHAR(1)", "'N'");

			if (!DbObjectCreator.TriggerExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, SyncConsignmentTriggerName))
			{
				var createTriggerSql = $@"
CREATE TRIGGER {SyncConsignmentTriggerName}
ON dbo.HVLVConsignment
AFTER UPDATE
AS
BEGIN
	IF UPDATE(HVC_ImportCustomsClearanceStatus) OR UPDATE(HVC_ExportCustomsClearanceStatus)
	BEGIN
		DECLARE @recordsNeedingUpdate AS TABLE
		(
			ClusterKey INT,
			PK UNIQUEIDENTIFIER,
			ReleaseStatus CHAR(3)
		)

		BEGIN TRY
			INSERT INTO @recordsNeedingUpdate (ClusterKey, PK, ReleaseStatus)
			SELECT
				inserted.HVC_ClusterKey,
				inserted.HVC_PK,
				inserted.HVC_ReleaseStatus
			FROM
				INSERTED AS inserted
			WHERE
				inserted.HVC_ReleaseStatus <> 'NON'
		
			IF UPDATE(HVC_ImportCustomsClearanceStatus)
			BEGIN
				UPDATE hvc
				SET
					hvc.HVC_ImportReleaseStatus = u.ReleaseStatus,
					hvc.HVC_SystemLastEditTimeUtc = GETUTCDATE(),
					hvc.HVC_SystemLastEditUser = '~BP'
				FROM
					dbo.HVLVConsignment hvc
				JOIN
					@recordsNeedingUpdate AS u 
					ON 
						HVC_ClusterKey = u.ClusterKey
						AND hvc.HVC_PK = u.PK

				UPDATE hvi
				SET
					hvi.HVI_ImportReleaseStatus = LEFT(u.ReleaseStatus, 1),
					hvi.HVI_SystemLastEditTimeUtc = GETUTCDATE(),
					hvi.HVI_SystemLastEditUser = '~BP'
				FROM
					dbo.HVLVItem hvi
				JOIN
					@recordsNeedingUpdate AS u 
					ON 
						hvi.HVI_ClusterKey = u.ClusterKey
						AND hvi.HVI_HVC_Consignment = u.PK
			END
			IF UPDATE(HVC_ExportCustomsClearanceStatus)
			BEGIN
				UPDATE hvc
				SET
					hvc.HVC_ExportReleaseStatus = u.ReleaseStatus,
					hvc.HVC_SystemLastEditTimeUtc = GETUTCDATE(),
					hvc.HVC_SystemLastEditUser = '~BP'
				FROM
					dbo.HVLVConsignment hvc
				JOIN
					@recordsNeedingUpdate AS u 
					ON 
						HVC_ClusterKey = u.ClusterKey
						AND hvc.HVC_PK = u.PK

				UPDATE hvi
				SET
					hvi.HVI_ExportReleaseStatus = LEFT(u.ReleaseStatus, 1),
					hvi.HVI_SystemLastEditTimeUtc = GETUTCDATE(),
					hvi.HVI_SystemLastEditUser = '~BP'
				FROM
					dbo.HVLVItem hvi
				JOIN
					@recordsNeedingUpdate AS u 
					ON 
						hvi.HVI_ClusterKey = u.ClusterKey
						AND hvi.HVI_HVC_Consignment = u.PK
			END
		END TRY
		BEGIN CATCH
			THROW
		END CATCH
	END
END
";

				Db.Connection.ExecuteNonQuery(createTriggerSql);
			}
		}

		void UpdateImportAndExportReleaseStatusInBatches(Guid minConsignmentPK, Guid maxConsignmentPK)
		{
			var updateImportAndExportReleaseStatusSql = @"
DECLARE @updatedValues AS TABLE
(
	ClusterKey INT,
	PK UNIQUEIDENTIFIER,
	ImportReleaseStatus CHAR(3),
	ExportReleaseStatus CHAR(3)
)
DECLARE @UpdatedAt DATETIME = GETUTCDATE()

BEGIN TRY
	UPDATE dbo.HVLVConsignment
	SET
		HVC_ImportReleaseStatus = (CASE WHEN HVC_ImportCustomsClearanceStatus = '' AND HVC_ExportCustomsClearanceStatus <> '' THEN 'NON' ELSE HVC_ReleaseStatus END),
		HVC_ExportReleaseStatus = (CASE WHEN HVC_ExportReleaseStatus = 'NON' AND HVC_ExportCustomsClearanceStatus <> '' THEN HVC_ReleaseStatus ELSE HVC_ExportReleaseStatus END),
		HVC_SystemLastEditTimeUtc = @UpdatedAt,
		HVC_SystemLastEditUser = '~BP'
	OUTPUT
		INSERTED.HVC_ClusterKey,
		INSERTED.HVC_PK,
		INSERTED.HVC_ImportReleaseStatus,
		INSERTED.HVC_ExportReleaseStatus
	INTO @updatedValues
	FROM dbo.HVLVConsignment WITH (FORCESEEK, INDEX(PK_UX__HVC_PK))
	WHERE
		HVC_ReleaseStatus <> 'NON'
		AND HVC_PK >= @MinConsignmentPK
		AND HVC_PK <= @MaxConsignmentPK

	UPDATE dbo.HVLVItem
	SET 
		HVI_ImportReleaseStatus = LEFT(UpdatedConsignments.ImportReleaseStatus, 1),
		HVI_ExportReleaseStatus = LEFT(UpdatedConsignments.ExportReleaseStatus, 1),
		HVI_SystemLastEditTimeUtc = @UpdatedAt,
		HVI_SystemLastEditUser = '~BP'
	FROM dbo.HVLVItem
		INNER JOIN @updatedValues UpdatedConsignments
		ON
			HVI_ClusterKey = UpdatedConsignments.ClusterKey
			AND HVI_HVC_Consignment = UpdatedConsignments.PK

END TRY
BEGIN CATCH
	THROW
END CATCH
";

			var result = Db.Connection.ExecuteNonQuery(
				 updateImportAndExportReleaseStatusSql,
				 cmd =>
				 {
					 cmd.AddParameter("@MinConsignmentPK", SqlDbType.UniqueIdentifier, minConsignmentPK);
					 cmd.AddParameter("@MaxConsignmentPK", SqlDbType.UniqueIdentifier, maxConsignmentPK);
				 });
		}
	}
}
