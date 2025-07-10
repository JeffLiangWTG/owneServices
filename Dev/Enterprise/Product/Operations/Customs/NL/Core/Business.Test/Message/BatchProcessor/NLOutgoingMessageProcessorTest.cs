using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLOutgoingMessageProcessorTest : TestCaseWithFactory
{
	[TestDate(2022, 03, 01, 15, 01, 23)]
	public void TestProcess()
	{
		var messages = new NonDependentEDIMessageCollection(Factory);
		var message1 = Factory.New<NLEDIMessage>();
		message1.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message1.EM_MessageSubType = ExportSendMessageTypes.Codes.AMD;
		message1.EM_MessageText = "TestMessage1";
		message1.EM_MessageOwner = "CW1_Test";
		message1.EM_Status = EDIMessage.Status.Queued;

		var message2 = Factory.New<NLEDIMessage>();
		message2.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message2.EM_MessageSubType = ExportSendMessageTypes.Codes.AMD;
		message2.EM_MessageText = "TestMessage2";
		message2.EM_MessageOwner = "CW1_Test";
		message2.EM_Status = EDIMessage.Status.Queued;
		messages.AddRange(new NLEDIMessage[] { message1, message2 });

		Factory.Save();
		var messageProcessor = new NLOutgoingMessageProcessor(new LoggingInformationForTesting());
		messageProcessor.ProcessMessage(CancellationToken.None);
		message1.Reload();
		message2.Reload();

		CombineAssertions(() =>
		{
			AssertNotEquals("Each message should have its own interchange", message1.EM_EI, message2.EM_EI);
			AssertNotNull("Interchange message 1", message1.Interchange);
			AssertNotNull("Interchange message 2", message2.Interchange);
			AssertEquals("Message 1 - status should be sent", EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals("Message 2 - status should be sent", EDIMessage.Status.Sent, message2.EM_Status);
			var interchange = message1.Interchange;
			AssertEquals("Interchange - Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("Interchange - From", message1.Company.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("Interchange - BodyText", "TestMessage1", interchange.EI_BodyText);
			AssertEquals("Footer Text is empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("Interchange - Direction", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("Interchange - Transport Type", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
		});
	}

	public void TestOutgoingMessageTypes()
	{
		var messageTypeList = NLOutgoingMessageProcessor.GetOutgoingMessageTypes().ToList();
		CombineAssertions(() =>
		{
			Assert("OutgoingMessageTypes should contain DMS", messageTypeList.Contains(NLEDIMessageTypes.Codes.DMS));
			Assert("OutgoingMessageTypes should contain NCT", messageTypeList.Contains(NLEDIMessageTypes.Codes.NCT));
		});
	}
}
