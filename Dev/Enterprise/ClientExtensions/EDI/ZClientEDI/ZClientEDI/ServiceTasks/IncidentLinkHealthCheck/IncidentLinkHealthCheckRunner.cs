using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck
{
	public interface IIncidentLinkHealthCheckRunner
	{
		void Run(CancellationToken token);
	}
	public class IncidentLinkHealthCheckRunner : IIncidentLinkHealthCheckRunner
	{
		readonly ILogger logger;

		public IncidentLinkHealthCheckRunner(ILogger logger)
		{
			this.logger = logger;
		}

		public void Run(CancellationToken token)
		{
			var recentPdfUpdatesClient = new RecentPdfUpdatesApiClient(logger);
			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(logger);
			var checker = new MyAccountPortalDocumentChecker(logger, recentPdfUpdatesClient) as IMyAccountPortalDocumentChecker;
			incidentAutoresponderMessageFetcher.RunProcessing(token, new BusinessObjectFactory(Db.Connection), checker);
		}
	}
}
