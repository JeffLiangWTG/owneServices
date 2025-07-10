using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyFetchStrategy : BusinessObjectFetchStrategy, IDocumentSupporterFetchStrategy
	{
		public DummyFetchStrategy(BusinessObject parent)
			: base(parent)
		{
		}

		public void AddDocumentSupporterFetchHints()
		{
			FetchStrategyInitialised = true;
		}

		public bool FetchStrategyInitialised;
	}
}
