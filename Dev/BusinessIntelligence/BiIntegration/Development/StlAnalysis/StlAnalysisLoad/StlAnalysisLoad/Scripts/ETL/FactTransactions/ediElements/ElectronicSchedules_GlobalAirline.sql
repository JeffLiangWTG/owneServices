-- [Source] EDIPROD
-- Transaction and Content Services
---- ediElements
------ Electronic Schedules
-------- Global Airline Schedules

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = '1SS';
DECLARE @DbServerCode char(3) = 'SYD';

SELECT
	CompanyCode = co.LCC_Code,
	TransactionDate = convert(date, ccu.U1_PeriodStart),
	TransactionCount = convert(bigint, round(sum(ccu.U1_UnitCount), 0))
FROM
	ClientChargeableUsage ccu WITH (NOLOCK)
	INNER JOIN LicenceDatabase ld on ld.LD_PK = ccu.U1_LD
	INNER JOIN LicenceEnterprise le on ld.LD_LE = le.LE_PK
	INNER JOIN ClientCompany co on ccu.U1_LCC = co.LCC_PK
WHERE
	le.LE_EnterpriseCode = @EnterpriseCode
	AND ld.LD_ServerCode = @DbServerCode
	AND ccu.U1_PeriodStart >= @StartDateInclusive
	AND ccu.U1_PeriodStart < @EndDateExclusive
	AND ccu.U1_Code = 'RSH'
GROUP BY
	co.LCC_Code,
	convert(date, ccu.U1_PeriodStart)
