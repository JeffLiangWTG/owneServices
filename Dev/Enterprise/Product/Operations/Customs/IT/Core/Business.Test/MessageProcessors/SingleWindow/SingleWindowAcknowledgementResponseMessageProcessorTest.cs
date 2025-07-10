using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowAcknowledgementResponseMessageProcessorTest : SingleWindowIncomingMessageProcessorTest<SingleWindowAcknowledgementResponseMessageProcessor>
{
	public void TestProcessPositiveAcknowledgement() => TestProcessAcknowledgement("WSA");
	public void TestProcessNegativeAcknowledgement() => TestProcessAcknowledgement("WSE");

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "WSA", "WSE" };

	protected override SingleWindowAcknowledgementResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowAcknowledgementResponseMessageProcessor(logger);

	void TestProcessAcknowledgement(string messageType)
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageType: messageType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		var acknowledgmentMessage = entryHeader.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageType == messageType);
		AssertNotNull($"A {messageType} Message is expected", acknowledgmentMessage);
	}
}
