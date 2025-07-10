-- [Source] DENIEDPARTY
-- Customs & Country Specific Integrations
---- ediComplianceManager
------ Embargo / Denied Parties / Parties of Interest etc
-------- Denied Party Screening

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
		AND B0_TransactionType = 'DPS'
		AND B0_TransactionSubType = 'SCR'
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
