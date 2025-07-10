using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AutoJobStatusUpdateProcessor
	{
		public AutoJobStatusUpdateProcessor(ILogger logger)
			: this(logger, null, null)
		{
		}

		internal AutoJobStatusUpdateProcessor(ILogger logger, IJCSJobProvider jobProvider, IBusinessObjectFactoryProviderForJCS businessObjectFactoryProvider)
		{
			this.serviceLogger = Argument.NotNull(logger, nameof(logger));
			this.jobProvider =  jobProvider ?? new JCSJobProvider();
			this.businessObjectFactoryProvider = businessObjectFactoryProvider ?? new BusinessObjectFactoryProviderForJCS();
		}

		readonly ILogger serviceLogger;

		readonly IJCSJobProvider jobProvider;

		readonly IBusinessObjectFactoryProviderForJCS businessObjectFactoryProvider;

		public void Process(CancellationToken token)
		{
			if (CompaniesWithConfigurations.Any())
			{
				if (ProcessJobQueue(token))
				{
					Logger.LogDebug("Job Status Update cycle completed.");
				}
				else
				{
					Logger.LogDebug("Job Status Update process aborted due to an error.");
				}
			}
		}

		bool ProcessJobQueue(CancellationToken token)
		{
			var result = false;

			using (var contextProvider = GetUserContextProvider())
			{
				try
				{
					Logger.LogDebug("Starting job status updating process.");

					var jobCount = 0;
					while (jobProvider.CanContinue(jobCount))
					{
						token.ThrowIfCancellationRequested();

						Logger.LogDebug("Checking queue for jobs.");
						var pickedJob = jobProvider.LoadJobPKFromQueue();

						if (pickedJob != null)
						{
							foreach (var updater in JobStatusUpdaters)
							{
								LoadJobAndUpdateStatus(contextProvider, pickedJob.Value, updater);
								if (!updater.CanChainToNextSubscriber())
								{
									break;
								}
							}
							jobCount++;
						}
						else if (jobProvider.CanTriggerQueuePopulation)
						{
							if (jobProvider.HasCounterReachedEndOfQueue())
							{
								Logger.LogDebug("All jobs in the queue have been processed. Attempting to repopulate the queue.");
								if (!PopulateQueueAndMoveWatermarkForward())
								{
									Logger.LogDebug("No more job for auto update status.");
									break;
								}
							}
						}
					}

					result = true;

					if (jobCount > 0)
					{
						Logger.LogInformation($"Job Status Update cycle completed.\r\nAssessed {jobCount} jobs for status update.");
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Logger.LogError(string.Format(CultureInfo.InvariantCulture, "Aborting updating Job status process. {0}\r\nStack trace: {1}", ex.Message, ex.StackTrace));
				}
				finally
				{
					jobProvider.UpdateRowNumberInRegistry();
				}
			}
			return result;
		}

		void LoadJobAndUpdateStatus(IJCSUserContextProvider contextProvider, PickedJob pickedJob, IJCSSubscriber updater)
		{
			for (int tryCount = 1; tryCount <= 3; tryCount++)
			{
				var processResult = LoadJobAndUpdateStatusCore(contextProvider, pickedJob, updater);
				switch (processResult)
				{
					case AutoJobClosureResult.Yield:
						return;
					case AutoJobClosureResult.Retry:
						break;
					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognized: {0}", processResult));
				}
			}
		}

		AutoJobClosureResult LoadJobAndUpdateStatusCore(IJCSUserContextProvider contextProvider, PickedJob pickedJob, IJCSSubscriber updater)
		{
			var newFactory = businessObjectFactoryProvider.GetNewBusinessObjectFactory($"JCS Factory For Job: {pickedJob.PK}");
			newFactory.SetContext(BusinessContext.JCLServiceTask);

			var filter = new ZQuery(JobHeaderSchema.PK, pickedJob.PK);
			filter.AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator: SQLComparisonOperator.NotEqual, JobHeaderStatus.Closed.Code);
			var job = newFactory.LoadTop1<Job>(filter);

			if (job != null)
			{
				Logger.LogDebug(FormattableString.Invariant($"[Row {pickedJob.RowNumber}][{updater.Code}]: Loaded job {job.JH_JobNum} into memory along with all related data"));

				contextProvider.SetUserContext(job.JH_GC, job.JH_GB);

				using (Env.StartContextSwitchTrace(new UserContextSwitchLogger()))
				using (Logger.SetLogPrefix(FormattableString.Invariant($"[{job.Company.GC_Code}][{job.JH_JobNum}][{updater.Code}]: ")))
				{
					if (updater.CanProcess(job))
					{
						try
						{
							if (updater.Process(job, businessObjectFactoryProvider.SaveChanges) && businessObjectFactoryProvider.SaveChanges)
							{
								Logger.LogDebug("Attempting to save changes.");
								newFactory.Save();
								Logger.LogDebug("Successfully saved changes.");
							}
						}
						catch (OnSavingCriticalCheckException ex)
						{
							HandleCriticalValidationException(ex);
						}
						catch (ZSaveConcurrencyException ex)
						{
							Logger.LogDebug(FormattableString.Invariant($"Concurrency exception occurred while saving.\r\nError Details:\r\n{ex.Message}"));
							return AutoJobClosureResult.Retry;
						}
						catch (ZSaveException ex) when (ex.IndexNameIfUniqueIndexViolation == JobHeaderSchema.Constants.Indexes.NR_UX__JH_JobNum_JH_GC)
						{
							HandleUniqueJobNumberByCompanyIndexViolation(ex);
						}
						catch (Exception ex) when (ex?.Message.Contains((NoResString)"Cannot close the job when new profit share charges are created automatically. After you close and re-open the form, please re-enter the data, save all changes before closing the job.") ?? false)
						{
							var errorMessage = ex.GetExceptionMessageAndStackTrace();
							Logger.LogDiagnostic(FormattableString.Invariant($"Job could not be closed due to the following error. You can try manually close the job from Job Management module.\r\nError: {errorMessage}"));
							//We want to investigate why this exception is not caught by catch block for OnSavingCriticalCheckException. Hence we are reporting this exception as an issue.
							ErrorReporter.ReportOnce("ProfitShareChargesCreatedByJCS"
								, FormattableString.Invariant($"[{job.Company.GC_Code}][{job.JH_JobNum}][{updater.Code}]: {ex.Message}")
								, ex);
						}
					}
					else
					{
						Logger.LogDiagnostic("This job is not eligible for automatic status update");
					}
				}
			}
			return AutoJobClosureResult.Yield;
		}

		void HandleUniqueJobNumberByCompanyIndexViolation(ZSaveException ex)
		{
			var helper = ObjectFactory.Get<IJobNumUniqueIndexErrorHelper>();
			var message = helper.GetErrorMessage(ex);
			if (!message.IsNullOrEmpty())
			{
				Logger.LogError(message);
			}
			else
			{
				Logger.LogError(ex.Message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Logs should be in English Only")]
		void HandleCriticalValidationException(OnSavingCriticalCheckException ex)
		{
			var errorReportID = (ex as IHasErrorReportID)?.ErrorReportID ?? string.Empty;
			var errorTextForLog = new StringBuilder();
			if (!string.IsNullOrEmpty(errorReportID))
			{
				errorTextForLog.AppendLine(FormattableString.Invariant($"A critical validation exception is reported with error ID: {errorReportID}"));
			}
			errorTextForLog.AppendLine("Error Details:->");
			errorTextForLog.AppendLine(ex.Message);
			Logger.LogDebug(errorTextForLog.ToString());
		}

		bool PopulateQueueAndMoveWatermarkForward()
		{
			var result = false;

			Db.Connection.RunLocked(key: "JCSQueueStatus", //Check whether it uses Transaction
				max_tries: 1,
				process: (isFirstRun) =>
				{
					if (WatermarkUpdater.CanMoveForwardWatermark)
					{
						Logger.LogDebug("Truncating the queue table before populating it again.");
						QueueResetter.Reset();

						Logger.LogDebug("Populating queue...");
						var currentWatermarkDate = AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.Value;
						result = QueuePopulator.Populate(currentWatermarkDate, CompaniesWithConfigurations);

						Logger.LogDebug("Moving watermark forward.");
						WatermarkUpdater.Update();
						jobProvider.Reset();
					}
				});

			return result;
		}

		IEnumerable<IJCSSubscriber> JobStatusUpdaters => jobStatusUpdaters ??
			(jobStatusUpdaters = new IJCSSubscriber[]
										{
											new DSBJobBatcher(logger),
											new JobStatusUpdaterForJFC(Logger),
											new JobStatusUpdaterForClosingJobs(Logger)
										});
		IEnumerable<IJCSSubscriber> jobStatusUpdaters;

		IJCSUserContextProvider GetUserContextProvider() => new JCSUserContextProvider();

		IJobQueueResetter QueueResetter => queueResetter ?? (queueResetter = new JobQueueResetter());
		IJobQueueResetter queueResetter;

		IJCSWatermarkUpdater WatermarkUpdater => watermarkUpdater ?? (watermarkUpdater = new JCSWatermarkUpdater());
		IJCSWatermarkUpdater watermarkUpdater;

		IJCSQueuePopulator QueuePopulator => queuePopulator ?? (queuePopulator = new JCSQueuePopulator());
		IJCSQueuePopulator queuePopulator;

		IEnumerable<ZGuid> CompaniesWithConfigurations => companiesWithConfigurations ?? (companiesWithConfigurations = AutoJobClosureConfigurationFinder.GetCompanyPKsThatHaveAutoJoClosureConfiguration());
		IEnumerable<ZGuid> companiesWithConfigurations;

		IJCSLogger Logger => logger ?? (logger = new JCSLogger(serviceLogger));
		IJCSLogger logger;

		enum AutoJobClosureResult
		{
			Yield = 0,
			Retry = 1,
		}

#if DEBUG
		public void SubstituteWatermarkUpdater_ForTestOnly(IJCSWatermarkUpdater substituteWatermarkUpdater)
		{
			watermarkUpdater = substituteWatermarkUpdater;
		}

		public void SubstituteQueuePopulator_ForTestOnly(IJCSQueuePopulator substituteQueuePopulator)
		{
			queuePopulator = substituteQueuePopulator;
		}

		public void SubstituteLogger_ForTestOnly(IJCSLogger substituteLogger)
		{
			logger = substituteLogger;
		}

		public void SubstituteJobStatusUpdaters_ForTestOnly(IJCSSubscriber[] substituteobStatusUpdaters)
		{
			jobStatusUpdaters = substituteobStatusUpdaters;
		}
#endif
	}
}
