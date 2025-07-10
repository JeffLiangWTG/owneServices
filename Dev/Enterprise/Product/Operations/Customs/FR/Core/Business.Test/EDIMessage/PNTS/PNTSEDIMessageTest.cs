using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(PNTSEDIMessage))]
	public class PNTSEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var message = Factory.New<PNTSEDIMessage>();
			AssertEquals("Message type", "STO", message.EM_MessageType);
			AssertEquals("Message Subtype", ZString.Empty, message.EM_MessageSubType);
			AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<PNTSEDIMessage>();
			message.EM_MessageSubType = "AAA";
			AssertNull(message.MessageDataObject);

			message.EM_MessageSubType = "016";
			AssertType<IETS016MessageDataObject>(message.MessageDataObject);

			var message2 = Factory.New<PNTSEDIMessage>();
			message2.EM_MessageSubType = "028";
			AssertType<IETS028MessageDataObject>(message2.MessageDataObject);

			var message3 = Factory.New<PNTSEDIMessage>();
			message3.EM_MessageSubType = "029";
			AssertType<IETS029MessageDataObject>(message3.MessageDataObject);

			var message4 = Factory.New<PNTSEDIMessage>();
			message4.EM_MessageSubType = "030";
			AssertType<IETS030MessageDataObject>(message4.MessageDataObject);

			var message5 = Factory.New<PNTSEDIMessage>();
			message5.EM_MessageSubType = "095";
			AssertType<IETS095MessageDataObject>(message5.MessageDataObject);

			var message6 = Factory.New<PNTSEDIMessage>();
			message6.EM_MessageSubType = "410";
			AssertType<IETS410MessageDataObject>(message6.MessageDataObject);

			var message7 = Factory.New<PNTSEDIMessage>();
			message7.EM_MessageSubType = "460";
			AssertType<IETS460MessageDataObject>(message7.MessageDataObject);

			var message8 = Factory.New<PNTSEDIMessage>();
			message8.EM_MessageSubType = "928";
			AssertType<IETS928MessageDataObject>(message8.MessageDataObject);

			var message9 = Factory.New<PNTSEDIMessage>();
			message9.EM_MessageSubType = "906";
			AssertType<IETS906MessageDataObject>(message9.MessageDataObject);
		}
	}
}
