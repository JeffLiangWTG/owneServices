/* ClientAccPLBSExcludingAlternates (@BSHStartPeriod as int, @PLStartPeriod as int, @EndPeriod as int, @Company as uniqueidentifier) */
RETURNS @Result TABLE 
(
	AG_PK uniqueidentifier,
	CurrentAmount money
)
WITH SCHEMABINDING
BEGIN

INSERT @Result (AG_PK, CurrentAmount)
	SELECT AG_PK, NULL
	FROM   dbo.AccGLHeader
	WHERE  AG_AccountType = 'HDR'
               OR AG_AccountType = 'CLN'

	UNION

        SELECT RawTTL.* FROM dbo.ClientAccRawTTLExcludingAlternates As RawTTL

UPDATE	@Result
SET	CurrentAmount = (SELECT SUM(CurrentAmount)
                         FROM	@Result R
				INNER JOIN dbo.AccGLHeader ON R.AG_PK = AccGLHeader.AG_PK
                         WHERE	AG_AG_ConsolidationNum = ToBeUpdated.AG_PK)
FROM	@Result ToBeUpdated
        INNER JOIN (
                    SELECT	AG_PK
                    FROM	@Result R
		                INNER JOIN  dbo.AccGLHeader ON R.AG_PK = AccGLHeader.AG_PK
                    WHERE	AG_AccountType = 'CLN'
                    ) ConsolidationAccounts ON ToBeUpdated.AG_PK = ConsolidationAccounts.AG_PK

RETURN
END


/* ClientAccTTLExcludingAlternates */
SELECT * FROM dbo.ClientAccRawPLBSExcludingAlternates

UNION

SELECT   Total.AG_PK, SUM(Amount) AS Amount
FROM     dbo.AccGLHeader Main
         INNER JOIN dbo.ClientAccRawPLBSExcludingAlternates() RawAgg ON RawAgg.AG_PK = Main.AG_PK
         INNER JOIN dbo.AccGLHeader Total ON Main.AG_AccountNum < Total.AG_AccountNum
	       AND Main.AG_AccountNum > ISNULL
               (
		(SELECT   TOP 1 T.AG_AccountNum
                 FROM     dbo.AccGLHeader T
		 WHERE    T.AG_TotalLevel >= Total.AG_TotalLevel AND T.AG_AccountNum < Total.AG_AccountNum
		 ORDER BY T.AG_AccountNum DESC)
                , '')

WHERE    Total.AG_AccountType = 'TTL'
GROUP BY Total.AG_PK

/* ClientAccRawPLBSExcludingAlternates */
DECLARE @PLApp uniqueidentifier
DECLARE @RetainedEarnings money

SELECT @PLApp = dbo.PLAppropriationAccount()

SELECT @RetainedEarnings = SUM(T5_Amount)
FROM   #CognosExport
       INNER JOIN dbo.AccGLHeader ON AG_PK = T5_AG
WHERE  AG_AccountType = 'P&L'

SELECT AG_PK, 
       CASE
           WHEN AG_PK = @PLApp THEN ISNULL(CurrentAmount, 0) + @RetainedEarnings
           ELSE CurrentAmount
       END AS CurrentAmount

FROM   (
              SELECT   T1_AG AS AG, SUM(Amount) AS CurrentAmount
              FROM     #CognosAccounts
                       LEFT OUTER JOIN #CognosExport ON T5_AG = T1_AG
              GROUP BY T1_AG
        ) CognosAggregates
        INNER JOIN dbo.AccGLHeader ON AG_PK = AG

WHERE   AG_AccountType = 'P&L'
        OR (AG_AccountType = 'BSH' AND AG_PK != @PLApp)
        OR (AG = @PLApp)

