using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeOutgoingMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessage()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");
		Factory.Save();

		var logger = new LoggingInformation();
		var xTradeOutgoingMessageProcessor = new XTradeOutgoingMessageProcessor(logger);
		xTradeOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);
		Factory.Save();
		message.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", "SNT", message.EM_Status);
			AssertNotNull("Message Interchange", message.Interchange);
			AssertEquals("Log Count", 1, logger.UserLogStrings.Count);
			AssertEquals($"1st Log [{logger.UserLogStrings[0]}] contains expected message?", true, logger.UserLogStrings[0].EndsWith("1 message(s) have been processed."));
		});
	}

	public void TestMessageFilter()
	{
		var logger = new LoggingInformation();
		var xTradeOutgoingMessageProcessor = new XTradeOutgoingMessageProcessorForTest(logger);
		AssertEquals("Message Filter", "EM_ApplicationCode = 'ITH'", xTradeOutgoingMessageProcessor.MessageFilterExposed.LiteralTextADO);
	}

	public void TestCreateNewInterchangeProvider()
	{
		var logger = new LoggingInformation();
		var xTradeOutgoingMessageProcessor = new XTradeOutgoingMessageProcessorForTest(logger);
		AssertType<XTradeInterchangeProvider>("Interchange Provider Type", xTradeOutgoingMessageProcessor.CreateNewInterchangeProviderExposed());
	}
}

class XTradeOutgoingMessageProcessorForTest : XTradeOutgoingMessageProcessor
{
	public XTradeOutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	public ZQuery MessageFilterExposed => MessageFilter;

	public InterchangeProviderBase CreateNewInterchangeProviderExposed() => CreateNewInterchangeProvider(new NonDependentEDIMessageCollection(new BusinessObjectFactory()));
}
