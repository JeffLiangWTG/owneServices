using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UnapprovedPaymentEmailServiceTask.Code,
	"Payment Approvals Notification Email",
	"ACC",
	typeof(UnapprovedPaymentEmailServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ServiceTasks
{
	public class UnapprovedPaymentEmailServiceTask : ServiceProviderImpl
	{
		public const string Code = "UPA";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Payment Approvals Notification Email service task started.");
			try
			{
				token.ThrowIfCancellationRequested();
				RunTaskCore();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Payment Approvals Notification Email service task ended abruptly.\r\nException: {0}\r\nException Message: {1}.", ex.GetType(), ex.Message));
				if (ex is InvalidOperationException || ex is NullReferenceException)
				{
					ErrorReporter.ReportOnce("6f6b3dfa-f550-4856-86ac-4cdcb95a1f40", "UPA Service Task Invalid Behavior Error", ex);
				}
			}
			ServiceLogger.Log(LogType.Information, "Payment Approvals Notification Email service task completed.");
		}

		protected virtual void RunTaskCore()
		{
			new UnapprovedPaymentEmailNotificationProcessor(ServiceLogger).SendEmail();
		}
	}
}
