using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface ICFGUniversalCustomsMessageProcessor
	{
		void ProcessMessage(EDIMessage message, ILoggingInformation logger);

		ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, ILoggingInformation logger);
	}
}
