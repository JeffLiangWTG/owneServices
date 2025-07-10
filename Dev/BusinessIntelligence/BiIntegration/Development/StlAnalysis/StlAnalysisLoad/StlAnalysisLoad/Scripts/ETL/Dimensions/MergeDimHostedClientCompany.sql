MERGE dbo.DimHostedClientCompany AS tgt
	USING (SELECT @EnterpriseCode, @DatabaseServerCode, @CompanyCode)
	AS src (EnterpriseCode, DatabaseServerCode, CompanyCode)
	ON (
		tgt.EnterpriseCode = src.EnterpriseCode
		AND tgt.DatabaseServerCode = src.DatabaseServerCode
		AND tgt.CompanyCode = src.CompanyCode
	)
WHEN MATCHED THEN
	UPDATE SET
		OrganisationCode = @OrganisationCode,
		OrganisationName = @OrganisationName,
		CompanyOrgCode = @CompanyOrgCode,
		CompanyOrgName = @CompanyOrgName,
		CompanyCurrency = @CompanyCurrency,
		HomePort = @HomePort,
		PortName = @PortName,
		HomeCountry = @HomeCountry,
		CountryName = @CountryName,
		HostedLocation = @HostedLocation,
		ServerName = @ServerName,
		DatabaseName = @DatabaseName,
		IsCurrent = 1
WHEN NOT MATCHED THEN
	INSERT (
		EnterpriseCode, DatabaseServerCode, CompanyCode,
		OrganisationCode, OrganisationName, CompanyOrgCode, CompanyOrgName, CompanyCurrency,
		HomePort, PortName, HomeCountry, CountryName,
		HostedLocation, ServerName, DatabaseName, IsCurrent)
	VALUES (
		src.EnterpriseCode, src.DatabaseServerCode, src.CompanyCode,
		@OrganisationCode, @OrganisationName, @CompanyOrgCode, @CompanyOrgName, @CompanyCurrency,
		@HomePort, @PortName, @HomeCountry, @CountryName,
		@HostedLocation, @ServerName, @DatabaseName, 1
	);