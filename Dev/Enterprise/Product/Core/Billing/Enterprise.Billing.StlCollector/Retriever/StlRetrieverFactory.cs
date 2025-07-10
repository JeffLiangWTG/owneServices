using Enterprise.Integration;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlRetrieverFactory : IStlRetrieverFactory
	{
		public IStlRetriever Create(ILogger logger) => new StlRetriever(logger);
	}
}
