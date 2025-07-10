-- [Source] EHUB
-- Transaction and Content Services
---- ediElements
------ Messaging and Electronic Submission
-------- Airlines Messaging (FWB/FHL/FSU/FNA/FMA)- via (BT|CCN+Descartes+CCSJ|Delta|GLSHK|TRAXON)

-- SERVER  : WG1-VSQL-1
-- DATABASE: StlStage

-----------------------------------------------------------------------------------------------------------

/*
-- DROP TABLE Stl_Load_BillingReportForAirMessaging
CREATE TABLE Stl_Load_BillingReportForAirMessaging
(
	ClientID varchar(max),
	MessageType varchar(max),
	AirlineCodeOrPrefix varchar(max),
	Network varchar(36),
	AWB varchar(max),
	MessageDateTimeUtc datetime,
	MessageXml XML
)

-- DROP TABLE Stl_BillingReportForAirMessaging
CREATE TABLE Stl_BillingReportForAirMessaging
(
	Network varchar(36),
	EnterpriseCode char(3),
	DatabaseServerCode char(3),
	CompanyCode char(3),
	MessageDate date,
	MessageCount int,
	CONSTRAINT PK_StlBillingReportForAirMessaging
		PRIMARY KEY CLUSTERED (Network, EnterpriseCode, DatabaseServerCode, CompanyCode, MessageDate)
)

SELECT * FROM Stl_BillingReportForAirMessaging;
SELECT * FROM Stl_Load_BillingReportForAirMessaging;
*/

-----------------------------------------------------------------------------------------------------------

DECLARE @LoadYear int = 2014;
DECLARE @LoadMonth int = 4;

TRUNCATE TABLE Stl_Load_BillingReportForAirMessaging;
INSERT INTO Stl_Load_BillingReportForAirMessaging
	EXEC [eHubArchiveOnline].dbo.[BillingReportForAirMessaging]
		@Month = @LoadMonth,
		@Year = @LoadYear;

SELECT distinct ClientID
	FROM Stl_Load_BillingReportForAirMessaging
	WHERE len(ltrim(rtrim(ClientID))) <> 9;
go

-----------------------------------------------------------------------------------------------------------

IF (@@rowcount = 0)
BEGIN
	BEGIN TRANSACTION;

	BEGIN TRY
		INSERT
			INTO Stl_BillingReportForAirMessaging
			SELECT
				Network,
				EnterpriseCode = left(ltrim(ClientID), 3),
				DatabaseServerCode = substring(ltrim(ClientID), 7, 3),
				CompanyCode = substring(ltrim(ClientID), 4, 3),
				MessageDate = convert(date, MessageDateTimeUtc),
				MessageCount = count(*)
			FROM
				Stl_Load_BillingReportForAirMessaging
			WHERE
				len(ltrim(rtrim(ClientID))) = 9
			GROUP BY
				Network,
				ClientID,
				convert(date, MessageDateTimeUtc);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE();
	END CATCH
END
