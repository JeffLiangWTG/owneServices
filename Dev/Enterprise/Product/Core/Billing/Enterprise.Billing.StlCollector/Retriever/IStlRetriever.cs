using System.Threading;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public interface IStlRetriever
	{
		void CollectAndSend(CancellationToken token);
	}
}
