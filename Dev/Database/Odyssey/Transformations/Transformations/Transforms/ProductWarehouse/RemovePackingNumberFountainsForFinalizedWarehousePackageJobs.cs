using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class RemovePackingNumberFountainsForFinalizedWarehousePackageJobs : DataTransformation
	{
		public override string UserDescription => "Remove packing number fountains for finalized warehouse package jobs.";
		public const string LastProcessedChunkPKName = "RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.LastProcessedChunkPK";
		public const string MaxPickFinalizedDate = "RemovePackingNumberFountainsForFinalizedWarehousePackageJobs.MaxPickFinalizedDate";
		const int BatchSize = 1000;
		public const string LastPackingFountainsDeleteTimeUtcRegistryName = "LastPackingFountainsDeleteTimeUtc";

		#region OnlinePreUpgradeTransform

		protected override void OnlinePreUpgradeTransform()
		{
			if (ShouldTransformationRun())
			{
				var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
				var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

				var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, MaxPickFinalizedDate);
				var currentMaxPickFinalizedDate = DateTime.TryParse(maxPickFinalizedDateString, out var maxDate) ? maxDate : (DateTime?)null;

				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, PkgPackageJobSchema.Constants.TableName);
				var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

				var stopWatch = Stopwatch.StartNew();
				foreach (var chunk in chunks)
				{
					var maxPickFinalisedDateFromQuery = DeleteChunk(chunk.LowerBound, chunk.UpperBound);
					currentMaxPickFinalizedDate = GetNewMaxPickFinalizedDate(maxPickFinalisedDateFromQuery, currentMaxPickFinalizedDate);
					if (stopWatch.Elapsed.TotalMinutes > 1)
					{
						lastProcessedChunkPKString = chunk.UpperBound.ToString();
						ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);

						stopWatch.Restart();
					}
				}
			}
		}

		bool ShouldTransformationRun()
		{
			return DbObjectCreator.TableExists(Db.Connection, WhsPickSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, PkgPackageJobSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, WhsDocketSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, "StmNums")
				&& DbObjectCreator.TableExists(Db.Connection, StmDataSchema.Constants.TableName)
				&& !HasServiceTaskToDeletePackingFountainsRan();
		}

		bool HasServiceTaskToDeletePackingFountainsRan()
		{
			var deletePackingFountainServiceTaskHasRan = false;
			var binValue = new RegistryTransformationHelper().GetStmDataValue(LastPackingFountainsDeleteTimeUtcRegistryName);
			if (binValue != null && DateTime.TryParse(Encoding.Unicode.GetString(binValue), out var result))
			{
				deletePackingFountainServiceTaskHasRan = result > new DateTime(1900, 1, 1);
			}
			return deletePackingFountainServiceTaskHasRan;
		}

		DateTime? DeleteChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(RemovePackingNumberFountainsForFinalizedWarehousePackageJobsQuery))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					DateTime? maxUpdatedDate = reader[0] is DateTime dateTime ? dateTime : null;
					return maxUpdatedDate;
				}
			}
		}

		const string RemovePackingNumberFountainsForFinalizedWarehousePackageJobsQuery = @"
BEGIN TRY
	DROP TABLE IF EXISTS #NumberFountainsToDelete
	CREATE TABLE #NumberFountainsToDelete
	(
		FountainOwner UNIQUEIDENTIFIER NOT NULL,
		FountainName VARCHAR(256) COLLATE database_default NOT NULL,
		FountainSequence SMALLINT NOT NULL,
		CompletedDateTime SMALLDATETIME NOT NULL,
		INDEX NameOwnerSequenceIndex UNIQUE CLUSTERED(FountainName, FountainOwner, FountainSequence),
		INDEX CompletedDateTimeIndex NONCLUSTERED(CompletedDateTime)
	)

	INSERT INTO #NumberFountainsToDelete
	SELECT
		SN_Owner,
		SN_Name,
		SN_Sequence,
		WP_FinalizedDateUtc
	FROM
		dbo.PkgPackageJob
		JOIN dbo.WhsDocket ON KJ_ParentID = WD_PK
		JOIN dbo.WhsPick ON WD_WP = WP_PK
		JOIN dbo.StmNums ON SN_Owner = KJ_PK
	WHERE
		1=1
		AND KJ_PK >= @FromPK
		AND KJ_PK <= @ToPK
		AND WP_FinalizedDateUtc IS NOT NULL
		AND SN_Name LIKE 'GeneratorFountain-PKGID%'

	DELETE dbo.StmNums
	FROM
		#NumberFountainsToDelete
	WHERE
		SN_Name = FountainName
		AND SN_Owner = FountainOwner
		AND SN_Sequence =  FountainSequence

	SELECT MAX(CompletedDateTime) FROM #NumberFountainsToDelete

	DROP TABLE #NumberFountainsToDelete
END TRY
BEGIN CATCH
	THROW
END CATCH
";

		DateTime? GetNewMaxPickFinalizedDate(DateTime? maxPickFinalizedDateFromQuery, DateTime? currentMaxPickFinalizedDate)
		{
			var newMaxPickFinalizedDate = currentMaxPickFinalizedDate;
			if (maxPickFinalizedDateFromQuery.HasValue)
			{
				newMaxPickFinalizedDate = currentMaxPickFinalizedDate.HasValue && currentMaxPickFinalizedDate.Value > maxPickFinalizedDateFromQuery.Value
					? currentMaxPickFinalizedDate
					: maxPickFinalizedDateFromQuery;

				var maxPickFinalizedDateString = SqlFormatInfo.ToSqlDateTimeString(newMaxPickFinalizedDate.Value);
				ExtProperty.Database.Update(Db.Connection, MaxPickFinalizedDate, maxPickFinalizedDateString);
			}

			return newMaxPickFinalizedDate;
		}

		#endregion

		#region OfflinePostUpgradeTransform

		protected override void OfflinePostUpgradeTransform()
		{
			var maxPickFinalizedDateString = ExtProperty.Database.Select(Db.Connection, MaxPickFinalizedDate);
			var maxPickFinalizedDate = DateTime.TryParse(maxPickFinalizedDateString, out var maxDate) ? maxDate : (DateTime?)null;
			if (maxPickFinalizedDate.HasValue)
			{
				UpdateMaxPickFinalizedDateRegistry(maxPickFinalizedDate.Value);
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
			ExtProperty.Database.Delete(Db.Connection, MaxPickFinalizedDate);
		}

		static void UpdateMaxPickFinalizedDateRegistry(DateTime maxPickFinalizedDate)
		{
			var binValue = Encoding.Unicode.GetBytes(maxPickFinalizedDate.ToString());
			var registryHelper = new RegistryTransformationHelper();
			if (registryHelper.GetStmDataRowCount(LastPackingFountainsDeleteTimeUtcRegistryName) > 0)
			{
				var updateSqlQuery = $@"UPDATE dbo.StmData
						SET SD_BinaryValue = @SD_BinaryValue
						WHERE SD_Name = @SD_Name";

				using (var cmd = Db.Connection.Command(updateSqlQuery))
				{
					cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", binValue, StmDataSchema.SD_BinaryValue);
					cmd.AddParameterBasedOnDbColumn("@SD_Name", LastPackingFountainsDeleteTimeUtcRegistryName, StmDataSchema.SD_Name);
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				registryHelper.InsertStmDataRow(LastPackingFountainsDeleteTimeUtcRegistryName, "DT", binValue);
			}
		}

		#endregion
	}
}
