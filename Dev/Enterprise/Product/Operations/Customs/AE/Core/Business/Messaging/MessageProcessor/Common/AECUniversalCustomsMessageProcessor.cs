using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

[assembly: UniversalCustomsMessageProcessor(EDIMessage.ApplicationCodes.UnitedArabEmirates, typeof(Enterprise.Customs.AE.Business.AECUniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.AE.Business;

public sealed class AECUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
{
	public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
	{
		if (MessageProcessorFactory.Instance.Value.GetMessageProcessor(message) is { } processor)
		{
			return processor.GetLinkedBusinessObjectMetaData(message, logger);
		}
		return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, GetUnknownMessageTypeError(message));
	}

	internal static MultilingualString GetUnknownMessageTypeError(EDIMessage message) => ResString.GetMultilingualString("1BD17565-ED70-49B5-A568-A94C5E1015C9", "Unknown Message Type {0}. Message will be discarded", message.EM_MessageType);

	public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
	{
		var processor = MessageProcessorFactory.Instance.Value.GetMessageProcessor(message);
		return processor?.GetBranch(message, logger, linkedBusinessObjectBranchPk);
	}

	public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
	{
		var processor = MessageProcessorFactory.Instance.Value.GetMessageProcessor(message);
		return processor?.GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);
	}

	public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

	public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
	{
		var processor = MessageProcessorFactory.Instance.Value.GetMessageProcessor(message);
		processor?.ProcessMessage(message, logger);
	}
}
