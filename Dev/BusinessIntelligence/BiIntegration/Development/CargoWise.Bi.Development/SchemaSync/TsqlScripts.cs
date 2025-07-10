using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Bi.Common;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.Bi.Development.SchemaSync
{
	internal enum TargetSystem { Audit, EdwStaging, EdwTransform, EdwModel }

	public static class TsqlScripts
	{
		#region SuppressResourceStringsCheckRegion

		#region Schema Files

		public static void GenerateSchemaFiles()
		{
			BiLogger.StartTask("Generating T-SQL Scripts");
			string auditSchemaDefinitions = string.Format(CultureInfo.InvariantCulture,
@"EXEC ('CREATE SCHEMA [{0}]');

", BiConstants.BiAdminSchemaName);
			string auditCdcStateTableDefinition = string.Empty;
			string auditTableDefinition = string.Empty;
			string auditStartLsnIndexDefinition = string.Empty;
			string edwSchemaDefinition = string.Format(CultureInfo.InvariantCulture,
@"EXEC ('CREATE SCHEMA [Staging]');

EXEC ('CREATE SCHEMA [Transform]');

EXEC ('CREATE SCHEMA [{0}]');

EXEC ('CREATE SCHEMA [{1}]');

EXEC ('CREATE SCHEMA [Dax]');

", BiConstants.BiAdminSchemaName, BiConstants.BiTempSchemaName);
			string edwCdcStateTableDefinition = string.Empty;
			string stagingTableDefinitions = string.Empty;
			string edwModelTableDefinition = string.Empty;

			try
			{
				InitializeAuditSchemaDefinition(ref auditSchemaDefinitions, ref auditCdcStateTableDefinition);
				InitializeEdwSchemaDefinition(ref edwSchemaDefinition, ref edwCdcStateTableDefinition);

				foreach (var cdcTableConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetOrderedCdcTableConfigRows())
				{
					string pkName = GetStagingPrimaryKeyName(cdcTableConfig);

					if (cdcTableConfig.TableInAudit)
					{
						auditTableDefinition += BiScriptHelper.GenerateAuditTableDefinition(cdcTableConfig);
						auditStartLsnIndexDefinition += BiScriptHelper.GenerateAuditStartLsnIndexDefinition(cdcTableConfig.SourceSchema, cdcTableConfig.SourceTable, pkName);
					}

					if (cdcTableConfig.TableInEdw)
					{
						stagingTableDefinitions +=
							"CREATE TABLE [Staging].[" + cdcTableConfig.SourceTable + "]\r\n(\r\n"
							+ GetStagingColumnDefinitionClause(cdcTableConfig) + "\r\n"
							+ " );\r\n\r\n"
							+ "CREATE CLUSTERED INDEX [CX_Staging_" + cdcTableConfig.SourceTable + "_" + pkName + "] ON Staging.[" + cdcTableConfig.SourceTable + "] ([" + pkName + "]);"
							+ "\r\n\r\n";
					}
				}

				foreach (var edwTableConfig in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.TransformId == 1))
				{
					edwModelTableDefinition +=
						"CREATE TABLE [" + edwTableConfig.Schema + "].[" + edwTableConfig.Name + "]\r\n(\r\n\t[__$transform_id] int DEFAULT 1 NOT NULL,\r\n\t"
						+ GetEdwModelColumnDefinitionClause(edwTableConfig)
						+ "\r\n) ON [PRIMARY];\r\n\r\n"
						+ "CREATE CLUSTERED COLUMNSTORE INDEX [cci_" + edwTableConfig.Schema + "_" + edwTableConfig.Name + "] ON [" + edwTableConfig.Schema + "].[" + edwTableConfig.Name + "] WITH (DROP_EXISTING = OFF);\r\n\r\n"
						+ BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCustomIndexScriptQueryForEdwTable(edwTableConfig)
						+ BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetEditableCustomIndexScriptQueryForEdwTable(edwTableConfig);
				}
				
				foreach (var edwDenormTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig)
				{
					edwModelTableDefinition +=
						"CREATE TABLE [" + edwDenormTable.Schema + "].[" + edwDenormTable.Name + "]\r\n(\r\n\t"
						+ GetDenormalizedTableColumnDefinitionClause(edwDenormTable)
						+ "\r\n) ON [PRIMARY];\r\n\r\n"
						+ "CREATE CLUSTERED COLUMNSTORE INDEX [cci_" + edwDenormTable.Schema + "_" + edwDenormTable.Name + "] ON [" + edwDenormTable.Schema + "].[" + edwDenormTable.Name + "] WITH (DROP_EXISTING = OFF);\r\n\r\n"
						+ BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCustomIndexScriptQueryForDenormalizedTable(edwDenormTable)
						+ BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetEditableCustomIndexScriptQueryForDenormalizedTable(edwDenormTable);
				}

				foreach (var edwCustomTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig)
				{
					edwModelTableDefinition +=
						"CREATE TABLE [" + edwCustomTable.Schema + "].[" + edwCustomTable.Name + "]\r\n(\r\n\t"
						+ GetCustomTableColumnDefinitionClause(edwCustomTable)
						+ "\r\n) ON [PRIMARY];\r\n\r\n"
						+ "CREATE CLUSTERED COLUMNSTORE INDEX [cci_" + edwCustomTable.Schema + "_" + edwCustomTable.Name + "] ON [" + edwCustomTable.Schema + "].[" + edwCustomTable.Name + "] WITH (DROP_EXISTING = OFF);\r\n\r\n"
						+ BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetEditableCustomIndexScriptQueryForCustomTable(edwCustomTable);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail("T-SQL Scripts generation failed");
				throw new BiDatabaseSyncException(ex.Message);
			}

			WriteToAuditSchema(auditSchemaDefinitions, auditCdcStateTableDefinition, auditTableDefinition, auditStartLsnIndexDefinition);
			WriteToEdwSchema(edwSchemaDefinition, edwCdcStateTableDefinition, stagingTableDefinitions, edwModelTableDefinition);
		}
		
		static void InitializeAuditSchemaDefinition(ref string auditSchemaDefinitions, ref string auditCdcStateTableDefinition)
		{
			var auditSchemaList = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.AsEnumerable()
													.Where(cdcTableConfig => cdcTableConfig.TableInAudit && cdcTableConfig.SourceSchema != Db.SqlDbOwnerSchema)
													.Select(cdcTableConfig => cdcTableConfig.SourceSchema);
			var auditDistinctSchemaList = auditSchemaList.Distinct();

			foreach (string auditSchema in auditDistinctSchemaList)
			{
				auditSchemaDefinitions += "EXEC ('CREATE SCHEMA [" + auditSchema + "]');\r\n\r\n";
			}

			auditCdcStateTableDefinition += string.Format(CultureInfo.InvariantCulture,
@"CREATE TABLE [{0}].[TableConfiguration]
(
	[TableID] [int] IDENTITY(1,1) NOT NULL,
	[SourceSchemaName] [varchar](128) NULL,
	[SourceTableName] [varchar](128) NULL,
	[TableColumnList] [nvarchar](max) NULL,
	[TableColumnConvertList] [nvarchar](max) NULL,
	[AuditFilter] [varchar](700) NULL,
	[PkName] [varchar](128) NULL
)
ALTER TABLE  [{0}].[TableConfiguration]
	ADD CONSTRAINT [TableID_TableConfiguration] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE TABLE [{0}].[TableState]
(
	[SourceSchemaName] [varchar](128) NOT NULL,
	[SourceTableName] [varchar](128) NOT NULL,
	[CurrentState] [varchar](50) NULL,
	[StateModifiedTimestamp] [datetime] NULL CONSTRAINT [DF_TableState_state_modified_timestamp]  DEFAULT (getdate()),
	[LoadRecordCount] [bigint] NULL,
	[LoadDurationMS] [bigint] NULL,
	[SqlErrorMessage] [varchar](max) NULL,
	[SqlErrorDatetimeUTC] [datetime] NULL,
	[IsIndexReorganized] BIT NULL,
	[IndexReorganizeDurationMs] BIGINT NULL,
	[IndexReorganizationSqlErrorMessage] VARCHAR(MAX) NULL,
	[IndexReorganizationSqlErrorNumber] INT NULL,
	[PartitioningStatus] [varchar](150) NULL,
	[PartitioningStatusModifiedTimestamp] [datetime] NULL,
    [AetMinHistorySummaryLsn] [binary](10) NULL,
    [AdmHistorySummaryStatus] [bit] NULL,
	[AetHWMHistorySummaryLsn] [binary](10) NULL
)
ALTER TABLE  [{0}].[TableState]
	ADD CONSTRAINT [SourceSchemaName_SourceTableName_TableState] PRIMARY KEY CLUSTERED ([SourceSchemaName], [SourceTableName]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[MasterState]
(
	[ParamID] [int] IDENTITY(1,1) NOT NULL,
	[ParamName] [nvarchar](128) NULL,
	[ParamValue] [nvarchar](max) NULL,
)
ALTER TABLE  [{0}].[MasterState]
	ADD CONSTRAINT [ParamID_MasterState] PRIMARY KEY CLUSTERED ([ParamID]) WITH (ALLOW_PAGE_LOCKS = OFF);
ALTER TABLE [{0}].[MasterState]
	ADD CONSTRAINT [UX_ParamName] UNIQUE (ParamName);

CREATE TABLE [{0}].[LsnTimeMapping]
(
	StartLsn BINARY(10) NOT NULL,
	TranEndTimeUtc DATETIME NOT NULL
);

ALTER TABLE [{0}].[LsnTimeMapping]
	ADD CONSTRAINT [LsnTimeMapping_StartLsn_PK] PRIMARY KEY CLUSTERED (StartLsn) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [LsnTimeMapping_TranEndTimeUtc_IX] ON [{0}].[LsnTimeMapping] (TranEndTimeUtc ASC);

CREATE TABLE [{0}].[DataLossLog] 
(
	SourceSchemaName VARCHAR(128) NOT NULL,
	SourceTableName VARCHAR(128) NOT NULL,
	StartLsn BINARY(10) NOT NULL,
	EndLsn BINARY(10) NOT NULL,
	MinTableLsn BINARY(10) NOT NULL,
	LossType VARCHAR(10) NOT NULL,
	CreateDateTimeUTC DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE CLUSTERED INDEX [CX_{0}_DataLossLog_StartLsn] on [{0}].[DataLossLog] (StartLsn);

CREATE TABLE [{0}].[CdcHistorySummary]
(
	[CdcHistorySummaryID] [bigint] IDENTITY(1,1) NOT NULL,
	[Lsn] [binary](10) NOT NULL,
	[SchemaName] [varchar](128) NOT NULL,
	[ChangedTableName] [varchar](128) NOT NULL,
	[NumberOfRows] [int] NOT NULL,
	[NumberOfRowsDelete] [int] NOT NULL DEFAULT 0,
	[NumberOfRowsInsert] [int] NOT NULL DEFAULT 0,
	[NumberOfRowsUpdate] [int] NOT NULL DEFAULT 0,
	[LsnPeriod] [smallint] NOT NULL,
	[TranEndTimeUTC] [datetime] NOT NULL DEFAULT GETUTCDATE()
);

CREATE CLUSTERED COLUMNSTORE INDEX [cci_{0}_CdcHistorySummary] on [{0}].[CdcHistorySummary];
CREATE NONCLUSTERED INDEX [IX_CdcHistorySummary_Lsn] ON [{0}].[CdcHistorySummary] (Lsn) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX [IX_CdcHistorySummary_SchemaName_ChangedTableName_Lsn] ON [{0}].[CdcHistorySummary] (SchemaName,ChangedTableName,Lsn) INCLUDE (TranEndTimeUTC) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [{0}].[CdcHistorySummaryStaging]
(
	TableID int,
	Lsn binary(10),
	LsnPeriod smallint,
	Operation int
)

CREATE TABLE [{0}].SchemaVersionHistory(
	SchemaVersionId int IDENTITY(1,1) NOT NULL,
	SchemaVersion varchar(128) NOT NULL,
	MaxLsn binary(10)
);

CREATE TABLE [{0}].SchemaMappingHistory(
	SchemaVersionId int NOT NULL,
	SchemaName sysname NOT NULL,
	TableName sysname NOT NULL,
	ColumnName sysname NOT NULL,
	MappedName sysname NULL,
	ColumnOrdinal int NOT NULL,
	DataType sysname NOT NULL,
	MaxLength int NOT NULL,
	Precision int NOT NULL,
	Scale int NOT NULL
);

", BiConstants.BiAdminSchemaName);
		}

		static void InitializeEdwSchemaDefinition(ref string edwSchemaDefinitions, ref string edwCdcStateTableDefinition)
		{
			var edwDistinctSchemaList = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Select(t => t.Schema).Where(s => s != BiConstants.BiAdminSchemaName).Distinct();

			foreach (string edwSchema in edwDistinctSchemaList)
			{
				edwSchemaDefinitions += "EXEC ('CREATE SCHEMA [" + edwSchema + "]');\r\n\r\n";
			}

			edwCdcStateTableDefinition += string.Format(CultureInfo.InvariantCulture,
@"CREATE TABLE [{0}].[StagingTableConfiguration]
(
	TableID INT IDENTITY(1,1),
	SourceSchemaName VARCHAR(128),
	StagingSchemaName VARCHAR(128),
	StagingTableName VARCHAR(128),
	PkName NVARCHAR(1000),
	IndexedColumnName NVARCHAR(1000),
	StagingTableDefinition NVARCHAR(MAX),
	StagingTableColumnListInsert NVARCHAR(MAX),
	StagingTableColumnListSelect NVARCHAR(MAX),
	ColumnEnumeratedList NVARCHAR(MAX),
	ColumnEnumeratedListWithDefinition NVARCHAR(MAX),
	EdwFilter VARCHAR(MAX) NULL
)
ALTER TABLE  [{0}].[StagingTableConfiguration]
	ADD CONSTRAINT [TableID_StagingTableConfiguration] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[TransformTableConfiguration]
(
	TableID INT IDENTITY(1,1),
	SourceTableName VARCHAR(700),
	ModelSchemaName VARCHAR(128),
	ModelTableName VARCHAR(128),
	DependencyOrder INT,
	TransformId INT DEFAULT 1,
	IsLastTransform BIT,
	HasChildTables BIT,
	IsSelfReferenced BIT,
	InitialLoadQuery NVARCHAR(MAX),
	IncrementalInsertQuery NVARCHAR(MAX),
	IncrementalDeleteQuery NVARCHAR(MAX),
	WhereCondition NVARCHAR(MAX),
	OltpPartitionColumnExpression VARCHAR(128),
	EdwPartitionColumnName VARCHAR(128),
	CustomIndexScript NVARCHAR(MAX)
)
ALTER TABLE  [{0}].[TransformTableConfiguration]
	ADD CONSTRAINT [TableID_TransformTableConfiguration] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE  [{0}].[TransformTableConfiguration]
	ADD CONSTRAINT [UC_TransformTableConfiguration_DependencyOrder_TransformId] UNIQUE ([DependencyOrder], [TransformId]);

CREATE TABLE [{0}].[CustomTableConfiguration]
(
	TableID INT IDENTITY(1,1),
	ModelSchemaName VARCHAR(128),
	ModelTableName VARCHAR(128),
	RunBeforeTransform BIT,
	DependencyOrder INT,
	TableDependencyList NVARCHAR(MAX),
	CustomTableDependencyList NVARCHAR(MAX),
	ViewName VARCHAR(128),
	InitialLoadQuery NVARCHAR(MAX),
	IncrementalLoadQuery NVARCHAR(MAX)
)
ALTER TABLE  [{0}].[CustomTableConfiguration]
	ADD CONSTRAINT [TableID_CustomTableConfiguration] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[StagingTableState]
(
	TableID INT IDENTITY(1,1),
	SourceTableName VARCHAR(700) NOT NULL,
	CurrentMaxLsn BINARY(10),
	CurrentState VARCHAR(100),
	InitialLoadRequired BIT DEFAULT 1,
	StateModifiedTimestamp DATETIME DEFAULT GETDATE(),
	InitialLoadRecordCount BIGINT,
	InitialLoadDurationMs BIGINT,
	IncrementalLoadRecordCount BIGINT,
	IncrementalLoadDurationMs BIGINT,
	SqlErrorMessage VARCHAR(MAX),
	SqlErrorDatetimeUTC DATETIME NULL,
	NextSortValueToLoad SQL_VARIANT NULL,
	InitialLoadRowCountOLTP BIGINT NULL,
	NumberOfCompletedBatches INT NULL,
	EnableEtl BIT NOT NULL DEFAULT 1,
	ExcessiveMemoryGrantWarningThreshold NUMERIC(9,5) NOT NULL DEFAULT 0.04000	-- 4%
)
ALTER TABLE  [{0}].[StagingTableState]
	ADD CONSTRAINT [TableID_StagingTableState] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[TransformTableState]
(
	TableID INT IDENTITY(1,1),
	ModelSchemaName VARCHAR(128),
	ModelTableName VARCHAR(128) NOT NULL,
	TransformId INT DEFAULT 1 NOT NULL,
	InitialLoadRequired BIT DEFAULT 1,
	CurrentState VARCHAR(100),
	StateModifiedTimestamp DATETIME DEFAULT GETDATE(),
	InitialTransformRecordCount BIGINT,
	InitialTransformDurationMs BIGINT,
	InitialTransformTimestamp DATETIME,
	MergeTransformInsertRecordCount BIGINT,
	MergeTransformInsertDurationMs BIGINT,
	MergeTransformDeleteRecordCount BIGINT,
	MergeTransformDeleteDurationMs BIGINT,
	MergeTransformTimestamp DATETIME,
	IndexReorganizeDurationMs BIGINT,
	SqlErrorMessage VARCHAR(MAX),
	IsIndexReorganized BIT NULL,
	IndexReorganizationSqlErrorMessage VARCHAR(MAX) NULL,
	IndexReorganizationSqlErrorNumber INT NULL,
	SqlErrorDatetimeUTC DATETIME NULL
)
ALTER TABLE  [{0}].[TransformTableState]
	ADD CONSTRAINT [TableID_TransformTableState] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[CustomTableState]
(
	TableID INT IDENTITY(1,1),
	ModelSchemaName VARCHAR(128),
	ModelTableName VARCHAR(128) NOT NULL,
	InitialLoadRequired BIT DEFAULT 1,
	CurrentState VARCHAR(100),
	StateModifiedTimestamp DATETIME DEFAULT GETDATE(),
	InitialTransformRecordCount BIGINT,
	InitialTransformDurationMs BIGINT,
	InitialTransformTimestamp DATETIME,
	MergeTransformInsertRecordCount BIGINT,
	MergeTransformInsertDurationMs BIGINT,
	MergeTransformDeleteRecordCount BIGINT,
	MergeTransformDeleteDurationMs BIGINT,
	MergeTransformTimestamp DATETIME,
	IndexReorganizeDurationMs BIGINT,
	SqlErrorMessage VARCHAR(MAX),
	IsIndexReorganized BIT NULL,
	IndexReorganizationSqlErrorMessage VARCHAR(MAX) NULL,
	IndexReorganizationSqlErrorNumber INT NULL,
	SqlErrorDatetimeUTC DATETIME NULL
)
ALTER TABLE  [{0}].[CustomTableState]
	ADD CONSTRAINT [TableID_CustomTableState] PRIMARY KEY CLUSTERED ([TableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[MasterState]
(
	ParamID INT IDENTITY(1,1),
	ParamName VARCHAR(128),
	ParamValue VARCHAR(max)
)
ALTER TABLE  [{0}].[MasterState]
	ADD CONSTRAINT [ParamID_MasterState] PRIMARY KEY CLUSTERED ([ParamID]) WITH (ALLOW_PAGE_LOCKS = OFF);
ALTER TABLE [{0}].[MasterState]
	ADD CONSTRAINT [UX_ParamName] UNIQUE (ParamName);

CREATE TABLE [{0}].[SsasCube]
(
	SsasCubeID INT IDENTITY(1,1),
	SsasModelFileName VARCHAR(128),
	SsasModelLogicalName VARCHAR(128),
	SsasModelVersion VARCHAR(128),
	LastSourceLsnDateTimeUTC DATETIME,
	LastProcessingStartDateTimeUTC DATETIME,
	LastProcessingFinishDateTimeUTC DATETIME,
	LastResetModelInfoUTC DATETIME,
	IsCubeProcessing smallint default 0, --cube not processed
	DeployToServer BIT NOT NULL DEFAULT 1,
	EnableEtl BIT NOT NULL DEFAULT 1,
	RedeployOnNextBID BIT NOT NULL DEFAULT 0
)
ALTER TABLE [{0}].[SsasCube]
	ADD CONSTRAINT [SsasCubeID_SsasCube] PRIMARY KEY CLUSTERED ([SsasCubeID]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE TABLE [{0}].[SsasTable]
(
	SsasTableID INT IDENTITY(1,1),
	SsasCubeID INT,
	TableName VARCHAR(128),
	PartitionQuery NVARCHAR(MAX),
	PartitionKey NVARCHAR(128)
)
ALTER TABLE [{0}].[SsasTable]
	ADD CONSTRAINT [SsasTableID_SsasTable] PRIMARY KEY CLUSTERED ([SsasTableID]) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [{0}].[SsasTable]
	ADD FOREIGN KEY (SsasCubeID) REFERENCES [{0}].SsasCube(SsasCubeID)

CREATE TABLE [{0}].[SsasPartition]
(
	SsasPartitionID INT IDENTITY(1,1),
	SsasTableID INT,
	PartitionName VARCHAR(128),
	FromValue date,
	ToValue date,
	ProcessingRequired BIT DEFAULT 1
)
ALTER TABLE [{0}].[SsasPartition]
	ADD CONSTRAINT [SsasPartitionID_SsasPartition] PRIMARY KEY CLUSTERED ([SsasPartitionID]) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [{0}].[SsasPartition]
	ADD FOREIGN KEY (SsasTableID) REFERENCES [{0}].SsasTable(SsasTableID)

CREATE TABLE [{0}].[EdwModelTableToSsasTableMapping]
(
	EdwModelSchema VARCHAR(128),
	EdwModelTable VARCHAR(128),
	SsasTableID INT
)

CREATE CLUSTERED COLUMNSTORE INDEX [cci_{0}_EdwModelTableToSsasTableMapping] ON [{0}].[EdwModelTableToSsasTableMapping];

CREATE TABLE [{0}].[ModelTableConfiguration]
(
	ModelSchemaName VARCHAR(200) NOT NULL,
	ModelTableName VARCHAR(200) NOT NULL,
	DependencyOrder INT,
	SourceTables VARCHAR(MAX) NULL,
	EdwPartitionColumnName VARCHAR(128) NULL
)

CREATE UNIQUE NONCLUSTERED INDEX [UC_ModelTableConfiguration_ModelSchemaName_ModelTableName] ON [{0}].[ModelTableConfiguration] (ModelSchemaName ASC, ModelTableName ASC)

CREATE TABLE [{0}].[ModelTableState]
(
	ModelSchemaName VARCHAR(200) NOT NULL,
	ModelTableName VARCHAR(200) NOT NULL,
	CurrentState VARCHAR(100) NULL,
	StateModifiedTimestamp DATETIME NULL,
	InitialLoadRecordCount BIGINT NULL,
	InitialLoadDurationMs BIGINT NULL,
	IncrementalDeleteRecordCount BIGINT NULL,
	IncrementalDeleteDurationMs BIGINT NULL,
	IncrementalInsertRecordCount BIGINT NULL,
	IncrementalInsertDurationMs BIGINT NULL,
	SqlErrorMessage VARCHAR(MAX) NULL,
	InitialLoadRequired BIT NULL,
	IsIndexReorganized BIT NULL,
	IndexReorganizationSqlErrorMessage VARCHAR(MAX) NULL,
	IndexReorganizationSqlErrorNumber INT NULL,
	IndexReorganizeDurationMs BIGINT NULL,
	SqlErrorDatetimeUTC DATETIME NULL
)

CREATE UNIQUE NONCLUSTERED INDEX [UC_ModelTableState_ModelSchemaName_ModelTableName] ON [{0}].[ModelTableState] (ModelSchemaName ASC, ModelTableName ASC)

CREATE TABLE [{0}].[TransformedRow]
(
	SchemaName VARCHAR(128),
	TableName VARCHAR(128),
	KeyValue BIGINT,
	PKValue UNIQUEIDENTIFIER NULL,
	TransformId INT NULL,
	CreateDate DATE NULL,
	RefValue1 SQL_VARIANT NULL,
	RefValue2 SQL_VARIANT NULL,
	RefValue3 SQL_VARIANT NULL,
	RefValue4 SQL_VARIANT NULL,
	RefValue5 SQL_VARIANT NULL
)

CREATE CLUSTERED INDEX [CX_{0}_TransformedRow] ON [{0}].[TransformedRow] (SchemaName, TableName, TransformId);

CREATE TABLE [{0}].[SsasPartitionUnprocessedDate]
(
	SchemaName VARCHAR(800),
	TableName VARCHAR(800),
	CreateDate DATE NULL
)

CREATE CLUSTERED COLUMNSTORE INDEX [cci_{0}_SsasPartitionUnprocessedDate] ON [{0}].[SsasPartitionUnprocessedDate];

CREATE TABLE [{0}].[ReportParameterConfiguration]
(
	CompanyID UNIQUEIDENTIFIER NULL,
	StaffID UNIQUEIDENTIFIER NULL,
	CheckPointCode VARCHAR(300) NOT NULL,
	ConfigurationName VARCHAR(300) NOT NULL,
	ConfigurationData VARBINARY(MAX) NOT NULL,
	LastEditTimeUtc SMALLDATETIME NOT NULL,
	LastEditUser UNIQUEIDENTIFIER NOT NULL
);

CREATE TABLE [{0}].[ReportConfiguration] (
	[ReportConfigurationID] UNIQUEIDENTIFIER NOT NULL,
	[CompanyCode] VARCHAR(3),
	[ReportName] VARCHAR(100) NOT NULL,
	[VisualName] VARCHAR(100) NOT NULL,
	[ConfigurationName] VARCHAR(100) NOT NULL,
	[ColumnConfiguration] VARCHAR(MAX) NOT NULL,
	[IsDefault] BIT NOT NULL,
	[LastModifiedDateUTC] DATETIME
);

ALTER TABLE [{0}].[ReportConfiguration]
	ADD CONSTRAINT [ReportConfigurationID_ReportConfiguration] PRIMARY KEY CLUSTERED ([ReportConfigurationID]) WITH (ALLOW_PAGE_LOCKS = OFF);

", BiConstants.BiAdminSchemaName);
		}

		static void WriteToAuditSchema(string auditSchemaDefinitions, string auditCdcStateTableDefinition, string auditTableDefinitions, string auditStartLsnIndexDefinition)
		{
			BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Saving Audit table schema to {0}", BiFiles.GeneratedAuditTableSchemaPath));
			BiFiles.SaveFile(BiFiles.GeneratedAuditTableSchemaPath,
				auditSchemaDefinitions
				+ auditCdcStateTableDefinition
				+ SubscriberControlTableDefinition
				+ SchemaMappingSummaryTableDefinition
				+ auditTableDefinitions
				+ auditStartLsnIndexDefinition);
		}

		static void WriteToEdwSchema(string edwSchemaDefinitions, string edwCdcStateTableDefinition, string stagingTableDefinitions, string edwModelTableDefinitions)
		{
			BiLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "Saving EDW table schema to {0}", BiFiles.GeneratedEDWTableSchemaPath));
			BiFiles.SaveFile(BiFiles.GeneratedEDWTableSchemaPath,
				edwSchemaDefinitions
				+ edwCdcStateTableDefinition
				+ stagingTableDefinitions
				+ edwModelTableDefinitions);
		}

		static string SubscriberControlTableDefinition
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture,
@"CREATE TABLE [{0}].[SubscriberControl](
	[SubscriberCode] char(3) NOT NULL,
	[Description] varchar(128) NULL,
	[PeriodHighWaterMark] smallint NULL,
	[LsnHighWaterMark] binary(10) NOT NULL DEFAULT 0x,
	[SeqValHighWaterMark] binary(10) NOT NULL DEFAULT 0xFFFFFFFFFFFFFFFFFFFF,
	[CommandIdHighWaterMark] int NOT NULL DEFAULT 2147483647,
	[OperationHighWaterMark] int NOT NULL DEFAULT 2147483647
);

ALTER TABLE  [{0}].[SubscriberControl]
	ADD CONSTRAINT [PK_SubscriberControl] PRIMARY KEY CLUSTERED ([SubscriberCode]) WITH (ALLOW_PAGE_LOCKS = OFF);

",
					BiConstants.BiAdminSchemaName);
			}
		}

		static string SchemaMappingSummaryTableDefinition
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture,
@"CREATE TABLE [{0}].[SchemaMappingSummary]
(
	[MaxLsn] binary(10) NOT NULL,
	[MaxLsnTimeUTC] datetime NOT NULL,
	[EffectiveSchemaVersion] varchar(128) NOT NULL,
	[TableName] varchar(128) NOT NULL,
	[RenamedColumn] varchar(128) NOT NULL,
	[MappedColumn] varchar(128) NOT NULL
);

ALTER TABLE  [{0}].[SchemaMappingSummary]
	ADD CONSTRAINT [PK_SchemaMappingSummary] PRIMARY KEY CLUSTERED ([TableName], [MappedColumn], MaxLsn) WITH (ALLOW_PAGE_LOCKS = OFF);
",
					BiConstants.BiAdminSchemaName);
			}
		}

		#region Column Structure

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		static string GetSurrogateKeyName(BiAutomationConfigDataSet.EdwTableConfigRow edwTableConfig)
		{
			return edwTableConfig != null ? edwTableConfig.Name + "Key" : null;
		}

		static string GetDataTypeDefinition(BiAutomationConfigDataSet.CdcColumnConfigRow cdcColumnConfig, TargetSystem targetSystem)
		{
			string dataTypeDefinition;

			if ((targetSystem == TargetSystem.EdwModel | targetSystem == TargetSystem.EdwTransform)
				&& (!string.IsNullOrEmpty(cdcColumnConfig.ReferenceTable)))
			{
				dataTypeDefinition = "int";
			}
			else
			{
				dataTypeDefinition = GetDataTypeDefinition(cdcColumnConfig.DataType, cdcColumnConfig.MaxLength, cdcColumnConfig.Precision, cdcColumnConfig.Scale);
			}

			return dataTypeDefinition;
		}

		static string GetDataTypeDefinition(BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow column)
		{
			return GetDataTypeDefinition(column.DataType, column.MaxLength, column.Precision, column.Scale);
		}

		static string GetDataTypeDefinition(BiAutomationConfigDataSet.EdwColumnConfigRow edwColumnConfig)
		{
			return GetDataTypeDefinition(edwColumnConfig.DataType, edwColumnConfig.MaxLength, edwColumnConfig.Precision, edwColumnConfig.Scale);
		}

		static string GetDataTypeDefinition(BiAutomationConfigDataSet.EdwCustomColumnConfigRow edwCustomColumnConfig)
		{
			return GetDataTypeDefinition(edwCustomColumnConfig.DataType, edwCustomColumnConfig.MaxLength, edwCustomColumnConfig.Precision, edwCustomColumnConfig.Scale);
		}

		static string GetDataTypeDefinition(string dataType, int maxLength, int precision, int scale)
		{
			var dataTypeDefinition = dataType;

			switch (dataType)
			{
				case "decimal":
					dataTypeDefinition += "(" + precision + "," + scale + ")";
					break;
				case "char":
				case "varchar":
				case "nchar":
				case "nvarchar":
				case "varbinary":
					dataTypeDefinition += "(" + (maxLength == -1 ? "max" : maxLength.ToString()) + ")";
					break;
				case "datetimeoffset":
					if (scale >= 0 && scale < 7)
					{
						dataTypeDefinition += $"({scale})";
					}
					break;
			}

			return dataTypeDefinition;
		}

		static string GetStagingColumnDefinitionClause(BiAutomationConfigDataSet.CdcTableConfigRow cdcTableConfig)
		{
			StringBuilder clause = new StringBuilder();

			clause.AppendLine("\t[__$start_lsn] binary(10)");
			clause.AppendLine("\t,OP_TYPE tinyint");

			var cdcColumnConfigRows = cdcTableConfig.GetOrderedCdcColumnConfigRows();
			foreach (BiAutomationConfigDataSet.CdcColumnConfigRow cdcColumnConfig in cdcColumnConfigRows)
			{
				if (cdcColumnConfig.ColumnInEdw || cdcColumnConfig.IsPrimaryKey)
				{
					clause.AppendLine(
						"\t,[" + cdcColumnConfig.SourceColumn + "]"
						+ " " + GetDataTypeDefinition(cdcColumnConfig, TargetSystem.EdwStaging)); // sql query building
				}
			}

			return clause.ToString();
		}

		static string GetStagingPrimaryKeyName(BiAutomationConfigDataSet.CdcTableConfigRow cdcTableConfig)
		{
			var cdcColumnConfigRows = cdcTableConfig.GetOrderedCdcColumnConfigRows();
			foreach (BiAutomationConfigDataSet.CdcColumnConfigRow cdcColumnConfig in cdcColumnConfigRows)
			{
				if (cdcColumnConfig.IsPrimaryKey)
				{
					return cdcColumnConfig.SourceColumn;
				}
			}
			return "";
		}

		static string GetEdwModelColumnDefinitionClause(BiAutomationConfigDataSet.EdwTableConfigRow edwTableConfig)
		{
			var edwModelDefinition = new List<string>();

			foreach (var edwColumnConfig in edwTableConfig.GetEdwColumnConfigRows())
			{
				edwModelDefinition.Add(
					string.Format(CultureInfo.InvariantCulture, "[{0}] {1} {2}",
					edwColumnConfig.Name,
					GetDataTypeDefinition(edwColumnConfig),
					((edwColumnConfig.Name == edwTableConfig.BaseName + "ID" ||
					 edwColumnConfig.Name == edwTableConfig.BaseName + "Key") ? "NOT NULL" : "NULL")));
			}

			return string.Join(",\r\n\t", edwModelDefinition);
		}

		static string GetDenormalizedTableColumnDefinitionClause(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable)
		{
			var edwDenormTableDefinition = new List<string>();

			foreach (var column in denormTable.GetEdwDenormalizedColumnConfigRows())
			{
				edwDenormTableDefinition.Add(
					string.Format(CultureInfo.InvariantCulture, "[{0}] {1} {2}",
					column.Name,
					GetDataTypeDefinition(column),
					((column.Name == string.Format(CultureInfo.InvariantCulture, "{0}Key", denormTable.BaseName)) ? "NOT NULL" : "NULL")));
			}

			return string.Join(",\r\n\t", edwDenormTableDefinition);
		}

		static string GetCustomTableColumnDefinitionClause(BiAutomationConfigDataSet.EdwCustomTableConfigRow customTable)
		{
			var edwCustomTableDefinition = new List<string>();

			foreach (var column in customTable.GetEdwCustomColumnConfigRows())
			{
				edwCustomTableDefinition.Add(
					string.Format(CultureInfo.InvariantCulture, "[{0}] {1} NULL",
					column.Name,
					GetDataTypeDefinition(column)));
			}

			return string.Join(",\r\n\t", edwCustomTableDefinition);
		}

		#endregion

		#endregion

		#endregion
	}
}
