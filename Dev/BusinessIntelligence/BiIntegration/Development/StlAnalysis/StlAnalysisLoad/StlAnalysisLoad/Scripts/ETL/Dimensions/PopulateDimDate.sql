INSERT dbo.DimDate (
	IdDate, DateKey,
	DayNumberOfMonth, MonthNumberOfYear, EnglishMonthName,
	CalendarYearMonth, CalendarYearQuarter, CalendarYearSemester,
	CalendarQuarter, CalendarSemester, CalendarYear,
	DayNumberOfWeek, EnglishDayNameOfWeek, DayNumberOfYear, WeekNumberOfYear
)
SELECT
	(YEAR(AllDates.DateValue) * 10000) + (MONTH(AllDates.DateValue) * 100) + (DAY(AllDates.DateValue)),
	AllDates.DateValue, DAY(AllDates.DateValue), MONTH(AllDates.DateValue),
	CASE MONTH(AllDates.DateValue)
		WHEN 1 THEN 'January' WHEN 2 THEN 'February' WHEN 3 THEN 'March' WHEN 4 THEN 'April' WHEN 5 THEN 'May' WHEN 6 THEN 'June'
		WHEN 7 THEN 'July' WHEN 8 THEN 'August' WHEN 9 THEN 'September' WHEN 10 THEN 'October' WHEN 11 THEN 'November' WHEN 12 THEN 'December'
	END,
	(YEAR(AllDates.DateValue) * 100) + MONTH(AllDates.DateValue), (YEAR(AllDates.DateValue) * 10) + DATEPART(quarter, AllDates.DateValue),
	(YEAR(AllDates.DateValue) * 10) + CASE WHEN DATEPART(quarter, AllDates.DateValue) <=2 THEN 1 ELSE 2 END,
	DATEPART(quarter, AllDates.DateValue), CASE WHEN DATEPART(quarter, AllDates.DateValue) <=2 THEN 1 ELSE 2 END, YEAR(AllDates.DateValue),
	DATEPART(weekday, AllDates.DateValue),
	CASE DATEPART(weekday, AllDates.DateValue)
		WHEN 1 THEN 'Sunday' WHEN 2 THEN 'Monday' WHEN 3 THEN 'Tuesday' WHEN 4 THEN 'Wednesday' WHEN 5 THEN 'Thursday' WHEN 6 THEN 'Friday' WHEN 7 THEN 'Saturday'
	END,
	DATEPART(dayofyear, AllDates.DateValue), DATEPART(week, AllDates.DateValue)
FROM
	(
		SELECT convert(date, DATEADD(day, number, @StartDateInclusive)) DateValue
		FROM master..spt_values 
		WHERE type = 'P' AND number >= 0
		AND DATEADD(day, number, @StartDateInclusive) < @EndDateExclusive
	) AllDates
	LEFT JOIN DimDate ddt ON ddt.DateKey = AllDates.DateValue
WHERE
	ddt.DateKey is null
ORDER BY
	AllDates.DateValue asc;
