Use AirMessagingDB
IF OBJECT_ID('AIrMessagingDB..AirMessaging') IS NOT NULL
TRUNCATE TABLE  AirMessagingDB.dbo.AirMessaging;

INSERT INTO  AirMessagingDB.dbo.AirMessaging
SELECT 
	[Sender ID],
	[Recipient ID],
	[Sender Name],
	[Recipient Name],
	AM_Status,
	AM_ApplicationCode,
	[Raw Message],
	CAST([AM_RecipientMessageXML] AS xml) AS [AM_RecipientMessageXML],
	AM_ReceivedFromSenderUTC,
   [All eHub TrackingID],
   [Sender AirProviderflag],
   [Recipient EnterpriseClient],
	AM_ErrorMessage
From
OPENQUERY([EHUB_ARCHIVE],'
DECLARE @StartDateTime datetime = ''2019-07-15 00:00''	-- ENTER THE START DATE TIME (UTC TIME ZONE)--
DECLARE @EndDateTime datetime = ''2019-07-22 00:00''	-- ENTER THE END DATE TIME (UTC TIME ZONE)--
DECLARE @ClientID nvarchar(6) = ''DFOPRO''				-- ENTER THE CLIENT ENTERPRISE CODE + SERVER CODE--



DECLARE @EnterpriseCode nvarchar(3) = Left(@ClientID,3)
DECLARE @ServerCode nvarchar(3) = Right(@ClientID,3)
Select so.cc_ID AS [Sender ID], RO.CC_ID AS [Recipient ID], so.CC_FriendlyName AS [Sender Name], ro.CC_FriendlyName As [Recipient Name],
  m.AM_Status, 
  m.AM_ApplicationCode,
  [AM_SenderMessageRawDecoded]  AS [Raw Message],
  [AM_RecipientMessageXmlDecoded] AS AM_RecipientMessageXML , m.AM_ReceivedFromSenderUTC,
  ''http://ehubadmin.wtg.zone/Messages/Details?AM_PK=''+Cast(AM_PK as nvarchar(max)) AS [All eHub TrackingID],
  so.CC_IsAirServiceProvider AS [Sender AirProviderflag], ro.CC_SystemCategory AS [Recipient EnterpriseClient],  
  m.AM_ErrorMessage
   FROM eHubArchiveMessageDecoded m WITH (NOLOCK)
       LEFT JOIN dbo.eHubClient so WITH (NOLOCK) ON so.CC_PK = m.AM_CC_SenderOutbox
       LEFT JOIN dbo.eHubClient ro WITH (NOLOCK) ON ro.CC_PK = m.AM_CC_RecipientOutbox
       WHERE (
	   m.AM_CC_SenderInbox in (Select cc_pk from dbo.eHubClient where cc_id like @EnterpriseCode+''%''+@ServerCode and m.AM_CC_RecipientInbox = ''9819EFF9-9CD8-4622-B58E-32A251115722'') -- ri.CC_ID = ''eHubAirService''
	   OR 
	    (m.AM_CC_RecipientOutbox in (Select cc_pk from dbo.eHubClient where cc_id like @EnterpriseCode+''%''+@ServerCode and so.CC_IsAirServiceProvider = 1))
	   )
	   AND (AM_DT_RecipientMessageType in (''6C73FDC6-0A91-46F2-9EB1-3E98816E87F0'',
''EA824425-11B2-4B08-8C4C-B47FC1E03A3C'',
''16C507EE-68BE-4D1F-9888-BE865204A388'')
OR AM_DT_RecipientMessageType is null)
AND AM_OutboxMessageTrackingID is not null
              AND m.AM_ReceivedFromSenderUTC >= @StartDateTime
              AND m.AM_ReceivedFromSenderUTC < @EndDateTime;') a
GO