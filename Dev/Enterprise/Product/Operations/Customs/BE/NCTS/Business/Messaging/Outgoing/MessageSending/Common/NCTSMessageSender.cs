using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public abstract class NCTSMessageSender<TProvider> : BEMessageSender<NCTSMessage, TProvider>
		where TProvider : INCTSMessageHeader
	{
		protected readonly MessageSendingAction messageSendingAction;

		protected NCTSMessageSender(MessageSendingAction messageSendingAction)
			: this(Argument.NotNull(messageSendingAction, nameof(messageSendingAction)).Header, messageSendingAction.EntryType, messageSendingAction.ShouldSend, messageSendingAction.IsTestDeclaration)
		{
			this.messageSendingAction = messageSendingAction;
		}

		NCTSMessageSender(NctsHeader header, string messageName, bool sendWithErrors, bool isTestMessage) : base(header, messageName, sendWithErrors, isTestMessage)
		{
			MessageObject = header;
			messageSubTypeForEntryType.TryGetValue(messageName, out MessageSubType);
		}

		protected virtual ZString NewCustomsStatus => ZString.Empty;

		protected abstract ZString NewPhase { get; }

		protected ZString NewMessageStatus => LogicalStatusList.Codes.Sent;

		protected sealed override void SendCore(BEMessage newMessage)
		{
			var nctsHeader = (NctsHeader)MessageObject;
			var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;

			movementHeader.BM_CustomsStatus = NewCustomsStatus.IsEmpty ? movementHeader.BM_CustomsStatus : NewCustomsStatus;
			movementHeader.BM_Phase = NewPhase.IsEmpty ? movementHeader.BM_Phase : NewPhase;
			nctsHeader.EffectiveMessageStatus = NewMessageStatus;

			newMessage.EM_MessageInterpretation = new NctsEdiMessagePrettier(newMessage).MakeOutboundPrettyForInterpretation(nctsHeader);

			if (movementHeader is NctsDepartureMovementHeader departureMovement)
			{
				departureMovement.Messages.Add(newMessage);
			}
			else
			{
				nctsHeader.Messages.Add(newMessage);
			}
		}

		readonly Dictionary<string, string> messageSubTypeForEntryType = new Dictionary<string, string>
		{
			[NctsMessageTypeList.Codes.Declaration] = BEOutgoingMessageTypes.Codes.CC015C,
			[NctsMessageTypeList.Codes.ArrivalNotification] = BEOutgoingMessageTypes.Codes.CC007C,
			[NctsMessageTypeList.Codes.InvalidationCancellation] = BEOutgoingMessageTypes.Codes.CC014C,
			[NctsMessageTypeList.Codes.Amendment] = BEOutgoingMessageTypes.Codes.CC013C,
			[NctsMessageTypeList.Codes.UnloadingRemarks] = BEOutgoingMessageTypes.Codes.CC044C,
			[NctsMessageTypeList.Codes.RequestARelease] = BEOutgoingMessageTypes.Codes.CC054C,
			[NctsMessageTypeList.Codes.PresentationNotification] = BEOutgoingMessageTypes.Codes.CC170C,
			[NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement] = BEOutgoingMessageTypes.Codes.CC141C
		};
	}
}
