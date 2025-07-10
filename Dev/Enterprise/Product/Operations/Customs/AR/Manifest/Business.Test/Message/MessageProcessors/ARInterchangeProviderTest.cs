using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class ARInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new ARInterchangeProvider(new LoggingInformation(), collection);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var message1 = CreateMessage(MessageTypes.Codes.ARA) as TestEdiMessage;
			var message2 = CreateMessage(MessageTypes.Codes.ARD) as TestEdiMessage;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message1, message2 });

			var logger = new LoggingInformation();
			var provider = new ARInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);

				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);

				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);
			});
		}

		public void TestDoNotSendInterchangeWithEmptyBody()
		{
			var message = CreateMessage(MessageTypes.Codes.ARA);
			message.EM_MessageText = "";

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new ARInterchangeProvider(logger, messageCollection);

			AssertEquals(0, provider.Interchanges.Length);
			AssertContains("The Interchange Body is empty even though there are 1 messages.", logger.UserLogStrings[0]);

			message.EM_MessageText = "MESSAGE TEXT";
			provider = new ARInterchangeProvider(logger, messageCollection);
			AssertEquals(1, provider.Interchanges.Length);
		}

		public void TestNewSeaInterchangeWithEnvelope()
		{
			var message = CreateMessage(MessageTypes.Codes.ARA) as TestEdiMessage;
			message.EM_MessageText = "Message Text";

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message });

			var logger = new LoggingInformation();
			var provider = new ARInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message.Reload();

			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI);

			AssertNotNull("Interchange 1 is linked to message 1", interchange);
			Assert(interchange.EI_BodyText.Contains("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ar=\"Ar.Gob.Afip.Dga.Org.wgesinformacionanticipada\">"));
			AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message.EM_Status);
		}

		EDIMessage CreateMessage(ZString messageType)
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "MESSAGETEXT";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
