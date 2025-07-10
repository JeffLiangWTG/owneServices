using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public abstract class AESMessageSender<TProvider> : BEMessageSender<AESMessage, TProvider>
	where TProvider : IMessageHeader
{
	protected AESMessageSender(ExportEntryMessageSendingAction messageSendingAction)
		: base(Argument.NotNull(messageSendingAction, nameof(messageSendingAction)).Header, messageSendingAction.TypeOfEntry, messageSendingAction.ShouldSend, messageSendingAction.IsTestDeclaration)
	{
		this.messageSendingAction = messageSendingAction;
		MessageObject = messageSendingAction.Header;
		messageSubTypeForEntryType.TryGetValue(messageSendingAction.TypeOfEntry, out MessageSubType);
	}
	protected readonly ExportEntryMessageSendingAction messageSendingAction;

	protected abstract ZString EntryStatus { get; }

	protected override void SendCore(BEMessage newMessage)
	{
		var header = (CusEntryHeader)MessageObject;
		header.MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
		header.CH_EntryStatus = EntryStatus;
		header.Messages.Add(newMessage);
	}

	readonly Dictionary<string, string> messageSubTypeForEntryType = new Dictionary<string, string>
	{
		[BEExportEntryTypeList.Codes.PresentationNotification] = ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection,
		[BEExportEntryTypeList.Codes.CancellationRequest] = ExportOperationBusinessRejectionTypeList.Codes.InvalidationRequestRejection,
		[BEExportEntryTypeList.Codes.ExportDeclaration] = ExportOperationBusinessRejectionTypeList.Codes.DeclarationRejection,
		[BEExportEntryTypeList.Codes.ExportAmendment] = ExportOperationBusinessRejectionTypeList.Codes.DeclarationAmendmentRejection,
	};
}
