using Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Operations;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public static class RetrieverFactory
	{
		public static Retriever GetRetriever(string type, AncillaryImportServices sessionServices)
		{
			Retriever retriever;
			var entitySetRepository = new RetrieveOperation(sessionServices);

			switch (type)
			{
				case RetrieveType.Key:
					retriever = new KeyRetriever(sessionServices)
					{
						RetrieveOperation = entitySetRepository
					};
					return retriever;

				case RetrieveType.Partial:
					retriever = new PartialRetriever(sessionServices)
					{
						RetrieveOperation = entitySetRepository
					};
					return retriever;

				case RetrieveType.Score:
					retriever = new ScoreRetriever(sessionServices)
					{
						RetrieveOperation = entitySetRepository
					};
					return retriever;

				default:
					throw new NativeXMLUserVisibleException("Unknown retrieve type - " + type);
			}
		}
	}
}
