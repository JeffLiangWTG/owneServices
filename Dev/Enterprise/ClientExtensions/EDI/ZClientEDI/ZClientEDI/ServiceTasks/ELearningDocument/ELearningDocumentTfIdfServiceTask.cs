using System.Threading;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.ELearningDocumentTfIdfServiceTask.Code,
	"eLearning document Tf-Idf",
	"SYS",
	typeof(Enterprise.Client.EDI.ELearningDocumentTfIdfServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "3hours"
	)]

namespace Enterprise.Client.EDI
{
	public class ELearningDocumentTfIdfServiceTask : ServiceProviderImpl
	{
		public const string Code = "EDT";

		public override void RunTask(CancellationToken cancellationToken)
		{
			new ELearningDocumentTfIdfRunner().Run(cancellationToken,
				ServiceLogger,
				new MyAccountShareFolderClient(ServiceLogger),
				(ILogger logger) => new PdfTextExtractor(logger),
				new TfIdfTransformer());
		}
	}
}
