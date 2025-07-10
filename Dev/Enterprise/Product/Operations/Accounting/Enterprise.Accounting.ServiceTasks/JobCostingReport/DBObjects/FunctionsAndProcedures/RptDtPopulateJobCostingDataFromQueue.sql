CREATE PROCEDURE RptDtPopulateJobCostingDataFromQueue
(
	@BatchSize INT,
	@StartingPeriod INT
)
AS

DECLARE @processedRow INT
SET @processedRow = 0

BEGIN TRY
	IF OBJECT_ID('tempdb..#JobCostingDataQueueTemp') IS NOT NULL
	BEGIN
		DROP TABLE #JobCostingDataQueueTemp;	
	END

	IF OBJECT_ID('tempdb..#JobCostingDataQueueSnapShot') IS NOT NULL
	BEGIN
		DROP TABLE #JobCostingDataQueueSnapShot;	
	END
END TRY
BEGIN CATCH
	THROW
END CATCH

BEGIN TRY

	CREATE TABLE #JobCostingDataQueueSnapShot
	(
		JCQ_ALPK uniqueidentifier NOT NULL,
		JCQ_PostDate datetime NULL,
		JCQ_ReverseDate datetime NULL,
		JCQ_GCPK uniqueidentifier NOT NULL,		
	);

	DELETE TOP (@BatchSize) FROM dbo.JobCostingDataQueue
	OUTPUT DELETED.JCQ_ALPK, DELETED.JCQ_PostDate, DELETED.JCQ_ReverseDate, DELETED.JCQ_GCPK
	INTO	#JobCostingDataQueueSnapShot
	WHERE JCQ_HasNoPeriod = 0

	CREATE TABLE #JobCostingDataQueueTemp
	(
		AL_PK uniqueidentifier NOT NULL,
		AL_PostDate datetime NULL,
		PostPeriod int NULL,
		AL_ReverseDate datetime NULL,
		ReversePeriod int NULL,
		AL_GC uniqueidentifier NOT NULL
	);

	DELETE JCS
	OUTPUT DELETED.JCQ_ALPK,
		DELETED.JCQ_PostDate,
		AM_PostDate.AM_Period,
		DELETED.JCQ_ReverseDate,
		AM_ReverseDate.AM_Period,
		DELETED.JCQ_GCPK
	INTO #JobCostingDataQueueTEMP
	FROM #JobCostingDataQueueSnapShot JCS
			LEFT JOIN dbo.AccPeriodManagement AM_PostDate ON AM_PostDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_PostDate between AM_PostDate.AM_StartDate AND AM_PostDate.AM_EndDate
			LEFT JOIN dbo.AccPeriodManagement AM_ReverseDate ON AM_ReverseDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_ReverseDate between AM_ReverseDate.AM_StartDate AND AM_ReverseDate.AM_EndDate
	WHERE 	
			(	CASE 
					WHEN JCS.JCQ_PostDate IS NOT NULL AND JCS.JCQ_ReverseDate IS NOT NULL AND AM_ReverseDate.AM_Period >= @StartingPeriod AND AM_PostDate.AM_Period >= @StartingPeriod THEN 1
					WHEN JCS.JCQ_PostDate IS NOT NULL AND  AM_PostDate.AM_Period >= @StartingPeriod AND JCS.JCQ_ReverseDate IS NULL  THEN 1
					WHEN JCS.JCQ_ReverseDate IS NOT NULL AND AM_ReverseDate.AM_Period >= @StartingPeriod AND JCS.JCQ_PostDate IS NULL THEN 1
					ELSE 0
				END
			) = 1

	SET @processedRow = (SELECT @@ROWCOUNT AS DELETED);


	DECLARE @regValue CHAR(1)
	SET @regValue = 'F'
	SELECT  @regValue = ISNULL(CONVERT(CHAR(1), SD_BinaryValue), 'F') FROM dbo.StmData where SD_Name = 'RemoveLineThatDoesnotHaveAccountingPeriodFromJobCostingDataQueueTable' and SD_Owner IS NULL

	IF @regValue ='F'
	BEGIN
		INSERT INTO dbo.JobCostingDataQueue (JCQ_ALPK, JCQ_PostDate, JCQ_ReverseDate, JCQ_GCPK, JCQ_HasNoPeriod)
		SELECT JCQ_ALPK, JCQ_PostDate, JCQ_ReverseDate, JCQ_GCPK, 1
		FROM #JobCostingDataQueueSnapShot JCS
			LEFT JOIN dbo.AccPeriodManagement AM_PostDate ON AM_PostDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_PostDate between AM_PostDate.AM_StartDate AND AM_PostDate.AM_EndDate
			LEFT JOIN dbo.AccPeriodManagement AM_ReverseDate ON AM_ReverseDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_ReverseDate between AM_ReverseDate.AM_StartDate AND AM_ReverseDate.AM_EndDate
		WHERE 	
			(	CASE 
					WHEN (JCS.JCQ_PostDate IS NOT NULL OR JCS.JCQ_ReverseDate IS NOT NULL) AND (AM_ReverseDate.AM_Period < @StartingPeriod OR AM_PostDate.AM_Period < @StartingPeriod) THEN 1
					ELSE 0
				END
			) = 0
	END

	DROP TABLE IF EXISTS #JobCostingDataInserted;
	CREATE TABLE #JobCostingDataInserted
	(
		JCD_JH UNIQUEIDENTIFIER NOT NULL,
		JCD_GC UNIQUEIDENTIFIER NOT NULL,
		JCD_OH UNIQUEIDENTIFIER NULL,
		JCD_PostPeriod INT NOT NULL DEFAULT 0,
		JCD_RX_NKLocalCurrency CHAR(3) NOT NULL DEFAULT '',
		JCD_LineType CHAR(3) NOT NULL DEFAULT '', 
		JCD_LineAmount MONEY NOT NULL DEFAULT 0,
	);

	INSERT INTO dbo.RptDtJobCostingData
	(
		JCD_AL,
		JCD_AH,
		JCD_JH,
		JCD_AC,
		JCD_AG,
		JCD_OH,
		JCD_GE,
		JCD_GB,
		JCD_GC,
		JCD_LineType,
		JCD_Desc,	
		JCD_PostDate,
		JCD_PostPeriod,
		JCD_RevRecognitionType,
		JCD_LineAmount,
		JCD_GSTVAT,
		JCD_RX_NKLocalCurrency,
		JCD_OSAmount,
		JCD_RX_NKCurrency,
		JCD_TransactionNum,
		JCD_TransactionType,
		JCD_Ledger,
		JCD_ParentID,
		JCD_ParentTableCode,
		JCD_PeriodCompanyKey
	)
	OUTPUT INSERTED.JCD_JH,
			INSERTED.JCD_GC,
			INSERTED.JCD_OH,
			INSERTED.JCD_PostPeriod,
			INSERTED.JCD_RX_NKLocalCurrency,
			INSERTED.JCD_LineType,
			INSERTED.JCD_LineAmount
	INTO	#JobCostingDataInserted
	SELECT
		AL_PK,
		AL_AH,
		AL_JH,
		AL_AC,
		AL_AG,
		AL_OH,
		AL_GE,
		AL_GB,
		AL_GC,
		AL_LineType,
		AL_Desc,
		AL_PostDate,
		AL_PostPeriod,
		AL_RevRecognitionType,
		AL_LineAmount,
		AL_GSTVAT,
		GC_RX_NKLocalCurrency,
		AL_OSAmount,
		AL_RX_NKTransactionCurrency,
		AH_TransactionNum,
		AH_TransactionType,
		AH_Ledger,
		JH_ParentID,
		JH_ParentTableCode,
		(CONVERT(char(6), AL_PostPeriod) + CONVERT(char(36), AL_GC)) as JCD_PeriodCompanyKey
	FROM
	(
		SELECT
			AL.AL_PK,
			AL_AH,
			AL_JH,
			AL_AC,
			AL_AG,
			AL_OH,
			AL_GE,
			AL_GB,
			AL.AL_GC,
			AL_LineType,
			AL_Desc,
			AL.AL_PostDate,
			AL_PostPeriod = TQ.PostPeriod,
			AL_RevRecognitionType,
			-AL_LineAmount AS AL_LineAmount, 
			-AL_GSTVAT AS AL_GSTVAT, 
			GC_RX_NKLocalCurrency,
			-AL_OSAmount AS AL_OSAmount,
			AL_RX_NKTransactionCurrency,
			'' as AH_TransactionNum,
			'' as AH_TransactionType,
			'JC' AS AH_Ledger,
			JH_ParentID,
			JH_ParentTableCode
		FROM 
			#JobCostingDataQueueTemp TQ
			INNER JOIN dbo.AccTransactionLines AL
				WITH (FORCESEEK, INDEX(PK_UX__AL_PK))
				ON AL.AL_PK = TQ.AL_PK
				AND AL.AL_PostDate = TQ.AL_PostDate
				AND AL.AL_GC = TQ.AL_GC
			INNER JOIN dbo.JobHeader
				WITH (FORCESEEK, INDEX(PK_UX__JH_PK))
				ON AL_JH = JH_PK
			INNER JOIN dbo.GlbCompany
				ON GC_PK = AL.AL_GC 

		WHERE
			AL_LineType in ('ACR', 'WIP')
			AND AL.AL_PostDate IS NOT NULL
			AND AL.AL_JH IS NOT NULL
			AND AL.AL_AG IS NOT NULL

		UNION ALL

		SELECT
			AL.AL_PK,
			AL_AH,
			AL_JH,
			AL_AC,
			AL_AG,
			AL_OH,
			AL_GE,
			AL_GB,
			AL.AL_GC,
			AL_LineType,
			AL_Desc,	
			AL.AL_ReverseDate AS AL_PostDate,
			AL_PostPeriod = TQ.ReversePeriod,
			AL_RevRecognitionType,
			AL_LineAmount,  
			AL_GSTVAT, 
			GC_RX_NKLocalCurrency,
			AL_OSAmount,
			AL_RX_NKTransactionCurrency,
			'' as AH_TransactionNum,
			'' as AH_TransactionType,
			'JC' AS AH_Ledger,
			JH_ParentID,
			JH_ParentTableCode
		FROM 
			#JobCostingDataQueueTemp TQ
			INNER JOIN dbo.AccTransactionLines AL
				WITH (FORCESEEK, INDEX(PK_UX__AL_PK))
				ON AL.AL_PK = TQ.AL_PK
				AND AL.AL_ReverseDate = TQ.AL_ReverseDate
				AND AL.AL_GC = TQ.AL_GC
			INNER JOIN dbo.JobHeader
				WITH (FORCESEEK, INDEX(PK_UX__JH_PK))
				ON AL_JH = JH_PK
			INNER JOIN dbo.GlbCompany
				ON GC_PK = AL.AL_GC
		WHERE
			AL_LineType in ('ACR', 'WIP')
			AND AL.AL_ReverseDate IS NOT NULL
			AND AL.AL_JH IS NOT NULL
			AND AL.AL_AG IS NOT NULL

		UNION ALL

		SELECT 
			AL.AL_PK,
			AL_AH,
			AL_JH,
			AL_AC,
			AL_AG,
			ISNULL(AL_OH, AH_OH) AS AL_OH,
			AL_GE,
			AL_GB,
			AL.AL_GC,
			AL_LineType,
			AL_Desc,	
			AL.AL_ReverseDate AS AL_PostDate,
			AL_PostPeriod = TQ.ReversePeriod,
			AL_RevRecognitionType,
			AL_LineAmount,  
			AL_GSTVAT, 
			GC_RX_NKLocalCurrency,
			AL_OSAmount, 
			AL_RX_NKTransactionCurrency,
			AH_TransactionNum,
			AH_TransactionType,
			AH_Ledger,
			JH_ParentID,
			JH_ParentTableCode
		FROM 
			#JobCostingDataQueueTemp TQ
			INNER JOIN dbo.AccTransactionLines AL
				WITH (FORCESEEK, INDEX(PK_UX__AL_PK))
				ON AL.AL_PK = TQ.AL_PK
				AND AL.AL_ReverseDate = TQ.AL_ReverseDate
				AND AL.AL_GC = TQ.AL_GC
			INNER JOIN dbo.AccTransactionHeader AH
				WITH (FORCESEEK, INDEX(PK_UC__AH_PK))
				ON AH_PK = AL_AH
			INNER JOIN dbo.JobHeader
				WITH (FORCESEEK, INDEX(PK_UX__JH_PK))
				ON AL_JH = JH_PK
			INNER JOIN dbo.GlbCompany
				ON GC_PK = AL.AL_GC
		WHERE
			AL_LineType in ('CST', 'REV')
			AND AL.AL_ReverseDate IS NOT NULL
			AND AL.AL_JH IS NOT NULL
			AND AL.AL_AG IS NOT NULL
			AND AH.AH_TransactionType IN ('JRJ', 'JNL', 'INV', 'CRD', 'ADJ')
	) allData
	WHERE AL_PostPeriod IS NOT NULL
	ORDER BY JCD_PeriodCompanyKey, AL_PostDate
	OPTION (MAXDOP 1, USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'));

	-- Update dependent table RptJobCostingDataAmountByJob
	MERGE INTO RptDtJobCostingDataAmountByJob 
		WITH (FORCESEEK, INDEX(RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency)) 
		AS tgt
	USING (
		SELECT JCD_JH, JCD_GC, JCD_OH, JCD_PostPeriod, JCD_RX_NKLocalCurrency COLLATE SQL_Latin1_General_CP1_CI_AS, 
			SUM(CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN CONVERT(DECIMAL(28,4), JCD_LineAmount) ELSE 0 END) AS Revenue,
			SUM(CASE WHEN JCD_LineType IN ('CST', 'ACR') THEN CONVERT(DECIMAL(28,4), JCD_LineAmount) ELSE 0 END) AS Cost,
			COUNT_BIG(1) AS RowCount_1
		FROM #JobCostingDataInserted AS src
		GROUP BY JCD_JH, JCD_GC, JCD_OH, JCD_PostPeriod, JCD_RX_NKLocalCurrency
	) src (JCD_JH, JCD_GC, JCD_OH, JCD_PostPeriod, JCD_RX_NKLocalCurrency, Revenue, Cost, RowCount_1)
	ON (src.JCD_PostPeriod = tgt.JCA_PostPeriod
		AND src.JCD_GC = tgt.JCA_GC
		AND ISNULL(src.JCD_OH, 0x) = ISNULL(tgt.JCA_OH_DebtorOrCreditor, 0x)
		AND src.JCD_JH = tgt.JCA_JH
		AND src.JCD_RX_NKLocalCurrency = tgt.JCA_RX_NKLocalCurrency
	)
	WHEN MATCHED THEN
		UPDATE SET tgt.JCA_Revenue = tgt.JCA_Revenue  + src.Revenue
				, tgt.JCA_Cost     = tgt.JCA_Cost     + src.Cost
				, tgt.JCA_RowCount = tgt.JCA_RowCount + src.RowCount_1
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (JCA_JH, JCA_GC, JCA_OH_DebtorOrCreditor, JCA_PostPeriod, JCA_RX_NKLocalCurrency, JCA_Revenue, JCA_Cost, JCA_RowCount)
		VALUES (JCD_JH, JCD_GC, JCD_OH,                  JCD_PostPeriod, JCD_RX_NKLocalCurrency, Revenue,     Cost,     RowCount_1)
	OPTION (MAXDOP 1, USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'));

	RETURN @processedRow

END TRY
BEGIN CATCH
	THROW
END CATCH
