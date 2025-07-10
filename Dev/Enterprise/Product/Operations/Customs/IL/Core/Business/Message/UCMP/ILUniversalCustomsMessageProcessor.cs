using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsMessageProcessor(EDIInterchange.ApplicationCodes.ILCustoms, typeof(Enterprise.Customs.IL.Business.ILUniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.IL.Business
{
	public class ILUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		{
			return ILMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, message.EM_MessageSubType, logger).GetBranch(message, logger, linkedBusinessObjectBranchPk);
		}

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			return ILMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, message.EM_MessageSubType, logger).GetLinkedBusinessObjectMetaData(message, logger);
		}

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			return ILMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, message.EM_MessageSubType, logger).GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);
		}

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
		{
			ILMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, message.EM_MessageSubType, logger).ProcessMessage(message);
		}

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;
	}
}
