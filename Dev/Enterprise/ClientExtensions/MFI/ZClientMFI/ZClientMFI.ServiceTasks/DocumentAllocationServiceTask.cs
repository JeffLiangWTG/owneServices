using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.MFI;
using Enterprise.Client.MFI.AutoeDocAllocation;
using Enterprise.Client.MFI.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MFIConstants.ServiceTasks.DocumentAllocation,
	"Document Allocation",
	"CSP",
	typeof(DocumentAllocationServiceTask),
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]
namespace Enterprise.Client.MFI.ServiceTasks
{
	public class DocumentAllocationServiceTask : ServiceProviderImpl
	{
		#region Service Task Imlp

		public override void RunTask(CancellationToken token)
		{
			if (AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory))
			{
				Run(token);
			}
		}

		#endregion

		#region Implementation

		protected virtual void Run(CancellationToken token)
		{
			DocumentAllocator docAllocator = new HoldDocumentAllocator();
			docAllocator.AllocateFiles(Notify, token);

			docAllocator = new SourceDocumentAllocator();
			docAllocator.AllocateFiles(Notify, token);
		}

		protected virtual NotificationBuffer Notify
		{
			get { return notify ?? (notify = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber())); }
		}
		NotificationBuffer notify;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
