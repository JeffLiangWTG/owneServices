using CargoWise.Customs.IE.MessageContracts.PBN;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNMessageSender
	{
		public PBNMessageSender(PBNMessageSendingObject sendingAction)
		{
			this.sendingAction = sendingAction;
			factory = sendingAction.Header.Factory;
		}
		readonly PBNMessageSendingObject sendingAction;
		readonly BusinessObjectFactory factory;

		public bool Send()
		{
			var messageAttachee = sendingAction.MessageAttachee;
			var messageType = sendingAction.MessageType;

			var newMessage = factory.New<PBNOutboundEDIMessage>();
			var branch = messageAttachee.Branch;
			newMessage.EM_GB = branch.PK;
			newMessage.EM_MessageType = messageType;
			newMessage.EM_ApplicationReference = sendingAction.JobNumber;

			// for LPB or LPC, we don't need any message text
			if (messageType != PBNMessageTypes.Codes.LookupPBN && messageType != PBNMessageTypes.Codes.LookupPBNChannel)
			{
				var messageBuilder = CreateMessageBuilder(messageType);
				if (messageBuilder == null)
				{
					newMessage.Delete();
					return false;
				}

				var jsonMessage = messageBuilder.GenerateJsonMessage();
				newMessage.EM_MessageText = sendingAction.MessageCreated(jsonMessage.GetSerializedString());
			}

			newMessage.EM_Status = EDIMessage.Status.Queued;
			newMessage.EM_GP = branch.Company.GetCredentialPK();
			newMessage.EM_LinkedObject = sendingAction.Header;

			messageAttachee.LogicalStatus = LogicalStatusList.Codes.Sent;
			sendingAction.AddMessage(newMessage);
			return true;
		}

		IJsonMessageBuilder CreateMessageBuilder(ZString messageType)
		{
			IJsonMessageBuilder result = null;
			switch (messageType)
			{
				case PBNMessageTypes.Codes.CreatePBN:
				case PBNMessageTypes.Codes.UpdatePBN:
					result = new CPBAndUPBMessageBuilder(new CPBAndUPBMessageProvider(sendingAction));
					break;
				case PBNMessageTypes.Codes.UpdatePBNDeclarations:
					result = new UPDMessageBuilder(new UPDMessageProvider(sendingAction));
					break;
				default:
					break;
			}
			return result;
		}
	}
}
