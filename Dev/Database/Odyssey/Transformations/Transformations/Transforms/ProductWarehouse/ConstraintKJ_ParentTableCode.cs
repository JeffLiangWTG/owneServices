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
	public class ConstraintKJ_ParentTableCode : DataTransformation
	{
		public override string UserDescription => "Try to repair PkgPackageJob records with invalid KJ_ParentTableCode, else delete invalid records and children";

		const string LastProcessedChunkPKName = "ConstraintKJ_ParentTableCode.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, PkgPackageJobSchema.Constants.TableName);

			var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				using (var command = Db.Connection.Command(RepairElseDeletePackageJobsSQL))
				{
					command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, chunk.LowerBound);
					command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, chunk.UpperBound);

					command.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedChunkPKString = chunk.UpperBound.ToString();
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);
					manager.ShowInfoMessage($"Last processed batch chunk: {chunk.UpperBound}.");

					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}
			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		const string RepairElseDeletePackageJobsSQL = @"
BEGIN TRY

	UPDATE
		dbo.PkgPackageJob
	SET
		KJ_ParentTableCode = TableCode,
		KJ_SystemLastEditUser = '~BP',
		KJ_SystemLastEditTimeUtc = GETUTCDATE()
	FROM
		dbo.PkgPackageJob
		JOIN
		(
			SELECT
				CUL_PK as ParentPK,
				'CUL' as TableCode
			FROM
				dbo.CusPackingList

			UNION ALL
		
			SELECT
				JS_PK as ParentPK,
				'JS' as TableCode
			FROM
				dbo.JobShipment

			UNION ALL
		
			SELECT
				KB_PK as ParentPK,
				'KB' as TableCode
			FROM
				dbo.DtbBookingConsolidation

			UNION ALL
		
			SELECT
				KM_PK as ParentPK,
				'KM' as TableCode
			FROM
				dbo.DtbBooking

			UNION ALL
		
			SELECT
				KPU_PK as ParentPK,
				'KPU' as TableCode
			FROM
				dbo.PkgHandlingUnit

			UNION ALL
		
			SELECT
				LTC_PK as ParentPK,
				'LTC' as TableCode
			FROM
				dbo.DtbConsignment

			UNION ALL
		
			SELECT
				WD_PK as ParentPK,
				'WD' as TableCode
			FROM
				dbo.WhsDocket

			UNION ALL
		
			SELECT
				WDC_PK as ParentPK,
				'WDC' as TableCode
			FROM
				dbo.WhsItemDispatchConsignment

			UNION ALL
		
			SELECT
				WDH_PK as ParentPK,
				'WDH' as TableCode
			FROM
				dbo.WhsItemDispatchTransportationUnit

			UNION ALL
		
			SELECT
				WRC_PK as ParentPK,
				'WRC' as TableCode
			FROM
				dbo.WhsItemReceiveConsignment

			UNION ALL
		
			SELECT
				WRH_PK as ParentPK,
				'WRH' as TableCode
			FROM
				dbo.WhsItemReceiveTransportationUnit

		) Parents ON ParentPK = KJ_ParentID 
	WHERE 1=1
		AND KJ_PK >= @FromPK
		AND KJ_PK <= @ToPK
		AND KJ_ParentTableCode NOT IN ('CUL', 'JS', 'KB', 'KM', 'KPU', 'LTC', 'WD', 'WDC', 'WDH', 'WRC', 'WRH')


	DECLARE @Delete_PkgPackageJob_PKs dbo.TVP_uniqueidentifier
	DECLARE @Delete_PkgPackage_PKs dbo.TVP_uniqueidentifier
	DECLARE @Delete_PkgPackageBookedDetail_PKs dbo.TVP_uniqueidentifier

	INSERT INTO @Delete_PkgPackageJob_PKs
	SELECT 
		KJ_PK
	FROM 
		dbo.PkgPackageJob
	WHERE 1=1
		AND KJ_PK >= @FromPK
		AND KJ_PK <= @ToPK
		AND KJ_ParentTableCode NOT IN ('CUL', 'JS', 'KB', 'KM', 'KPU', 'LTC', 'WD', 'WDC', 'WDH', 'WRC', 'WRH')

	INSERT INTO @Delete_PkgPackage_PKs
	SELECT 
		KP_PK
	FROM 
		dbo.PkgPackage
	WHERE 1=1
		AND KP_KJ_ParentPackageJob IN (SELECT Value FROM @Delete_PkgPackageJob_PKs)

	INSERT INTO @Delete_PkgPackageBookedDetail_PKs
	SELECT 
		KPB_PK
	FROM 
		dbo.PkgPackageBookedDetail
	WHERE
		KPB_KP_Package IN (SELECT Value FROM @Delete_PkgPackage_PKs)

	DELETE
		dbo.PkgPackageContainer
	WHERE
		K0_KP_Package IN (SELECT Value FROM @Delete_PkgPackage_PKs)

	UPDATE
		dbo.PkgPackageBookedDetail
	SET
		KPB_KPB_OriginalBookedDetail = NULL,
		KPB_SystemLastEditUser = '~BP',
		KPB_SystemLastEditTimeUtc = SYSUTCDATETIME()
	WHERE
		KPB_KPB_OriginalBookedDetail IN (SELECT Value FROM @Delete_PkgPackageBookedDetail_PKs)

	DELETE
		dbo.PkgPackageBookedDetail
	WHERE
		KPB_PK IN (SELECT Value FROM @Delete_PkgPackageBookedDetail_PKs)

	DELETE
		dbo.PkgPackage
	WHERE
		KP_PK IN (SELECT Value FROM @Delete_PkgPackage_PKs)

	DELETE
		dbo.PkgPackageJob
	WHERE
		KJ_PK IN (SELECT Value FROM @Delete_PkgPackageJob_PKs)		
END TRY
BEGIN CATCH
	THROW;
END CATCH
";
	}
}
