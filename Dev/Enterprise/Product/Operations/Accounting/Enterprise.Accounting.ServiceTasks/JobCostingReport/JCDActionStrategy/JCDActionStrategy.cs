using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.ServiceTasks
{
	public abstract class JCDActionStrategy
	{
		protected JCDActionStrategy(DbConnection connection, ILogger logger)
		{
			this.connection = connection;
			this.logger = logger;
			this.backlogWaiter = new BacklogWaiter(new IBacklogInfoProvider[] { new LogFullnessProvider() });
		}

		protected readonly DbConnection connection;
		protected readonly ILogger logger;
		readonly BacklogWaiter backlogWaiter;

		public void Process()
		{
			logger.Log(LogType.Debug, "Checking whether action can be performed");

			if (CanPerform())
			{
				using (connection.TemporarySetDefaultCommandTimeOut(DbCommand.Timeout.Infinite))
				{
					SynchronizeDBObjects();

					SynchronizePartitionKeysWithAccountingPeriods();
				}

				ProcessData();

				NudgeIfRequired();
			}
			else
			{
				LogErrorIfCannotPerform();
			}
		}

		protected virtual void LogErrorIfCannotPerform()
		{
			var errMessage = GetCannotPerformMessage();
			if (!errMessage.IsNullOrEmpty())
			{
				logger.Log(LogType.Error, errMessage);
			}
		}

		protected abstract bool CanPerform();

		void SynchronizeDBObjects()
		{
			var regItemValueHolder = GetValueHolderForJCDRelatedRegitryItems();

			bool isSynchronized = false;

			using (var manager = connection.BeginTransactionWithManager())
			{
				try
				{
					logger.Log(LogType.Debug, "Starting DB Object Synchronization.");

					SynchronizeDBObjectsCore();

					manager.CommitTransaction();

					UpdateControllerRegistryStatusAfterDBSync();

					logger.Log(LogType.Debug, "DB Object Synchronization has been completed.");

					isSynchronized = true;
				}
				catch (Exception e)
				{
					var result = CanRetryTheAction(e, (NoResString)"Failed to Synchronize DB Objects.");
					if (result.retry)
					{
						NudgeAfterSqlException();
					}
					else if(result.throwTheOriginalException)
					{
						throw;
					}
				}
			}

			if (!isSynchronized)
			{
				regItemValueHolder.Rollback();
			}
		}

		protected virtual void SynchronizePartitionKeysWithAccountingPeriods()
		{
			try
			{
				var createPartition = IsThereAnyNewParitionToCreate();
				if (createPartition && DBObjectChecker.DoesAllPermanentJCDDBObjectExist())
				{
					var dependentObjectVersionManager = JCDDependentObjectList.GetVersionManager();
					var periodUpdater = new PeriodCompanyPartitionUpdater(connection
						, dependentObjectVersionManager.DBObjects.OfType<IDeleteDataWhenRemovePartition>()
						, logger);
					periodUpdater.SyncPartitionsWithAccountingPeriods();
				}
				else
				{
					logger.Log(LogType.Debug, "Partition Keys are up-to-date.");
				}
			}
			catch (Exception e)
			{
				var result = CanRetryTheAction(e, (NoResString)"Failed to create partition Key.");
				if (result.retry)
				{
					NudgeAfterSqlException();
				}
				else if (result.throwTheOriginalException)
				{
					throw;
				}
			}
		}

		protected virtual bool IsThereAnyNewParitionToCreate()
		{
			return AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.Value;
		}

		protected abstract void ProcessData();

		void NudgeIfRequired()
		{
			if (IsNudgingRequired)
			{
				string msg = string.Empty;
				if (!TryNudge(ref msg))
				{
					logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Failed to Nudge. Error: {0}", msg));
				}
				else
				{
					logger.Log(LogType.Debug, "Service Task has been nudged successfully");
				}
			}
		}

		protected abstract string GetCannotPerformMessage();

		#region DB Synchronization related

		protected virtual void SynchronizeDBObjectsCore()
		{
			bool resetDBObjectChecker = false;

			var versionManager = JCDTempFunctionList.GetVersionManager();
			if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value != JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed)
			{
				if (!versionManager.IsUpToDate && DBObjectChecker.DoesAllTempJCDTableExist())
				{
					resetDBObjectChecker = new JCDDependentObjectSynchronizer(connection, logger, versionManager).Synchronize();
				}
				else
				{
					logger.Log(LogType.Debug, FormattableString.Invariant($"{versionManager.Description} are up-to-date."));
				}
			}

			versionManager = JCDDependentObjectList.GetVersionManager();
			if (!versionManager.IsUpToDate && DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist())
			{
				resetDBObjectChecker |= new JCDDependentObjectSynchronizer(connection, logger, versionManager).Synchronize();
			}
			else
			{
				logger.Log(LogType.Debug, FormattableString.Invariant($"{versionManager.Description} are up-to-date."));
			}

			if (resetDBObjectChecker)
			{
				ResetDBObjectChecker();
			}
		}

		protected virtual void UpdateControllerRegistryStatusAfterDBSync()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query, Log Message. Not Displayed in an UI")]
		protected void DeleteAllDBObjects()
		{
			//Delete all DB Function and Procedures related to JobCosting
			DeleteAllFunctionsAndProcedures();

			//Delete all Temp Tables created for processing Old AL Record
			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(connection, (logText) => logger.Log(LogType.Debug, logText));

			//Delete all Tables and Partitions created for storing Job Costing Data
			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(connection, (logText) => logger.Log(LogType.Debug, logText));

			//Delete Everyting from dbo.JobCostingDataQueue table
			connection.ExecuteNonQuery("TRUNCATE TABLE dbo.JobCostingDataQueue");

			//Reset Registry
			ResetAllJCDRelatedRegistry();

			logger.Log(LogType.Debug, "All Job Costing Report related DB Objects are dropped.");
		}

		void DeleteAllFunctionsAndProcedures()
		{
			//Delete all DB Function, Procedures and Triggers related to JobCosting
			new JCDDependentObjectSynchronizer(connection, logger, JCDDependentObjectList.GetVersionManager()).DropObjects(0);

			//Delete all DB Function, Procedures and Triggers created for processing Old AL Record
			new JCDDependentObjectSynchronizer(connection, logger, JCDTempFunctionList.GetVersionManager()).DropObjects(0);
		}

		#endregion

		#region Data Process related

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not Displayed in the UI, SQL Query, SQL statement, Log Message. Not displayed in UI")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void PopulateTempTables()
		{
			logger.Log(LogType.Debug, "Getting AccTransactionLines that need to be transformed for JobCosting Reports.");

			BackLogWaiterChecker();
			using (var manager = connection.BeginTransactionWithManager())
			{
				try
				{
					//Delete Everyting from RptDtUnprocessedAccTransactionLines, RptDtUnprocessedReversedAL, JobCostingDataQueue and RptDtJobCostingData table
					connection.ExecuteNonQuery("TRUNCATE TABLE dbo.RptDtUnprocessedAccTransactionLines");
					connection.ExecuteNonQuery("TRUNCATE TABLE dbo.RptDtUnprocessedReversedAL");
					connection.ExecuteNonQuery("TRUNCATE TABLE dbo.JobCostingDataQueue");
					connection.ExecuteNonQuery("DELETE FROM dbo.RptDtJobCostingData");

					//Populate RptDtUnprocessedAccTransactionLines tables
					using (var cmd = connection.Command(@"INSERT INTO dbo.RptDtUnprocessedAccTransactionLines (UL_ALPK) SELECT AL_PK FROM dbo.AccTransactionLines", 1800))
					{
						cmd.CommandType = CommandType.Text;
						cmd.ExecuteNonQuery();
					}

					manager.CommitTransaction();

					AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ProcessingOldTransactionLines);
				}
				catch (Exception ex)
				{
					var result = CanRetryTheAction(ex, (NoResString)"Failed to load data into RptDtUnprocessedAccTransactionLines table.");
					if (result.retry)
					{
						NudgeAfterSqlException();
					}
					else if (result.throwTheOriginalException)
					{
						throw;
					}
				}
			}

			logger.Log(LogType.Debug, "Temporary tables have been populated successfully");
		}

		#endregion

		#region Nudging related

		protected virtual bool IsNudgingRequired
		{
			get { return nudgeToTryAgain; }
		}
		bool nudgeToTryAgain;

		bool TryNudge(ref string msg)
		{
			var result = false;

			try
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("JCD");
				result = true;
			}
			catch (AggregateException ae)
			{
				ZStringBuilder errMsgBuilder = new ZStringBuilder();
				ae.Handle(e =>
				{
					ErrorReporter.ReportOnce("7f375fa1-9f36-45b1-8d73-d8419933e12f", "Error nudging", e);
					errMsgBuilder.Append(e.Message);
					return true; // Don't allow nudging to crash the service task...
				});

				msg = errMsgBuilder.ToStringWithNewLineBetweenAppends();
			}

			return result;
		}

		#endregion

		#region Registry Value setting related helper functions and class

		protected void ResetAllJCDRelatedRegistry()
		{
			AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ResetJCDDBObjectVesionNoInRegistry();
		}

		protected void ResetRegitryItemsThatRelatedToTempJCDDBObjects()
		{
			AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
		}

		protected void ResetJCDDBObjectVesionNoInRegistry()
		{
			ChangeJCDDBObjectVesionNoInRegistry(-1, -1);
		}

		protected void UpdateJCDDBObjectVesionNoInRegistryAfterTableAndPartitionCreation(int startingPeriod)
		{
			ChangeJCDDBObjectVesionNoInRegistry(0, 0);
			AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingPeriod);
		}

		void ChangeJCDDBObjectVesionNoInRegistry(int jobCostingReportRelatedDBObjectVersion, int jobCostingQueueDBObjectVersion)
		{
			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCostingReportRelatedDBObjectVersion);
			AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCostingQueueDBObjectVersion);
		}

		RegistryValueHolder GetValueHolderForJCDRelatedRegitryItems() => new RegistryValueHolder(AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark,
																								AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated,
																								AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion,
																								AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion);

		protected RegistryValueHolder GetValueHolderForRegitryItemsThatRelatedToTempJCDDBObjects() => new RegistryValueHolder(AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark,
																																AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion);

		protected class RegistryValueHolder
		{
			public RegistryValueHolder(params IRegistryItem[] regItems)
			{
				regItemWithValues = new Dictionary<IRegistryItem, object>();
				foreach (IRegistryItem regItem in regItems)
				{
					regItemWithValues.Add(regItem, regItem.Value);
				}
			}

			readonly Dictionary<IRegistryItem, object> regItemWithValues;

			public void Rollback()
			{
				foreach (IRegistryItem regItem in regItemWithValues.Keys)
				{
					regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regItemWithValues[regItem]);
				}
			}
		}

		#endregion

		#region Helper Functions

		protected void BackLogWaiterChecker()
		{
			backlogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) => {
				if (success)
				{
					logger.Log(LogType.Debug, "Waiting for backlog to clear");
				}
				else
				{
					logger.Log(LogType.Information, failureReason);
				}
			});
		}

		protected (bool retry, bool throwTheOriginalException) CanRetryTheAction(Exception ex, string prefixToErrorMessage)
		{
			var shouldRetry = false;
			var errorMessage = ex.Message;
			var logType = LogType.Error;
			var isDBLockoutError = false;

			if (ex is System.Data.Common.DbException sqlException)
			{
				var errorHandler = new DbErrorHandler(sqlException, connection);
				errorMessage = string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", errorHandler.GetDBErrorUserFriendlyMessage(), errorHandler.GetExtraDebugInformation());
				isDBLockoutError = errorHandler.ExceptionType == DbErrorType.LockTimeoutExpired;
				shouldRetry = errorHandler.ShouldBeRetried && !isDBLockoutError; //We do not want to retry immediately when DB is too busy.
				if (isDBLockoutError || shouldRetry)
				{
					logType = LogType.Information; //When it is lock out error, it is probably because DB is too busy. Instead of logging it as an error, we want to log it as info. Probably when JCD runs next time, DB will be able to complete the sql execution.
				}
			}

			logger.Log(logType, string.Format(CultureInfo.CurrentCulture, "{0} \r\nException: {1} \r\nMessage: {2} \r\nStackTrace: {3}", prefixToErrorMessage, ex.GetType(), errorMessage, ex.StackTrace));

			return (shouldRetry, !shouldRetry && !isDBLockoutError);
		}

		protected void NudgeAfterSqlException()
		{
			nudgeToTryAgain = true;
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Will nudge the service task to try again..."));
		}

		#endregion

		protected JCDDBObjectsChecker DBObjectChecker
		{
			get
			{
				if (checker == null)
				{
					checker = new JCDDBObjectsChecker(connection);
				}
				return checker;
			}
		}
		JCDDBObjectsChecker checker;

		protected void ResetDBObjectChecker() => checker = null;
	}
}
