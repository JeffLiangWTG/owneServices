using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class PNTSInbondMessageCreator : InboundMessageCreator
	{
		protected override List<FREDIMessage> CreateMessagesFromInterchange(EDIInterchange interchange)
		{
			var message = interchange.Factory.New<PNTSEDIMessage>();
			var messageRootName = XDocument.Parse(interchange.EI_BodyText).Root?.Name?.ToString() ?? ZString.Empty;
			message.EM_MessageSubType = new ZString(messageRootName).Right(3);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = interchange.EI_BodyText;
			return new List<FREDIMessage> { message };
		}
	}
}
