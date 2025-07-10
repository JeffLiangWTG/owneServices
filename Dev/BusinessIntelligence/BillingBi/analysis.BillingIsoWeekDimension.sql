CREATE VIEW [analysis].[BillingIsoWeekDimension]

AS

SELECT 
	IsoWeek,
	IsoWeekNumber,
	IsoWeekYear,
	EndOfWeekDate AS EndOfIsoWeekDate,
	StartOfWeekDate AS StartOfIsoWeekDate
FROM [analysis].[GetBillingIsoWeekDimension] (2010, 2030)
