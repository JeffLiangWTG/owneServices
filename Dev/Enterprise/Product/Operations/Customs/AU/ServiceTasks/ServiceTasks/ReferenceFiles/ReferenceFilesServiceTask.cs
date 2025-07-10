using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.AU.ServiceTasks.ReferenceFilesServiceTask.Code,
	Enterprise.Customs.AU.ServiceTasks.ReferenceFilesServiceTask.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.ReferenceFilesServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "30minutes"
	)]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class ReferenceFilesServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "AUR";
		public const string Name = "Australian Customs Reference File Retriever";
		const string CMRReferenceFileMode = "CMRReferenceFileMode";
		const string ReferenceFileModeTest = "TEST";
		const string ReferenceFileModeProduction = "PROD";

		public bool CustomsEnvironmentIsValid()
		{
			bool hasCustomsRegistration = false;
			string errorMessage = string.Empty;
			var certificateChecker = new CertificateChecker(new BusinessObjectFactory());

			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Australia))
			{
				if (!branch.Company.GC_CustomsRegistrationNo.IsEmpty)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						var companyKeyValid = certificateChecker.CheckCompanyKey(out var message, false);
						if (companyKeyValid == StabilityResultLevel.Healthy || companyKeyValid == StabilityResultLevel.Warning)
						{
							hasCustomsRegistration = true;
							break;
						}
						errorMessage += branch.Company.GC_Code + "->" + branch.GB_Code + ": " + message + System.Environment.NewLine;
					}
				}
			}

			if (!hasCustomsRegistration)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "There is currently no 'Customs>Australia>CMR>Company Certificate Key File' established for any AU Company. - CMR Reference File Downloader is not required." + errorMessage));
				return false;
			}

			return true;
		}

		protected override void RunTaskCore(CancellationToken token)
		{
			if (CustomsEnvironmentIsValid())
			{
				try
				{
					var factory = new BusinessObjectFactory();

					InitialiseAttributes(factory);
					CheckAndRunUpdate(factory);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Warning, ex.Message);
				}
			}
		}

		protected CustomsWebMediator webMediator;
		string cmrAuRefDbName;

		enum UpdateStatus
		{
			NotRequired,
			LockedByAnotherSystem,
			PerformedSuccessfully,
		}

		protected void InitialiseAttributes(BusinessObjectFactory factory)
		{
			cmrAuRefDbName = GetCmrAuReferenceDatabaseName(Db.Connection);
			webMediator = new CustomsWebMediator(factory, ServiceLogger, IsCMRTestMode);
		}

		bool IsCMRTestMode => (AUCustomsDataRegistry.Instance.AlwaysUseIndustryTestCMRFiles.Value && !EnvProxy.Instance.IsProductionSystem)
								|| (Env.Registry.CMRTestMode && cmrAuRefDbName != null && !RefDbTableNameResolver.IsSharedDatabase(cmrAuRefDbName));

		protected internal virtual string GetCmrAuReferenceDatabaseName(IPhysicalRefDbLocation refdbLocator)
		{
			return refdbLocator.GetReferenceDatabaseName(RefDbTypeEnum.Customs, Core.Constants.CountryCodes.Australia);
		}

		void CheckAndRunUpdate(BusinessObjectFactory factory)
		{
			var branch = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Australia).FirstOrDefault()?.FirstActiveBranch;
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					ServiceLogger.Log(LogType.Debug, "Running as " + branch.GB_Code);
					if (GetCustomsLastModifiedTime(out var sourceLastModified))
					{
						var updateLog = new CMRReferenceFileUpdateLog(factory);
						try
						{
							RunUpdateIfRequired(sourceLastModified, updateLog);
						}
						catch (UpdateReferenceFilesException ex)
						{
							ServiceLogger.Log(LogType.Error, ex.Message);
							ErrorReporter.ReportOnce("Error while importing CMR Reference Files. The table has not been updated.", ex);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (!ex.IsCriticalException())
							{
								updateLog.LogUpdateFailure();
							}
							throw;
						}
					}
				}
			}
		}

		protected internal virtual bool GetCustomsLastModifiedTime(out DateTime lastModified)
		{
			return webMediator.GetWebSourceLastModifiedTime(out lastModified);
		}

		void RunUpdateIfRequired(DateTime sourceLastModified, CMRReferenceFileUpdateLog updateLog)
		{
			var fullUpdateStatus = UpdateStatus.NotRequired;
			using (var refDbCnx = Db.NewExtraConnectionToMainDb())
			{
				fullUpdateStatus = RunFullUpdateIfRequired(refDbCnx, sourceLastModified, updateLog);
			}

			LogUpdateResult(fullUpdateStatus, sourceLastModified);
		}

		void LogUpdateResult(UpdateStatus fullUpdateStatus, DateTime sourceLastModified)
		{
			if (fullUpdateStatus == UpdateStatus.PerformedSuccessfully)
			{
				ServiceLogger.Log(LogType.Information, "AU Customs Reference files updated - Last Modified = " + sourceLastModified.ToString(CultureInfo.CurrentCulture));
			}
			else if (fullUpdateStatus == UpdateStatus.LockedByAnotherSystem)
			{
				ServiceLogger.Log(LogType.Information, "Full update currently being performed by another system.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "Update not required - Last Modified = " + sourceLastModified.ToString(CultureInfo.CurrentCulture));
			}
		}

		#region Full AU CMR Update

		UpdateStatus RunFullUpdateIfRequired(DbConnection refDbCnx, DateTime sourceLastModified, CMRReferenceFileUpdateLog updateLog)
		{
#if DEBUG
			if (ZArchitecture.Environment.Globals.IsTest)
			{
				refDbCnx = Db.Connection;
			}
#endif

			var fullUpdateStatus = UpdateStatus.NotRequired;
			if (IsFullUpdateRequired(refDbCnx, sourceLastModified))
			{
				fullUpdateStatus = AcquireLockAndCheckAgainBeforeRunningFullUpdate(refDbCnx, sourceLastModified, updateLog);
			}

			return fullUpdateStatus;
		}

		/// <summary>
		/// Gets the last successful full data update timestamp from the CMR AU reference database
		/// and compares it with the last time the source data was updated.
		/// </summary>
		/// <returns>TRUE if source last update is later than last full update time</returns>
		bool IsFullUpdateRequired(DbConnection refDbCnx, DateTime sourceLastModified)
		{
			var referenceFileMode = DataUtils.LoadDbExtendedProperty(refDbCnx, CMRReferenceFileMode, cmrAuRefDbName);
			var requiredFileMode = webMediator.IsTestMode ? ReferenceFileModeTest : ReferenceFileModeProduction;
			if (referenceFileMode != requiredFileMode)
			{
				return true;
			}

			var lastSuccessfulUpdateStr = DataUtils.LoadDbExtendedProperty(refDbCnx, CMRReferenceFileUpdateLog.ReferenceTableName, cmrAuRefDbName);
			var lastSuccessfulUpdate = string.IsNullOrEmpty(lastSuccessfulUpdateStr)
				? DateTime.MinValue
				: SqlFormatInfo.FromSqlDateTime(lastSuccessfulUpdateStr);
			return sourceLastModified.Subtract(lastSuccessfulUpdate).TotalSeconds >= 1;
		}

		UpdateStatus AcquireLockAndCheckAgainBeforeRunningFullUpdate(DbConnection refDbCnx, DateTime sourceLastModified, CMRReferenceFileUpdateLog updateLog)
		{
			var fullUpdateStatus = UpdateStatus.NotRequired;
			using (var manager = refDbCnx.BeginTransactionWithManager())
			{
				if (AcquireUpdateLock(refDbCnx))
				{
					if (IsFullUpdateRequired(refDbCnx, sourceLastModified))
					{
						RunFullUpdate(refDbCnx, sourceLastModified, updateLog);
						fullUpdateStatus = UpdateStatus.PerformedSuccessfully;
					}
				}
				else
				{
					fullUpdateStatus = UpdateStatus.LockedByAnotherSystem;
				}

				manager.CommitTransaction();
			}

			return fullUpdateStatus;
		}

		/// <summary>
		/// CMR AU Reference Database can be shared amonst different Enterprise systems in the same server.
		/// So to control concurrency, an application lock is used as a semaphore control.
		/// </summary>
		bool AcquireUpdateLock(DbConnection refDbCnx)
		{
			return (CmrAuReferenceFileUpdateMutex.AcquireUpdateLock(refDbCnx, TimeSpan.FromSeconds(0)));
		}

		protected internal virtual void RunFullUpdate(DbConnection refDbCnx, DateTime sourceLastModified, CMRReferenceFileUpdateLog updateLog)
		{
			UpdateReferenceData();
			SetLastSuccessfulUpdateTime(refDbCnx, sourceLastModified);
			updateLog.LogUpdateSuccess(sourceLastModified);
		}

		void UpdateReferenceData()
		{
			byte[] gzipFileContents = webMediator.DownloadReferenceDataZippedFile();
			new ImportReferenceFileData(ServiceLogger).ImportData(gzipFileContents);
		}

		/// <summary>
		/// Sets the last successful data update timestamp in the CMR AU reference database
		/// </summary>
		protected void SetLastSuccessfulUpdateTime(DbConnection refDbCnx, DateTime sourceLastModified)
		{
			string sourceLastModifiedStr = SqlFormatInfo.ToSqlDateTimeString(sourceLastModified);
			DataUtils.SaveDbExtendedProperty(refDbCnx, CMRReferenceFileUpdateLog.ReferenceTableName, sourceLastModifiedStr, cmrAuRefDbName);
			var newReferenceFileModeValue = webMediator.IsTestMode ? ReferenceFileModeTest : ReferenceFileModeProduction;
			DataUtils.SaveDbExtendedProperty(refDbCnx, CMRReferenceFileMode, newReferenceFileModeValue, cmrAuRefDbName);
		}

		#endregion
	}
}
