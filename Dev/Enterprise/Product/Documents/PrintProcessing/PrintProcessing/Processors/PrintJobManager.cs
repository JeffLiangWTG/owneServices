using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PrintProcessing
{
	public class PrintJobManager : IPrintJobManager
	{
		#region Process

		public StmPrintJob CurrentProcessingJob { get; internal set; }

		public void ProcessPrintJobs(StmPrintJob[] jobs)
		{
			foreach (List<StmPrintJob> jobsOfBranchGroup in GroupJobsByBranch(jobs))
			{
				if (jobsOfBranchGroup.Count > 0)
				{
					using (DisposableEnvironment.ForBranch(jobsOfBranchGroup[0].ProperBranchPK.ToGuid()))
					{
						try
						{
							ProcessPrintJobsCore(jobsOfBranchGroup);
						}
						catch (Exception ex) when (!ex.IsCriticalException() && ex is not DatabaseConnectionClosedException)
						{
							ErrorReporter.ReportDeveloperExceptionOnce($"ProcessPrintJobs_{ex.GetType().Name}", "Exception was thrown out when processing print jobs. Exception message: " + ex.Message , ex);
						}
					}
				}
			}
		}

		IEnumerable<List<StmPrintJob>> GroupJobsByBranch(IEnumerable<StmPrintJob> jobs)
		{
			Dictionary<ZGuid, List<StmPrintJob>> jobOfBranchGroups = new Dictionary<ZGuid, List<StmPrintJob>>();

			foreach (StmPrintJob job in jobs)
			{
				if (!jobOfBranchGroups.ContainsKey(job.SP_GB))
				{
					jobOfBranchGroups.Add(job.SP_GB, new List<StmPrintJob>());
				}
				jobOfBranchGroups[job.SP_GB].Add(job);
			}

			return jobOfBranchGroups.Select(jobOfBranchGroup => jobOfBranchGroup.Value);
		}

		internal virtual void ProcessPrintJobsCore(IEnumerable<StmPrintJob> jobs)
		{
			var printJobsGroupedByRecipient = StmPrintJobGroupCollection.MergePrintJobsByRecipient(jobs);
			if (printJobsGroupedByRecipient.Count > 0)
			{
				var queuedItem = 1;
				var totalJobCount = printJobsGroupedByRecipient.Count;

				foreach (StmPrintJobMergedCollection mergedPrintJobs in printJobsGroupedByRecipient)
				{
					var pks = mergedPrintJobs.Select(j => j.PK).ToList();
					LogProgress(TraceEventType.Information, Res.GetString("a173bc19-5705-4b86-8409-23e51e25c88f", "Processing {0} of {1} queued {2} jobs", queuedItem++, totalJobCount, mergedPrintJobs.JobType));
					try
					{
						PrepareAttachmentsForDelivery(mergedPrintJobs);
						DeliverPrintJobs(mergedPrintJobs);
						mergedPrintJobs.DeleteStoredAttachments();
						NotifyPrintJobsDelivered(mergedPrintJobs);
						mergedPrintJobs.Factory.Save();
					}
					catch (ZCannotSaveException cannotSaveException) when (string.Equals(cannotSaveException.Heading, (NoResString)"Document tracking concurrency error")) // Exception string used to compare, no reason to create Res.GetString
					{
						BumpUpRetryAttemptsInAnotherFactory(pks, cannotSaveException.Message);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException())
						{
							BumpUpRetryAttemptsInAnotherFactory(pks, exception.Message);
							throw;
						}

						if (exception is DatabaseConnectionClosedException)
						{
							mergedPrintJobs.DeleteStoredAttachments();
							throw;
						}

						new PrintJobManagerExceptionHandler(LogProgress).HandlePrintException(mergedPrintJobs, exception);

						try
						{
							mergedPrintJobs.Factory.Save();
						}
						catch (Exception ex) when (ex is ZConcurrencyCheckFailureException or ZSaveConcurrencyException)
						{
							var filters = new ZQuery();
							filters.AddToFilter(StmPrintJobSchema.SP_Status, SQLComparisonOperator.NotEqual, nameof(PrintJobStatus.FAL));
							filters.AddToFilter(StmPrintJobSchema.SP_JobType, SQLComparisonOperator.NotEqual, nameof(PrintJobType.DDS));

							BumpUpRetryAttemptsInAnotherFactory(pks, ex.Message, filters);
						}
						catch (NoConcreteTypeException noConcreteTypeException)
						{
							ErrorReporter.ReportOnce("ErrorFactorySavingWhenProcessingPrintJobs",
								string.Format(CultureInfo.InvariantCulture,
									"NoConcreteTypeException occurs in Factory.Save() when processing PrintJobs, See Job Details: \r\n {0}",
									PrintJobManagerExceptionHandler.GetJobDetails(mergedPrintJobs, true)), noConcreteTypeException);

							var sql = string.Format(CultureInfo.InvariantCulture, "UPDATE dbo.StmPrintJob SET {0} = '{1}' WHERE {2} IN ({3})",
								StmPrintJobSchema.Constants.SP_Status, PrintJobStatus.FAL, StmPrintJobSchema.Constants.PK,
								string.Join(",", mergedPrintJobs.Select(job => "'" + job.PK + "'")));

							using (var cmd = Db.Connection.Command(sql)) // Factory.Save() is broken here
							{
								cmd.ExecuteNonQuery();
							}
						}
					}
				}
			}
		}

		void BumpUpRetryAttemptsInAnotherFactory(IEnumerable<ZGuid> pks, string message, ZQuery additionalFilters = null)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddToFilter(StmPrintJobSchema.PK, pks);

			if (additionalFilters != null)
			{
				query.AddToFilter(additionalFilters);
			}

			var failureReason = new ZString(message).SubstringSafe(0, AutoStmPrintJob.Schema.SP_FaxDestinationMaxLength);

			var newLoadedJobs = factory.Load<StmPrintJob>(query);

			newLoadedJobs.ForEach(j =>
			{
				j.SP_RetryAttempts++;
				if (j.SP_RetryAttempts >= MaxRetryAttempts && j.SP_FailureReason.IsEmpty)
				{
					j.SP_FailureReason = failureReason;
				}
			});

			factory.Save();
		}

		protected virtual void DeliverPrintJobs(StmPrintJobMergedCollection mergedPrintGroup)
		{
			if (mergedPrintGroup.Count > 0 && mergedPrintGroup.RetryAttempts < MaxRetryAttempts)
			{
				MergedPrintGroupProcessor processor = GetProcessor(mergedPrintGroup, LogProgress);

				if (processor != null)
				{
					LogProgress(TraceEventType.Information, Res.GetString("25b2d8bd-da40-4d87-820b-3352bc62a8f7",
						"Starting {0} to process \"{1}\" print jobs", processor.GetType().Name, mergedPrintGroup.JobType.ToUpper()));
					processor.Process();
				}
				else
				{
					LogProgress(TraceEventType.Warning, Res.GetString("4ed68634-8936-4977-a76b-8d3545ee5ba5",
						"No print job processor has been found for \"{0}\" print jobs", mergedPrintGroup.JobType.ToUpper()));
				}
			}
		}

		protected virtual void NotifyPrintJobsDelivered(StmPrintJobMergedCollection mergedPrintJobs)
		{
			mergedPrintJobs.NotifyAllJobsDelivered();
		}

		public const int MaxRetryAttempts = 3;

		internal virtual MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection mergedPrintGroup, ProgressDelegate logProgress)
		{
			return ProcessorFactory.GetProcessor(mergedPrintGroup, logProgress, this, RunNotification);
		}

		#endregion

		#region Purge / Delete

		public void PurgeOld()
		{
			DateTime purgeDate = ZDateTime.Now.AddDays(-14).ToDateTime();

			ZQuery query = new ZQuery();
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.LessThanOrEqualTo, purgeDate);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			Delete(factory.Load<StmPrintJob>(query));
		}

		public virtual void Delete(StmPrintJob[] jobs)
		{
			if (jobs.Length == 0)
			{
				return;
			}

			BusinessObjectFactory jobFactory = jobs[0].Factory;
			foreach (StmPrintJob job in jobs)
			{
				job.Delete();
			}

			try
			{
				Save(jobFactory);
			}
			catch (ZSaveConcurrencyException) { }
		}

		internal virtual void Save(BusinessObjectFactory bof)
		{
			bof.Save();
		}

		#endregion

		#region Progress Logging

		public delegate void ProgressDelegate(TraceEventType eventType, string statusMessage);
		public ProgressDelegate OnProgress;

		void LogProgress(TraceEventType eventType, string message)
		{
			OnProgress?.Invoke(eventType, message);
		}

		public delegate void NotificationDelegate(bool successful, string notificationMessage);
		public NotificationDelegate OnNotification;

		void RunNotification(bool successful, string notificationMessage)
		{
			OnNotification?.Invoke(successful, notificationMessage);
		}

		#endregion

		#region Processor Factory

