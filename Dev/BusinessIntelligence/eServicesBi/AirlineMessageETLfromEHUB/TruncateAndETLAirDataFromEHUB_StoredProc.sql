USE [AirMessagingDB]
GO

/****** Object:  StoredProcedure [dbo].[TruncateAndETLAirDataFromEHUB]    Script Date: 8/10/2019 9:25:40 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[TruncateAndETLAirDataFromEHUB]
AS
BEGIN
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
OPENQUERY([EHUB_ARCHIVE],'DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0);
DECLARE @dt2 DATE = DATEADD(DAY, + 32, @dt0);
  
   select so.cc_ID AS [Sender ID], RO.CC_ID AS [Recipient ID], so.CC_FriendlyName AS [Sender Name], ro.CC_FriendlyName As [Recipient Name],
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
	   m.AM_CC_RecipientInbox = ''9819EFF9-9CD8-4622-B58E-32A251115722'' -- ri.CC_ID = ''eHubAirService''
	   OR 
	    so.CC_IsAirServiceProvider = 1
	   )
	   AND (AM_DT_RecipientMessageType in (''6C73FDC6-0A91-46F2-9EB1-3E98816E87F0'',
''EA824425-11B2-4B08-8C4C-B47FC1E03A3C'',
''16C507EE-68BE-4D1F-9888-BE865204A388'')
OR AM_DT_RecipientMessageType is null)
AND AM_OutboxMessageTrackingID is not null
              AND m.AM_ReceivedFromSenderUTC >= @dt0
              AND m.AM_ReceivedFromSenderUTC < @dt1') a
END



GO


