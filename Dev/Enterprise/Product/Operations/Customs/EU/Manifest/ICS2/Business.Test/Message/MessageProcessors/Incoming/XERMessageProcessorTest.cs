using CargoWise.Customs.Shared.MessageDefinitions.Universal.UniversalEvent;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class XERMessageProcessorTest : MessageProcessorWithEmailNotificationTest<XERMessageProcessor, UniversalEventData>
	{
		public void TestProcessMessage()
		{
			var (header, incomingMessage, outgoingMessage) = PrepareData();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("AMA_MessageStatus", MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
				AssertEquals("Outgoing message: EM_Status", EDIMessageStatusList.Codes.Error, outgoingMessage.EM_Status);
				AssertEquals("Incoming message: EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertSame("LinkedObject", header, incomingMessage.EM_LinkedObject);
			});
		}

		protected override TestEdiMessage GetIncomingMessage(string primaryReferenceNumber)
		{
			var incomingMessage = Factory.NewWithValidTestData<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = xerMessageType;
			incomingMessage.EM_MessageSubType = xerMessageType;
			incomingMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			incomingMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
	<Event>
		<EventTime>2025-04-08 19:21:42.974</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>XER</MessageType>
			<Type>BusinessError</Type>
			<Reason>[3] POST TaskCancelledException: The Http Request task was cancelled due to timeout 100000s</Reason>
		</EventParameters>
	</Event>
</UniversalEvent>";

			return incomingMessage;
		}

		protected override (AsycudaManifestHeader Header, TestEdiMessage Message) PrepareForEmailTesting()
		{
			var (header, incomingMessage, _) = PrepareData();
			return (header, incomingMessage);
		}

		protected override XERMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new XERMessageProcessor(logger);

		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override string ExpectedEmailSubject => $"ICS2: Message sending failed for {CommonManifestJobReference} due to xT sending error.";

		protected override string[] ExpectedEmailBody => new[] { ExpectedEmailSubject };

		protected override string StatusOfOutgoingEDIMessageToFindHeaderBySessionGUID => EDIMessage.Status.ProcessedOK;

		string xerMessageType => Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;

		(AsycudaManifestHeader header, TestEdiMessage incomingMessage, TestEdiMessage outgoingMessage) PrepareData()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusList.Codes.AwaitingResponse;
			header.AMA_JobReference = CommonManifestJobReference;
			header.AMA_MasterBill = CommonMasterBill;

			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.EUICS2;
			outgoingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			var outgoingMessage = Factory.NewWithValidTestData<TestEdiMessage>();
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingMessage.EM_MessageType = MessageTypes.Codes.F14;
			outgoingMessage.EM_LinkedObject = header;
			outgoingMessage.EM_SystemCreateUser = staffCode;
			outgoingMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.EUICS2;
			incomingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			incomingInterchange.EI_InterchangeType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;

			var incomingMessage = GetIncomingMessage(null);
			incomingMessage.EM_EI = incomingInterchange.PK;
			Factory.Save();

			return (header, incomingMessage, outgoingMessage);
		}
	}
}
