-- [Source] DENIEDPARTY
-- Transaction and Content Services
---- ediElements
------ Distance Calculations
-------- Distance Calculator (Google)

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = '1SS';
DECLARE @DbServerCode char(3) = 'SYD';

WITH EhubAuditCte AS (
	SELECT
		EnterpriseCode = substring(B0_LicenceCode, 1, 3),
		CompanyCode = substring(B0_LicenceCode, 5, 3),
		DbServerCode = substring(B0_LicenceCode, 9, 3),
		TransactionDate = convert(date, B0_RequestUTC)
	FROM
		DpsAuditTransactions.dbo.eHubAuditRequest WITH (NOLOCK)
	WHERE
		B0_RequestUTC >= @StartDateInclusive
		AND B0_RequestUTC < @EndDateExclusive
		AND B0_TransactionType = 'DIS'
		AND B0_TransactionSubType in ('GOO', 'PCM')
)
SELECT
	CompanyCode,
	TransactionDate,
	TransactionCount = count(*)
FROM
	EhubAuditCte
WHERE
	EnterpriseCode = @EnterpriseCode
	AND DbServerCode = @DbServerCode
GROUP BY
	CompanyCode,
	TransactionDate
