using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsInterchangeUnpacker
	{
		IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger);
	}
}
