using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsPickPickTypeColumn : DataTransformation
	{
		public override string UserDescription => "Populate WhsPick.PickType based on attached docket type";

		public const string TriggerName = "TG_WhsDocket_SetPickType";
		const int BatchSize = 1000;
		const string LastProcessedChunkPKName = "PopulateWhsPickPickTypeColumn.LastProcessedChunkPK";

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_PickType, "char(3)", "'ORD'"))
			{
				if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketSchema.Constants.TableName, TriggerName))
				{
					Db.Connection.ExecuteNonQuery(CreateSetPickTypeTriggerSQL);
				}

				var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
				var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsPickSchema.Constants.TableName);
				var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK).ToArray();
			
				var stopWatch = Stopwatch.StartNew();
				foreach (var chunk in chunks)
				{
					UpdateChunk(chunk.LowerBound, chunk.UpperBound);

					if (stopWatch.Elapsed.TotalMinutes > 1)
					{
						lastProcessedChunkPKString = chunk.UpperBound.ToString();
						ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);

						manager.ShowInfoMessage($"Processing populating WP_PickType up to : {lastProcessedChunkPKString}.");

						stopWatch.Restart();
					}
				}
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(PopulateWhsPickPickTypeColumnSQL))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		const string PopulateWhsPickPickTypeColumnSQL = @"
;WITH
WhsPickPickTypeData as (
	SELECT
		COALESCE(WD_DocketType, 'ORD') as PickType,
		WP_PickType,
		WP_SystemLastEditUser,
		WP_SystemLastEditTimeUtc
	FROM
		WhsPick
		OUTER APPLY
		(
			SELECT TOP 1
				WD_DocketType
			FROM
				WhsDocket
			WHERE
				WD_WP = WP_PK
		) Docket
	WHERE 1=1
		AND WP_PK >= @FromPK
		AND WP_PK <= @ToPK
		AND WD_DocketType <> 'ORD'
)
UPDATE 
	WhsPickPickTypeData
SET
	WP_PickType = PickType,
	WP_SystemLastEditUser = '~BP',
	WP_SystemLastEditTimeUtc = GetUtcDate()
";

		const string CreateSetPickTypeTriggerSQL = @"
CREATE TRIGGER dbo.TG_WhsDocket_SetPickType
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	
	IF UPDATE (WD_WP)
	BEGIN
		WITH
		WhsPickPickTypeData as 
		(
			SELECT 
				inserted.WD_DocketType as PickType,
				WP_PickType,
				WP_SystemLastEditUser,
				WP_SystemLastEditTimeUtc
			FROM
				inserted
				JOIN WhsPick ON WP_PK = WD_WP
			WHERE
				inserted.WD_DocketType <> WP_PickType
		)
		UPDATE 
			WhsPickPickTypeData
		SET
			WP_PickType = PickType,
			WP_SystemLastEditUser = '~BP',
			WP_SystemLastEditTimeUtc = GetUtcDate()
	END
END
";
	}
}
