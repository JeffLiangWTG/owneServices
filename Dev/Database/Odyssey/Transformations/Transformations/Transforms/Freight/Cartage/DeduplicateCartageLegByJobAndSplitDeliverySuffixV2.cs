using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Cartage
{
	sealed class DeduplicateCartageLegByJobAndSplitDeliverySuffixV2 : DataTransformation
	{
		public override string UserDescription => "De-duplicate JU_SplitDeliverySuffix on JobContainerLegs belonging to the same JobCartage.JJ_PK (via JobBookedCtgMove.EW_PK)";
		const string LastProcessedChunkPKName = "DeduplicateCartageLegByJobAndSplitDeliverySuffixV2.JJ_PK.LastProcessedChunkPKName";
		const int BatchSize = 2000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var chunks = GuidChunker.GenerateChunks(BatchSize, DataUtils.GetApproximateRowCountForTable(Db.Connection, JobCartageSchema.Constants.TableName), lastProcessedPK);

			var loggingStopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				var chunkHasBeenFixed = false;

				while (!chunkHasBeenFixed && !token.IsCancellationRequested)
				{
					// check that a pesky user hasn't gone in and added extra legs to the cartage jobs being fixed while the transform SQL was running - if they have, run the chunk again
					chunkHasBeenFixed = ProcessChunk(chunk.LowerBound, chunk.UpperBound);
				}

				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 2)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());
					loggingStopWatch.Restart();
				}

				token.ThrowIfCancellationRequested();
			}

			DropTempNumbersWithSuffixesTable();

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		bool ProcessChunk(Guid fromJobCartagePK, Guid toJobCartagePK)
		{
			var chunkHasBeenFixed = false;
			using (var command = Db.Connection.Command(DeduplicateCartageLegByJobAndSplitDeliverySuffixChunkSQL))
			{
				command.AddParameter("@fromJobCartagePK", SqlDbType.UniqueIdentifier, fromJobCartagePK);
				command.AddParameter("@toJobCartagePK", SqlDbType.UniqueIdentifier, toJobCartagePK);
				chunkHasBeenFixed = (bool)command.ExecuteScalar();
			}
			return chunkHasBeenFixed;
		}

		void DropTempNumbersWithSuffixesTable()
		{
			Db.Connection.ExecuteNonQuery(DropTempNumbersWithSuffixesTableSQL);
		}

		const string DeduplicateCartageLegByJobAndSplitDeliverySuffixChunkSQL = @"
	BEGIN TRY
		IF OBJECT_ID('tempdb..#NumbersWithSuffixes') IS NULL
		BEGIN
			CREATE TABLE #NumbersWithSuffixes
			(SNS_Number INT,
			SNS_Suffix varchar(2) COLLATE DATABASE_DEFAULT PRIMARY KEY,
			UNIQUE (SNS_Number)
			);

			INSERT INTO #NumbersWithSuffixes
			(SNS_Number, SNS_Suffix)
			SELECT SNS_Number, 
			Coalesce((SELECT Char(65 + (SNS_Number - 27) / 26 % 26) WHERE SNS_Number >= 27), '')
				  + (SELECT Char(65 + (SNS_Number - 1) % 26)) AS SNS_Suffix
			FROM dbo.StmNumberSequence
			WHERE SNS_Number BETWEEN 1 AND 702;
		END

		DECLARE @LegPKsChanged TABLE 
		(
			JU_PK uniqueidentifier PRIMARY KEY,
			OldSplitDeliverySuffix varchar(3) NOT NULL,
			NewSplitDeliverySuffix varchar(3) NOT NULL,
			JJ_PK uniqueidentifier,
			INDEX JJ_PK (JJ_PK)
		);

		BEGIN TRANSACTION;
		

		UPDATE JobContainerLegs
		SET JU_SplitDeliverySuffix = SuggestedNewSuffix,
		JU_SystemLastEditTimeUtc = GETUTCDATE(),
		JU_SystemLastEditUser = '~BP'
		OUTPUT LegsToFix.JJ_PK, inserted.JU_PK, deleted.JU_SplitDeliverySuffix, inserted.JU_SplitDeliverySuffix 
			INTO @LegPKsChanged (JJ_PK, JU_PK, OldSplitDeliverySuffix, NewSplitDeliverySuffix)
		FROM
		dbo.JobContainerLegs JOIN
		(
			SELECT 
				JJ_PK,
				JU_PK, 
				NumberLookup.SNS_Suffix AS SuggestedNewSuffix
			FROM
			(	
				SELECT 
					JJ_PK,
					JU_PK, 
					HighestSuffixNumber + ROW_NUMBER() OVER (PARTITION BY JJ_PK ORDER BY JU_SplitDeliverySuffix, JU_PK) AS SuggestedNewSuffixNumber
				FROM
				(
					SELECT 
						EW_JJ AS JJ_PK, 
						JU_SplitDeliverySuffix,
						JU_PK,
						MAX(SuffixLookup.SNS_Number) OVER (PARTITION BY EW_JJ) AS HighestSuffixNumber, 
						ROW_NUMBER() OVER (PARTITION BY EW_JJ, JU_SplitDeliverySuffix ORDER BY JU_PK) AS RowNumByJobAndSuffix
					FROM dbo.JobBookedCtgMove 
					JOIN dbo.JobContainerLegs ON EW_PK = JU_EW
					JOIN #NumbersWithSuffixes SuffixLookup ON JU_SplitDeliverySuffix = SuffixLookup.SNS_Suffix
					WHERE EW_JJ BETWEEN @fromJobCartagePK AND @toJobCartagePK
				) AllLegs
				WHERE RowNumByJobAndSuffix > 1
			) LegsToFixStage1
			JOIN #NumbersWithSuffixes NumberLookup ON NumberLookup.SNS_Number = SuggestedNewSuffixNumber
		) LegsToFix ON LegsToFix.JU_PK = JobContainerLegs.JU_PK;

		COMMIT;
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK TRANSACTION;
		END;
		SELECT 0 AS ChunkIsFixed;
		THROW;
	END CATCH;

	WITH DuplicateLegCheck AS
	(
		SELECT JobCartage.JJ_PK, JU_SplitDeliverySuffix, COUNT(*) AS cnt
		FROM dbo.JobCartage
			JOIN dbo.JobBookedCtgMove ON JobCartage.JJ_PK = EW_JJ
			JOIN dbo.JobContainerLegs ON EW_PK = JU_EW
			JOIN (SELECT JJ_PK FROM @LegPKsChanged GROUP BY JJ_PK) CartageJobsAffected ON CartageJobsAffected.JJ_PK = JobCartage.JJ_PK
		WHERE JobCartage.JJ_PK BETWEEN @fromJobCartagePK AND @toJobCartagePK
		GROUP BY JobCartage.JJ_PK, JU_SplitDeliverySuffix
		HAVING COUNT(*) > 1
	)
	SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM DuplicateLegCheck) THEN 0 ELSE 1 END AS bit) AS ChunkIsFixed;

";
		const string DropTempNumbersWithSuffixesTableSQL = @"
IF OBJECT_ID('tempdb..#NumbersWithSuffixes') IS NOT NULL
BEGIN
	DROP TABLE #NumbersWithSuffixes;
END
";
	}
}
