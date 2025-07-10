---------------------
-- DimLicensedFeature
---------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.DimLicensedFeature;
CREATE TABLE dbo.DimLicensedFeature
(
	FeatureId int identity(1,1) NOT NULL,
	FeatureCode char(3) NOT NULL,
	RoleName varchar(50) NOT NULL,
	ModuleName varchar(50) NOT NULL,
	FunctionName varchar(50) NOT NULL,
	FeatureName varchar(75) NOT NULL,
	IsSystemLevel bit NOT NULL,
	IsCompanyLevel bit NOT NULL,
	StlBasis varchar(35) NOT NULL,
	StlEntityCounted varchar(35) NOT NULL,
	StlUnitWeight decimal(9, 3) NOT NULL,
	StlQuery varchar(4000) NOT NULL,
	StlDetailSqlExpression varchar(500) NOT NULL,
	StlDetailCountExpression varchar(500) NULL,
	StlRules xml NULL,
	DataSource varchar(20) NOT NULL,
	CONSTRAINT PK_DimLicensedFeature PRIMARY KEY CLUSTERED (FeatureId),
	CONSTRAINT UK_DimLicensedFeature UNIQUE NONCLUSTERED (FeatureName)
);

-- DROP INDEX dbo.DimLicensedFeature.UX_DimLicensedFeature_FeatureCode
CREATE UNIQUE NONCLUSTERED
	INDEX [UX_DimLicensedFeature_FeatureCode]
	ON dbo.DimLicensedFeature (FeatureCode)
	WHERE FeatureCode is not null;

-- DROP INDEX dbo.DimLicensedFeature.UX_DimLicensedFeature_RoleName_ModuleName_FunctionName_FeatureName
CREATE UNIQUE NONCLUSTERED
	INDEX [UX_DimLicensedFeature_RoleName_ModuleName_FunctionName_FeatureName]
	ON dbo.DimLicensedFeature (RoleName,ModuleName,FunctionName,FeatureName);

ALTER TABLE dbo.DimLicensedFeature
	ADD CONSTRAINT DF_DimLicensedFeature_DataSource
	DEFAULT 'CLIENT' FOR DataSource;

go

-- TRUNCATE TABLE DimLicensedFeature;
-- SELECT * FROM DimLicensedFeature;
-- SELECT * FROM DimLicensedFeature WHERE StlQuery <> '';
-- EXEC sp_columns DimLicensedFeature

-- MUST return no rows (FunctionName MUST be unique across Modules)
SELECT FunctionName, min(ModuleName), max(ModuleName)
	FROM dbo.DimLicensedFeature
	GROUP BY FunctionName
	HAVING min(ModuleName) <> max(ModuleName);

-- MUST return no rows (ModuleName MUST be unique across Roles)
SELECT ModuleName, min(RoleName), max(RoleName)
	FROM dbo.DimLicensedFeature
	GROUP BY ModuleName
	HAVING min(RoleName) <> max(RoleName);

-- MUST return no rows ([TransactionCount = ] used to change summary query to detail)
SELECT *
	FROM dbo.DimLicensedFeature
	WHERE StlQuery <> ''
	AND StlQuery not like '%TransactionCount = %'
