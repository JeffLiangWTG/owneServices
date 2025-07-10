using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

class CHOOutboundMessageProcessorTest : TestCaseWithFactory
{
	public void TestGetMessageFilterQuery() => CombineAssertions(() =>
	{
		var message1 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: MessageTypeCodeList.Codes.Export, messageText: ZString.Empty);
		var message2 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsPassar, messageType: MessageTypeCodeList.Codes.PassarNcts, messageText: ZString.Empty);
		var message3 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsCharteraOutput, messageType: MessageTypeCodeList.Codes.REQ, messageText: ZString.Empty);
		Factory.Save();

		var logger = new LoggingInformation();
		var processor = new CHOOutboundMessageProcessor(logger);
		processor.ProcessMessage(CancellationToken.None);

		message1.Reload();
		AssertEquals($"EM_ApplicationCode={message1.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
		message2.Reload();
		AssertEquals($"EM_ApplicationCode={message2.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
		message3.Reload();
		AssertNotEquals($"EM_ApplicationCode={message3.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message3.EM_Status);
	});
}
