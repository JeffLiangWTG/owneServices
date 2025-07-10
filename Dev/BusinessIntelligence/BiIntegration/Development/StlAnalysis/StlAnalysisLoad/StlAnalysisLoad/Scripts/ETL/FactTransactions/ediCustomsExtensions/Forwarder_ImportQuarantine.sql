-- [Source] EDIPROD
-- Customs & Country Specific Integrations
---- ediCustomsExtensions
------ Forwarder Functions
-------- Import Manifest Quarantine Dec (eBACCa)

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = '1SS';
DECLARE @DbServerCode char(3) = 'SYD';

SELECT
	CompanyCode = lc.LC_CompanyCode,
	TransactionDate = AedUsage.U2_Date,
	TransactionCount = sum(AedUsage.UnitCount)
FROM
	(
		SELECT
			U2_LC,
			convert(date, U2_Date) AS U2_Date,
			count(*) AS UnitCount
		FROM
			[syderouter.db.corporate.cargowise.com].eRouter.dbo.ChargeableMessage WITH (NOLOCK)
		WHERE
			U2_ApplicationCode = 'NZM'
			AND U2_Date >= @StartDateInclusive 
			AND U2_Date < @EndDateExclusive
			AND U2_LC IS NOT NULL
		GROUP BY
			U2_LC,
			convert(date, U2_Date)
	) AedUsage
	INNER JOIN LicenceCompany lc ON lc.LC_PK = AedUsage.U2_LC
	INNER JOIN LicenceEnterprise le ON le.LE_PK = lc.LC_LE
	INNER JOIN LicenceHeader la ON la.LA_LC = lc.LC_PK
	INNER JOIN LicenceDatabase ld ON le.LE_PK = ld.LD_LE AND la.LA_LD = ld.LD_PK
WHERE
	le.LE_EnterpriseCode = @EnterpriseCode
	AND ld.LD_ServerCode = @DbServerCode
	AND la.LA_IsActive = 1
GROUP BY
	lc.LC_CompanyCode,
	AedUsage.U2_Date
