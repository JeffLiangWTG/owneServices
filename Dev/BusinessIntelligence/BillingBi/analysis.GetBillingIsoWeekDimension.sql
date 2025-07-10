CREATE FUNCTION [analysis].[GetBillingIsoWeekDimension] 

(
@START_YEAR INT,
@END_YEAR INT
)
RETURNS TABLE 
AS
RETURN 
(

WITH cte AS
(
SELECT DATEADD(dd, Q.Num, DATEFROMPARTS(@START_YEAR, 1, 1)) AS WeekDate
FROM
(
	SELECT ones.n + 10*tens.n + 100*hundreds.n + 1000*thousands.n AS Num
	FROM (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) ones(n),
		 (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) tens(n),
		 (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) hundreds(n),
		 (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) thousands(n)
	WHERE ones.n + 10*tens.n + 100*hundreds.n + 1000*thousands.n BETWEEN 0 AND DATEDIFF(dd, DATEFROMPARTS(@START_YEAR, 1, 1), DATEFROMPARTS(@END_YEAR, 12, 31))
	) Q
),

cte_main AS

(SELECT 

	DISTINCT

	DATEPART(ISO_WEEK, WeekDate) AS IsoWeek,
	CASE 
		WHEN DATEPART(ISO_WEEK, WeekDate) > 1 AND DATEPART(ISO_WEEK, WeekDate) < 52 THEN YEAR(WeekDate)
		WHEN DATEPART(ISO_WEEK, WeekDate) = 1 AND MONTH(WeekDate) = 1 THEN YEAR(WeekDate)
		WHEN DATEPART(ISO_WEEK, WeekDate) = 1 AND MONTH(WeekDate) = 12 THEN YEAR(WeekDate)+1
		WHEN DATEPART(ISO_WEEK, WeekDate) >= 52 AND MONTH(WeekDate) = 1 THEN YEAR(WeekDate)-1
		WHEN DATEPART(ISO_WEEK, WeekDate) >= 52 AND MONTH(WeekDate) = 12 THEN YEAR(WeekDate)
	END AS IsoWeekYear,
	DATEADD(dd, 7-(((DatePart(WEEKDAY, WeekDate) + @@DATEFIRST + 6 - 1 ) % 7) + 1), WeekDate) AS EndOfWeekDate
	
FROM CTE)

SELECT 
	CAST(IsoWeekYear AS VARCHAR(4)) + 'W' + CASE WHEN IsoWeek < 10 THEN '0' ELSE '' END + CAST(IsoWeek AS VARCHAR(2)) AS IsoWeek, 
	IsoWeek AS IsoWeekNumber, 
	IsoWeekYear, 
	EndOfWeekDate,
	DATEADD(dd, -6, EndOfWeekDate) AS StartOfWeekDate
FROM cte_main
)
