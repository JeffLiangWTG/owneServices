-- [Source] EHUB
-- Transaction and Content Services
---- ediElements
------ Messaging and Electronic Submission
-------- Airlines Messaging (FWB/FHL/FSU/FNA/FMA)- via (BT|CCN+Descartes+CCSJ|Delta|GLSHK|TRAXON)

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = '1SS';
DECLARE @DbServerCode char(3) = 'SYD';

DECLARE @FeatureId int;
DECLARE @NetworkList TABLE (Network varchar(36));
DECLARE @NewTransaction TABLE (ClientId int NOT NULL, TransactionDate date NOT NULL, TransactionCount bigint NOT NULL, ClientSystem int NOT NULL);

-- Initialise NETWORK specific variables
---- BT
  --SET @FeatureId = (SELECT FeatureId FROM dbo.DimLicensedFeature WHERE FeatureName = 'Airlines Messaging (FWB/FHL/FSU/FNA/FMA) - via BT');
  --INSERT @NetworkList VALUES ('BT'), ('BT_Test'), ('BT_WithCargoNautSupport');
---- CCN/Descartes/CCSJ
  --SET @FeatureId = (SELECT FeatureId FROM dbo.DimLicensedFeature WHERE FeatureName = 'Airlines Messaging (FWB/FHL/FSU/FNA/FMA) - via CCN/Descartes/CCSJ');
  --INSERT @NetworkList VALUES ('CCN'), ('CCSJ'), ('Descartes'), ('Descartes_WithUnsupported');
---- Delta
  --SET @FeatureId = (SELECT FeatureId FROM dbo.DimLicensedFeature WHERE FeatureName = 'Airlines Messaging (FWB/FHL/FSU/FNA/FMA) - via Delta');
  --INSERT @NetworkList VALUES ('Delta');
---- GLSHK
  --SET @FeatureId = (SELECT FeatureId FROM dbo.DimLicensedFeature WHERE FeatureName = 'Airlines Messaging (FWB/FHL/FSU/FNA/FMA) - via GLSHK');
  --INSERT @NetworkList VALUES ('GLSHK'), ('GLSHK_WithUnsupported');
---- Traxon
  --SET @FeatureId = (SELECT FeatureId FROM dbo.DimLicensedFeature WHERE FeatureName = 'Airlines Messaging (FWB/FHL/FSU/FNA/FMA) - via Traxon');
  --INSERT @NetworkList VALUES ('Traxon');

-- Load message count for FACT and CONTROL tables
INSERT
	@NewTransaction (ClientId, TransactionDate, TransactionCount, ClientSystem)
SELECT
	clientCpy.ClientId, airMsg.MessageDate, sum(airMsg.MessageCount), min(clientSys.ClientId)
FROM
	[WG1-VSQL-1].[StlStage].[dbo].[Stl_BillingReportForAirMessaging] airMsg
	INNER JOIN DimHostedClientCompany clientCpy
		ON clientCpy.EnterpriseCode = airMsg.EnterpriseCode
		AND clientCpy.DatabaseServerCode = airMsg.DatabaseServerCode
		AND clientCpy.CompanyCode = airMsg.CompanyCode
	INNER JOIN DimHostedClientCompany clientSys
		ON clientSys.EnterpriseCode = clientCpy.EnterpriseCode
		AND clientSys.DatabaseServerCode = clientCpy.DatabaseServerCode
		AND clientSys.CompanyCode = ''
	LEFT JOIN ControlLastLoad loadCtrl
		ON loadCtrl.ClientId = clientSys.ClientId
		AND loadCtrl.FeatureId = @FeatureId
WHERE
	airMsg.Network in (SELECT Network FROM @NetworkList)
	AND airMsg.MessageDate > isnull(loadCtrl.TransactionDate, '2011-12-31')
GROUP BY
	clientCpy.ClientId, airMsg.MessageDate;

IF exists(SELECT null FROM @NewTransaction)
BEGIN
	-- FactTransaction
	INSERT dbo.FactTransaction (ClientId, FeatureId, TransactionDate, TransactionCount)
		SELECT newTran.ClientId, @FeatureId, newTran.TransactionDate, newTran.TransactionCount
		FROM @NewTransaction newTran;

	-- ControlLastLoad
	MERGE dbo.ControlLastLoad AS tgt
		USING (
			SELECT ClientSystem, max(TransactionDate)
			FROM @NewTransaction newTran
			GROUP BY ClientSystem
		) AS src (ClientId, TransactionDate)
		ON (tgt.ClientId = src.ClientId AND tgt.FeatureId = @FeatureId)
	WHEN MATCHED THEN
		UPDATE SET TransactionDate = src.TransactionDate
	WHEN NOT MATCHED THEN
		INSERT (ClientId, FeatureId, TransactionDate)
		VALUES (src.ClientId, @FeatureId, src.TransactionDate);
END