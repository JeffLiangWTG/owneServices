 Use AirMessagingDB 
DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0);
DECLARE @dt2 DATE = DATEADD(DAY, + 32, @dt0);

IF OBJECT_ID('tempdb..#T1') IS NOT NULL
       DROP TABLE #T1;

CREATE TABLE #T1 (
       [AWB Serial # 1] NVARCHAR(8)
	   ,[HBS Serial # 1] NVARCHAR(80)
       ,[AWB Prefix 1] NVARCHAR(3)
       ,[Client System Code 1] NVARCHAR(6)
       ,[Client Licence Code 1] NVARCHAR(36)
       ,[Client Licence Name 1] NVARCHAR(128)
       ,[Service Provider 1] NVARCHAR(36)
       ,[Airline Code] NVARCHAR(2)
       ,[Origin City] NVARCHAR(3)
       ,[Origin Country] NVARCHAR(2)
	   ,[Destination City] NVARCHAR(3)
	   ,[Destination Country] NVARCHAR(2)
       ,[Type 1] NVARCHAR(8)
       ,[Sub-Type 1] NVARCHAR(20)
	   ,[Airline Sub-Type 1] NVARCHAR(20)
       ,[Sent UTC 1] DATETIME
       ,[MAWBNumber] NVARCHAR(15)
	   ,[eHub TrackingID 1] NVARCHAR(150)
	   ,[IRJ Reason 1] NVARCHAR(max)
       )

INSERT INTO #T1
SELECT SUBSTRING([MAWBNumber], 4, 8)
	   ,CASE When SUBSTRING([messageBody], 2, 3) = 'FWB'
			Then 'Not Applicable'
			When SUBSTRING([messageBody], 2, 3) = 'FHL'
			Then SUBSTRING([messageBody], CHARINDEX('HBS/', [messageBody])+4, CHARINDEX('/', substring([messageBody],CHARINDEX('HBS/', [messageBody])+4,50))-1) 
			Else 'Unknown'
		End
       ,SUBSTRING([MAWBNumber], 1, 3)
       ,[Client System Code]
       ,[Client Licence Code]
       ,[Client Licence Name]
       ,[Service Provider]
       ,[Airline Code]
       ,CASE When SUBSTRING(MessageBody, 2, 3) = 'FWB'
			Then SUBSTRING(MessageBody, 21, 3)
			When SUBSTRING(MessageBody, 2, 3) = 'FHL'
			Then SUBSTRING([messageBody], CHARINDEX('HBS/', [messageBody])+4+(CHARINDEX('/', substring([messageBody],CHARINDEX('HBS/', [messageBody])+4,50))), 3 )
		End
       ,CASE When SUBSTRING(MessageBody, 2, 3) = 'FWB'
			Then SUBSTRING(MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10) + 'SHP' + CHAR(10), MessageBody, 1) + 5) + 1) + 1) + 2, 2)
			When SUBSTRING(MessageBody, 2, 3) = 'FHL'
			Then SUBSTRING(MessageBody,  
			CHARINDEX(Char(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX('SHP/', MessageBody, 1) + 5)+1)+1)+2, 2)
		END
		---- new columns --
		 ,CASE When SUBSTRING(MessageBody, 2, 3) = 'FWB'
			Then SUBSTRING(MessageBody, 24, 3)
			When SUBSTRING(MessageBody, 2, 3) = 'FHL'
			Then SUBSTRING([messageBody], CHARINDEX('HBS/', [messageBody])+4+(CHARINDEX('/', substring([messageBody],CHARINDEX('HBS/', [messageBody])+4,50))+3), 3 )
		End
       ,CASE When SUBSTRING(MessageBody, 2, 3) = 'FWB'
			Then SUBSTRING(MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10), MessageBody, CHARINDEX(CHAR(10) + 'CNE' + CHAR(10), MessageBody, 1) + 5) + 1) + 1) + 2, 2)
			When SUBSTRING(MessageBody, 2, 3) = 'FHL'
			Then SUBSTRING(MessageBody,  
			CHARINDEX(Char(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX(CHAR(10), MessageBody, 
			CHARINDEX('CNE/', MessageBody, 1) + 5)+1)+1)+2, 2)
		END
       ,SUBSTRING(MessageBody, 2, 3)
       ,'No Sub-Type'
	   ,'No Sub-Type'
       ,[Sent UTC]
       ,[MAWBNumber]
	   ,[All eHub TrackingID]
	   ,NULL
