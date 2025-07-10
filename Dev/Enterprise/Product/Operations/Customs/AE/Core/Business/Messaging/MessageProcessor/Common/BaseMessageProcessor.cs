using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

abstract class BaseMessageProcessor<TDataProvider> : IMessageProcessor<TDataProvider>
	where TDataProvider : class, IInboundMessageDataProvider
{
	public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
	{
		var provider = MessageDataProviderFactory.Instance.Value.GetMessageDataProvider(message);
		return provider is TDataProvider dataProvider && GetOutgoingMessage(message, dataProvider) is { } outgoingMessage
			? GetLinkedBusinessObjectMetaData(outgoingMessage)
			: ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, GetDiscardReasonWhenOutgoingMessageNotExist());
	}

	ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message)
	{
		if (message.EM_LinkedObject is { } linkedObject and IMessageAttachee attachee)
		{
			return new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, message.EM_GB, attachee.JobNumber);
		}

		return LinkedBusinessObjectMetaData.Empty;
	}

	protected abstract EDIMessage GetOutgoingMessage(EDIMessage message, TDataProvider dataProvider);

	protected virtual MultilingualString GetDiscardReasonWhenOutgoingMessageNotExist() => ResString.GetMultilingualString("f067e068-e680-4540-98e4-a07e3f0efe1d", "Message is discarded. Cannot find corresponding sent message.");

	public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
	{
		return linkedBusinessObjectBranchPk;
	}

	public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
	{
		if (linkedBusinessObjectMetaData is null || linkedBusinessObjectMetaData.JobNumber.IsEmpty)
		{
			return GetKeysResultWhenJobNumberIsEmpty();
		}

		return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			linkedBusinessObjectMetaData.JobNumber
		});
	}

	protected virtual SerializationKeysResult GetKeysResultWhenJobNumberIsEmpty() => SerializationKeysResult.SerialProcessingInReceivedOrder;

	public void ProcessMessage(EDIMessage message, LoggingInformation logger)
	{
		var messageStatus = (string)message.EM_Status;
		switch (messageStatus)
		{
			case EDIMessageStatusList.Codes.PreProcessedOK:
				ProcessPreProcessOKMessage(message, logger);
				break;
			default:
				throw new System.InvalidOperationException($"Unsupported message status: {messageStatus}");
		}
	}

	static TDataProvider GetDataProvider(EDIMessage message) => MessageDataProviderFactory.Instance.Value.GetMessageDataProvider(message) as TDataProvider;

	void ProcessPreProcessOKMessage(EDIMessage message, LoggingInformation logger)
	{
		var dataProvider = GetDataProvider(message);
		ProcessPreProcessOKMessageCore(message, dataProvider, logger);

		if (GetMessageInterpreter(message) is IMessageInterpreter<TDataProvider> interpreter)
		{
			message.EM_MessageInterpretation = interpreter.GetMessageInterpretation(dataProvider);
		}
	}

	protected void UpdateMessage(EDIMessage message, ZString status, ZString error)
	{
		message.EM_Status = status;
		message.Notes.AddNew(true, InterchangeProviderBase.ProcessingLogDescription, error);
	}

	protected abstract void ProcessPreProcessOKMessageCore(EDIMessage message, TDataProvider dataProvider, LoggingInformation logger);

	protected virtual IMessageInterpreter<TDataProvider> GetMessageInterpreter(EDIMessage message) => null;
}
