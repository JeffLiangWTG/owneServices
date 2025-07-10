using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public abstract class PNTSMessageHeaderProvider : IMessageHeader
{
	public PNTSMessageHeaderProvider(TemporaryStorageMessageSendingObject messageSendingAction)
	{
		temporaryStorageHeader = Argument.NotNull(messageSendingAction.Header, $"{nameof(messageSendingAction)}.{nameof(TemporaryStorageMessageSendingObject.Header)}");
	}
	protected readonly TemporaryStorageHeader temporaryStorageHeader;

	public string MessageSender => BECustomsRegistry.Instance.SenderIDs.GetTargetSystemName(SendMessageTypes.Codes.PN);

	public string MessageRecipient => BECustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(SendMessageTypes.Codes.PN);

	public string MessageType => null;

	public DateTime PreparationDateTime => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

	public string CorrelationIdentifier => string.Empty;
}
