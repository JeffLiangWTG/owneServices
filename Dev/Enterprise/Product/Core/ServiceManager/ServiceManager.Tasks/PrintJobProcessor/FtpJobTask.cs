using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"FDD",
	"FTP documents delivery",
	"DOC",
	typeof(FtpJobTask),
	ConfigControlType = typeof(FtpJobConfigControl),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("FDD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=FTP" }, "FTP documents delivery")]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class FtpJobTask : PrintJobTaskCore, IServiceTaskConfigurationUser
	{
		public override void RunTask(CancellationToken token)
		{
			RunTaskForJobTypes(requiresPrintServer: false, token, PrintJobType.FTP);
		}

#if DEBUG
		public void LogTesting(TraceEventType eventType, string message)
		{
			Log(eventType, message);
		}
#endif

		protected override void Log(TraceEventType eventType, string message)
		{
			base.Log(eventType, message);

			if ((eventType == TraceEventType.Error || eventType == TraceEventType.Warning) && CurrentProcessingJob != null)
			{
				using (((IBusinessObjectInternals)CurrentProcessingJob).SuppressReportRowDeletedError())
				{
					if (CurrentReportScheduleTask != null)
					{
						CurrentReportScheduleTask.Logs.AddNew(AutoEvents.DocumentNotDelivered, StmALog.GenerateEventReferenceToFitInReferenceMaxLength(message, new[] { new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Failed) }));
						CurrentReportScheduleTask.Factory.Save();
					}
				}
			}
		}

		protected override void Notify(bool successful, string message)
		{
			if (successful && FtpJobConfig.NotifyOnSuccess && CurrentProcessingJob != null)
			{
				NotifySuccess();
			}
			else if (!successful && FtpJobConfig.NotifyOnFailure)
			{
				NotifyFailure(message);
			}

			base.Notify(successful, message);
		}

		StmPrintJob CurrentProcessingJob => PrintJobManager.CurrentProcessingJob;

		ReportScheduleTask CurrentReportScheduleTask
		{
			get
			{
				if (CurrentProcessingJob != null)
				{
					if (currentReportScheduleTask == null || currentReportScheduleTask.PK != CurrentProcessingJob.SP_ParentGuid)
					{
						var factory = new BusinessObjectFactory();
						currentReportScheduleTask = factory.Load<ReportScheduleTask>(CurrentProcessingJob.SP_ParentGuid);
					}

					return currentReportScheduleTask;
				}

				return null;
			}
		}

		ReportScheduleTask currentReportScheduleTask;

		void NotifySuccess()
		{
			var email = new EmailDef
			{
				Subject = Res.GetString("E1D5E390-08EA-4D14-BF7B-E1F9B99BF8E0", "Scheduled Report FTP delivery successfully"),
				Body = Res.GetString("BB939488-6BD9-4D4D-ACD4-66DB7394F19E", "Scheduled report '{0}' has been successfully uploaded to the FTP server.", CurrentReportScheduleTask?.S5_ScheduleDescription)
			};
			SendNotification(email);
		}

		void NotifyFailure(string reason)
		{
			var email = new EmailDef
			{
				Subject = Res.GetString("42B0D886-95CC-4A89-8B71-C609EF4D74CD", "Scheduled Report FTP delivery failure"),
				Body = Res.GetString("328C71DA-983B-4F0D-8A4E-02D03D19C1C9", @"FTP delivery for scheduled report '{0}' has failed due to the following:
{1}", currentReportScheduleTask?.S5_ScheduleDescription, reason)
			};
			SendNotification(email);
		}

		void SendNotification(EmailDef email)
		{
			try
			{
				if (FtpJobConfig.NotifyPrintUser && CurrentReportScheduleTask != null)
				{
					email.AddRecipientForUserCommunication(CurrentReportScheduleTask.PrintUser.GS_EmailAddress);
				}

				if (FtpJobConfig.NotificationGroup != null)
				{
					Env.OutgoingMailManager.CreateAndSave(email, FtpJobConfig.NotificationGroup_PK.ToGuid(), GroupSourceLocator.GetFromGroup(FtpJobConfig.NotificationGroup));
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
			catch (EmailHasNoRecipientsException e)
			{
				Log(TraceEventType.Error, Res.GetString("C3F38BC8-3FD4-4277-A651-39AFFB2D336E", "Failure to send notification email: Could not find a recipient for group ({0}) or Print User ({1}). Reason: {2}", FtpJobConfig.NotificationGroup?.GG_Code, CurrentReportScheduleTask?.PrintUser?.GS_Code, e.Message));
			}
		}

		public string ConfigString { get; set; }

		public FtpJobConfig FtpJobConfig => new (ConfigString);
	}
}
