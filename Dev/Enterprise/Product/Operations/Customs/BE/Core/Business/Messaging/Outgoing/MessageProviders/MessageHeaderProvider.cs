using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BE.Business;

public abstract class MessageHeaderProvider : IMessageHeader
{
	protected MessageHeaderProvider(BEJobDeclarationMessageSendingObject messageSendingAction)
	{
		this.messageSendingAction = Argument.NotNull(messageSendingAction, nameof(messageSendingAction));
		this.entryHeader = Argument.NotNull(messageSendingAction.Header, $"{nameof(messageSendingAction)}.{nameof(ExportEntryMessageSendingAction.Header)}");
		this.declaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(messageSendingAction)}.{nameof(entryHeader)}.{nameof(CusEntryHeader.Declaration)}");
	}
	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;
	protected readonly BEJobDeclarationMessageSendingObject messageSendingAction;

	public abstract string MessageSender { get; }

	public abstract string MessageRecipient { get; }

	public DateTime PreparationDateTime => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public string MessageIdentification => AESMessage.MessageNumberPlaceHolder;

	public string CorrelationIdentifier => CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, entryHeader.CountryCode)?.CE_EntryLineReference ?? string.Empty;

	public abstract string MessageType { get; }

	string IMessageHeader.MessageType => MessageType;
}
