using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Customs.IE.Messaging;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public class MessageAcknowledgementServiceErrorInterpreter : InboundMessageInterpreter<ITransaction>
	{
		public MessageAcknowledgementServiceErrorInterpreter(BaseEDIMessage originalMessage, ITransaction incomingMessageProvider)
			: base(originalMessage, incomingMessageProvider)
		{
		}

		protected override string Summary
		{
			get
			{
				var errorCode = provider.ErrorCode;
				return Res.GetString(
					"D5AAC0C8-BE96-4AF3-A139-6BC0EFAE0D2B",
					"Error submitting message. Error Code: {0} - {1}",
					errorCode,
					RevenueErrorsList.GetDescriptionFromCode(errorCode)
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
				yield return Res.GetString("494C58DE-4D23-4E3E-A24C-A18019358A77", "Declaration message status has been set FAL");
			}
			else if (linkedObject is IMessageAttachee)
			{
				yield return Res.GetString("3FFA6DD3-8395-4C0A-AB6B-7E69BF01F0FB", "Entry status has been set ERR");
			}
		}
	}
}
