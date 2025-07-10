using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class MessageProcessorBaseTest : TestCaseWithFactory
{
	[TestDateIncremental(seconds: 10)]
	public void TestManifestHeaderLinkedUsingMRN()
	{
		var firstManifestHeader = Factory.New<AsycudaManifestHeader>();
		firstManifestHeader.RegistrationNumber = RegistrationNumber;

		var secondManifestHeader = Factory.New<AsycudaManifestHeader>();
		secondManifestHeader.PreviousMRN = RegistrationNumber;

		var outgoingMessage = Factory.NewWithValidTestData<TestEdiMessage>();
		outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
		outgoingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
		outgoingMessage.EM_MessageNum = "ICS2TST00001";
		outgoingMessage.EM_LinkedObject = firstManifestHeader;

		var outboundInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		outboundInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
		outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
		outboundInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
		outboundInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		outboundInterchange.EI_InterchangeNum = "ICS2TST00001";
		outboundInterchange.ContainedMessages.Add(outgoingMessage);

		Factory.Save();

		var incomingMessage = GetIncomingMessage(RegistrationNumber);
		incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
		incomingMessage.EM_MessageNum = "ICS2TST00002";
		incomingMessage.EM_RetryCount = 1;

		var incomingInterchange = incomingMessage.Interchange ?? Factory.NewWithValidTestData<EDIInterchange>();
		incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
		incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
		incomingInterchange.EI_InterchangeNum = "ICS2TST00002";
		incomingMessage.EM_EI = incomingInterchange.PK;

		var logger = new LoggingInformation();
		var processor = new MessageProcessorForTest(logger);
		processor.PreProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertSame("Should link via MRN", firstManifestHeader, incomingMessage.EM_LinkedObject);
			AssertNotSame("Should not link via Previous MRN", secondManifestHeader, incomingMessage.EM_LinkedObject);
		});
	}

	TestEdiMessage GetIncomingMessage(string registrationNumber)
	{
		var incomingMessage = Factory.New<TestEdiMessage>();
		incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
		incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingMessage.EM_Status = "QUE";
		incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
															<TESTMSG xmlns=""urn:wco:datamodel:eu:ics2:2"">
																<MRN>{0}</MRN>
															</TESTMSG>", registrationNumber);
		return incomingMessage;
	}

	const string RegistrationNumber = "23DE12345678901234";
}

sealed class MessageProcessorForTest : MessageProcessor<MessageForTest>
{
	public MessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => "TESTMSG";
	protected override void ProcessMessageCore(EDIMessage message, MessageForTest messageObject)
	{
	}

	protected override Func<MessageForTest, string> GetMasterReferenceNumber => messageObject => messageObject.MRN;
}

[XmlRoot("TESTMSG", Namespace = "urn:wco:datamodel:eu:ics2:2")]
public sealed class MessageForTest
{
	public string MRN { get; set; }
}
