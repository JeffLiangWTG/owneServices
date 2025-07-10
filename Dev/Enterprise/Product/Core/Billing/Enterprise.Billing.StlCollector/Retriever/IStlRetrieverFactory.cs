using Enterprise.Integration;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public interface IStlRetrieverFactory
	{
		IStlRetriever Create(ILogger logger);
	}
}
