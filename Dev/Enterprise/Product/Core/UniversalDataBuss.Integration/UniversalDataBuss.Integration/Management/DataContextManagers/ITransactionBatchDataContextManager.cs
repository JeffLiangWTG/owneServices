using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITransactionBatchDataContextManager : IDataContextManager
	{
		bool ManagesTransactionBatches { get; }
		ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager);

		bool UseIncomingTransactionBatchData(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory);

		IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory);
	}
}
