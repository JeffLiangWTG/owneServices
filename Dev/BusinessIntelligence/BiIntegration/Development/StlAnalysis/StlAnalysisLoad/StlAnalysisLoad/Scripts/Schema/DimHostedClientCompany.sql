-------------------------
-- DimHostedClientCompany
-------------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.DimHostedClientCompany
CREATE TABLE dbo.DimHostedClientCompany
(
	ClientId int identity(1,1) NOT NULL,
	EnterpriseCode char(3) NOT NULL,
	DatabaseServerCode char(3) NOT NULL,
	CompanyCode char(3) NOT NULL,
	OrganisationCode nvarchar(12) NOT NULL,
	OrganisationName nvarchar(100) NOT NULL,
	CompanyOrgCode nvarchar(12) NOT NULL,
	CompanyOrgName nvarchar(100) NOT NULL,
	CompanyCurrency varchar(3) NOT NULL,
	HomePort char(5) NOT NULL,
	PortName varchar(35) NOT NULL,
	HomeCountry char(2) NOT NULL,
	CountryName varchar(35) NOT NULL,
	HostedLocation char(3) NOT NULL,
	ServerName varchar(128) NOT NULL,
	DatabaseName varchar(64) NOT NULL,
	IsCurrent bit NOT NULL,
	CONSTRAINT PK_DimHostedClientCompany PRIMARY KEY CLUSTERED (ClientId),
	CONSTRAINT UK_DimHostedClientCompany UNIQUE NONCLUSTERED (EnterpriseCode, DatabaseServerCode, CompanyCode)
)
GO

-- TRUNCATE TABLE DimHostedClientCompany;
-- SELECT * FROM DimHostedClientCompany;
-- SELECT * FROM DimHostedClientCompany WHERE CompanyCode = '';
-- EXEC sp_columns DimHostedClientCompany
