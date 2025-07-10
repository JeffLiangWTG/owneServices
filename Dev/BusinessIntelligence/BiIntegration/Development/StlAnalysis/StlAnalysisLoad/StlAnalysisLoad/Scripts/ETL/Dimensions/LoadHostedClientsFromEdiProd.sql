WITH EnterpriseClientCte (
	LicenceEnterprisePk, EnterpriseCode,
	LicenceDatabasePk, DatabaseServerCode,
	ClientOrgCode, ClientOrgName,
	HomePort, PortName, HomeCountry, CountryName,
	HostedLocation, ServerName, DatabaseName) AS
(
	SELECT
		le.LE_PK AS LicenceEnterprisePk,
		le.LE_EnterpriseCode AS EnterpriseCode,
		ld.LD_PK AS LicenceDatabasePk,
		ld.LD_ServerCode AS DatabaseServerCode,
		oh.OH_Code AS ClientOrgCode,
		oh.OH_FullName AS ClientOrgName,
		oh.OH_RL_NKClosestPort AS HomePort,
		rl.RL_PortName AS PortName,
		rl.RL_RN_NKCountryCode AS HomeCountry,
		rn.RN_Desc AS CountryName,
		ld.LD_HostedLocation AS HostedLocation,
		ld.LD_HostServerName AS ServerName,
		ld.LD_HostDBName AS DatabaseName
	FROM
		LicenceEnterprise le
		INNER JOIN LicenceDatabase ld ON ld.LD_LE = le.LE_PK
		INNER JOIN dbo.OrgHeader oh ON le.LE_OH = oh.OH_PK
		INNER JOIN dbo.RefUNLOCO rl ON rl.RL_Code = oh.OH_RL_NKClosestPort
		INNER JOIN dbo.RefCountry rn ON rn.RN_Code = rl.RL_RN_NKCountryCode
	WHERE 1=1
		AND ld.LD_LicenceExpiry > dateadd(month, -1, sysutcdatetime())
		AND ld.LD_LicenceType = 'PRD'
		--[=CLIENT_PREDICATE=START=]
		AND
		(
			ld.LD_HostedLocation <> 'NCW'
			AND ld.LD_HostDBName = 'Odyssey' + le.LE_EnterpriseCode + ld.LD_ServerCode
		)
		--[=CLIENT_PREDICATE=END=]
)
SELECT
	ec.EnterpriseCode, ec.DatabaseServerCode, CompanyCode = '',
	ec.ClientOrgCode, ec.ClientOrgName,
	CompanyOrgCode = '', CompanyOrgName = '', CompanyCurrency = '',
	ec.HomePort, ec.PortName, ec.HomeCountry, ec.CountryName,
	ec.HostedLocation, ec.ServerName, ec.DatabaseName
FROM
	EnterpriseClientCte ec
UNION ALL
SELECT
	ec.EnterpriseCode, ec.DatabaseServerCode, CompanyCode = lc.LC_CompanyCode,
	ec.ClientOrgCode, ec.ClientOrgName,
	CompanyOrgCode = coh.OH_Code, CompanyOrgName = coh.OH_FullName, CompanyCurrency = lc.LC_RX_NKCurrency,
	ec.HomePort, ec.PortName, ec.HomeCountry, ec.CountryName,
	ec.HostedLocation, ec.ServerName, ec.DatabaseName
FROM
	EnterpriseClientCte ec
	INNER JOIN LicenceCompany lc ON lc.LC_LE = ec.LicenceEnterprisePk
	INNER JOIN LicenceHeader la ON la.LA_LC = lc.LC_PK AND la.LA_LD = ec.LicenceDatabasePk
	INNER JOIN dbo.OrgHeader coh ON coh.OH_PK = lc.LC_OH
ORDER BY EnterpriseCode, DatabaseServerCode, CompanyCode;
