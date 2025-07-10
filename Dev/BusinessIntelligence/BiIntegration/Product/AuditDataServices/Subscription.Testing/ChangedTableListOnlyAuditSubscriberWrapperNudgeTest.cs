using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Notification;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	[TestedType(typeof(AuditSubscriberProcessorTask))]
	class ChangedTableListOnlyAuditSubscriberWrapperNudgeTest : ServiceTaskTestCase<AuditSubscriberProcessorTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[UseSnapshotProtection]
		public void TestASPIsNudged()
		{
			var testAuditDatabaseName = "Test_Audit";
			try
			{
				var nudgedTaskCodes = new List<string>();
				using (var connection = Db.NewAdminConnection())
				{
					connection.CreateDatabase(testAuditDatabaseName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(testAuditDatabaseName))
					{
						connection.ExecuteNonQuery(CreateTestAuditDatabaseText);
						connection.ExecuteNonQuery(CreateTestAuditDatabaseSPText);
						connection.ExecuteNonQuery(CreateStoredProcGetMasterStateParameter);
						connection.ExecuteNonQuery(CreateGetLsnPeriodFunctionText);

						var serviceTaskNudger = new Mock<IServiceTaskNudger>();
						serviceTaskNudger
							.Setup(o => o.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
							.Callback<string, TimeSpan?>((taskCode, _) => nudgedTaskCodes.Add(taskCode));

						using (ObjectFactory.Substitute(serviceTaskNudger.Object))
						{
							var ietTask = new AuditSubscriberProcessorTask();
							InitialiseTaskSchedule(ietTask);
							var tables = new ITableSchema[] { RefCurrencySchema.Instance };
							var testSubscriber = new GenericTestChangedTableListSubscriber("*RX", "Dummy Test Changed Table List Subscriber", tables);
							NudgeASP(connection, testSubscriber);
							AssertCollectionContains("ASP should be nudged", AuditSubscriberProcessorTask.ServiceTaskCode, nudgedTaskCodes);
						}
					}
				}
			}
			finally
			{
				using (var dropDatabaseConnection = Db.NewAdminConnection())
				{
					var sqlText = string.Format("USE [master] IF db_id('{0}') IS NOT NULL BEGIN ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE DROP DATABASE [{0}] END", testAuditDatabaseName);
					dropDatabaseConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestASPIsNotNudged()
		{
			var nudgedTaskCodes = new List<string>();

			var serviceTaskNudger = new Mock<IServiceTaskNudger>();
			serviceTaskNudger
				.Setup(o => o.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
				.Callback<string, TimeSpan?>((taskCode, _) => nudgedTaskCodes.Add(taskCode));

			using (var connection = AuditTestHelper.GetAuditConnection())
			using (ObjectFactory.Substitute(serviceTaskNudger.Object))
			{
				var ietTask = new AuditSubscriberProcessorTask();
				InitialiseTaskSchedule(ietTask);
				var tables = new ITableSchema[] { RefCurrencySchema.Instance };
				var testSubscriber = new GenericTestChangedTableListSubscriber("*RX", "Dummy Test Changed Table List Subscriber", tables);
				NudgeASP(connection, testSubscriber);
				AssertCollectionNotContains("ASP should not be nudged", AuditSubscriberProcessorTask.ServiceTaskCode, nudgedTaskCodes);
			}
		}

		[UseSnapshotProtection]
		public void TestShouldNotUpdateHighWaterMarkIfTheConditionWasSetToFalse()
		{
			var testAuditDatabaseName = "Test_Audit";
			try
			{
				var nudgedTaskCodes = new List<string>();
				using (var connection = Db.NewAdminConnection())
				{
					connection.CreateDatabase(testAuditDatabaseName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(testAuditDatabaseName))
					{
						connection.ExecuteNonQuery(CreateTestAuditDatabaseText);
						connection.ExecuteNonQuery(CreateStoredProc_AddSubscriberToSubscriberControlTable);
						connection.ExecuteNonQuery(CreateStoredProcGetMasterStateParameter);
						connection.ExecuteNonQuery(CreateGetLsnPeriodFunctionText);
						var tables = new ITableSchema[] { RefCurrencySchema.Instance };
						var rawSubscriber = new GenericTestChangedTableListSubscriber("*RX", "Dummy Test Changed Table List Subscriber", tables);
						var initializeLsnAndTime = $@"INSERT INTO[biadmin].[MasterState] (ParamName, ParamValue) VALUES('{BiConstants.LastMaxLsnProcessed}', '0x0C'),('{BiConstants.LastMaxLsnTimeProcessed}', '2017-11-10 00:00:00.000')";
						connection.ExecuteNonQuery(initializeLsnAndTime);
						var testSubscriber = rawSubscriber.GetWrapper(connection, new LoggerForTest());
						testSubscriber.ShouldRunSubscriber();
						AssertSubscriberHighWaterMark(connection, testSubscriber.Code, expected: "0C000000000000000000");
					}
				}
			}
			finally
			{
				using (var dropDatabaseConnection = Db.NewAdminConnection())
				{
					var sqlText = string.Format("USE [master] IF db_id('{0}') IS NOT NULL BEGIN ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE DROP DATABASE [{0}] END", testAuditDatabaseName);
					dropDatabaseConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		void NudgeASP(DbConnection auditConnection, IAuditSubscriber rawSubscriber)
		{
			var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest());
			testSubscriber.HasChangesToProcess();
		}

		void AssertSubscriberHighWaterMark(DbConnection connection, string subscriberCode, string expected)
		{
			string sqlText = $@"
				SELECT LsnHighWaterMark
				FROM [{BiConstants.BiAdminSchemaName}].SubscriberControl
				WHERE SubscriberCode = '{subscriberCode}'
			";
			var actualObj = connection.ExecuteScalar(sqlText);
			string actualValue = (actualObj == null) ? null : string.Join("", ((byte[])actualObj).Select(b => string.Format("{0:x2}", b))).ToUpper();
			AssertEquals("LsnHighWaterMark for subscriber [" + subscriberCode + "]", expected, actualValue);
		}

		const string CreateTestAuditDatabaseSPText = @"
			CREATE PROCEDURE biadmin.usp_GetChangesBySchemaAndTable
			(
				@SubscriberCode char(3),
				@LatestLsn BINARY(10),
				@LatestPeriod smallint,
				@SchemaTableNames dbo.TVP_SchemaTableColumnMapping READONLY,
				@LatestSeqVal binary(10) = NULL output
			)

			AS

			BEGIN
				SET NOCOUNT ON;

				SELECT
					@LatestSeqVal = 0xFFFFFFFFFFFFFFFFFFFF;

				DECLARE @LsnHighWaterMark binary(10);

				SELECT
					@LsnHighWaterMark = LsnHighWaterMark
				FROM biadmin.SubscriberControl
					WHERE SubscriberCode = @SubscriberCode;

				SELECT t.SchemaName, t.TableName, t.ColumnName
					FROM @SchemaTableNames t
					WHERE EXISTS
							(
							SELECT NULL
							FROM biadmin.TableState ts
								 WHERE ts.SourceSchemaName = t.SchemaName
									  AND ts.SourceTableName = t.TableName
									  AND ts.AetHWMHistorySummaryLsn > @LsnHighWaterMark
									  AND ts.AetHWMHistorySummaryLsn <= @LatestLsn
							)
			END
		";

		const string CreateStoredProcGetMasterStateParameter = @"
CREATE PROCEDURE biadmin.usp_GetMasterStateParameter

@ParamName NVARCHAR(MAX),
@ParamValue NVARCHAR(MAX) OUTPUT

AS

BEGIN

	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM [biadmin].[MasterState] WHERE ParamName = @ParamName)
	BEGIN
		SELECT @ParamValue = ParamValue FROM [biadmin].[MasterState] WHERE ParamName = @ParamName
	END ELSE
	BEGIN
		SET @ParamValue = NULL

		INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue)
		VALUES(@ParamName, @ParamValue)
	END

END";

		const string CreateStoredProc_AddSubscriberToSubscriberControlTable = @"
			CREATE PROCEDURE biadmin.usp_AddSubscriberToSubscriberControlTable
			(
				@SubscriberCode		CHAR(3),
				@Description			VARCHAR(128),
				@LatestLsn				BINARY(10),
				@LatestPeriod			SMALLINT
			)

			WITH EXECUTE AS OWNER

			AS

			BEGIN
				INSERT [biadmin].SubscriberControl
					(SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark, PeriodHighWaterMark)
					VALUES (@SubscriberCode, @Description, @LatestLsn, 0x, @LatestPeriod);

			END
		";

		const string CreateTestAuditDatabaseText = @"
												EXEC ('CREATE SCHEMA [biadmin]');
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
													[PeriodHighWaterMark] smallint NULL,
													[LsnHighWaterMark] binary(10) NOT NULL DEFAULT 0x,
													[SeqValHighWaterMark] binary(10) NOT NULL DEFAULT 0x
												);

												ALTER TABLE  [biadmin].[SubscriberControl]
													ADD CONSTRAINT [PK_SubscriberControl] PRIMARY KEY CLUSTERED ([SubscriberCode]);

												CREATE TABLE [biadmin].[MasterState]
												(
													[ParamID] [int] IDENTITY(1,1) NOT NULL,
													[ParamName] [nvarchar](MAX) NULL,
													[ParamValue] [nvarchar](MAX) NULL,
												)
												ALTER TABLE  [biadmin].[MasterState]
													ADD CONSTRAINT [ParamID_MasterState] PRIMARY KEY CLUSTERED ([ParamID]);

												CREATE TYPE dbo.TVP_SchemaTableColumnMapping AS TABLE
													(
														[SchemaName] varchar(128) NOT NULL,
														[TableName] varchar(128) NOT NULL,
														[ColumnName] sysname NOT NULL
													)
												";
		const string CreateGetLsnPeriodFunctionText = @"
			CREATE FUNCTION [biadmin].[udf_GetLsnPeriodFromLsn](@lsn BINARY(10))
			RETURNS SMALLINT
			AS BEGIN
				RETURN 2202;
			END
			";
	}
}
