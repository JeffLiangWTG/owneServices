------------------
-- FactTransaction
------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.FactTransaction;
CREATE TABLE dbo.FactTransaction
(
	ClientId int NOT NULL,
	FeatureId int NOT NULL,
	TransactionDate date NOT NULL,
	TransactionCount bigint NOT NULL,
	CONSTRAINT PK_FactTransaction PRIMARY KEY CLUSTERED (ClientId,FeatureId,TransactionDate)
);
-- ALTER TABLE dbo.FactTransaction DROP CONSTRAINT [CK_FactTransaction_TransactionDate];
ALTER TABLE dbo.FactTransaction WITH CHECK
	ADD CONSTRAINT [CK_FactTransaction_TransactionDate]
	CHECK (TransactionDate >= '2012-01-01');
-- ALTER TABLE dbo.FactTransaction DROP CONSTRAINT [FK_FactTransaction_DimHostedClientCompany];
ALTER TABLE dbo.FactTransaction WITH CHECK
	ADD CONSTRAINT [FK_FactTransaction_DimHostedClientCompany]
	FOREIGN KEY (ClientId)
	REFERENCES dbo.DimHostedClientCompany (ClientId);
-- ALTER TABLE dbo.FactTransaction DROP CONSTRAINT [FK_FactTransaction_DimLicensedFeature];
ALTER TABLE dbo.FactTransaction WITH CHECK
	ADD CONSTRAINT [FK_FactTransaction_DimLicensedFeature]
	FOREIGN KEY (FeatureId)
	REFERENCES dbo.DimLicensedFeature (FeatureId);
-- ALTER TABLE dbo.FactTransaction DROP CONSTRAINT [FK_FactTransaction_DimDate];
ALTER TABLE dbo.FactTransaction WITH CHECK
	ADD CONSTRAINT [FK_FactTransaction_DimDate]
	FOREIGN KEY (TransactionDate)
	REFERENCES dbo.DimDate (DateKey);
go

-- TRUNCATE TABLE FactTransaction;
-- TRUNCATE TABLE ControlLastLoad;
-- SELECT * FROM FactTransaction;
-- EXEC sp_columns FactTransaction