FROM (
       SELECT [Client System Code]
              ,[Client Licence Code]
              ,[Client Licence Name]
              ,[Service Provider]
       ,SenderMessage.value('(/*:CargoIMP/*:Carrier)[1]', 'NVARCHAR(MAX)') AS [Airline Code]
           ,SenderMessage.value('(/*:CargoIMP/*:MAWB)[1]', 'NVARCHAR(MAX)') AS [MAWBNumber]
           ,SenderMessage.value('(/*:CargoIMP/*:Body)[1]', 'NVARCHAR(MAX)') AS [MessageBody]
		   ,[All eHub TrackingID]
              ,[Sent UTC]
       FROM (
SELECT LEFT([Sender ID], 3) + RIGHT([Sender ID], 3) AS [Client System Code]
                     ,[Sender ID] AS [Client Licence Code]
                     ,rtrim(ltrim(replace(replace(replace([Sender Name],char(9),' '),char(10),' '),char(13),' '))) AS [Client Licence Name]
                     ,CASE 
                           WHEN CHARINDEX('_', [Recipient ID]) = 0
                                  THEN [Recipient ID]
                           ELSE LEFT([Recipient ID], CHARINDEX('_', [Recipient ID]) - 1)
                           END AS [Service Provider],
                     AM_ReceivedFromSenderUTC AS [Sent UTC]
                     ,CONVERT(XML, [Raw Message]) AS 'SenderMessage'
					 , [All eHub TrackingID]
              FROM (select * from AirMessagingDB.dbo.AirMessaging
			  Where AM_ApplicationCode = 'CIM'
			  AND AM_ReceivedFromSenderUTC >= @dt0
                     AND AM_ReceivedFromSenderUTC < @dt1 )myTable)myTable2)MyTable3
			  CREATE INDEX IDX_T1_AWBSerialAndClientSystemCode ON #T1 (
       [MAWBNumber]
       ,[Client System Code 1]
       )

----------------------------------------------------------------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#T2') IS NOT NULL
       DROP TABLE #T2;

CREATE TABLE #T2 (
       [AWB Serial # 2] NVARCHAR(8)
       ,[AWB Prefix 2] NVARCHAR(3)
       ,[Client System Code 2] NVARCHAR(6)
       ,[Client Licence Code 2] NVARCHAR(36)
       ,[Client Licence Name 2] NVARCHAR(128)
       ,[Service Provider 2] NVARCHAR(36)
       ,[Type 2] NVARCHAR(8)
       ,[Sub-Type 2] NVARCHAR(20)
	   ,[Airline Sub-Type 2] NVARCHAR(20)
       ,[Sent UTC 2] DATETIME
       ,[MAWBNumber] NVARCHAR(15)
	   ,[eHub TrackingID 2] NVARCHAR(150)
	   ,[IRJ Reason 2] NVARCHAR(max)
       )

INSERT INTO #T2
SELECT SUBSTRING(MAWBNumber, 4, 8)
       ,SUBSTRING([MAWBNumber], 1, 3)
       ,[Client System Code]
       ,[Client Licence Code]
       ,[Client Licence Name]
       ,[Service Provider]
       ,CASE 
               WHEN EventType = 'IRA-FWB'
                     OR EventType = 'IRJ-FWB'
					 OR EventType = 'IRA-FHL'
					 OR EventType = 'IRJ-FHL'
                     THEN EventType
              ELSE 'FSU'
              END
       ,CASE 
              WHEN  EventType = 'IRA-FWB'
                     OR EventType = 'IRJ-FWB'
					 OR EventType = 'IRA-FHL'
					 OR EventType = 'IRJ-FHL'
                     THEN 'No Sub-Type'
              ELSE EventType
              END
       ,CASE 
              WHEN  AirlineEventType = 'IRA-FWB'
                     OR AirlineEventType = 'IRJ-FWB'
					 OR AirlineEventType = 'IRA-FHL'
					 OR AirlineEventType = 'IRJ-FHL'
                     THEN 'No Sub-Type'
              ELSE AirlineEventType
              END
       ,[Sent UTC]
       ,[MAWBNumber]
	   ,[eHub TrackingID 2]
	   ,[IRJ Reason 2]
