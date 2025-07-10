using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface INettingTransactionImporter
	{
		bool ImportNettingTransaction(IEDIMessage message, ITopLevelDataObject dataObject);
		IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject);
	}
}
