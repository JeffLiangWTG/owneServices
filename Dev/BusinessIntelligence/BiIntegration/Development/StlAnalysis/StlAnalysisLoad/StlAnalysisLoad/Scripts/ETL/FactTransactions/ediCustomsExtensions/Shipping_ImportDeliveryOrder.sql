-- [Source] EDIPROD
-- Customs & Country Specific Integrations
---- ediCustomsExtensions
------ Shipping/Agency Functions
-------- Import Delivery Order - Electronic (EIDO)

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = 'CSH';
DECLARE @DbServerCode char(3) = 'SYD';

SELECT
	CompanyCode = lc.LC_CompanyCode,
	TransactionDate = convert(date, lx.LX_UsageTime),
	TransactionCount = count(*)
FROM
	ClientLicenceUsage lx WITH (NOLOCK)
	INNER JOIN ClientCompany lcc on lx.LX_LCC = lcc.LCC_PK
	INNER JOIN LicenceDatabase ld on lcc.LCC_LD = ld.LD_PK
	INNER JOIN LicenceCompany lc on lcc.LCC_OH = lc.LC_OH
	INNER JOIN LicenceHeader la on la.LA_LD = ld.LD_PK and la.LA_LC = lc.LC_PK	
	INNER JOIN LicenceEnterprise le on ld.LD_LE = le.LE_PK
	INNER JOIN ClientStaff ls on lx.LX_LS = ls.LS_PK
WHERE
	le.LE_EnterpriseCode = @EnterpriseCode
	AND ld.LD_ServerCode = @DbServerCode
	AND lx.LX_UsageTime >= @StartDateInclusive
	AND lx.LX_UsageTime < @EndDateExclusive
	AND lx.LX_LicenceMode = 'CPT'
	AND lx.LX_ModuleCode = 'SDT'
	AND ls.LS_Email NOT LIKE '%@cargowise.com'
GROUP BY
	lc.LC_CompanyCode,
	convert(date, lx.LX_UsageTime)
