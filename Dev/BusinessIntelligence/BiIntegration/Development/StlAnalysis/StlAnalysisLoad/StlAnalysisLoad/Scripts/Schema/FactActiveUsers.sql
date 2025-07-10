------------------
-- FactActiveUsers
------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.FactActiveUsers;
CREATE TABLE dbo.FactActiveUsers
(
	ClientId int NOT NULL,
	MonthStartDate date NOT NULL,
	UserCount int NOT NULL,
	CONSTRAINT PK_FactActiveUsers PRIMARY KEY CLUSTERED (ClientId,MonthStartDate)
);
-- ALTER TABLE dbo.FactActiveUsers DROP CONSTRAINT [CK_FactActiveUsers_MonthStartDate];
ALTER TABLE dbo.FactActiveUsers WITH CHECK
	ADD CONSTRAINT [CK_FactActiveUsers_MonthStartDate]
	CHECK (MonthStartDate >= '2012-01-01' AND datepart(day, MonthStartDate) = 1);
-- ALTER TABLE dbo.FactActiveUsers DROP CONSTRAINT [FK_FactActiveUsers_DimHostedClientCompany];
ALTER TABLE dbo.FactActiveUsers WITH CHECK
	ADD CONSTRAINT [FK_FactActiveUsers_DimHostedClientCompany]
	FOREIGN KEY (ClientId)
	REFERENCES dbo.DimHostedClientCompany (ClientId);
-- ALTER TABLE dbo.FactActiveUsers DROP CONSTRAINT [FK_FactActiveUsers_DimDate];
ALTER TABLE dbo.FactActiveUsers WITH CHECK
	ADD CONSTRAINT [FK_FactActiveUsers_DimDate]
	FOREIGN KEY (MonthStartDate)
	REFERENCES dbo.DimDate (DateKey);
go

-- TRUNCATE TABLE FactActiveUsers;
-- TRUNCATE TABLE ControlLastLoad;
-- SELECT * FROM FactActiveUsers;
-- EXEC sp_columns FactActiveUsers

-- MUST return no rows (Active User = system level => EnterpriseCode + DatabaseServerCode)
SELECT *
	FROM dbo.FactActiveUsers fau
	INNER JOIN dbo.DimHostedClientCompany dhcc ON dhcc.ClientId = fau.ClientId
	WHERE dhcc.CompanyCode <> '';
