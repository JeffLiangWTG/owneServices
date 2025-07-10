/* AccMultilingualTrialBalance @Period, @Company, @Language */
SELECT 	AccGLHeader.AG_Description AS AccountName,
	AccGLHeader.AG_AccountNum AS AccountNumber,
	AccGLHeader.AG_AccountType AS AccountType,
	CurrentDebit,
	CurrentCredit,
	ClosingDebit,
	ClosingCredit,
	HasData,
	AJ_Language,
	AJ_AccountDescription,
	AJ_LocalAccountNumber,
	AJ_ReportType

FROM	dbo.AccGLAccountDescriptor
	LEFT JOIN AccMultilingualTrialBalanceAmounts(@Period, @Company) Base
		ON Base.AJ_PK = AccGLAccountDescriptor.AJ_PK
	LEFT JOIN AccMultilingualTrialBalanceAlternateAmounts(@Period, @Company) Alternate
		ON Alternate.AJ_PK = AccGLAccountDescriptor.AJ_PK
	LEFT JOIN dbo.AccGLHeader ON AccGlHeader.AG_PK = AJ_PK

WHERE	AJ_ReportCategory NOT IN ('CLN', 'TTL', 'CFW')
        AND AJ_Language = @Language and AJ_ReportType = 'COA'

/* AccMultilingualTrialBalanceAmounts @Period, @Company */
SELECT  AJ_PK, CurrentAmount, ClosingAmount
FROM    dbo.AccGLAccountDescriptor 
		LEFT JOIN dbo.AccGLDescriptorPivot ON YJ_AJ = AJ_PK and AJ_ReportType = 'COA'
        LEFT JOIN dbo.AccGLHeader ON AccGLHeader.AG_PK = YJ_AG
        LEFT JOIN AccMultilingualTrialBalanceExcludingAlternates(@Period, @Company) Base ON AccGLHeader.AG_PK = Base.AG_PK

/* AccMultilingualTrialBalanceExcludingAlternates @Period, @Company */
SELECT  AG_PK, [Current].CurrentAmount, Closing.CurrentAmount
FROM    dbo.AccGLHeader
        LEFT JOIN AccPLBSExcludingAlternates(@Period, @Period, @Period, @Company) [Current] ON AccGLHeader.AG_PK = [Current].AG_PK
        LEFT JOIN AccPLBSExcludingAlternates(0, @ThisYearStart, @Period, @Company) Closing ON AccGLHeader.AG_PK = Closing.AG_PK
WHERE   AG_AccountType NOT IN ('TTL', 'CLN')

/* AccPLBSExcludingAlternates @BSHStartPeriod, @PLStartPeriod, @EndPeriod, @Company */
INSERT  @Result (@AG_PK, CurrentAmount)
        SELECT  AG_PK, NULL
        FROM    dbo.AccGLHeader
        WHERE   AG_AccountType = 'HDR' OR AG_AccountType = 'CLN'
        
        UNION
        
        SELECT  *
        FROM AccRawTTLExcludingAlternates(@BSHStartPeriod, @PLStartPeriod, @EndPeriod, @Company)
        

DECLARE @ConsolidationAccount uniqueidentifier, @ConsolidationAmount money
DECLARE ResultCursor CURSOR FAST_FORWARD READ_ONLY FOR
SELECT AG_PK
FROM dbo.AccGLHeader 
WHERE AG_AccountType = 'CLN'
ORDER BY AG_AccountNum

OPEN ResultCursor

FETCH NEXT FROM ResultCursor INTO @ConsolidationAccount

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @ConsolidationAmount = SUM(CurrentAmount)
	FROM @Result R JOIN dbo.AccGLHeader A ON R.AG_PK = A.AG_PK
	WHERE AG_AG_ConsolidationNum = @ConsolidationAccount

	UPDATE @Result
	SET CurrentAmount = @ConsolidationAmount
	WHERE AG_PK = @ConsolidationAccount

	FETCH NEXT FROM ResultCursor INTO @ConsolidationAccount
END

CLOSE ResultCursor
DEALLOCATE ResultCursor


/* AccRawTTLExcludingAlternates @BSHStartPeriod, @PLStartPeriod, @EndPeriod, @Company */
DECLARE @PLApp uniqueidentifier
DECLARE @RetainedEarnings money
SELECT @PLApp = dbo.PLAppropriationAccount()
SELECT @RetainedEarnings = ISNULL(dbo.RetainedEarnings(@BSHStartPeriod,@PLStartPeriod, @Company), 0)

INSERT @Result (AG_PK, Amount)
SELECT AG_PK, CurrentAmount
FROM RawAggregate(@PLStartPeriod, @EndPeriod, @Company) JOIN dbo.AccGLHeader ON AG_PK = AG
WHERE AG_AccountType = 'P&L'

INSERT @Result (AG_PK, Amount)
SELECT AG_PK, CurrentAmount
FROM RawAggregate(@BSHStartPeriod, @EndPeriod, @Company) JOIN dbo.AccGLHeader  ON AG_PK = AG
WHERE AG_AccountType = 'BSH'
      AND AG_PK <> @PLApp

INSERT @Result (AG_PK, Amount)
SELECT AG, ISNULL(CurrentAmount, 0) + @RetainedEarnings
FROM	RawAggregate(@BSHStartPeriod, @EndPeriod, @Company)
WHERE	AG = @PLApp

/* RawAggregate @StartPeriod, @EndPeriod, @Company */
SELECT
	AG_PK AS AG,
	SUM(AA_Amount) AS CurrentAmount
FROM
	AccGLHeader
	LEFT JOIN dbo.AccGLAggregate  ON AG_PK = AA_AG
	LEFT JOIN dbo.GlbBranch  ON AA_GB = GB_PK
WHERE
	(AA_Period BETWEEN @StartPeriod AND @EndPeriod OR AA_PK IS NULL)
	AND
	(GB_GC = @Company OR GB_GC IS NULL OR @Company IS NULL)
GROUP BY
	AG_PK
	

/* AccMultilingualTrialBalanceAlternateAmounts @Period, @company*/
SELECT Original.AJ_AJ_AlternativeNum as AJ_PK, CurrentAmount, ClosingAmount
FROM   dbo.AccGLAccountDescriptor Original
		LEFT JOIN dbo.AccGLDescriptorPivot ON YJ_AJ = AJ_PK And AJ_ReportType = 'COA'
       Left Join dbo.AccGLHeader  ON AccGLHeader.AG_PK = YJ_AG
       Left Join AccMultilingualTrialBalanceExcludingAlternates(@Period, @Company) Base ON AccGLHeader.AG_PK = Base.AG_PK


