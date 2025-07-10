CREATE PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord 
(
	@startRowNumber bigInt,
	@endRowNumber bigInt,
	@rowLeft int OUTPUT
)
AS

BEGIN TRY
	IF OBJECT_ID('tempdb..#TopThousandUnprocessedAccTransactionLines') IS NOT NULL 
	DROP TABLE #TopThousandUnprocessedAccTransactionLines
END TRY
BEGIN CATCH
	THROW
END CATCH

BEGIN TRY
	IF OBJECT_ID('RptDtUnprocessedAccTransactionLines') IS NOT NULL AND OBJECT_ID('RptDtUnprocessedReversedAL') IS NOT NULL 
	BEGIN
		CREATE TABLE #TopThousandUnprocessedAccTransactionLines( AL_PK uniqueidentifier NOT NULL );

		INSERT INTO #TopThousandUnprocessedAccTransactionLines
		SELECT UL_ALPK FROM dbo.RptDtUnprocessedAccTransactionLines WHERE UL_RowNumber between @startRowNumber and @endRowNumber


		INSERT INTO dbo.JobCostingDataQueue (JCQ_ALPK, JCQ_PostDate, JCQ_ReverseDate, JCQ_GCPK)
		SELECT
			AL_PK,
			MAX(AL_PostDate) AS AL_PostDate,
			MAX(AL_ReverseDate) AS AL_ReverseDate,
			AL_GC
		FROM
		(
			-- WE PROCESS THE POST DATE FOR ANYTHING FROM #TopThousandUnprocessedAccTransactionLines
			SELECT
				AL.AL_PK,
				AL.AL_PostDate,
				null AL_ReverseDate,
				AL.AL_GC
			FROM
				dbo.AccTransactionLines AL
				INNER JOIN #TopThousandUnprocessedAccTransactionLines TAL ON TAL.AL_PK = AL.AL_PK
			WHERE
				AL_LineType in ('ACR', 'WIP')
				AND AL_PostDate IS NOT NULL 
				AND AL_JH IS NOT NULL
				AND AL_AG IS NOT NULL
				AND AL_GC IS NOT NULL

			UNION ALL

			SELECT
				AL.AL_PK,
				null,
				AL.AL_ReverseDate,
				AL.AL_GC
			FROM
				dbo.AccTransactionLines AL
				INNER JOIN #TopThousandUnprocessedAccTransactionLines TAL ON TAL.AL_PK = AL.AL_PK
				LEFT JOIN dbo.RptDtUnprocessedReversedAL URL ON URL_ALPK = TAL.AL_PK
			WHERE
				URL.URL_ALPK IS NULL -- EXCLUDE THE REVERSALS OF ANYTHING THAT HAS BEEN REVERSED SINCE THE UPGRADE.
				AND AL_LineType in ('REV','WIP','CST','ACR')
				AND AL_ReverseDate IS NOT NULL
				AND AL_JH IS NOT NULL
				AND AL_AG IS NOT NULL
				AND AL_GC IS NOT NULL
		) IQ
		GROUP BY AL_PK, AL_GC;

		SELECT @rowLeft = MAX(UL_RowNumber) - @endRowNumber FROM dbo.RptDtUnprocessedAccTransactionLines;
	END
END TRY
BEGIN CATCH
	THROW
END CATCH