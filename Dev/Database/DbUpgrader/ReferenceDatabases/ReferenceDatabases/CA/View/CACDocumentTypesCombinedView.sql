CREATE VIEW CACDocumentTypesCombinedView
	WITH SCHEMABINDING
AS
SELECT FR_PK
, FR_Code
, FR_Desc
, FR_GovAgencyIDCode
FROM dbo.CACDocumentTypes

UNION ALL

SELECT FR_PK
, FR_Code
, FR_Desc
, 'CFIA' AS FR_GovAgencyIDCode
FROM dbo.CACFIARegTypes
