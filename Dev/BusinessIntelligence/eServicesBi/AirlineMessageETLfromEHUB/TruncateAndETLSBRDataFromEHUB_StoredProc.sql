USE [AirMessagingDB]
GO

/****** Object:  StoredProcedure [dbo].[TruncateAndETLSBRDataFromEHUB]    Script Date: 8/10/2019 9:29:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[TruncateAndETLSBRDataFromEHUB]
AS
BEGIN

IF OBJECT_ID('AIrMessagingDB..Sbr_AirMessages') IS NOT NULL
TRUNCATE TABLE  AirMessagingDB.dbo.Sbr_AirMessages;

INSERT INTO  AirMessagingDB.dbo.Sbr_AirMessages
SELECT 
	[Client System Code],
	[Client Licence Code],
	[Client Licence Name],
	[Service Provider],
	[Sent UTC],
	[SenderMessage],
	[eHub TrackingID]
From
OPENQUERY([EHUB_ARCHIVE],'DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0); 
  
   SELECT LEFT(so.cc_id, 3) + RIGHT(so.cc_id, 3) 
                        AS [Client System Code], 
                so.cc_id 
                        AS [Client Licence Code] 
, 
                Rtrim(Ltrim( 
Replace( 
Replace(Replace(so.cc_friendlyname, Char(9), '' ''), Char(10), '' ''), Char(13), '' ''))) AS [Client Licence Name],
ro.cc_id 
        AS [Service Provider], 
m.am_receivedfromsenderutc 
        AS [Sent UTC], 
 m.AM_RecipientMessageXmlDecoded
        AS ''SenderMessage'', 
''http://ehubadmin.wtg.zone/Messages/Details?AM_PK='' 
+ Cast(am_pk AS NVARCHAR(max)) 
        AS [eHub TrackingID] 
FROM   eHubArchiveMessageDecoded m WITH (nolock) 
LEFT JOIN dbo.ehubclient so WITH (nolock) 
       ON so.cc_pk = m.am_cc_senderoutbox 
LEFT JOIN dbo.ehubclient ro WITH (nolock) 
       ON ro.cc_pk = m.am_cc_recipientoutbox 
WHERE   am_cc_recipientinbox = ''1D9B1B57-B23B-4CE0-BF1E-B95F9F04C7E1'' 
AND  m.am_dt_recipientmessagetype = 
      ''6C73FDC6-0A91-46F2-9EB1-3E98816E87F0'' 
       --rt.DT_Code = http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange 
        
AND m.am_sendermessageraw IS NOT NULL 
AND m.am_receivedfromsenderutc >= @dt0 
AND m.am_receivedfromsenderutc < @dt1') a

IF OBJECT_ID('AIrMessagingDB..Fwb_AirMessages') IS NOT NULL
TRUNCATE TABLE  AirMessagingDB.dbo.Fwb_AirMessages;

INSERT INTO  AirMessagingDB.dbo.Fwb_AirMessages
SELECT 
	[Client System Code],
	[Service Provider],
	[FWB eHub TrackingID],
	[MAWBNumber],
	[SenderMessage]
From
OPENQUERY([EHUB_ARCHIVE],'DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0); 
SELECT    LEFT(so.CC_ID, 3) + RIGHT(so.CC_ID, 3) 
                        AS [Client system code] 
, 

CASE 
  WHEN Charindex(''_'', ro.cc_id) = 0 THEN ro.cc_id 
  ELSE LEFT(ro.cc_id, Charindex(''_'', ro.cc_id) - 1) 
END 
        AS [Service Provider], 
''http://ehubadmin.wtg.zone/Messages/Details?AM_PK='' 
+ Cast(am_pk AS NVARCHAR(max)) 
        AS [FWB eHub TrackingID] 
		, Left(Right(AM_OutboxFileNameOverride,11),3)+''-''+Right(AM_OutboxFileNameOverride,8) AS MAWBNumber,
		AM_SenderMessageRawDecoded AS ''SenderMessage''
		
FROM   eHubArchiveMessageDecoded m WITH (nolock) 
LEFT JOIN dbo.ehubclient so WITH (nolock) 
       ON so.cc_pk = m.am_cc_senderoutbox 
LEFT JOIN dbo.ehubclient ro WITH (nolock) 
       ON ro.cc_pk = m.am_cc_recipientoutbox 
WHERE  am_cc_recipientinbox = ''9819EFF9-9CD8-4622-B58E-32A251115722''  
AND  m.am_dt_recipientmessagetype = 
          ''16C507EE-68BE-4D1F-9888-BE865204A388'' 
     -- rt.DT_Code = ''http://www.cargowise.com/ehub/clients/edi/2010/06#FWB''
     
AND m.am_sendermessageraw IS NOT NULL 
AND m.am_receivedfromsenderutc >= @dt0 
AND m.am_receivedfromsenderutc < @dt1') b

END


GO