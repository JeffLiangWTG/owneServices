using System;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowInterchangeProviderTest : ITInterchangeProviderBaseTest
{
	public void TestInterchangeHeaderTextStrategy()
	{
		var fieldsProviderMock = new Mock<IOutgoingInterchangeHeaderTextFieldsProvider>();
		fieldsProviderMock.Setup(m => m.AccountNumber).Returns("11111111111-001");
		fieldsProviderMock.Setup(m => m.MessageType).Returns("A");

		var sadInterchangeProviderForTest = new SingleWindowInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		var headerTextStrategy = sadInterchangeProviderForTest.GetInterchangeHeaderTextStrategyExposed(fieldsProviderMock.Object);

		AssertExceptionThrown<ArgumentNullException>("fieldsProvider is required", () => sadInterchangeProviderForTest.GetInterchangeHeaderTextStrategyExposed(null));

		var expectedXML =
			"<ITMessage>" +
				"<MessageType>A</MessageType>" +
				"<AccountNumber>11111111111-001</AccountNumber>" +
			"</ITMessage>";
		AssertXMLEquals("Expected XML", expectedXML, headerTextStrategy.GetText());
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new SingleWindowInterchangeProvider(collection);

	protected override string ExpectedInterchangeHeaderText =>
		$"<ITMessage>" +
			$"<MessageType>{MessageProcessorConstants.InterchangeTypes.SingleWindowRequest}</MessageType>" +
			$"<AccountNumber>11111111111-001</AccountNumber>" +
		$"</ITMessage>";

	protected override string MessageTypeToTest => MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
}

class SingleWindowInterchangeProviderForTest : SingleWindowInterchangeProvider
{
	public SingleWindowInterchangeProviderForTest(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	public IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategyExposed(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider) => GetInterchangeHeaderTextStrategy(fieldsProvider);
}
