using System.Threading;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.ELearningDocumentDescriptionServiceTask.Code,
	"eLearning document descriptions",
	"SYS",
	typeof(Enterprise.Client.EDI.ELearningDocumentDescriptionServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "2hours"
	)]
namespace Enterprise.Client.EDI
{
	public class ELearningDocumentDescriptionServiceTask : ServiceProviderImpl
	{
		public const string Code = "ELD";

		public override void RunTask(CancellationToken cancellationToken)
		{
			new ELearningDocumentDescriptionRunner(ServiceLogger).Run(cancellationToken, new RecentPdfUpdatesApiClient(ServiceLogger));
		}
	}
}
