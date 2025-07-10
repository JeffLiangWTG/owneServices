CREATE FUNCTION RptDt_GetPeriodKeys
(
	@date1 smalldatetime,
	@date2 smalldatetime,
	@gcpk UNIQUEIDENTIFIER
)
RETURNS 
@PeriodKeys TABLE 
(
	PeriodKey Char(42),
	StartDate smalldatetime,
	EndDate smalldatetime,
	ptn int
)
AS  
BEGIN
	
	INSERT INTO @PeriodKeys
	SELECT	convert(char(6), AM_Period) + convert(char(36), AM_GC_Company) AS PartitionKey,
			IIF(@date1 > AM_StartDate, @date1, AM_StartDate) as StartDate,
			IIF(@date2 > AM_EndDate, AM_EndDate, @date2) as EndDate,
			$PARTITION.PF_AccountingPeriodCompany(convert(char(6), AM_Period) + convert(char(36), AM_GC_Company)) as ptn
	FROM dbo.AccPeriodManagement 
	WHERE AM_EndDate >= @date1 AND AM_StartDate <= @date2 And AM_GC_Company = ISNULL(@gcpk, AM_GC_Company)
	
	RETURN 
END
