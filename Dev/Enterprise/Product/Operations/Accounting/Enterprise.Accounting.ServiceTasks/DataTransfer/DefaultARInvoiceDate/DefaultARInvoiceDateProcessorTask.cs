using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ServiceTasks.DataTransfer.DefaultARInvoiceDate;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DefaultARInvoiceDateProcessorTask.Code,
	"Invoice Date Incrementing Suspension Notify Service",
	"ACC",
	typeof(DefaultARInvoiceDateProcessorTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour")
]

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DefaultARInvoiceDate
{
	internal class DefaultARInvoiceDateProcessorTask : ServiceProviderImpl
	{
		public const string Code = "IDN";

		public override void RunTask(CancellationToken token)
		{
			new DefaultARInvoiceDateNotifier(new BusinessObjectFactory()).Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
