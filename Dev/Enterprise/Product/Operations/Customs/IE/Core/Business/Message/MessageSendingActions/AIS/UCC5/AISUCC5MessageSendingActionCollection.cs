using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<AISUCC5MessageSendingAction>
	{
		public AISUCC5MessageSendingActionCollection(AISUCC5MessageSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected override AISUCC5MessageSendingAction CreateElementCore(CusEntryHeader entryHeader)
		{
			var messageSendingAction = base.CreateElementCore(entryHeader);

			var defaultMessageType = GetDefaultMessageType(entryHeader);
			if (!defaultMessageType.IsEmpty)
			{
				messageSendingAction.MessageType = defaultMessageType;
			}

			return messageSendingAction;
		}

		ZString GetDefaultMessageType(CusEntryHeader entryHeader)
		{
			var entryStatus = entryHeader.CH_EntryStatus;
			if (entryStatus.IsEmpty)
			{
				return AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			}

			switch (entryStatus.ToUpperInvariant())
			{
				case AISEntryStatusList.Codes.Rejected:
					return AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
				case AISEntryStatusList.Codes.AmendmentRequested:
					return AISOutgoingMessageTypeList.Codes.AmendmentRequest;
				case AISEntryStatusList.Codes.Prelodged:
					return AISOutgoingMessageTypeList.Codes.PresentationNotification;
				case AISEntryStatusList.Codes.Released:
					return AISOutgoingMessageTypeList.Codes.InvalidationRequest;
				default:
					return ZString.Empty;
			}
		}
	}
}
