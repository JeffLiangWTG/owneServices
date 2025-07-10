USE [AirMessagingDB]
GO

/****** Object:  StoredProcedure [dbo].[ServiceProviderResponsesReport]    Script Date: 13/02/2019 3:55:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ServiceProviderResponsesReport]
AS
BEGIN
DECLARE @dt0 DATE = dateadd(dd, - DATEDIFF(DD, 01, datepart(dd,getdate())) ,DateAdd(mm, -1 , GETDATE()));
DECLARE @dt1 DATE = DATEADD(DAY, + 31, @dt0);

SELECT [DateUTC], [TimeUTC] , [Service Provider], 
										--AWB Prefix--
COALESCE(
								---- Descartes AWB Prefix------------
CASE WHEN [Service Provider] = 'Descartes'
THEN (
Case 
when SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FSU'
Then SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])-3, 3)
									--FMA prefix--
When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/9',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/9',[Raw Message])+7, 3)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)
									--FNA Prefix--
When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)
Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])-3, 3 )
END
)
ELSE NULL END,

								---- CCN AWB Prefix----------

CASE
WHEN [Service Provider] = 'CCN'
THEN (Case 
when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FSU'
Then SUBSTRING([Raw Message], CHARINDEX('&#xD;',[Raw Message])+6, 3)

								----FMA prefix-- FOR FWB--
when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message])+12, 3)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+11, 3)

								----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

									----FNA Prefix--

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message])+12, 3)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+11, 3)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

Else 'xxx'
END)
ELSE NULL END,

								---- Champ Traxon Prefix ----------

CASE
WHEN [Service Provider] = 'Traxon'
THEN (
								----FMA prefix-- FOR FWB--
Case 
when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

								----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

									----FNA Prefix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

									----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])-3, 3)
END
)
Else NULL
END,

									----- GLSHK Prefix -----
CASE
WHEN [Service Provider] = 'GLSHK'
THEN (
									----FMA prefix-- FOR FWB--
Case 
when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

									----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

									----FNA Prefix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

									----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])-3, 3)
END
)
Else NULL
END,

										---- BT Prefix ------
CASE
WHEN [Service Provider] = 'BT'
THEN 
									----FMA prefix-- FOR FWB--
(
Case 
when SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

									----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)

									----FNA Prefix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+8, 3)

									----FMA prefix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+4, 3)


Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])-3, 3)
END
)
Else NULL
END

) AS [AWB Prefix],
										-- AWB Suffix--

COALESCE(

									---- Descartes AWB Suffix-----------

CASE WHEN [Service Provider] = 'Descartes'
THEN (
Case when SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FSU'
Then SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])+1, 8)
											--FMA Suffix--
When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/9',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/9',[Raw Message])+11, 8)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)
											--FNA Suffix--
When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

When SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 3)
Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])+1, 8 )
END
)
ELSE NULL END,

										---- CCN AWB Suffix----------

CASE
WHEN [Service Provider] = 'CCN'
THEN (Case 
when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FSU'
Then SUBSTRING([Raw Message], CHARINDEX('&#xD;',[Raw Message])+10, 8)

										----FMA Suffix-- FOR FWB--
when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message])+16, 8)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA' 
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+15, 8)

										----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

											----FNA Suffix--

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/16',[Raw Message])+16, 8)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+15, 8)

when SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

Else 'xxx'
END)
ELSE NULL END,

											-- Champ Traxon Suffix -------

CASE
WHEN [Service Provider] = 'Traxon'
THEN (
											----FMA Suffix-- FOR FWB--
Case 
when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

											----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

												----FNA Suffix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

												----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])+1, 8)
END
)
Else NULL
END,

												----- GLSHK Suffix -----
CASE
WHEN [Service Provider] = 'GLSHK'
THEN (
												----FMA Suffix-- FOR FWB--
Case 
when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

												----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

												----FNA Suffix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

												----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])+1, 8)
END
)
Else NULL
END,

													---- BT Suffix ------
CASE
WHEN [Service Provider] = 'BT'
THEN 
												----FMA Suffix-- FOR FWB--
(
Case 
when SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

												----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)

												----FNA Suffix--FOR FWB--

when SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message])+12, 8)

												----FMA Suffix-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('MBI/',[Raw Message])+8, 8)


Else SUBSTRING([Raw Message], CHARINDEX('-',[Raw Message])+1, 8)
END
)
Else NULL
END

) AS  [AWB Suffix],

												------- Message Type ----------

CASE
WHEN [Service Provider] = 'Descartes'
and 
SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
THEN SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3)

WHEN [Service Provider] = 'Descartes'
and 
SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
THEN SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3)

WHEN [Service Provider] = 'Descartes'
THEN SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 )

WHEN [Service Provider] = 'CCN'
Then  SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 )

WHEN [Service Provider] = 'BT'
and 
SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3) = 'FMA'
THEN SUBSTRING([Raw Message], CHARINDEX('FMA',[Raw Message]), 3)

WHEN [Service Provider] = 'BT'
and 
SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
THEN SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3)

WHEN [Service Provider] = 'BT'
and 
SUBSTRING([Raw Message], CHARINDEX('FSU',[Raw Message]), 3) = 'FSU'
THEN SUBSTRING([Raw Message], CHARINDEX('FSU',[Raw Message]), 3)

ELSE SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3)
END as [Type],

													--- Trashed --

Case when AM_Status = 3
Then 'NO'
When AM_Status = 255
Then 'YES'
Else 'Unknown'
End as [Trashed],

											---  eHub Error Reason --

Case When AM_ErrorMessage = ''
Then 'No Reason'
Else AM_ErrorMessage
End AS [eHUB Error Reason],

										------------- FNA Reason -------------
													--Descartes--
													--For FWB--
CASE WHEN [Service Provider] = 'Descartes'
AND
SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA'
AND
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then
SUBSTRING([Raw Message], 
CHARINDEX('ACK',[Raw Message]), 
CASE WHEN CHARINDEX('FWB/',[Raw Message])-
CHARINDEX('ACK',[Raw Message]) <=0
THEN 30
ELSE CHARINDEX('FWB/',[Raw Message])-
CHARINDEX('ACK',[Raw Message])
END)

												----FOR FHL-- 
												
WHEN [Service Provider] = 'Descartes'
AND 
SUBSTRING([Raw Message], CHARINDEX('CDATA',[Raw Message])+6, 3 ) = 'FNA'
AND
SUBSTRING([Raw Message], CHARINDEX('FHL',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], 
CHARINDEX('ACK',[Raw Message]), 
CASE WHEN CHARINDEX('FHL/',[Raw Message])-
CHARINDEX('ACK',[Raw Message]) <=0
THEN 30
ELSE CHARINDEX('FHL/',[Raw Message])-
CHARINDEX('ACK',[Raw Message])
END) 

													--CCN--
												   --For FWB--
WHEN [Service Provider] = 'CCN'
AND
SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
THEN SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FWB/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												----FOR FHL--

WHEN [Service Provider] = 'CCN'
AND
SUBSTRING([Raw Message], CHARINDEX('<Body>',[Raw Message])+6, 3 ) = 'FNA'
AND
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
THEN SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FHL/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												-- Traxon --
												--For FWB--
WHEN [Service Provider] = 'Traxon'
AND
SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FWB/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												----FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FHL/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )


												-- GLSHK -- 
												--For FWB--
WHEN [Service Provider] = 'GLSHK'
AND
SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FWB/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												----FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('+CIM',[Raw Message])+4, 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FHL/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												  -- BT --
												--For FWB--
WHEN [Service Provider] = 'BT'
AND
SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FWB/1',[Raw Message]), 3) = 'FWB'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FWB/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

												-- FOR FHL--

when SUBSTRING([Raw Message], CHARINDEX('FNA',[Raw Message]), 3) = 'FNA'
AND 
SUBSTRING([Raw Message], CHARINDEX('FHL/',[Raw Message]), 3) = 'FHL'
Then SUBSTRING([Raw Message], CHARINDEX('ACK',[Raw Message]), CHARINDEX('FHL/',[Raw Message])-CHARINDEX('ACK',[Raw Message]) )

Else 'No Reason'
End as [FNA Reason],

[Raw Message] 
From(

SELECT [Sender ID] AS [Service Provider]      
      ,[Raw Message]
	  ,AM_Status
	  ,Convert(date, AM_ReceivedFromSenderUTC) AS [DateUTC]
	  ,Convert(varchar(8), AM_ReceivedFromSenderUTC, 108) AS [TimeUTC]
      ,[AM_ErrorMessage]
  FROM [AirMessagingDB].[dbo].[AirMessaging]
  Where AM_ApplicationCode = 'BIZ' and [Sender ID] ! = 'CCSJ'
and AM_ReceivedFromSenderUTC >=@dt0 
and AM_ReceivedFromSenderUTC < @dt1)MyTable
END
GO


