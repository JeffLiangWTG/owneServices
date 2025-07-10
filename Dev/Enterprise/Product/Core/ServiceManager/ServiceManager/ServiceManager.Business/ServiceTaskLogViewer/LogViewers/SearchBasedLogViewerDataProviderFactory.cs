using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Business
{
	public class SearchBasedLogViewerDataProviderFactory : ISearchBasedLogViewerDataProviderFactory
	{
		public ISearchBasedLogViewerDataProvider GetProvider()
		{
			return new ElasticsearchSearchBasedLogViewerDataProvider();
		}

		class ElasticsearchSearchBasedLogViewerDataProvider : ISearchBasedLogViewerDataProvider
		{
			public ElasticsearchSearchBasedLogViewerDataProvider()
			{
				var elasticClient = new ElasticLogClientFactory().CreateElasticClient();
				processControllerLogger = new ProcessControllerLogger(Db.ServerName, Db.DatabaseName, SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.Value, elasticClient);
			}

			public byte[] GetBytes(ServiceTaskLogFilters filters)
			{
				return processControllerLogger.GetStream(filters, false);
			}

			readonly IProcessControllerLogger processControllerLogger;
		}
	}
}
