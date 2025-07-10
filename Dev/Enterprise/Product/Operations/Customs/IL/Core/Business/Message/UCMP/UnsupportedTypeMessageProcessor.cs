using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	class UnsupportedTypeMessageProcessor : IMessageProcessor
	{
		public UnsupportedTypeMessageProcessor(LoggingInformation logger)
		{
			this.logger = logger;
		}

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
			=> ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, GetDiscardReason(message));

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> ProcessingResult.New(ZGuid.Empty, GetDiscardReason(message));

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
			=> ProcessingResult.New(SerializationKeysResult.SerialProcessingInReceivedOrder, GetDiscardReason(message));

		public void ProcessMessage(EDIMessage message)
		{
			logger.Log(GetDiscardReason(message));
			message.EM_Status = EDIMessage.Status.Discarded;
		}

		MultilingualString GetDiscardReason(EDIMessage message)
			=> ResString.GetMultilingualString("0B406CB0-4812-4E6C-B128-4E800FAD103B", "Message type {0} is not supported by {1}", message.EM_MessageType, message.EM_ApplicationCode);

		readonly LoggingInformation logger;
	}
}