#if DEBUG
		internal
#endif
		static class ProcessorFactory
		{
			static readonly bool isSmsSenderRegistered = SmsSender.New() != null;

			static bool IsSmsSenderRegistered()
			{
				// if we are running in a test, don't use the cached evaluation
				return
#if DEBUG
					Globals.IsTest ? (SmsSender.New() != null) :
#endif
					isSmsSenderRegistered;
			}

			public static MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection mergedPrintGroup, ProgressDelegate logProgress, PrintJobManager jobManager = null, NotificationDelegate runNotification = null)
			{
				switch ((PrintType)Enum.Parse(typeof(PrintType), mergedPrintGroup.JobType, true))
				{
					case PrintType.EML:
						return new EmailProcessor(mergedPrintGroup, logProgress);
					case PrintType.FAX:
						return new FaxProcessor(mergedPrintGroup, logProgress);
					case PrintType.PRN:
					case PrintType.PRS:
						return new HardCopyProcessor(mergedPrintGroup, ObjectFactory.New<IPrinterFactory>());
					case PrintType.SMS:
						if (IsSmsSenderRegistered())
						{
							return new SmsProcessor(mergedPrintGroup, logProgress);
						}
						break;
					case PrintType.FTP:
						return new FtpJobProcessor(mergedPrintGroup, logProgress, runNotification, jobManager);
				}
				return null;
			}
		}

		#endregion

		#region Attachment Handling

		/// <summary>
		/// Converts all the print jobs to appropriate formats ready for delivery and saves them on the filesystem. 
		/// </summary>
