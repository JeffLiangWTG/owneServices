------------------
-- ControlLastLoad
------------------

USE StlAnalysis;
go

-- DROP TABLE dbo.ControlLastLoad;
CREATE TABLE dbo.ControlLastLoad
(
	ClientId int NOT NULL,
	FeatureId int NOT NULL,
	TransactionDate date NOT NULL,
	CONSTRAINT PK_ControlLastLoad PRIMARY KEY CLUSTERED (ClientId,FeatureId)
);
-- ALTER TABLE dbo.ControlLastLoad DROP CONSTRAINT [CK_ControlLastLoad_TransactionDate];
ALTER TABLE dbo.ControlLastLoad WITH CHECK
	ADD CONSTRAINT [CK_ControlLastLoad_TransactionDate]
	CHECK (TransactionDate >= '2012-01-01');
-- ALTER TABLE dbo.ControlLastLoad DROP CONSTRAINT [FK_ControlLastLoad_DimHostedClientCompany];
ALTER TABLE dbo.ControlLastLoad WITH CHECK
	ADD CONSTRAINT [FK_ControlLastLoad_DimHostedClientCompany]
	FOREIGN KEY (ClientId)
	REFERENCES dbo.DimHostedClientCompany (ClientId);
-- ALTER TABLE dbo.ControlLastLoad DROP CONSTRAINT [FK_ControlLastLoad_DimLicensedFeature];
ALTER TABLE dbo.ControlLastLoad WITH CHECK
	ADD CONSTRAINT [FK_ControlLastLoad_DimLicensedFeature]
	FOREIGN KEY (FeatureId)
	REFERENCES dbo.DimLicensedFeature (FeatureId);
go

-- TRUNCATE TABLE ControlLastLoad;
-- SELECT * FROM ControlLastLoad;
-- EXEC sp_columns ControlLastLoad
