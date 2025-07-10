using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SadInterchangeProviderTest : ITInterchangeProviderBaseTest
{
	public void TestInterchangeHeaderTextStrategy()
	{
		var fieldsProviderMock = new Mock<IOutgoingInterchangeHeaderTextFieldsProvider>();
		fieldsProviderMock.Setup(m => m.AccountNumber).Returns("11111111111-001");
		fieldsProviderMock.Setup(m => m.CustomsInterchangeHeader).Returns("1234            12340101.R00                      11111111111     001 00002");
		fieldsProviderMock.Setup(m => m.MessageType).Returns("A");
		fieldsProviderMock.Setup(m => m.Node).Returns("1234");
		fieldsProviderMock.Setup(m => m.Staff).Returns("UOLLAS");

		var sadInterchangeProviderForTest = new SadInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		var headerTextStrategy = sadInterchangeProviderForTest.GetInterchangeHeaderTextStrategyExposed(fieldsProviderMock.Object);

		AssertExceptionThrown<ArgumentNullException>("fieldsProvider is required", () => sadInterchangeProviderForTest.GetInterchangeHeaderTextStrategyExposed(null));

		var expectedXML =
			"<ITMessage>" +
				"<Staff>UOLLAS</Staff>" +
				"<Node>1234</Node>" +
				"<MessageType>A</MessageType>" +
				"<AccountNumber>11111111111-001</AccountNumber>" +
				"<Header>1234            12340101.R00                      11111111111     001 00002</Header>" +
			"</ITMessage>";
		AssertXMLEquals("Expected XML", expectedXML, headerTextStrategy.GetText());
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new SadInterchangeProvider(collection);

	protected sealed override string ExpectedInterchangeHeaderText
	{
		get
		{
			var interchangeFilename = $"1234{ZDate.Today:MMdd}.{SADConstants.CustomsInterchangeType.IdocR}00";
			return
				$"<ITMessage>" +
					$"<Staff>~U1</Staff>" +
					$"<Node>1234</Node>" +
					$"<MessageType>R</MessageType>" +
					$"<AccountNumber>11111111111-001</AccountNumber>" +
					$"<Header>1234            {interchangeFilename}                      LECFER123       001 00002</Header>" +
				"</ITMessage>";
		}
	}

	protected sealed override string MessageTypeToTest => SADConstants.CustomsInterchangeType.IdocR;
}

class SadInterchangeProviderForTest : SadInterchangeProvider
{
	public SadInterchangeProviderForTest(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	public IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategyExposed(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider) => GetInterchangeHeaderTextStrategy(fieldsProvider);
}