#if DEBUG
		public
#endif
		void PrepareAttachmentsForDelivery(StmPrintJobCollection printJobs)
		{
			var htmlJobs = printJobs.Cast<StmPrintJob>().Where(job => job.SP_EmailAttachmentFormat == OrgConstants.AttachmentType.HTML);
			if (htmlJobs.Any())
			{
				var firstHtmlJob = htmlJobs.First();
				firstHtmlJob.SP_CustomProperties = DocumentConverter.MergeXLSs(htmlJobs.Select(job => job.SP_CustomProperties));
				SaveAttachmentToFilesystem(firstHtmlJob, 0);

				printJobs.Cast<StmPrintJob>().Where(job => job.SP_EmailAttachmentFormat != OrgConstants.AttachmentType.HTML).ForEach(job => SaveAttachmentToFilesystem(job, 1));
			}
			else
			{
				for (var i = 0; i < printJobs.Count; ++i)
				{
					var printJob = printJobs[i];
					if (printJob.IsDeliveredExternally)
					{
						SaveAttachmentToFilesystem(printJob, i);
					}
				}
			}
		}

		/// <summary>
		/// Saves the blob in the print job to a file on the file system.
		/// The file is converted to the final format which it will be delivered (e.g. TIF for fax, or the user specified type if email)
		/// </summary>
		void SaveAttachmentToFilesystem(StmPrintJob printJob, int attachmentNumber)
		{
			RunBeforeSwitchingBlobTypeForTesting();
			printJob.SaveAttachmentToFilesystem(attachmentNumber);
			printJob.ErrorsWhenConverting.ForEach(e =>
			{
				LogProgress(TraceEventType.Error, e.message + (NoResString)"\r\nException Details:\r\n" + e.exception); // No need to be translated
			});
			printJob.ErrorsWhenConverting.Clear();
		}

		[Conditional("DEBUG")]
		protected virtual void RunBeforeSwitchingBlobTypeForTesting()
		{
		}

		#endregion
	}
}
