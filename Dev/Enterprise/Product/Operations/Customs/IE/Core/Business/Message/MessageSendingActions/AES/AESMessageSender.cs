using CargoWise.Customs.IE.MessageContracts.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AESMessageSender : MessageSender
	{
		public AESMessageSender(AESMessageSendingAction sendingAction) : base(sendingAction) { }

		protected new AESMessageSendingAction SendingAction => (AESMessageSendingAction)base.SendingAction;

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder result = null;
			var entryHeader = SendingAction.EntryHeader;
			var messageType = outgoingMessage.EM_MessageType;
			switch (messageType)
			{
				case AESOutgoingMessageTypeList.Codes.ExportPresentation:
					result = new IE511MessageBuilder(new IE511MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExportOriginal:
					result = new IE515MessageBuilder(new IE515MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExportAmendment:
					result = new IE513MessageBuilder(new IE513MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExportCancellation:
					result = new IE514MessageBuilder(new IE514MessageProvider(SendingAction));
					break;
				case AESOutgoingMessageTypeList.Codes.ReExport:
					result = new IE570MessageBuilder(new IE570MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ReExportAmendment:
					result = new IE573MessageBuilder(new IE573MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExitOriginal:
					result = new IE615MessageBuilder(new IE615MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExitAmendment:
					result = new IE613MessageBuilder(new IE613MessageProvider(entryHeader));
					break;
				case AESOutgoingMessageTypeList.Codes.ExitCancellation:
					result = new IE614MessageBuilder(new IE614MessageProvider(SendingAction));
					break;
				case AESOutgoingMessageTypeList.Codes.ReleaseAmendment:
					result = new EX513MessageBuilder(new EX513MessageProvider(entryHeader));
					break;
				default:
					break;
			}
			return result;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<AESOutboundEDIMessage>();
	}
}
