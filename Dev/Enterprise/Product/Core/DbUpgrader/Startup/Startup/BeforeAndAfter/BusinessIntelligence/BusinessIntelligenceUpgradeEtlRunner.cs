using System;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Bi.Common.Integration;
using CargoWise.Bi.Product.DataLoad;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	class BusinessIntelligenceUpgradeEtlRunner
	{
		public BusinessIntelligenceUpgradeEtlRunner(AdminConnection mainDbConnection, AdminConnection biConnection)
		{
			this.mainDbConnection = mainDbConnection;
			this.biConnection = biConnection;
			mainDbName = ((ICurrentDbControl)mainDbConnection).InitialDatabase;
		}

		readonly AdminConnection mainDbConnection;
		readonly AdminConnection biConnection;
		readonly string mainDbName;

		public void CapturePendingCdcChangesAndFlushToAuditDatabase(IUpgradeTaskWorkflowLogger logger)
		{
			if (CdcDatabase.IsEnabled(mainDbConnection, mainDbName))
			{
				PopulateLsnPeriodIfApplicable(logger);
				CaptureAndProcessCdcChanges(logger);
				logger.ShowInfoMessage(".");
			}
		}

		void CaptureAndProcessCdcChanges(IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				var scanner = new OfflineCdcScanner(logger);
				CaptureAllChanges(logger, scanner);
				FlushCdcDataToAuditDatabase(logger);
				SetFirstCdcRunExtProperty();
				LogColumnstoreRowGroupState(logger);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string msgHeader = "Failed to capture and process pending change data";
				throw new OdysseyException(String.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", msgHeader, ex.Message), ex);
			}
		}

		#region CDC Scan

		protected void CaptureAllChanges(IUpgradeTaskWorkflowLogger logger, CdcScanner scanner)
		{
			try
			{
				logger.StartTask($"Capturing pending changes using CDC. There are {CdcScanner.GetQueueSizeAndAge().Count} transactions in the queue.");
				if (IsSelfHosted)
				{
					scanner.CdcScannerLogger.Log("Please consider monitoring CDC scan progress with sys.dm_cdc_log_scan_sessions for more details.");
				}
				CaptureAllChangesCore(logger, scanner);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc)
			{
				HandleAnotherCaptureInstanceRunning(logger, isTransactionReplicationEnabled: false);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CdcFailedDueToTransactionalReplication)
			{
				HandleAnotherCaptureInstanceRunning(logger, isTransactionReplicationEnabled: true);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotAllowAlterCDCMetaObjects)
			{
				var errorMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
				logger.ShowTaskError(errorMessage);
				if (EnvProxy.IsHostedWithCargowise)
				{
					throw new MandatoryTraceFlagMissingException(errorMessage, ex);
				}
			}
			catch (CdcInternalStructureChangedException)
			{
				logger.ShowInfoMessage("Upgrading CDC metadata to fix scan error after SQL Server version upgrade. This might take a while.");
				CdcDatabase.RunInternalCdcUpgrade(mainDbConnection, mainDbName);

				logger.ShowInfoMessage("Re-trying CDC scan.");
				CaptureAllChangesCore(logger, scanner);
			}
		}

		protected virtual void CaptureAllChangesCore(IUpgradeTaskWorkflowLogger logger, CdcScanner scanner)
		{
			scanner.SetScanTimeout(DbCommand.Timeout.Infinite);
			long processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
			logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "{0} transactions processed.", processedTranCount.ToString(CultureInfo.InvariantCulture)));
		}

		void HandleAnotherCaptureInstanceRunning(IUpgradeTaskWorkflowLogger logger, bool isTransactionReplicationEnabled)
		{
			if (isTransactionReplicationEnabled)
			{
				logger.ShowInfoMessage("Transaction replication is enabled for the database. ");
				logger.ShowInfoMessage("Waiting for Log Reader Agent to capture all changes...");
			}
			else
			{
				logger.ShowInfoMessage("Change data capture is already running in another session.");
				logger.ShowInfoMessage("Waiting for CDC to capture all changes...");
			}

			var waitForCdcScanResult = WaitUntilAllChangesCaptured(mainDbConnection);
			switch (waitForCdcScanResult)
			{
				case WaitForCdcScanResult.CdcCaptured:
					logger.ShowInfoMessage("Log Reader has captured the all the data changes. It is now safe to proceed with DB upgrade.");
					break;
				case WaitForCdcScanResult.IncompleteCdcScan:
					if (isTransactionReplicationEnabled)
					{
						throw new OdysseyException("Log Reader has not captured all data changes within designed time interval. It may be in suspended, not running, or busy state. Check the log reader logs and retry upgrade. Make sure Log Reader is running in continuous mode.");
					}
					else
					{
						throw new OdysseyException("Log Reader has not captured all data changes within designed time interval. It may be in suspended, not running, or busy state. Check the log reader logs and retry upgrade.");
					}
				default:
					throw new OdysseyException("Unknown result from waiting for CDC scan to complete.");
			}
		}

		void SetFirstCdcRunExtProperty()
		{
			ExtProperty.Database.Update(mainDbConnection, CdcDatabase.IsFirstCdcRunExtPropertyName, "1");
		}

		#region Wait for Another session running CDC scan to capture changes

		WaitForCdcScanResult WaitUntilAllChangesCaptured(DbConnection conn)
		{
			using (var cmd = conn.Command(WaitUntilAllChangesCapturedSql, WaitTimeInSeconds + (int)TimeSpan.FromMinutes(1).TotalSeconds))
			{
				cmd.AddParameter("@WaitTimeInSeconds", System.Data.SqlDbType.Int, WaitTimeInSeconds);
				return (WaitForCdcScanResult)Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public enum WaitForCdcScanResult
		{
			IncompleteCdcScan = 0,
			CdcCaptured = 1
		}

		const string WaitUntilAllChangesCapturedSql = @"
--Delete old capture instance and related objects if exist
IF EXISTS(SELECT NULL FROM sys.objects WHERE name = 'fn_cdc_get_net_changes_dbo_DBUPG-PENDING-CDC-MARK')
	DROP FUNCTION [cdc].[fn_cdc_get_net_changes_dbo_DBUPG-PENDING-CDC-MARK]

IF EXISTS(SELECT NULL FROM sys.objects WHERE name = 'fn_cdc_get_all_changes_dbo_DBUPG-PENDING-CDC-MARK')
	DROP FUNCTION [cdc].[fn_cdc_get_all_changes_dbo_DBUPG-PENDING-CDC-MARK]

DECLARE @objid INT
SET @objid = (SELECT object_id FROM cdc.change_tables WHERE capture_instance = 'dbo_DBUPG-PENDING-CDC-MARK')

DELETE FROM cdc.index_columns WHERE object_id = @objid
DELETE FROM cdc.captured_columns WHERE object_id = @objid
DELETE FROM cdc.change_tables WHERE object_id = @objid

IF EXISTS (SELECT NULL FROM sys.tables WHERE name = 'dbo_DBUPG-PENDING-CDC-MARK_CT')
	DROP TABLE [cdc].[dbo_DBUPG-PENDING-CDC-MARK_CT]
IF EXISTS(SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK')
	DROP TABLE [dbo].[DBUPG-PENDING-CDC-MARK];

--Create new temp table
CREATE TABLE [dbo].[DBUPG-PENDING-CDC-MARK] (PKCOL UNIQUEIDENTIFIER NOT NULL PRIMARY KEY);

--Enable it for CDC
EXEC sys.sp_cdc_enable_table @source_schema = 'dbo', @source_name = 'DBUPG-PENDING-CDC-MARK', @role_name = null;

--Change the data in the table
INSERT [dbo].[DBUPG-PENDING-CDC-MARK] (PKCOL) VALUES (NEWID());

DECLARE @DT DATETIME = DATEADD(second, @WaitTimeInSeconds, GETDATE())
DECLARE @ChangeIsCaptured BIT = 0

WHILE @DT > GETDATE() AND @ChangeIsCaptured = 0
BEGIN
	IF EXISTS(SELECT 1 FROM cdc.[dbo_DBUPG-PENDING-CDC-MARK_CT])
		SET @ChangeIsCaptured = 1
	ELSE
		WAITFOR DELAY '00:00:01.000'
END

--Disable CDC
EXEC sys.sp_cdc_disable_table @source_schema = 'dbo', @source_name = 'DBUPG-PENDING-CDC-MARK', @capture_instance = 'dbo_DBUPG-PENDING-CDC-MARK'

--Drop table
DROP TABLE [dbo].[DBUPG-PENDING-CDC-MARK];

--Check if we can now proceed with DB upgrade.
SELECT @ChangeIsCaptured as ChangeIsCaptured";

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected virtual int WaitTimeInSeconds
		{
			get
			{
				return Convert.ToInt32(TimeSpan.FromMinutes(5).TotalSeconds);
			}
		}

		public bool IsSelfHosted
		{
			get
			{
				if (isSelfHosted == null)
				{
					isSelfHosted = !EnvProxy.IsHostedWithCargowise;
				}
				return isSelfHosted.Value;
			}
		}
		protected bool? isSelfHosted;

		#endregion

		#region Audit ETL execution

		void FlushCdcDataToAuditDatabase(IUpgradeTaskWorkflowLogger logger)
		{
			var runner = GetEtlExecutionManager(logger);
			if (runner != null)
			{
				logger.StartTask("Flushing data to Audit database.");
				runner.ExecuteAuditEtlProcess();
			}
		}

		internal virtual IEtlAuditExecution GetEtlExecutionManager(IUpgradeTaskWorkflowLogger logger)
		{
			return EtlExecutionManagerFactory.NewForUpgrade(mainDbConnection, biConnection, new LoggerWrapper(logger));
		}

		#endregion

		#region Add LSN Period Column

		[System.Diagnostics.Conditional("DEBUG")]
		void PopulateLsnPeriodIfApplicable(IUpgradeTaskWorkflowLogger logger)
		{
			if (biConnection.DatabaseExists(Db.AuditDatabaseName))
			{
				logger.StartTask("Populating LSN Period in Audit database.");

				string sqlText = String.Format(CultureInfo.InvariantCulture, @"
					DECLARE @SchemaName sysname;
					DECLARE @TableName sysname;
					DECLARE @PeriodColumnExists bit;
					DECLARE @SqlCommand nvarchar(4000);

					DECLARE LsnPeriodDefaultCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
						SELECT
							s.name, t.name, CONVERT(BIT, CASE WHEN pcol.name IS NULL THEN 0 ELSE 1 END)
						FROM
							[{0}].sys.tables t
							INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id
							INNER JOIN [{0}].sys.indexes i ON t.object_id = i.object_id
							INNER JOIN [{0}].sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
							INNER JOIN [{0}].sys.columns lcol ON lcol.object_id = t.object_id AND lcol.name = '__$start_lsn'
							LEFT JOIN [{0}].sys.columns pcol ON pcol.object_id = t.object_id AND pcol.name = '__$lsn_period'
						WHERE
							p.[Rows] > 0;

					OPEN LsnPeriodDefaultCursor;
					FETCH NEXT FROM LsnPeriodDefaultCursor INTO @SchemaName, @TableName, @PeriodColumnExists;

					WHILE (@@FETCH_STATUS = 0)
					BEGIN
						IF (@PeriodColumnExists = 0)
						BEGIN
							SET @SqlCommand = 'TRUNCATE TABLE [{0}].[' + @SchemaName + '].[' + @TableName + ']';
						END
						ELSE
						BEGIN
							SET @SqlCommand = '
								BEGIN TRY
									DECLARE @RowCount INT = 1;

									WHILE (@RowCount > 0)
									BEGIN
										UPDATE TOP (1000000) aut
											SET [__$lsn_period] = ((YEAR(ltm.TranEndTimeUtc) - 2000) * 100) + MONTH(ltm.TranEndTimeUtc)
												FROM [{0}].[' + @SchemaName + '].[' + @TableName + '] aut
												INNER JOIN [{0}].biadmin.LsnTimeMapping ltm ON ltm.StartLsn = aut.[__$start_lsn]
												WHERE aut.[__$lsn_period] <= 0
												AND YEAR(ltm.TranEndTimeUtc) >= 2000;
										SET @RowCount = @@ROWCOUNT;

										IF (@RowCount = 1000000)
										BEGIN
											WAITFOR DELAY ''00:00:00.100'';
										END
									END

									DELETE [{0}].[' + @SchemaName + '].[' + @TableName + '] WHERE [__$lsn_period] <= 0;
								END TRY
								BEGIN CATCH
									THROW;
								END CATCH';
						END

						EXEC (@SqlCommand);

						FETCH NEXT FROM LsnPeriodDefaultCursor INTO @SchemaName, @TableName, @PeriodColumnExists;
					END;

					CLOSE LsnPeriodDefaultCursor;
					DEALLOCATE LsnPeriodDefaultCursor;",
					Db.AuditDatabaseName);

				biConnection.ExecuteNonQuery(sqlText);
			}
		}

		#endregion

		#region Log Columnstore Row Group State

		void LogColumnstoreRowGroupState(IUpgradeTaskWorkflowLogger logger)
		{
			if (biConnection.DatabaseExists(Db.AuditDatabaseName))
			{
				var crgStateTable = ObjectFactory.Get<IBiColumnstoreRowGroupHelper>().GetColumnstoreRowGroupPhysicalState(biConnection);

				if (crgStateTable.Rows.Count > 0)
				{
					logger.ShowInfoMessage("The following tables residing in the Audit DB are pending deltastore rowgroup compression. If the upgrade fails, please try again:");

					foreach (DataRow row in crgStateTable.Rows)
					{
						var tableName = row["TableName"].ToString();
						var rowGroupState = row["RowGroupState"].ToString();
						logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "{0} has a rowgroup state of {1}", tableName, rowGroupState));
					}
				}
			}
		}

		#endregion
	}
}
