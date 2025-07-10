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
	public class RemoveDuplicateOrgPartUnit : DataTransformation
	{
		public override string UserDescription => "Remove duplicate OrgPartUnit.";
		const string TriggerName = "TG_OrgPartUnit_PreventDuplicateUnit";
		const string LastProcessedChunkPKName = "RemoveDuplicateOrgPartUnit.OF_OP.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, OrgPartUnitSchema.Constants.TableName))
			{
				AddTrigger();
				DoTransform();
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			DbObjectCreator.DropTriggerIfExists(Db.Connection, TriggerName);
		}

		void DoTransform()
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var productCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, OrgSupplierPartSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, productCount, lastProcessedPK);

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

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteScalar(
					RemoveDuplicateOrgPartUnitSQL(),
					cmd =>
					{
						cmd.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
						cmd.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
					});
				transactionManager.CommitTransaction();
			}
		}

		string RemoveDuplicateOrgPartUnitSQL()
		{
			return $@"
;WITH DuplicateUnits AS
(
	SELECT
		OF_PK,
		ROW_NUMBER() OVER
		(
			PARTITION BY
				OF_OP,
				OF_PackType,
				OF_ParentPackType
			ORDER BY
				CASE WHEN OF_QuantityInParent = 0 THEN 0 ELSE 1 END DESC,
				(
					CASE WHEN OF_Weight > 0 THEN 1 ELSE 0 END +
					CASE WHEN OF_Height > 0 THEN 1 ELSE 0 END +
					CASE WHEN OF_Width > 0 THEN 1 ELSE 0 END +
					CASE WHEN OF_Depth > 0 THEN 1 ELSE 0 END +
					CASE WHEN OF_Cubic > 0 THEN 1 ELSE 0 END
				) DESC,
				OF_SystemCreateTimeUtc
		) AS RowNum
	FROM
		dbo.OrgPartUnit
	WHERE
		OF_OP >= @FromPK
		AND OF_OP < @ToPK
)

DELETE FROM dbo.OrgPartUnit WHERE OF_PK in (SELECT OF_PK FROM DuplicateUnits WHERE RowNum > 1)
";
		}

		static void AddTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, OrgPartUnitSchema.Constants.TableName, TriggerName))
			{
				Db.Connection.ExecuteNonQuery(CreateTriggerSQL);
			}
		}

		const string CreateTriggerSQL = @"
CREATE TRIGGER dbo.TG_OrgPartUnit_PreventDuplicateUnit
	ON dbo.OrgPartUnit
	AFTER INSERT, UPDATE
AS
BEGIN
	IF (@@ROWCOUNT = 0) RETURN
	SET NOCOUNT ON;

	IF
	(
		EXISTS
		(
			SELECT
				NULL
			FROM
			(
				SELECT
					COUNT(*) as UniqueCount
				FROM
					dbo.OrgPartUnit
				WHERE
					OF_OP IN (SELECT OF_OP FROM inserted)
				GROUP BY
					OF_OP,
					OF_PackType,
					OF_ParentPackType
			) as UniqueCounts
			WHERE
				UniqueCount > 1
		)
	)
	BEGIN
		RAISERROR('Cannot insert a new Unit Conversion as there is already an entry for the same Package & Parent Package.', 16, 1)
		ROLLBACK
	END
END
";
	}
}
