---------------------
-- DimDate
---------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.DimDate;
CREATE TABLE dbo.DimDate
(
	IdDate int NOT NULL,
	DateKey date NOT NULL,
	DayNumberOfMonth tinyint NOT NULL,
	MonthNumberOfYear tinyint NOT NULL,
	EnglishMonthName varchar(15) NOT NULL,
	CalendarQuarter tinyint NOT NULL,
	CalendarSemester tinyint NOT NULL,
	CalendarYear smallint NOT NULL,
	DayNumberOfWeek tinyint NOT NULL,
	EnglishDayNameOfWeek varchar(10) NOT NULL,
	DayNumberOfYear smallint NOT NULL,
	WeekNumberOfYear tinyint NOT NULL,
	CalendarYearMonth int NOT NULL,
	CalendarYearQuarter smallint NOT NULL,
	CalendarYearSemester smallint NOT NULL,
	CONSTRAINT PK_DimDate PRIMARY KEY CLUSTERED (DateKey),
	CONSTRAINT UK_DimDate UNIQUE NONCLUSTERED (IdDate)
);
go

-- TRUNCATE TABLE DimDate;
-- SELECT * FROM DimDate;
-- EXEC sp_columns DimDate
