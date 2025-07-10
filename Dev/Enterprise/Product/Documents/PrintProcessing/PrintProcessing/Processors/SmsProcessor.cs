using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.PrintProcessing
{
	/// <summary>
	/// This class is tested via the PrintJobManager. If you modify behaviour here,
	/// please ensure the relevant testing is done in the PrintJobManager.
	/// </summary>
	class SmsProcessor : MergedPrintGroupProcessor
	{
		public SmsProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate progressDelegate)
			: base(mergedPrintGroup)
		{
			LogProgress = progressDelegate;

			#region Testing
#if DEBUG
			SmsProcessor.IsProcessStartedForTesting = true;
#endif
			#endregion
		}

		protected override void PreProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
			base.PreProcess(printJobs, groupNumber, totalGroupCount);

			failedToSendSmsJobs = new List<StmPrintJob>();
		}

		protected override void ProcessIndividualItemCore(StmPrintJob job)
		{
			Sms sms = new Sms(job.SP_FaxDestination, job.SP_CustomProperties.ToAscii());
			SmsSendResult sendResult = Sender.SendImmediately(sms);

			if (sendResult.Success)
			{
				Log(TraceEventType.Information, Res.GetString("7bc493d9-fe80-4599-9ae3-8d54ace25e21", "SMS sent. SMS Number: {0}. SMS Message: {1}.", sms.PhoneNumbers[0], sms.Message));
			}
			else
			{
				job.SP_RetryAttempts++;
				job.SP_RunDateTime = ZDateTime.UtcNow.AddSeconds(RetryTimeoutInSeconds);

				string sendFailureMsg = Res.GetString("e2331a30-f79e-4205-870f-d082e5a19963", "SMS send failed, attempt {0}. SMS Number: {1}. SMS Message: {2}. Error Message: {3}",
					job.SP_RetryAttempts, sms.PhoneNumbers[0], sms.Message, sendResult.Message);
				Log(TraceEventType.Error, sendFailureMsg);

				if (job.SP_RetryAttempts >= PrintJobManager.MaxRetryAttempts && job.SP_FailureReason.IsEmpty)
				{
					job.SP_FailureReason = new ZString(sendFailureMsg).SubstringSafe(0, AutoStmPrintJob.Schema.SP_FaxDestinationMaxLength);
				}

				failedToSendSmsJobs.Add(job); // retry failed send on the next run
			}
		}

		protected override void PostProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
			base.PostProcess(printJobs, groupNumber, totalGroupCount);

			// remove jobs that were not successfully delivered
			foreach (StmPrintJob sentSmsJob in failedToSendSmsJobs)
			{
				printJobs.Remove(sentSmsJob);
			}
		}

		List<StmPrintJob> failedToSendSmsJobs;

		#region Implementation

		public const int RetryTimeoutInSeconds = 300;

		SmsSender Sender
		{
			get
			{
				if (fSender == null)
				{
					fSender = SmsSender.New();
				}
				return fSender;
			}
		}

		SmsSender fSender;

#if DEBUG
		internal static bool IsProcessStartedForTesting;
#endif

		#endregion

		#region Logging

		void Log(TraceEventType eventType, string message)
		{
			LogProgress?.Invoke(eventType, message);
		}

		#endregion
	}
}
