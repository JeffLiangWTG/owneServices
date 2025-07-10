using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("PRA Response", messageProcessorFactory.MessageFriendlyName);
		}

		public void TestMessageProcessorFactoryCorruptedMessageFails()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestMessageProcessorFactoryMissingDecRefFails()
		{
			string messageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.PRA.Testing.NoErrorOneWarningEDIFACT.txt");
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestProcessingFreightResponseWorks()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00045159";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TRLU9030913";

			PRAMessage sentMessage = container.PRAMessages.AddNew();
			sentMessage.EM_MessageText = PRAMessage.MessageNumberPlaceHolder + PRAMessage.SendersReferencePlaceHolder;
			sentMessage.EM_ApplicationCode = PRAMessage.ApplicationCodes.OneStop;
			sentMessage.EM_ReceiveTransmit = PRAMessage.Direction.Transmit;
			sentMessage.EM_MessageSubType = "SSM";

			Factory.Save();
			IPRAContainerMessaging containerMsg = container;
			AssertEquals("PRA Submit Message Sent but not responded to yet.", containerMsg.CurrentPRAStatus);

			PRAMessage message = container.PRAMessages.AddNew();
			message.EM_MessageText = FreightResponseMessage.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = PRAMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = PRAMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(message);
			container.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("PRA Submit Message Sent and was Accepted.", containerMsg.CurrentPRAStatus);
		}

		public void TestProcessingCustomsResponseWorks()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B00045159";
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TRLU9030913";

			PRAMessage sentMessage = Factory.New<PRAMessage>();
			sentMessage.EM_MessageText = PRAMessage.MessageNumberPlaceHolder + PRAMessage.SendersReferencePlaceHolder;
			sentMessage.EM_ApplicationCode = PRAMessage.ApplicationCodes.OneStop;
			sentMessage.EM_ReceiveTransmit = PRAMessage.Direction.Transmit;
			sentMessage.EM_MessageSubType = "SSM";
			container.Messages.Add(sentMessage);

			Factory.Save();
			IPRAContainerMessaging containerMsg = container;
			AssertEquals("PRA Submit Message Sent but not responded to yet.", containerMsg.CurrentPRAStatus);

			PRAMessage message = Factory.New<PRAMessage>();
			message.EM_MessageText = CustomsResponseMessage.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = PRAMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = PRAMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(message);
			container.Reload();
			container.PRAMessages.Load();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("PRA Submit Message Sent and was Accepted.", containerMsg.CurrentPRAStatus);
		}

		MessageProcessorFactory messageProcessorFactory;

		protected override void SetUp()
		{
			base.SetUp();
			messageProcessorFactory = new MessageProcessorFactory(new LoggingInformation());
		}

		const string FreightResponseMessage = @"
UNH+12540295+APERAK:D:00A:UN:ANZ23'
BGM+7+12539977+9+AP'
DTM+137:20061123151031:204'
DOC+ERA+LEPMEL'
DTM+137:20061123150341:204'
RFF+ERN:CON-C00045159-TRLU9030913'
NAD+MS+CONWS'
NAD+MR+LEPMEL'
ERC+ERA0100'
FTX+AAO+++Message received without error'
RFF+EQD:TRLU9030913'
UNT+12+12540295'";

		const string CustomsResponseMessage = @"
UNH+12540295+APERAK:D:00A:UN:ANZ23'
BGM+7+12539977+9+AP'
DTM+137:20061123151031:204'
DOC+ERA+LEPMEL'
DTM+137:20061123150341:204'
RFF+ERN:CUS-B00045159-TRLU9030913'
NAD+MS+CONWS'
NAD+MR+LEPMEL'
ERC+ERA0100'
FTX+AAO+++Message received without error'
RFF+EQD:TRLU9030913'
UNT+12+12540295'";
	}
}