FROM (
select 
CASE 
When AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event[*:EventType="IRJ"]/*:EventParameters/*:MessageType)[1]', 'NVARCHAR(3)') = 'FHL'
THEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event[*:EventType="IRJ"]/*:ContextCollection/*:Context/*:Value)[5]', 'NVARCHAR(3)') 
When AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event[*:EventType="IRJ"]/*:EventParameters/*:MessageType)[1]', 'NVARCHAR(3)') = 'FWB' 
THEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event[*:EventType="IRJ"]/*:ContextCollection/*:Context/*:Value)[4]', 'NVARCHAR(3)')
ELSE 'No Reason Received'
END AS [IRJ REASON 2]
	   ,COALESCE(Left(AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventType)[1]', 'NVARCHAR(3)') +'-'+
AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="OriginalFWBFHLMessage"]/*:Value)[1]', 'NVARCHAR(MAX)'), 7), 
AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventType)[1]', 'NVARCHAR(3)')) AS EventType

,COALESCE(Left(AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:EventType)[1]', 'NVARCHAR(3)') +'-'+
AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="OriginalFWBFHLMessage"]/*:Value)[1]', 'NVARCHAR(MAX)'), 7), 
AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="SourceEventCode"]/*:Value)[1]', 'NVARCHAR(3)')) AS AirlineEventType

       ,CASE When 
len(REplace(Replace(REPLACE(AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context/*:Value)[1]', 'NVARCHAR(20)'), '-', ''), CHAR(13), ''), CHAR(10), '')) >= 11
Then left(REplace(Replace(REPLACE(AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context/*:Value)[1]', 'NVARCHAR(20)'), '-', ''), CHAR(13), ''), CHAR(10), ''), 11)
Else REplace(Replace(REPLACE(AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context/*:Value)[1]', 'NVARCHAR(20)'), '-', ''), CHAR(13), ''), CHAR(10), '')  
End AS [MAWBNumber]
              ,
			  AM_ReceivedFromSenderUTC AS [Sent UTC]
              ,LEFT([Recipient ID], 3) + RIGHT([Recipient ID], 3) AS [Client System Code]
              ,[Recipient ID] AS [Client Licence Code]
              ,[Recipient Name] AS [Client Licence Name]
              ,[Sender ID] AS [Service Provider]
					 , [All eHub TrackingID] AS [eHub TrackingID 2]




from (SELECT * FROM AirMessaging
where AM_ApplicationCode = 'biz' and [Recipient EnterpriseClient] = 'Enterprise'
AND AM_ReceivedFromSenderUTC >= @dt0
              AND AM_ReceivedFromSenderUTC < @dt1)MyTable )MyTable2
CREATE INDEX IDX_T2_AWBSerialAndClientSystemCode ON #T2 (
       [MAWBNumber]
       ,[Client System Code 2]
       )
--------------------------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#T3') IS NOT NULL
       DROP TABLE #T3;

CREATE TABLE #T3 (
       [AWB Serial # 1] NVARCHAR(8)
	   ,[HBS Serial # 1] NVARCHAR(80)
       ,[AWB Prefix 1] NVARCHAR(3)
       ,[Client System Code 1] NVARCHAR(6)
       ,[Client Licence Code 1] NVARCHAR(36)
       ,[Client Licence Name 1] NVARCHAR(128)
       ,[Service Provider 1] NVARCHAR(36)
       ,[Airline Code] NVARCHAR(2)
       ,[Origin City] NVARCHAR(3)
       ,[Origin Country] NVARCHAR(2)
	   ,[Destination City] NVARCHAR(3)
	   ,[Destination Country] NVARCHAR(2)
       ,[Type 1] NVARCHAR(8)
       ,[Sub-Type 1] NVARCHAR(20)
	   ,[Airline Sub-Type 1] NVARCHAR(20)
       ,[Sent UTC 1] DATETIME
       ,[AWB Serial # 2] NVARCHAR(8)
       ,[AWB Prefix 2] NVARCHAR(3)
       ,[Client System Code 2] NVARCHAR(6)
       ,[Client Licence Code 2] NVARCHAR(36)
       ,[Client Licence Name 2] NVARCHAR(128)
       ,[Service Provider 2] NVARCHAR(36)
       ,[Type 2] NVARCHAR(8)
       ,[Sub-Type 2] NVARCHAR(20)
	   ,[Airline Sub-Type 2] NVARCHAR(20)
	   ,[eHub TrackingID 2] NVARCHAR(150)
	   ,[IRJ Reason 2] NVARCHAR(max)
       ,[Sent UTC 2] DATETIME
       )

INSERT INTO #T3
SELECT [AWB Serial # 1]
	   ,[HBS Serial # 1]
       ,[AWB Prefix 1]
       ,[Client System Code 1]
       ,[Client Licence Code 1]
       ,[Client Licence Name 1]
       ,[Service Provider 1]
       ,[Airline Code]
       ,[Origin City]
       ,[Origin Country]
	   ,[Destination City]
	   ,[Destination Country]
       ,[Type 1]
       ,[Sub-Type 1]
	   ,[Airline Sub-Type 1]
       ,[Sent UTC 1]
       ,[AWB Serial # 2]
       ,[AWB Prefix 2]
       ,[Client System Code 2]
       ,[Client Licence Code 2]
       ,[Client Licence Name 2]
       ,[Service Provider 2]
       ,[Type 2]
       ,[Sub-Type 2]
	   ,[Airline Sub-Type 2]
	   ,[eHub TrackingID 2]
	   ,[IRJ Reason 2]
       ,[Sent UTC 2]
FROM #T1
FULL JOIN #T2 ON #T1.[MAWBNumber] = #T2.[MAWBNumber]
       AND #T1.[Client System Code 1] = #T2.[Client System Code 2]

-----------------------------------------------------------------------------------------------------------
-- Response messages that can be joined back to an FWB
--INSERT Into dbo.AirMessaging

SELECT [AWB Serial # 2] AS [AWB Serial #]
		,[HBS Serial # 1] AS [HSB Serial #]
       ,[AWB Prefix 2] AS [AWB Prefix]
       ,[Client System Code 2] AS [Client System Code]
       ,[Client Licence Code 2] AS [Client Licence Code]
       ,[Client Licence Name 2] AS [Client Licence Name]
       ,[Service Provider 2] AS [Service Provider]
       ,[Airline Code]
       ,[Origin City]
       ,[Origin Country]
	   ,[Destination City]
	   ,[Destination Country]
       ,[Type 2] AS [Type]
       ,[Sub-Type 2] AS [Sub-Type]
	   ,[Airline Sub-Type 2] AS [Airline Sub-Type]
	   ,[eHub TrackingID 2] AS [eHub TrackingID]
	   ,[IRJ Reason 2] AS [IRJ Reason]
       ,[Sent UTC 2] AS [Sent UTC]
FROM (
       SELECT ROW_NUMBER() OVER (
                     PARTITION BY [AWB Serial # 2]
                     ,[Type 2]
                     ,[Sub-Type 2]
					 ,[Airline Sub-Type 2]
                     ,[Sent UTC 2] ORDER BY [Sent UTC 1] DESC
                     ) AS rn
              ,*
       FROM #T3
       WHERE [AWB Serial # 1] IS NOT NULL
              AND [AWB Serial # 2] IS NOT NULL
              AND [Sent UTC 1] < [Sent UTC 2]
       ) MyTable
WHERE rn = 1

UNION ALL

-- Response messages that cannot be joined back to an FWB sent before the response was received
SELECT [AWB Serial # 2] AS [AWB Serial #]
		,[HBS Serial # 1] AS [HSB Serial #]
       ,[AWB Prefix 2] AS [AWB Prefix]
       ,[Client System Code 2] AS [Client System Code]
       ,[Client Licence Code 2] AS [Client Licence Code]
       ,[Client Licence Name 2] AS [Client Licence Name]
       ,[Service Provider 2] AS [Service Provider]
       ,'Unknown Airline Code' AS [Airline Code]
       ,'Unknown Origin City' AS [Origin City]
       ,'Unknown Origin Country' AS [Origin Country]
	   ,'Unknown Destination City' AS [Destination City]
	   ,'Unknown Destination Country' AS [Destination Country]
       ,[Type 2] AS [Type]
       ,[Sub-Type 2] AS [Sub-Type]
	   ,[Airline Sub-Type 2] AS [Airline Sub-Type]
	   ,[eHub TrackingID 2] AS [eHub TrackingID]
	   ,[IRJ Reason 2] AS [IRJ Reason]
       ,[Sent UTC 2] AS [Sent UTC]
FROM (
       SELECT ROW_NUMBER() OVER (
                     PARTITION BY [AWB Serial # 2]
                     ,[Type 2]
                     ,[Sub-Type 2]
					 ,[Airline Sub-Type 2]
                     ,[Sent UTC 2] ORDER BY [Sent UTC 1]
                     ) AS rn
              ,*
       FROM #T3
       WHERE [AWB Serial # 1] IS NOT NULL
              AND [AWB Serial # 2] IS NOT NULL
       ) MyTable
WHERE rn = 1
       AND [Sent UTC 1] >= [Sent UTC 2]

UNION ALL

-- Response messages that cannot be joined back to an FWB
SELECT [AWB Serial # 2] AS [AWB Serial #]
		,[HBS Serial # 1] AS [HSB Serial #]
       ,[AWB Prefix 2] AS [AWB Prefix]
       ,[Client System Code 2] AS [Client System Code]
       ,[Client Licence Code 2] AS [Client Licence Code]
       ,[Client Licence Name 2] AS [Client Licence Name]
       ,[Service Provider 2] AS [Service Provider]
       ,'Unknown Airline Code' AS [Airline Code]
       ,'Unknown Origin City' AS [Origin City]
       ,'Unknown Origin Country' AS [Origin Country]
	   ,'Unknown Destination City' AS [Destination City]
	   ,'Unknown Destination Country' AS [Destination Country]
       ,[Type 2] AS [Type]
       ,[Sub-Type 2] AS [Sub-Type]
	   ,[Airline Sub-Type 2] AS [Airline Sub-Type]
	   ,[eHub TrackingID 2] AS [eHub TrackingID]
	   ,[IRJ Reason 2] AS [IRJ Reason]
       ,[Sent UTC 2] AS [Sent UTC]
FROM #T3
WHERE [AWB Serial # 1] IS NULL
       AND [AWB Serial # 2] <> ''
	   AND [Sent UTC 2] <   @dt1

UNION ALL

-- FWBs

SELECT [AWB Serial # 1] AS [AWB Serial #]
		,[HBS Serial # 1] AS [HSB Serial #]
       ,[AWB Prefix 1] AS [AWB Prefix]
       ,[Client System Code 1] AS [Client System Code]
       ,[Client Licence Code 1] AS [Client Licence Code]
       ,[Client Licence Name 1] AS [Client Licence Name]
       ,[Service Provider 1] AS [Service Provider]
       ,[Airline Code]
       ,[Origin City]
       ,[Origin Country]
	   ,[Destination City]
	   ,[Destination Country]
       ,[Type 1] AS [Type]
       ,[Sub-Type 1] AS [Sub-Type]
	   ,[Airline Sub-Type 1] AS [Airline Sub-Type]
	   ,[eHub TrackingID 1] AS [eHub TrackingID]
	   ,[IRJ Reason 1] AS [IRJ Reason]
       ,[Sent UTC 1] AS [Sent UTC]
FROM #T1	   