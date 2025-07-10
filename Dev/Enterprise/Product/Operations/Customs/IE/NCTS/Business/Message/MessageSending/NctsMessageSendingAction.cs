using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsMessageSendingAction : IE.Business.IMessageSendingAction
	{
		public NctsMessageSendingAction(NctsHeaderMessageSendingObject sendingObject)
		{
			this.SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			MessageType = sendingObject.MessageType;
			ReleaseRequest = sendingObject.ReleaseRequest;
			Justification = sendingObject.Justification;
			this.NctsHeader = sendingObject.NctsHeader;
		}
		public readonly NctsHeaderMessageSendingObject SendingObject;
		public readonly NctsHeader NctsHeader;
		public ZString MessageType { get; set; }

		public ZString ReleaseRequest { get; set; }

		public ZString Justification { get; set; }

		IMessageAttachee IE.Business.IMessageSendingAction.MessageAttachee => NctsHeader.IsDepartureMovement ? NctsHeader.MovementHeader : NctsHeader;

		void IE.Business.IMessageSendingAction.AddMessage(IE.Business.OutboundEDIMessage message)
		{
			if (NctsHeader.IsDepartureMovement)
			{
				NctsHeader.MovementHeader.Messages.Add(message);
			}
			else
			{
				NctsHeader.Messages.Add(message);
			}
		}

		string IE.Business.IMessageSendingAction.MessageCreated(string messageText) => SendingObject.MessageCreated(messageText);
	}
}
