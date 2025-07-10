USE [WiseGridReporting]
GO

/****** Object:  Table [dbo].[Rpt_BI_MainDB]    Script Date: 1/04/2019 3:46:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Rpt_BI_MainDB](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecordedTime] [datetime] NULL,
	[IncrementId] [bigint] NOT NULL,
	[FQDN] [varchar](255) NULL,
	[InstanceName] [varchar](255) NULL,
	[ProductBuild] [sql_variant] NULL,
	[MainDbName] [varchar](128) NULL,
	[IsCdcEnabled] [bit] NULL,
	[DataSizeReservedGb] [numeric](12, 2) NULL,
	[LogSizeReservedGb] [numeric](12, 2) NULL,
	[DataSizeUsedGb] [numeric](12, 2) NULL,
	[LogSizeUsedGb] [numeric](12, 2) NULL,
	[AuditServerName] [varchar](128) NULL,
	[DwServerName] [varchar](128) NULL,
	[CdcEnabledUtcDate] [datetime] NULL,
	[EarliestCapturedLsnUtcDateTime] [datetime] NULL,
	[LatestCapturedLsnUtcDateTime] [datetime] NULL,
	[DbVersion] [nvarchar](257) NULL,
	[AppVersion] [nvarchar](128) NULL,
	[PackageBuiltDate] [datetime] NULL,
	[AuditRetentionPeriod] [int] NULL,
	[IPAddress] [nvarchar](48) NULL,
	[LtmMaxLsn] [varchar](22) NULL,
	[EdwEnabledDataSizeUsedGb] [numeric](12, 2) NULL,
	[SqlServerVersion] VARCHAR(128) NULL,
	[SqlServerProductVersion] VARCHAR(128) NULL,
	[ServiceTaskData] NVARCHAR(MAX) NULL,
	[BiResetChangeDataCapture] VARCHAR(128) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

CREATE NONCLUSTERED INDEX IX_Rpt_BI_MainDB_IncrementId ON dbo.Rpt_BI_MainDB (IncrementId) INCLUDE(DbVersion, DataSizeUsedGb, EdwEnabledDataSizeUsedGb)
CREATE NONCLUSTERED INDEX IX_Rpt_BI_MainDB_MainDbName ON dbo.Rpt_BI_MainDB (MainDbName)

GO

