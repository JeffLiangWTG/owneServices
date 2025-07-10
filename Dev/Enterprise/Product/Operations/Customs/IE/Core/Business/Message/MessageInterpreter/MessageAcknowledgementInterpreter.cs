using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Customs.IE.Messaging;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public class MessageAcknowledgementInterpreter : InboundMessageInterpreter<ITransaction>
	{
		public MessageAcknowledgementInterpreter(BaseEDIMessage originalMessage, ITransaction incomingMessageProvider) : base(originalMessage, incomingMessageProvider)
		{
		}

		protected override string Summary
		{
			get
			{
				var statusCode = provider.TransactionIdStatus;
				return Res.GetString(
					"5FCBD500-DB20-49BD-AFB0-0AD0ECCE4314",
					"Submission has been rejected. Transaction ID Status: {0} - {1}",
					statusCode,
					factory.GetCachedValue<TransactionIdStatusList>().GetDescriptionFromCode(statusCode)
				);
			}
		}

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.TransactionId, message.EM_ApplicationReference);
			yield return (CommonResStrings.MessageType, message.MessageTypeWithDescription);
			yield return (CommonResStrings.MessageNumber, message.EM_MessageNum);
		}

		protected override IEnumerable<string> GetDescriptions()
		{
			var linkedObject = message.EM_LinkedObject;
			if (linkedObject is Integration.Customs.IEEMCS.IEMCSJobDeclaration)
			{
				yield return Res.GetString("47A0FAEA-49E8-4100-9DED-58AED4F53318", "Declaration message status has been set FAL.");
			}
			else if (linkedObject is IMessageAttachee linkedEntry)
			{
				yield return Res.GetString("136545E2-E076-46F8-8FBE-CBF19E09E3D7", "Entry status has been set ERR, Movement Reference Number: {0}.", linkedEntry.MovementReferenceNumber);
			}
		}
	}
}
