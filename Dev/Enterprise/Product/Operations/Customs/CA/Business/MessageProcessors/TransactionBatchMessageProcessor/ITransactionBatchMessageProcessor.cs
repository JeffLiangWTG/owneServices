using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public interface ITransactionBatchMessageProcessor
	{
		bool Process();
		IKeysResult GetKeysForBlockingParallelImport();
	}
}
