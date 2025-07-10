using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BECIncomingMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessUnsupportedMessageType()
	{
		var unknowMessage = CreateMessage("T!T12", "02342323", "T!T", "SVE", "");
		processor.ExecuteBatch();
		CombineAssertions(() =>
		{
			AssertMultilineASCIIEquals("Logg", @"	Failed to locate a processor for message (Application Code: BEC, Application Reference: T!T12, Message Type: T!T, Message Sub Type: SVE)",
			string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			unknowMessage = newFactory.Load<BEMessage>(unknowMessage.PK);
			AssertEquals("unknowMessage.EM_Status", EDIMessage.Status.Failed, unknowMessage.EM_Status);
		});
	}

	public void TestProcessSupportedMessageType()
	{
		var entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001TEST";
		entry.MovementReferenceNumberSetter("22BEE00000000012J1TEST");
		Factory.Save();
		var message = CreateMessage(nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC556C), "02342323", SendMessageTypes.Codes.AES, "556",
			@"<?xml version=""1.0"" encoding=""UTF-8""?>
<CC556C>
	<messageSender xmlns="""">AES</messageSender>
	<messageRecipient xmlns="""">CW1@BE0452814806</messageRecipient>
	<preparationDateAndTime xmlns="""">2022-04-01T12:34:56.123456789Z</preparationDateAndTime>
	<messageIdentification xmlns="""">12af74cb-93e8-4632-98b0-c10b477eaf10</messageIdentification>
	<messageType xmlns="""">CC556C</messageType>
	<correlationIdentifier xmlns="""">37abde20-076c-40d6-ba58-28a5eae74b2d</correlationIdentifier>
	<ExportOperation xmlns="""">
		<LRN>22045281480600000001TEST</LRN>
		<MRN>22BEE00000000012J1TEST</MRN>
		<businessRejectionType>515</businessRejectionType>
		<rejectionDateAndTime>2022-04-01T12:34:56.123456789Z</rejectionDateAndTime>
		<rejectionCode>4</rejectionCode>
		<rejectionReason>invalid value transport type</rejectionReason>
	</ExportOperation>
	<CustomsOfficeOfExport xmlns="""">
		<referenceNumber>BE101000</referenceNumber>
	</CustomsOfficeOfExport>
	<Declarant xmlns="""">
		<identificationNumber>BE0449424358</identificationNumber>
		<name></name>
	</Declarant>
	<Representative xmlns="""">
		<identificationNumber>BE0452806814</identificationNumber>
		<status>2</status>
	</Representative>
	<FunctionalError xmlns="""">
		<errorPointer>cc515c.DepartureTransportMeans(2).typeOfIdentification</errorPointer>
		<errorCode>12</errorCode>
		<errorReason>Type of transport does not exist</errorReason>
		<originalAttributeValue>32</originalAttributeValue>
	</FunctionalError>
</CC556C>");
		processor.ExecuteBatch();
		CombineAssertions(() =>
		{
			AssertMultilineASCIIEquals("Logg", @"	Pre-Process Message #02342323
	Saving...
	1 message pre-processed
	Processing Message #02342323
	Saving...
	1 message processed",
		string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<BEMessage>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestProcessErrorMessageType()
	{
		var sessionGUID = ZGuid.BrettsGuid;
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var ediMessage = Factory.New<BEMessage>();
		ediMessage.EM_Status = "SNT";
		ediMessage.EM_LinkTable = cusEntryHeader.TableName;
		ediMessage.EM_LinkUniqueID = cusEntryHeader.PK;
		var interchangeOutgoing = Factory.New<BECInterchange>();
		interchangeOutgoing.EI_SessionGUID = sessionGUID;
		interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
		interchangeOutgoing.EI_From = "DEJOS";
		interchangeOutgoing.EI_To = "DEFRANS";
		ediMessage.EM_EI = interchangeOutgoing.PK;
		var interchangeIncoming = Factory.New<BECInterchange>();
		interchangeIncoming.EI_SessionGUID = sessionGUID;
		interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		interchangeIncoming.EI_From = "DEFRANS";
		interchangeIncoming.EI_To = "DEJOS";

		var message = CreateMessage("UniversalEventData", "02342323", SendMessageTypes.Codes.AES, "UER",
			UniversalEventTestDataHelper.CreateUniversalEventXml(Events.InterchangeRejectedCode, "XER", "Unauthorized", "ErrorMessage", "...."));
		message.EM_EI = interchangeIncoming.PK;
		message.EM_LinkedObject = cusEntryHeader;
		Factory.Save();

		processor.ExecuteBatch();
		CombineAssertions(() =>
		{
			AssertMultilineASCIIEquals("Logg", @"	Pre-Process Message #02342323
	Saving...
	1 message pre-processed
	Processing Message #02342323
	Saving...
	1 message processed",
		string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<BEMessage>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestMessageShouldBeProcessedInASeparateFactory()
	{
		var processor = new BECIncomingMessageProcessorForTest();
		AssertEquals(true, processor.MessageShouldBeProcessedInASeparateFactoryExposed);
	}

	public void TestValidAESMessageProcessors()
	{
		var aesMessageProcessors = ((Lazy<Dictionary<string, Type>>)processor.GetType().GetField("aesMessageProcessors", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(processor)).Value;
		CombineAssertions(() =>
		{
			AssertEquals("Number of defined AES Message Processors", 11, aesMessageProcessors.Count);

			AssertEquals(typeof(CC504CMessageProcessor), aesMessageProcessors["504"]);
			AssertEquals(typeof(CC509CMessageProcessor), aesMessageProcessors["509"]);
			AssertEquals(typeof(CC528CMessageProcessor), aesMessageProcessors["528"]);
			AssertEquals(typeof(CC529CMessageProcessor), aesMessageProcessors["529"]);
			AssertEquals(typeof(CC551CMessageProcessor), aesMessageProcessors["551"]);
			AssertEquals(typeof(CC556CMessageProcessor), aesMessageProcessors["556"]);
			AssertEquals(typeof(CC560CMessageProcessor), aesMessageProcessors["560"]);
			AssertEquals(typeof(CC599CMessageProcessor), aesMessageProcessors["599"]);
			AssertEquals(typeof(CC917CMessageProcessor), aesMessageProcessors["917"]);
			AssertEquals(typeof(CC928CMessageProcessor), aesMessageProcessors["928"]);
			AssertEquals(typeof(CustomsServiceErrorUniversalEventResponseMessageProcessor), aesMessageProcessors["UER"]);
		});
	}

	public void TestValidImportMessageProcessors()
	{
		var impMessageProcessors = ((Lazy<Dictionary<string, Type>>)processor.GetType().GetField("impMessageProcessors", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(processor)).Value;

		CombineAssertions(() =>
		{
			AssertEquals("Number of defined Import Message Processors", 2, impMessageProcessors.Count);

			AssertEquals(typeof(IE906MessageProcessor), impMessageProcessors["906"]);
			AssertEquals(typeof(IE928MessageProcessor), impMessageProcessors["928"]);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		processor = new BECIncomingMessageProcessor();
	}
	BECIncomingMessageProcessor processor;

	BEMessage CreateMessage(ZString applicationReference, ZString messageNum, ZString messageType, ZString messageSubType, ZString messageText)
	{
		var message = Factory.New<BEMessage>();
		message.EM_ApplicationReference = applicationReference;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = messageType;
		message.EM_MessageNum = messageNum;
		message.EM_MessageSubType = messageSubType;
		message.EM_MessageText = messageText;
		message.EM_Status = EDIMessage.Status.Queued;
		Factory.Save();
		return message;
	}

	sealed class BECIncomingMessageProcessorForTest : BECIncomingMessageProcessor
	{
		public BECIncomingMessageProcessorForTest() : base()
		{
		}

		public bool MessageShouldBeProcessedInASeparateFactoryExposed => MessageShouldBeProcessedInASeparateFactory;
	}
}
