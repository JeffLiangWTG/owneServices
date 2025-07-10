using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.PBN.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

sealed class PBNMessageProcessorTest : TestCaseWithFactory
{
	public void TestApplicationCode()
	{
		var message = CreateIncomingMessage();
		var processor = new PBNMessageProcessorForTest(new LoggingInformation());
		AssertEquals("Application code should be PBN", EDIMessage.ApplicationCodes.IECustomsPBN, processor.ApplicationCode);
	}

	PBNInboundEDIMessage CreateIncomingMessage()
	{
		var message = Factory.New<PBNInboundEDIMessage>();
		message.EM_MessageNum = "123";
		message.EM_MessageType = "CPB";
		message.EM_ApplicationReference = "";
		return message;
	}
}

class PBNMessageProcessorForTest : PBNMessageProcessor<CreateAndUpdatePBNProvider>
{
	public PBNMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(CreateAndUpdatePBNProvider))
	{
	}

	protected override string MessageFriendlyNameCore => string.Empty;

	protected override void ProcessMessageCore(BusinessObjectFactory factory, PBNInboundEDIMessage message, CreateAndUpdatePBNProvider provider)
	{ }
}
