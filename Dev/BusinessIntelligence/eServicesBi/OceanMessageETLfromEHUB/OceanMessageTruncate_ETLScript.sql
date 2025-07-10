--BEGIN
--IF OBJECT_ID('OceanMessagingDB..OceanMessaging') IS NOT NULL
--TRUNCATE TABLE  OceanMessagingDB.dbo.OceanMessaging;

INSERT INTO OceanMessagingDB.dbo.OceanMessaging
SELECT 
	[CustomerCode],
	[Customer],
	[XML Message],
	[Carriers],
	[AM_ReceivedFromSenderUTC]
FROM
OPENQUERY([EHUB_ARCHIVE_OLD],'DECLARE @dt0 DATETIME = ''2019-05-01 00:00''--End Date
DECLARE @dt1 DATETIME = ''2019-01-04 00:00:0''---Start Date-DATEADD(MONTH, -12, @dt0);
SELECT LEFT(cc_id_senderinbox, 3)
       + RIGHT(cc_id_senderinbox, 3)  AS CustomerCode,
       cc_friendlyname_senderinbox    AS Customer,
	   CAST([AM_SenderMessageXml] as nvarchar(max))  AS [XML Message],
	   [cc_id_recipientoutbox] AS [Carriers],
	   AM_ReceivedFromSenderUTC
FROM   ehubarchiveonline.dbo.ehubarchivemessageexp WITH (nolock)
WHERE  am_batchenvelopetrackingid IS NULL
       AND am_applicationcode = ''UDM''
       AND cc_id_recipientinbox = ''SHIPPING_INSTRUCTION''
       AND am_status = ''3''
       AND am_receivedfromsenderutc >= @dt1
       AND am_receivedfromsenderutc < @dt0 ') a

--END
--GO


