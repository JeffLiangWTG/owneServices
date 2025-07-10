using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEOutgoingMessageProcessorTest : TestCaseWithFactory
{
	[TestDate(2022, 10, 28, 16, 11, 29)]
	public void TestProcess()
	{
		var messages = new NonDependentEDIMessageCollection(Factory);
		var message1 = Factory.New<BEMessage>();
		message1.EM_MessageType = "NCT";
		message1.EM_MessageText = "TestMessage1";
		message1.EM_MessageOwner = "CW1_Test";
		message1.EM_Status = EDIMessage.Status.Queued;

		var message2 = Factory.New<BEMessage>();
		message2.EM_MessageType = "NCT";
		message2.EM_MessageText = "TestMessage2";
		message2.EM_MessageOwner = "CW1_Test";
		message2.EM_Status = EDIMessage.Status.Queued;
		messages.AddRange(new EDIMessage[] { message1, message2 });

		Factory.Save();
		var messageProcessor = new BEOutgoingMessageProcessor(new LoggingInformationForTesting());
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
			AssertEquals("Message processor log", "2 message(s) have been processed.", messageProcessor.Logger.Logs.FirstOrDefault().ToString());
		});
	}
}
