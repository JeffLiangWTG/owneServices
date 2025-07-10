using System.Threading;
using Enterprise.Client.YAS.Business.ProofOfDeliveryInterface;
using Enterprise.Client.YAS.ServiceTasks.ProofOfDeliveryInterface;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PODDataImportServiceTask.Code,
	"Import Proof Of Delivery",
	"CSP",
	typeof(PODDataImportServiceTask),
	MinimumPeriod = "1Miniute",
	MaximumPeriod = "1Day",
	DefaultScheduleRunEvery = "5minutes"
	)]
namespace Enterprise.Client.YAS.ServiceTasks.ProofOfDeliveryInterface
{
	class PODDataImportServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ImportProcessor.Execute(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		PODImportProcessor ImportProcessor
		{
			// Because this will run frequently, it is lazy loaded.
			get { return importProcessor ?? (importProcessor = new PODImportProcessor()); }
		}
		PODImportProcessor importProcessor;

		public const string Code = "ZY4";
	}
}
