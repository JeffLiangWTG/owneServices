USE [AirMessagingDB]
GO

/****** Object:  Table [dbo].[AirMessaging]    Script Date: 12/02/2019 11:52:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AirMessaging](
	[Sender ID] [nvarchar](20) NULL,
	[Recipient ID] [nvarchar](50) NULL,
	[Sender Name] [nvarchar](255) NULL,
	[Recipient Name] [nvarchar](255) NULL,
	[AM_Status] [tinyint] NULL,
	[AM_ApplicationCode] [nvarchar](3) NULL,
	[Raw Message] [nvarchar](max) NULL,
	[AM_RecipientMessageXML] [xml] NULL,
	[AM_ReceivedFromSenderUTC] [datetime] NULL,
	[All eHub TrackingID] [nvarchar](100) NULL,
	[Sender AirProviderflag] [bit] NULL,
	[Recipient EnterpriseClient] [nvarchar](50) NULL,
	[AM_ErrorMessage] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


