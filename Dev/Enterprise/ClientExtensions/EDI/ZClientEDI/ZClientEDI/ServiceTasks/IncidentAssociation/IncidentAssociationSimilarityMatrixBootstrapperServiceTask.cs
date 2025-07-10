using System.Threading;
using CargoWise.EntityFramework;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.IncidentAssociationSimilarityMatrixBootstrapperServiceTask.Code,
	"Incident Association Matrix Bootstrapper",
	"SYS",
	typeof(Enterprise.Client.EDI.IncidentAssociationSimilarityMatrixBootstrapperServiceTask),
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "3minutes",
	MaximumPeriod = "2hours",
	DefaultScheduleRunEvery = "5minutes"
	)]

namespace Enterprise.Client.EDI
{
	public class IncidentAssociationSimilarityMatrixBootstrapperServiceTask : ServiceProviderImpl
	{
		public const string Code = "IAM";

		public override void RunTask(CancellationToken cancellationToken)
		{
			new IncidentAssociationSimilarityMatrixBootstrapperRunner(ServiceLogger, new BusinessObjectFactory())
				.Run(IncidentAssociationSimilarityMatrixBootstrapperRunner.StrideLength);
		}
	}
}
