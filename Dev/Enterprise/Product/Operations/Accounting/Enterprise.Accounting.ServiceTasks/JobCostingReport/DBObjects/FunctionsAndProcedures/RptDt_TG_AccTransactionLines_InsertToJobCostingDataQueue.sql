CREATE TRIGGER RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue
ON AccTransactionLines
AFTER INSERT, UPDATE
AS
INSERT INTO dbo.JobCostingDataQueue (JCQ_ALPK, JCQ_PostDate, JCQ_ReverseDate, JCQ_GCPK)
SELECT
	AL_PK,
	MAX(AL_PostDate) AS AL_PostDate,
	MAX(AL_ReverseDate) AS AL_ReverseDate,
	AL_GC
FROM
(
	SELECT
		i.AL_PK,
		i.AL_PostDate,
		null AL_ReverseDate,
		i.AL_GC
	FROM
		INSERTED i
		LEFT JOIN DELETED d ON i.AL_PK = d.AL_PK
	WHERE	
		i.AL_LineType in ('ACR', 'WIP')
		AND i.AL_PostDate IS NOT NULL
		AND d.AL_PostDate IS NULL
		AND i.AL_JH IS NOT NULL
		AND i.AL_AG IS NOT NULL
		AND i.AL_GC IS NOT NULL

	UNION ALL

	SELECT
		i.AL_PK,
		null,
		i.AL_ReverseDate,
		i.AL_GC
	FROM
		INSERTED i
		LEFT JOIN DELETED d ON i.AL_PK = d.AL_PK
	WHERE
		i.AL_LineType in ('REV','WIP','CST','ACR')
		AND i.AL_ReverseDate IS NOT NULL
		AND d.AL_ReverseDate IS NULL
		AND i.AL_JH IS NOT NULL
		AND i.AL_AG IS NOT NULL
		AND i.AL_GC IS NOT NULL
) IQ
GROUP BY AL_PK, AL_GC