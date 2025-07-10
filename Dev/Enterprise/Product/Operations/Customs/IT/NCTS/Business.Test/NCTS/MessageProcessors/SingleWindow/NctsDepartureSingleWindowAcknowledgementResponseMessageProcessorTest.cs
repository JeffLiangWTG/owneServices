using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSingleWindowAcknowledgementResponseMessageProcessorTest : NctsDepartureSingleWindowIncomingMessageProcessorTest<SingleWindowAcknowledgementResponseMessageProcessor>
{
	public void TestProcessPositiveAcknowledgement()
	{
		TestProcessAcknowledgement("WSA");
	}

	public void TestProcessNegativeAcknowledgement()
	{
		TestProcessAcknowledgement("WSE");
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "WSA", "WSE" };

	protected override SingleWindowAcknowledgementResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowAcknowledgementResponseMessageProcessor(logger);

	void TestProcessAcknowledgement(string messageType)
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestData();
		receivedMessage.EM_MessageType = messageType;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		var acknowledgmentMessage = nctsHeader.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageType == messageType);
		AssertNotNull($"A {messageType} Message is expected", acknowledgmentMessage);
	}
}
