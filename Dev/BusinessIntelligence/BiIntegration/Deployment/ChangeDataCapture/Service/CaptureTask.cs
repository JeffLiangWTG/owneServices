using System;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Product.ServiceTask;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.ChangeDataCapture.Service.CaptureTask.ServiceTaskCode, Enterprise.ChangeDataCapture.Service.CaptureTask.ServiceTaskName, "BI",
	typeof(Enterprise.ChangeDataCapture.Service.CaptureTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1seconds",
	MaximumPeriod = "1minute",
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)
]

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask))]

namespace Enterprise.ChangeDataCapture.Service
{
	public class CaptureTask : ServiceProviderImpl, IBiNotificationSource
	{
		public const string ServiceTaskCode = "CDC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
		public const string ServiceTaskName = "Change Data Capture - scan service";
		public bool isHostedWithCargoWise = EnvProxy.IsHostedWithCargowise;

		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string CheckIsBiEnabled() => BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		protected CdcScanner Scanner
		{
			get
			{
				return scanner ?? (scanner = new OnlineCdcScanner(ServiceLogger));
			}
		}
		CdcScanner scanner;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			if (!CdcChecker.CheckCdcCleanupJobRunningStatusAndEnforceRetentionPeriod(ServiceLogger))
			{
				CaptureChangesAndReportResult(youMustReactToThisToken);
			}
		}

		#region CDC

		void CaptureChangesAndReportResult(CancellationToken token)
		{
			try
			{
				CaptureChangesCore(token);
			}
			catch (SqlException ex)
			{
				HandleSqlException(ex, token);
			}
			catch (CdcException ex)
			{
				ServiceLogger.Log(LogType.Error, ex.Message);
			}
			catch (CdcInternalStructureChangedException)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					ServiceLogger.Log(LogType.Warning, "Upgrading CDC metadata to fix scan error after SQL Server version upgrade.");
					CdcDatabase.RunInternalCdcUpgrade(adminConnection, Db.DatabaseName);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		void HandleSqlException(SqlException ex, CancellationToken token)
		{
			string errorMessage;
			var exceptionType = new DbErrorMatch(ex).ExceptionType;
			switch (exceptionType)
			{
				case DbErrorType.DatabaseNotEnabledForChangeDataCapture:
					ServiceLogger.Log(LogType.Debug, "Database is not enabled for change data capture. Retrying in 30 minutes.");
					break;
				case DbErrorType.CdcFailedDueToTransactionalReplication:
					ServiceLogger.Log(LogType.Debug, "Log Reader Agent is capturing data changes. Retrying in 30 minutes.");
					break;
				case DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc:
					HandleCdcCaptureJobError(token, ex);
					RetryCdcScan(token);
					break;
				case DbErrorType.OnlyOneLogReaderAgentCanConnectToDatabase:
					errorMessage = "The CDC Service Task must be the sole Log Reader. Other Log Readers such as the transactional replication Log Reader Agent are NOT supported.";
					if (!isHostedWithCargoWise)
					{
						throw new HostedServiceException(errorMessage, ex) { LogException = true };
					}
					else
					{
						ServiceLogger.Log(LogType.Debug, errorMessage);
					}
					break;
				case DbErrorType.DefinitionOfObjectHasChangedSinceCompilation:
					RetryCdcScan(token);
					break;
				case DbErrorType.CannotAllowAlterCDCMetaObjects:
					errorMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
					if (isHostedWithCargoWise)
					{
						throw new HostedServiceException(errorMessage, ex) { LogException = true };
					}
					else
					{
						ServiceLogger.Log(LogType.Error, errorMessage);
						break;
					}
				default:
					throw ex;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC capture job name, error msg")]
		void HandleCdcCaptureJobError(CancellationToken token, SqlException ex)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				ServiceLogger.Log(LogType.Debug, "Checking CDC capture job status.");
				string jobType = "capture";
				var captureJobEnabled = CdcDatabase.IsCdcJobEnabled(adminConnection, Db.DatabaseName, jobType);
				var captureJobRunning = CdcDatabase.IsCdcJobRunning(adminConnection, Db.DatabaseName, jobType);

				if (captureJobEnabled)
				{
					CdcDatabase.DisableCdcJobs(adminConnection, Db.DatabaseName, jobType);
				}
				if (captureJobRunning)
				{
					CdcDatabase.StopCdcJobs(adminConnection, Db.DatabaseName, jobType);
				}

				var sleepInterval = TimeSpan.FromSeconds(1);
				var waitTime = TimeSpan.FromSeconds(30);
				while (waitTime >= sleepInterval)
				{
					captureJobEnabled = CdcDatabase.IsCdcJobEnabled(adminConnection, Db.DatabaseName, jobType);
					captureJobRunning = CdcDatabase.IsCdcJobRunning(adminConnection, Db.DatabaseName, jobType);

					if (!captureJobEnabled && !captureJobRunning)
					{
						break;
					}

					token.ThrowIfCancellationRequested();
					Thread.Sleep(sleepInterval);
					waitTime = waitTime.Subtract(sleepInterval);
				}

				if (captureJobEnabled)
				{
					ServiceLogger.Log(LogType.Warning, "Failed to disable CDC capture job.", ex);
				}
				else
				{
					ServiceLogger.Log(LogType.Debug, "CDC capture job disabled.");
				}

				if (captureJobRunning)
				{
					var errorMsg = "Failed to stop CDC capture job.";
					ServiceLogger.Log(LogType.Warning, errorMsg, ex);
				}

				else
				{
					ServiceLogger.Log(LogType.Debug, "CDC capture job stopped. Retrying CDC scan.");
				}
			}
		}

		void RetryCdcScan(CancellationToken token)
		{
			try
			{
				CaptureChangesCore(token);
			}
			catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc)
			{
				ServiceLogger.Log(LogType.Debug, "Another session is running change data capture. Retrying in 30 minutes.");
			}
		}

		protected virtual void CaptureChangesCore(CancellationToken token)
		{
			var shouldDeleteIsFirstCdcRunExtPropertyName = false;
			if (ExtProperty.Database.Select(Db.Connection, CdcDatabase.IsFirstCdcRunExtPropertyName) == "1")
			{
				Scanner.SetScanTimeout((int)TimeSpan.FromHours(6).TotalSeconds);
				shouldDeleteIsFirstCdcRunExtPropertyName = true;
			}

			long processedTranCount = Scanner.ScanUntilNoTransactionsToProcess(() => ShouldQuit(token));

			if (processedTranCount > 0)
			{
				ServiceLogger.Log(LogType.Debug, String.Format(CultureInfo.InvariantCulture, "{0} transactions processed.", processedTranCount.ToString(CultureInfo.InvariantCulture)));
				NudgeAuditEtlTask();
			}

			if (shouldDeleteIsFirstCdcRunExtPropertyName)
			{
				ExtProperty.Database.Delete(Db.Connection, CdcDatabase.IsFirstCdcRunExtPropertyName);
			}
		}

		void NudgeAuditEtlTask()
		{
			ServiceLogger.Log(LogType.Debug, "Nudging " + AuditEtlExecutionTask.ServiceTaskName);
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(AuditEtlExecutionTask.ServiceTaskCode);
		}

		#endregion

		#region Auxiliary Methods

		bool ShouldQuit(CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				Thread.MemoryBarrier();
				return true;
			}
			return false;
		}

		#endregion

	}
}
