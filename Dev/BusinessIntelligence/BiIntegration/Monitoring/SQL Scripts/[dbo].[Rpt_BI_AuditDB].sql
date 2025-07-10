USE [WiseGridReporting]
GO

/****** Object:  Table [dbo].[Rpt_BI_AuditDB]    Script Date: 29/03/2019 12:18:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Rpt_BI_AuditDB](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RecordedTime] [datetime] NULL,
	[IncrementId] [bigint] NOT NULL,
	[FQDN] [varchar](255) NULL,
	[InstanceName] [varchar](255) NULL,
	[ProductBuild] [sql_variant] NULL,
	[AuditDbName] [varchar](128) NULL,
	[IPAddress] [nvarchar](48) NULL,
	[DataSizeReservedGb] [numeric](12, 2) NULL,
	[LogSizeReservedGb] [numeric](12, 2) NULL,
	[DataSizeUsedGb] [numeric](12, 2) NULL,
	[LogSizeUsedGb] [numeric](12, 2) NULL,
	[IsEmpty] [bit] NULL,
	[MainDbSchemaVersion] [varchar](128) NULL,
	[IsIndexReorganizeDurationMissing] [bit] NULL,
	[MinTranEndTimeUtc] [datetime] NULL,
	[MaxTranEndTimeUtc] [datetime] NULL,
	[LastMaxLsnProcessed] [varchar](22) NULL,
	[LtmMaxLsn] [varchar](22) NULL,
	[LastPartitioningDateUtc] [datetime] NULL,
	[LastIndexRebuildDateUtc] [datetime] NULL,
	[DataLossDetected] [bit] NULL,
	[PartitionCount] [int] NULL,
	[MinPartitionRangeValue] [int] NULL,
	[MaxPartitionRangeValue] [int] NULL,
	[NonPartitionedTableCount] [int] NULL,
	[IndexReorganizeDuration] [numeric](9, 3) NULL,
	[LastLoadDuration] [numeric](9, 3) NULL,
	[LastLoadRowCount] [int] NULL,
	[SqlServerVersion] VARCHAR(128) NULL,
	[SqlServerProductVersion] VARCHAR(128) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

CREATE NONCLUSTERED INDEX IX_Rpt_BI_AuditDB_IncrementId ON dbo.Rpt_BI_AuditDB (IncrementId)
CREATE NONCLUSTERED INDEX IX_Rpt_BI_AuditDB_AuditDbName ON dbo.Rpt_BI_AuditDB (AuditDbName)

GO
