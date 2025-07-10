using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Resource.Shared;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Schema.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Template.Testing
{
	sealed class AuditDbTemplateTest : TestCase
	{
		public void TestNoAuditTablesStartWithSpecificPrefixes()
		{
			MainDbTemplateMetadataTest.AssertNoTableNamesStartWithPrefix(new ScriptManager().AuditDbSchemaScript, "DUMMY");
		}

		public void TestMissingRequiredTablesAndColumnsAreCreated()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(testConnection, TestDbName);

					using (((ICurrentDbControl)testConnection).UseDatabase(TestDbName))
					{
						var testDb = (IAuxiliaryDbCreator)new AuditDbTemplateForTest(new DummyUpgradeManager(), TestDbName, testConnection.ServerName);
						testDb.CreateDropExisting();

						CombineAssertions(() =>
						{
							// Assert columns before
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "TestTable", "TT[_]PK", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "TestTable", "TT[_]JobID", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "AnotherTestTable", "ATT_Size", expected: false);
						});

						var testDbWithRequiredTables = (IAuxiliaryDbCreator)new AuditDbTemplateForTest(
							new DummyUpgradeManager(),
							TestDbName,
							testConnection.ServerName,
							new[]
							{
								new CdcRequiredTable("dbo", "TestTable", new[] { "TT_JobID" }),
								new CdcRequiredTable("dbo", "AnotherTestTable", new[] { "ATT_Size" }),
							});
						testDbWithRequiredTables.CreateDropExisting();

						CombineAssertions(() =>
						{
							// Assert columns after
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "TestTable", "TT[_]PK", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "TestTable", "TT[_]JobID", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "AnotherTestTable", "ATT_Size", expected: true);
						});
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
				}
			}
		}

		void AssertColumnExists(DbConnection testConnection, string schemaName, string tableName, string columnLikePattern, bool expected)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT 1
					FROM [{0}].sys.schemas s
					INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
					INNER JOIN [{0}].sys.columns c ON c.object_id = t.object_id
					WHERE s.name = '{1}'
					AND t.name = '{2}'
					AND c.name like '{3}'
				) SELECT 1 ELSE SELECT 0",
				TestDbName,
				schemaName,
				tableName,
				columnLikePattern);

			bool columnExist = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(schemaName + "." + tableName + "." + columnLikePattern + " column exists", expected, columnExist);
		}

		const string TestDbName = "Enterprise.DbUpgrader.Schema.Template.Testing.AuditDbTemplateTest.Db";

		class AuditDbTemplateForTest : AuditDbTemplate
		{
			public AuditDbTemplateForTest(IUpgradeManager manager, string templateDbName, string serverName, IEnumerable<CdcRequiredTable> requiredTables = null)
				: base(manager, templateDbName, serverName)
			{
				this.requiredTables = requiredTables ?? Enumerable.Empty<CdcRequiredTable>();
			}

			protected override IEnumerable<CdcRequiredTable> LoadCdcConfigurationFromShared()
			{
				return requiredTables;
			}

			protected override string GetAuditDbSchemaScript()
			{
				return
@"EXEC ('CREATE SCHEMA [biadmin]');

CREATE TABLE [biadmin].[TableConfiguration]
(
	[TableID] [int] IDENTITY(1,1) NOT NULL,
	[SourceSchemaName] [varchar](128) NULL,
	[SourceTableName] [varchar](128) NULL,
	[TableColumnList] [nvarchar](max) NULL,
    [AuditFilter] [varchar](700) NULL,
	[PkName] [sysname] NULL
)
ALTER TABLE  [biadmin].[TableConfiguration]
	ADD CONSTRAINT [TableID_TableConfiguration] PRIMARY KEY CLUSTERED ([TableID])

CREATE TABLE [biadmin].[TableState]
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
ALTER TABLE  [biadmin].[TableState]
	ADD CONSTRAINT [SourceSchemaName_SourceTableName_TableState] PRIMARY KEY CLUSTERED ([SourceSchemaName], [SourceTableName]);

CREATE TABLE [biadmin].[MasterState]
(
	[ParamID] [int] IDENTITY(1,1) NOT NULL,
	[ParamName] [nvarchar](MAX) NULL,
	[ParamValue] [nvarchar](MAX) NULL,
)
ALTER TABLE  [biadmin].[MasterState]
	ADD CONSTRAINT [ParamID_MasterState] PRIMARY KEY CLUSTERED ([ParamID]);

CREATE TABLE [biadmin].[LsnTimeMapping]
(
	StartLsn BINARY(10) NOT NULL,
	TranEndTimeUtc DATETIME NOT NULL
);

ALTER TABLE [biadmin].[LsnTimeMapping]
	ADD CONSTRAINT [LsnTimeMapping_StartLsn_PK] PRIMARY KEY CLUSTERED (StartLsn);

CREATE NONCLUSTERED INDEX [LsnTimeMapping_TranEndTimeUtc_IX] ON [biadmin].[LsnTimeMapping] (TranEndTimeUtc ASC);

CREATE TABLE [biadmin].[DataLossLog]
(
	SourceSchemaName VARCHAR(128) NOT NULL,
	SourceTableName VARCHAR(128) NOT NULL,
	StartLsn BINARY(10) NOT NULL,
	EndLsn BINARY(10) NOT NULL,
	MinTableLsn BINARY(10) NOT NULL,
	LossType VARCHAR(10) NOT NULL,
	CreateDateTimeUTC DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE CLUSTERED INDEX [CX_biadmin_DataLossLog_StartLsn] on [biadmin].[DataLossLog] (StartLsn);

CREATE TABLE [biadmin].[CdcHistorySummary]
(
	[CdcHistorySummaryID] [bigint] IDENTITY(1,1) NOT NULL,
	[Lsn] [binary](10) NOT NULL,
	[SchemaName] [varchar](128) NOT NULL,
	[ChangedTableName] [varchar](128) NOT NULL,
	[NumberOfRows] [int] NOT NULL,
	[LsnPeriod] [smallint] NOT NULL,
	[TranEndTimeUTC] [datetime] NOT NULL DEFAULT GETUTCDATE()
);

CREATE CLUSTERED COLUMNSTORE INDEX [cci_biadmin_CdcHistorySummary] on [biadmin].[CdcHistorySummary];
CREATE NONCLUSTERED INDEX [IX_CdcHistorySummary_Lsn] ON [biadmin].[CdcHistorySummary] (Lsn) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX [IX_CdcHistorySummary_SchemaName_ChangedTableName_Lsn] ON [biadmin].[CdcHistorySummary] (SchemaName,ChangedTableName,Lsn) INCLUDE (TranEndTimeUTC) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [biadmin].[SubscriberControl](
	[SubscriberCode] char(3) NOT NULL,
	[Description] varchar(128) NULL,
	[LsnHighWaterMark] binary(10) NOT NULL DEFAULT 0x,
	[SeqValHighWaterMark] binary(10) NOT NULL DEFAULT 0x
);

ALTER TABLE  [biadmin].[SubscriberControl]
	ADD CONSTRAINT [PK_SubscriberControl] PRIMARY KEY CLUSTERED ([SubscriberCode]);

CREATE TABLE [biadmin].[SchemaMappingSummary]
(
	[MaxLsn] binary(10) NOT NULL,
	[MaxLsnTimeUTC] datetime NOT NULL,
	[EffectiveSchemaVersion] varchar(128) NOT NULL,
	[TableName] varchar(128) NOT NULL,
	[RenamedColumn] varchar(128) NOT NULL,
	[MappedColumn] varchar(128) NOT NULL
);

ALTER TABLE  [biadmin].[SchemaMappingSummary]
	ADD CONSTRAINT [PK_SchemaMappingSummary] PRIMARY KEY CLUSTERED ([TableName], [MappedColumn], MaxLsn);
CREATE TABLE [dbo].[TestTable]
(
	[__$start_lsn] binary(10) NOT NULL
	,[__$seqval] binary(10) NOT NULL
	,[__$operation] int NOT NULL
	,[__$update_mask] varbinary(128) NOT NULL
	,[__$lsn_period] smallint NOT NULL
	,[TT_AutoVersion] smallint NULL
	,[TT_PK] uniqueidentifier NULL
	,[TT_SystemCreateTimeUtc] smalldatetime NULL
	,[TT_SystemCreateUser] varchar(3) NULL
	,[TT_SystemLastEditTimeUtc] smalldatetime NULL
	,[TT_SystemLastEditUser] varchar(3) NULL

 ) ON [PRIMARY];

CREATE CLUSTERED COLUMNSTORE INDEX [cci_dbo_TestTable] ON [dbo].[TestTable] WITH (DROP_EXISTING = OFF);

CREATE NONCLUSTERED INDEX IX_TestTable_StartLsn ON [dbo].[TestTable] (__$start_lsn,__$seqval,__$operation) INCLUDE (__$update_mask,TT_PK) WITH (DATA_COMPRESSION = PAGE);
";
			}

			protected override BiConfigurationData GetBiConfigurationData()
			{
				var biConfigurationData = base.GetBiConfigurationData();
				var cdcTableConfig = biConfigurationData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, false, "", "", "", false);
				biConfigurationData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TT_PK", "int", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
				biConfigurationData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TT_JobID", "int", 9, 8, 7, true, true, "", "", false, "", "", false, false, "");
				cdcTableConfig = biConfigurationData.CdcTableConfig.AddCdcTableConfigRow("dbo", "AnotherTestTable", "", "", "", false, false, "", "", "", false);
				biConfigurationData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "ATT_PK", "int", 9, 8, 7, true, true, "", "", false, "", "", false, false, "");
				biConfigurationData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "ATT_Size", "int", 9, 8, 7, true, true, "", "", false, "", "", false, false, "");
				return biConfigurationData;
			}

			readonly IEnumerable<CdcRequiredTable> requiredTables;
		}
	}
}
