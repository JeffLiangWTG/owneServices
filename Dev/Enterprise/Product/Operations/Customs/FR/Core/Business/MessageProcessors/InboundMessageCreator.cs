using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class InboundMessageCreator : IInboundMessageCreator
	{
		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var createdMessages = CreateMessagesFromInterchange(interchange);
			var multiMessageCreated = createdMessages.Count > 1;
			if (multiMessageCreated)
			{
				var seq = 1;
				createdMessages.ForEachWithBetween(m => PostCreationMessageUpdate(m, interchange, seq), () => seq++);
			}
			else
			{
				createdMessages.ForEach(m => PostCreationMessageUpdate(m, interchange));
			}
		}
		protected abstract List<FREDIMessage> CreateMessagesFromInterchange(EDIInterchange interchange);

		static void PostCreationMessageUpdate(FREDIMessage message, EDIInterchange interchange, int messageGroupSeq = 0)
		{
			interchange.ContainedMessages.Add(message);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageNum = ((ZString)$"{interchange.EI_InterchangeNum}{(messageGroupSeq == 0 ? string.Empty : "/" + messageGroupSeq.ToString())}").Right(EDIMessage.Schema.EM_MessageNumMaxLength);
		}
	}
}
