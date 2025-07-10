using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class SetUnitsPerClientUQOnOrgPartRelationV2 : DataTransformation
	{
		public override string UserDescription => "Set OU_UnitsPerClientUQ on all OrgPartRelation";

		const string StoredFromPKName = "SetUnitsPerClientUQOnOrgPartRelation.NewFromPK";
		const string StoredTotalUpdatedCount = "SetUnitsPerClientUQOnOrgPartRelation.NewTotalUpdatedCount";

		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var fromPKString = ExtProperty.Database.Select(Db.Connection, StoredFromPKName);
			var totalUpdatedCountString = ExtProperty.Database.Select(Db.Connection, StoredTotalUpdatedCount);
			var fromPK = !string.IsNullOrEmpty(fromPKString) && Guid.TryParse(fromPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var totalUpdatedCount = !string.IsNullOrEmpty(totalUpdatedCountString) ? long.Parse(totalUpdatedCountString) : 0;

			var rowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, OrgSupplierPartSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, rowCountApprox, fromPK);

			var loggingStopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				UpdateChunk(chunk.LowerBound, chunk.UpperBound);
				totalUpdatedCount = Math.Min(totalUpdatedCount + BatchSize, rowCountApprox);

				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, StoredFromPKName, chunk.UpperBound.ToString());
					ExtProperty.Database.Update(Db.Connection, StoredTotalUpdatedCount, totalUpdatedCount.ToString());
					manager?.ShowInfoMessage($"Processed {totalUpdatedCount} Products.");

					token.ThrowIfCancellationRequested();
					loggingStopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, StoredFromPKName);
			ExtProperty.Database.Delete(Db.Connection, StoredTotalUpdatedCount);
		}

		static void UpdateChunk(Guid fromPK, Guid toPK)
		{
			var updateSql = @"
DECLARE @CurrentUtcDate DATETIME = GETUTCDATE();

UPDATE dbo.OrgPartRelation
SET
	OU_UnitsPerClientUQ = ISNULL(ClientQuantity.ConversionFactor, 0),
	OU_SystemLastEditTimeUtc =
		CASE
			WHEN ISNULL(ClientQuantity.ConversionFactor, 0) > 0 THEN @CurrentUtcDate
			ELSE ISNULL(OU_SystemLastEditTimeUtc, @CurrentUtcDate)
		END,
	OU_SystemLastEditUser =
		CASE
			WHEN OU_SystemLastEditUser = '' THEN '~BP'
			ELSE OU_SystemLastEditUser
		END
FROM
	dbo.OrgPartRelation
	JOIN dbo.OrgSupplierPart ON OP_PK = OU_OP
	CROSS APPLY dbo.OrgSupplierPartQuantityConverter(OP_PK, OP_StockKeepingUnit, OU_ClientUQ, 1) AS ClientQuantity
WHERE
	OU_OP >= @FromPK AND
	OU_OP <= @ToPK AND
	OU_ClientUQ != '' AND
	OU_UnitsPerClientUQ != ISNULL(ClientQuantity.ConversionFactor, 0)
OPTION (MAXDOP 1)";
			using (var command = Db.Connection.Command(updateSql))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}
	}
}
