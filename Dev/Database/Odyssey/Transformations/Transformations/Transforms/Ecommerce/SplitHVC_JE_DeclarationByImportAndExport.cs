using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	public class SplitHVC_JE_DeclarationByImportAndExport : DataTransformation
	{
		public override string UserDescription => "Split HVC_JE_Declaration into HVC_JE_ImportDeclaration And HVC_JE_ExportDeclaration";

		const string SyncTriggerName = "TG_HVLVConsignment_SyncDeclaration";
		const string LastProcessedHVLVConsignmentPK = "LastProcessedHVLVConsignmentPK";
		const string UpdatedHVLVConsignmentCount = "UpdatedHVLVConsignmentCount";

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_Declaration"))
			{
				CreateNewColumnsAndSyncTriggerIfNeeded();

				Guid.TryParse(ExtProperty.Database.Select(Db.Connection, LastProcessedHVLVConsignmentPK), out var lastProcessedPK);
				int.TryParse(ExtProperty.Database.Select(Db.Connection, UpdatedHVLVConsignmentCount), out var rowsUpdated);

				var rowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, HVLVConsignmentSchema.Constants.TableName);

				var rowsAllToBeUpdated = Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM HVLVConsignment WHERE HVC_JE_Declaration IS NOT NULL");
				var loggingStopWatch = Stopwatch.StartNew();

				foreach (var chunk in GuidChunker.GenerateChunks(chunkSize: 1000, rowCountApprox, lastProcessedPK))
				{
					var batchNum = UpdateBatch(chunk.LowerBound, chunk.UpperBound);
					rowsUpdated += batchNum;

					if (loggingStopWatch.Elapsed.TotalMinutes > 1)
					{
						ExtProperty.Database.Update(Db.Connection, LastProcessedHVLVConsignmentPK, chunk.UpperBound.ToString());
						ExtProperty.Database.Update(Db.Connection, UpdatedHVLVConsignmentCount, rowsUpdated.ToString());

						if (batchNum > 0)
						{
							manager?.ShowInfoMessage($@"Processed {rowsUpdated} of {rowsAllToBeUpdated} Consignments.");
						}

						loggingStopWatch.Restart();
					}
				}

				ExtProperty.Database.Delete(Db.Connection, LastProcessedHVLVConsignmentPK);
				ExtProperty.Database.Delete(Db.Connection, UpdatedHVLVConsignmentCount);
			}
		}

		void CreateNewColumnsAndSyncTriggerIfNeeded()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_ImportDeclaration", "UNIQUEIDENTIFIER");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_ExportDeclaration", "UNIQUEIDENTIFIER");

			if (!DbObjectCreator.TriggerExists(Db.Connection, HVLVConsignmentSchema.Constants.TableName, SyncTriggerName))
			{
				var createTriggerSql = $@"
CREATE TRIGGER {SyncTriggerName}
ON dbo.HVLVConsignment
AFTER UPDATE
AS
BEGIN
    IF UPDATE(HVC_JE_Declaration)
    BEGIN
        DECLARE @Updates TABLE (
            HVC_PK UNIQUEIDENTIFIER,
            HVC_JE_Declaration UNIQUEIDENTIFIER,
            JE_MessageType CHAR(3)
        );

        INSERT INTO @Updates (HVC_PK, HVC_JE_Declaration, JE_MessageType)
        SELECT
            inserted.HVC_PK,
            inserted.HVC_JE_Declaration,
            je.JE_MessageType
        FROM
            INSERTED AS inserted
        JOIN
            dbo.JobDeclaration AS je ON inserted.HVC_JE_Declaration = je.JE_PK;

        UPDATE hvc
        SET
            hvc.HVC_JE_ImportDeclaration = CASE WHEN u.JE_MessageType = 'IMP' THEN u.HVC_JE_Declaration ELSE NULL END,
            hvc.HVC_JE_ExportDeclaration = CASE WHEN u.JE_MessageType = 'EXP' THEN u.HVC_JE_Declaration ELSE NULL END,
            hvc.HVC_SystemLastEditTimeUtc = GETUTCDATE(),
            hvc.HVC_SystemLastEditUser = '~BP'
        FROM
            dbo.HVLVConsignment hvc
        JOIN
            @Updates AS u ON hvc.HVC_PK = u.HVC_PK
    END
END;
";

				Db.Connection.ExecuteNonQuery(createTriggerSql);
			}
		}

		int UpdateBatch(Guid minConsignmentPK, Guid maxConsignmentPK)
		{
			var updateSql = $@"
DECLARE @UpdatedAt DATETIME = GETUTCDATE()

UPDATE
    hvc
SET
    hvc.HVC_JE_ImportDeclaration = CASE WHEN je.JE_MessageType = 'IMP' THEN je.JE_PK ELSE NULL END,
    hvc.HVC_JE_ExportDeclaration = CASE WHEN je.JE_MessageType = 'EXP' THEN je.JE_PK ELSE NULL END,
    hvc.HVC_SystemLastEditTimeUtc = @UpdatedAt,
    hvc.HVC_SystemLastEditUser = '~BP'
FROM
    dbo.HVLVConsignment hvc
JOIN dbo.JobDeclaration je ON je.JE_PK = hvc.HVC_JE_Declaration
WHERE
    hvc.HVC_PK >= @MinConsignmentPK AND
    hvc.HVC_PK <= @MaxConsignmentPK AND
    hvc.HVC_JE_Declaration IS NOT NULL AND
    hvc.HVC_JE_ImportDeclaration IS NULL AND
    hvc.HVC_JE_ExportDeclaration IS NULL

SELECT @@ROWCOUNT;
";

			var result = Db.Connection.ExecuteScalar(
				 updateSql,
				 cmd =>
				 {
					 cmd.AddParameter("@MinConsignmentPK", SqlDbType.UniqueIdentifier, minConsignmentPK);
					 cmd.AddParameter("@MaxConsignmentPK", SqlDbType.UniqueIdentifier, maxConsignmentPK);
				 });

			return result is int rowsUpdated ? rowsUpdated : 0;
		}
	}
}
