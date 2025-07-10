USE [AirMessagingDB]
GO

/****** Object:  StoredProcedure [dbo].[SBRMonthlyReport]    Script Date: 28/05/2019 2:39:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SBRMonthlyReport]
AS
BEGIN
DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0); 


--Temp Table #T1--

IF OBJECT_ID('tempdb..#T1') IS NOT NULL
       DROP TABLE #T1;

CREATE TABLE #T1 ([Key 1] NVARCHAR(100)
,[Client System Code] NVARCHAR(6)
       ,[Client Licence Code] NVARCHAR(36)
       ,[Client Licence Name] NVARCHAR(128)
       ,[Service Provider] NVARCHAR(36)
       ,[Airline Code] NVARCHAR(2)
       ,[Origin City] NVARCHAR(3)
       ,[Origin Country] NVARCHAR(2)
	   ,[Destination City] NVARCHAR(3)
	   ,[Destination Country] NVARCHAR(2)
       ,[Type] NVARCHAR(8)
       ,[Sent UTC] DATETIME
       ,[MAWBNumber] NVARCHAR(15)
	   ,[SBR eHub TrackingID] NVARCHAR(150)
       )

INSERT INTO #T1
SELECT 
	   [MAWBNumber]+[Client System Code]+[Origin Country]  AS [Key 1],
	   [Client system code], 
       [Client Licence Code], 
       [Client Licence Name], 
       [Service Provider], 
       [Airline Code],  
	   [Origin City],
	   [Origin Country],
	   [Destination City],
	   [Destination Country],
	   [Type],
	   [Sent UTC], 
	   [MAWBNumber], 
	   [SBR eHub TrackingID] 
FROM   (
SELECT
		[Client System Code],
		[Client Licence Code],
		[Client Licence Name],
		[Service Provider], 
CASE 
WHEN  
LEFT( 
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventParameters/*:VoyageFlightNumber)[1]', 'NVARCHAR(MAX)'), 2) IS NOT NULL THEN 
LEFT( 
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventParameters/*:VoyageFlightNumber)[1]', 'NVARCHAR(MAX)'), 2) 
WHEN 
LEFT( 
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventParameters/*:VoyageFlightNumber)[1]', 'NVARCHAR(MAX)'), 2) IS NULL THEN 
LEFT( 
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context/*:SubContextCollection/*:SubContext[*:Type="FlightNumber"]/*:Value)[1]', 'NVARCHAR(MAX)'), 2)
ELSE NULL 
END AS [Airline Code],
CASE
WHEN
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MAWBOriginIATAAirportCode"]/*:Value)[1]', 'NVARCHAR(3)') is NULL
THEN Right(sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MBOLOriginUNLOCO"]/*:Value)[1]', 'NVARCHAR(5)'), 3)
ELSE sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MAWBOriginIATAAirportCode"]/*:Value)[1]', 'NVARCHAR(3)')
END AS [Origin City], 
LEFT(sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MBOLOriginUNLOCO"]/*:Value)[1]', 'NVARCHAR(5)'), 2) 
AS [Origin Country],
CASE
WHEN
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MAWBDestinationIATAAirportCode"]/*:Value)[1]', 'NVARCHAR(3)') is NULL
THEN RIGHT( sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MBOLDestinationUNLOCO"]/*:Value)[1]', 'NVARCHAR(5)'), 3)
ELSE sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MAWBDestinationIATAAirportCode"]/*:Value)[1]', 'NVARCHAR(3)')
END AS [Destination City], 
LEFT( sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MBOLDestinationUNLOCO"]/*:Value)[1]', 'NVARCHAR(5)'), 2)
 AS [Destination Country],
 sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventType)[1]', 'NVARCHAR(3)') 
 AS [Type], 
sendermessage.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="MAWBNumber"]/*:Value)[1]', 'NVARCHAR(MAX)') AS [MAWBNumber],  
[ehub trackingid] AS [SBR eHub TrackingID], 
[Sent UTC], 
SenderMessage 
 FROM   (
SELECT  * FROM [dbo].[Sbr_AirMessages]

) MyTable
)MyTable1

CREATE INDEX IDX_T1_Key1 ON #T1 (
       [Key 1]
       )


--Temp Table #T2---
IF OBJECT_ID('tempdb..#T2') IS NOT NULL
       DROP TABLE #T2;

CREATE TABLE #T2 ([Key 2] NVARCHAR(100)
	   ,[FWB eHub TrackingID] NVARCHAR(150)
       )

INSERT INTO #T2

SELECT MAWBNumber+[Client system code]+[Origin Country Code] AS Key2, [FWB eHub TrackingID]
FROM ( 
SELECT [Client system code], [Service Provider], [FWB eHub TrackingID], MAWBNumber, [MessageBody],
CASE When SUBSTRING(MessageBody, 2, 3) = 'FWB'
			Then SUBSTRING(MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10) + 'SHP' + CHAR(10), MessageBody, 1) + 5) + 1) + 1) + 2, 2)
			When SUBSTRING(MessageBody, 2, 3) = 'FHL'
			Then SUBSTRING(MessageBody,  
			CHARINDEX(Char(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX('SHP/', MessageBody, 1) + 5)+1)+1)+2, 2)
		END AS [Origin Country Code]
FROM(
SELECT [Client system code], [Service Provider], [FWB eHub TrackingID], MAWBNumber, 
 SenderMessage.value('(/*:CargoIMP/*:Body)[1]', 'NVARCHAR(MAX)') AS [MessageBody]  
 FROM(

SELECT  [Client System Code],
	[Service Provider],
	[FWB eHub TrackingID],
	[MAWBNumber],
	CAST([SenderMessage] as xml) AS [SenderMessage]
FROM  [dbo].[Fwb_AirMessages] 

)
MyTable
)
MyTable1
)
MyTable2

CREATE INDEX IDX_T2_Key2 ON #T2 (
       [Key 2]
       )

select 
sbr.[Key 1] AS [Key Code]
,sbr.[Client System Code]
       ,sbr.[Client Licence Code]
       ,sbr.[Client Licence Name]
       ,sbr.[Service Provider]
       ,sbr.[Airline Code]
       ,sbr.[Origin City]
       ,sbr.[Origin Country]
	   ,sbr.[Destination City]
	   ,sbr.[Destination Country]
       ,sbr.[Type]
       ,sbr.[Sent UTC]
       ,sbr.[MAWBNumber]
	   ,sbr.[SBR eHub TrackingID]
	   ,fwb.[FWB eHub TrackingID]
	   ,CASE 
	   WHEN fwb.[Key 2] is null
	   THEN 'Import'
	   ELSE 'Export'
	   END AS [Direction]

 from #T1 SBR Left join #T2 FWB
on sbr.[Key 1] = fwb.[Key 2]
END
GO


