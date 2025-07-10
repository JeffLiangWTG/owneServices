CREATE VIEW [analysis].[BillingTransactions]

AS

SELECT

	ch.CH_Period AS BillingPeriod,
	ch.CH_Category + '.' + ch.CH_PriceItemCode AS PriceItemKey,
	ch.CH_ReportingSource AS ReportingSource,
	ch.CH_Category AS Category,
	ch.CH_PriceItemCode AS PriceItemCode,
	ch.CH_ClientID AS LicenceClientKey,
	ch.CH_DatabaseNumber AS DatabaseNumber,
	cc.LCC_PK AS ClientCompanyPK,
	ch.CH_ClientStaffCode AS ClientStaffCode,
	CAST(ch.CH_BillableCount AS BIGINT) AS BillableCount,
	CAST(CASE
		WHEN ch.CH_Category = 'STL' AND ch.CH_PriceItemCode = 'USR' THEN ch.CH_BillableCount
		ELSE NULL
	END AS BIGINT) AS ActiveStaff,
	CAST(CASE
		WHEN p.[Function] = 'General Forwarding Engine' THEN ch.CH_BillableCount
		ELSE NULL
	END AS BIGINT) AS GeneralForwardingEngine,
	CAST(CASE 
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) > 1 AND DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) < 52 THEN YEAR(ch.CH_ServiceOccuredUtc)
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) = 1 AND MONTH(ch.CH_ServiceOccuredUtc) = 1 THEN YEAR(ch.CH_ServiceOccuredUtc)
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) = 1 AND MONTH(ch.CH_ServiceOccuredUtc) = 12 THEN YEAR(ch.CH_ServiceOccuredUtc)+1
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) >= 52 AND MONTH(ch.CH_ServiceOccuredUtc) = 1 THEN YEAR(ch.CH_ServiceOccuredUtc)-1
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) >= 52 AND MONTH(ch.CH_ServiceOccuredUtc) = 12 THEN YEAR(ch.CH_ServiceOccuredUtc)
		END AS VARCHAR(4)) + 'W' + CASE WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) < 10 THEN '0' ELSE '' END + CAST(DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) AS VARCHAR(2)) AS IsoWeek
FROM
	[edi].[Chargeable] ch
	INNER JOIN edi.ClientCompany cc ON ch.CH_DatabaseNumber = cc.DatabaseNumber and ch.CH_CompanyNumber = cc.CompanyNumber
	LEFT JOIN analysis.PriceItem p ON p.Category = ch.CH_Category AND p.PriceItemCode = ch.CH_PriceItemCode
WHERE
	ch.CH_Period >= 201001