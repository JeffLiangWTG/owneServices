CREATE FUNCTION [analysis].[GetBillingPeriodDimension] 

(
	@START_YEAR INT, 
	@END_YEAR INT
)
RETURNS TABLE 
AS
RETURN 
(

SELECT 
	
	CAST(CONVERT(VARCHAR(6), DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1), 112) AS INT) AS BillingPeriod,

	Q.Num + 1 AS SortOrder,
	Q.Num / 12 + @START_YEAR AS YearNumber,
	Q.Num % 12 + 1 AS MonthNumber,

	DATENAME(month, DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1)) AS FullMonthName,
	
	DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1) AS MonthDate,
	DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 / 3 * 3 + 1, 1) AS QuarterDate,
	DATEFROMPARTS(Q.Num / 12 + @START_YEAR, 1, 1) AS YearDate,
	
	CAST(DATENAME(month, DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1)) AS CHAR(3)) + ' CY' + CAST(Q.Num / 12 + @START_YEAR AS VARCHAR(4)) AS [Month],
	'Q' + CAST(Q.Num % 12 / 3 + 1 AS CHAR(1)) + ' CY' + CAST(Q.Num / 12 + @START_YEAR AS VARCHAR(4)) AS [Quarter],
	'CY' + CAST(Q.Num / 12 + @START_YEAR AS VARCHAR(4)) AS [Year],

	DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1) AS FiscalMonthDate,
	DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 / 3 * 3 + 1, 1) AS FiscalQuarterDate,
	DATEFROMPARTS((Q.Num + 6) / 12 + @START_YEAR, 7, 1) AS FiscalYearDate,

	CAST(DATENAME(month, DATEFROMPARTS(Q.Num / 12 + @START_YEAR, Q.Num % 12 + 1, 1)) AS CHAR(3)) + ' FY' + CAST((Q.Num + 6) / 12 + @START_YEAR AS VARCHAR(4)) AS [FiscalMonth],
	'Q' + CAST((Q.Num + 6) % 12 / 3 + 1 AS CHAR(1)) + ' FY' + CAST((Q.Num + 6) / 12 + @START_YEAR AS VARCHAR(4)) AS [FiscalQuarter],
	'FY' + CAST((Q.Num + 6) / 12 + @START_YEAR AS VARCHAR(4)) AS [FiscalYear]

FROM 

	(
	SELECT ones.n + 10*tens.n + 100*hundreds.n AS Num
	FROM (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) ones(n),
		 (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) tens(n),
		 (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) hundreds(n)
	WHERE ones.n + 10*tens.n + 100*hundreds.n BETWEEN 0 AND (@END_YEAR - @START_YEAR + 1) * 12 - 1
	) Q
)