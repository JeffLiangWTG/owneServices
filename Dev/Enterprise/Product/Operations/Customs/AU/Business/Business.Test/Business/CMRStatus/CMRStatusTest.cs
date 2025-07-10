using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRStatusTest : TestCaseWithFactory
	{
		protected EDIMessage GetIncomingMessage(Type messageType, ZString messageTextFilename)
		{
			var result = (EDIMessage)Factory.New(messageType);
			result.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles." + messageTextFilename).Replace("\r\n", "");
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return result;
		}
	}
}
