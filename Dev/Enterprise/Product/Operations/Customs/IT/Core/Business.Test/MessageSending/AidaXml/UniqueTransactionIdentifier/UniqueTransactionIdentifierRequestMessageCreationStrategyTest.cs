using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier.Testing;

sealed class UniqueTransactionIdentifierRequestMessageCreationStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new UniqueTransactionIdentifierRequestMessageCreationStrategy(factory: null, messageCreationContext: GetMessageCreationContext()));
			AssertExceptionThrown<ArgumentNullException>("When messageCreationContext is null", () => new UniqueTransactionIdentifierRequestMessageCreationStrategy(Factory, messageCreationContext: null));
		});
	}

	public void TestGenerateMessage()
	{
		var requestContext = GetMessageCreationContext();
		var messageCreationStrategy = (IOutgoingCustomsMessageCreationStrategy)new UniqueTransactionIdentifierRequestMessageCreationStrategy(Factory, requestContext);
		var generatedMessage = messageCreationStrategy.GenerateMessage();

		const string expectedSoapMessage = @"<soapenv:Envelope xmlns:ser=""http://service.ws.sogei.it"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <ser:recuperaEsito>
      <iut>20220307D11000328189</iut>
    </ser:recuperaEsito>
  </soapenv:Body>
</soapenv:Envelope>";

		AssertNotNull("Generated Message", generatedMessage);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationCode), "ITH", generatedMessage.EM_ApplicationCode);
			AssertEquals(nameof(ITEDIMessage.EM_MessageType), "IUT", generatedMessage.EM_MessageType);
			AssertEquals(nameof(ITEDIMessage.EM_MessageSubType), "XXX", generatedMessage.EM_MessageSubType);
			AssertEquals(nameof(ITEDIMessage.EM_ReceiveTransmit), "TRX", generatedMessage.EM_ReceiveTransmit);
			AssertEquals(nameof(ITEDIMessage.EM_Status), "QUE", generatedMessage.EM_Status);
			AssertEquals(nameof(ITEDIMessage.EM_LinkedObject), dummyBusinessObject, generatedMessage.EM_LinkedObject);
			AssertEquals(nameof(ITEDIMessage.EM_GP), requestContext.MauCertificate.PK, generatedMessage.EM_GP);
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationReference), "IUT", generatedMessage.EM_ApplicationReference);
			AssertMultilineASCIIEquals(nameof(ITEDIMessage.EM_MessageText), expectedSoapMessage, generatedMessage.EM_MessageText);
		});
	}

	public void TestMessageNumberStrategyForGeneratedMessage()
	{
		var requestContext = GetMessageCreationContext();
		var messageCreationStrategy = (IOutgoingCustomsMessageCreationStrategy)new UniqueTransactionIdentifierRequestMessageCreationStrategy(Factory, requestContext);
		var generatedMessage = messageCreationStrategy.GenerateMessage();

		AssertNotNull("Generated Message", generatedMessage);
		AssertEquals(nameof(ITEDIMessage.MessageNumberStrategy), "ABCDEF123456", generatedMessage.MessageNumberStrategy.GetMessageReferenceNumber());
	}

	protected override void SetUp()
	{
		base.SetUp();
		dummyBusinessObject = Factory.New<DummyBusinessObject>();
	}

	DummyBusinessObject dummyBusinessObject;

	IUniqueTransactionIdentifierRequestContext GetMessageCreationContext()
	{
		var mauCertificateMock = new Mock<IGlbExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.BrettsGuid);

		var requestContextMock = new Mock<IUniqueTransactionIdentifierRequestContext>();
		requestContextMock.Setup(x => x.RequestParent).Returns(dummyBusinessObject);
		requestContextMock.Setup(x => x.UniqueTransactionID).Returns("20220307D11000328189");
		requestContextMock.Setup(x => x.MauCertificate).Returns(mauCertificateMock.Object);
		requestContextMock.Setup(x => x.MessageNumber).Returns("ABCDEF123456");
		return requestContextMock.Object;
	}
}
