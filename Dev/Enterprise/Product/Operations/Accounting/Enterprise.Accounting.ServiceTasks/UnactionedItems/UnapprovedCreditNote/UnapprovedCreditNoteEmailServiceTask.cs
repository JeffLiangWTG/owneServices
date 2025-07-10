using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UnapprovedCreditNoteEmailServiceTask.Code,
	"Credit Note Approvals Notification Email",
	"ACC",
	typeof(UnapprovedCreditNoteEmailServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ServiceTasks
{
	public class UnapprovedCreditNoteEmailServiceTask : ServiceProviderImpl
	{
		public const string Code = "UCN";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Credit Note Approvals Email Notification service task started.");
			try
			{
				token.ThrowIfCancellationRequested();
				RunTaskCore();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Credit Note Approvals Email Notification service task ended abruptly.\r\nException: {0}\r\nException Message: {1}.", ex.GetType(), ex.Message));
				if (ex is InvalidOperationException || ex is NullReferenceException)
				{
					ErrorReporter.ReportOnce("C95659F3-7E3E-46D2-82FC-00E134BA1502", "UCN Service Task Invalid Behavior Error", ex);
				}
			}
			ServiceLogger.Log(LogType.Information, "Credit Note Approvals Email Notification service task completed.");
		}

		protected virtual void RunTaskCore()
		{
			new UnapprovedCreditNoteEmailNotificationProcessor(ServiceLogger).SendEmail();
		}
	}
}
