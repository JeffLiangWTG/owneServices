using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.RemoteDesktopServices;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AidaXmlOutgoingCustomsMessageCreationStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var valuesProviderMock = new Mock<IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider>();
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new AidaXmlOutgoingCustomsMessageCreationStrategy(null, valuesProviderMock.Object));
		AssertExceptionThrown<ArgumentNullException>("valuesProvider is required", () => new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, null));
	}

	[TestDate(2022, 1, 2)]
	public void TestGenerateMessage()
	{
		var parentBizObj = Factory.New<DummyBusinessObject>();

		var mauCertificatePK = Guid.NewGuid();
		var signerMock = SetUpSignerMock();
		var valuesProviderMock = SetUpValuesProviderMock(parentBizObj, signerMock, mauCertificatePK);

		var generator = (IOutgoingCustomsMessageCreationStrategy)new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, valuesProviderMock.Object);
		var generatedMessage = generator.GenerateMessage();
		AssertType<ITEDIMessage>("Type", generatedMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", "ITH", generatedMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "ABC", generatedMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "H1", generatedMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", "TRX", generatedMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", generatedMessage.EM_Status);
			AssertXMLEquals("EM_MessageText", ExpectedSignedMessageText, generatedMessage.EM_MessageText);
			AssertSame("EM_LinkedObject", parentBizObj, generatedMessage.EM_LinkedObject);
			AssertEquals("MessageReferenceNumber", "2022EDIDAT000000000001", generatedMessage.MessageNumberStrategy.GetMessageReferenceNumber());
			AssertEquals("EM_GP", mauCertificatePK, generatedMessage.EM_GP);
			AssertEquals("EM_ApplicationReference", "XXX", generatedMessage.EM_ApplicationReference);
		});
	}

	[TestDate(2022, 1, 2)]
	public void TestGenerateMessage_WithAutomaticSignatureModeEnabledAndDisabled()
	{
		var parentBizObj = Factory.New<DummyBusinessObject>();
		var mauCertificatePK = Guid.NewGuid();
		var signerMock = SetUpSignerMock();
		var valuesProviderMockWithOutSignature = SetUpValuesProviderMock(parentBizObj, signerMock, mauCertificatePK, false);

		valuesProviderMockWithOutSignature.Setup(x => x.HasValidAutomaticSignature).Returns(false);
		IOutgoingCustomsMessageCreationStrategy generator = new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, valuesProviderMockWithOutSignature.Object);
		var generatedMessage = generator.GenerateMessage();
		AssertType<ITEDIMessage>("Type", generatedMessage);
		AssertGeneratedMessage(generatedMessage, "QUE", ExpectedSignedMessageText, "2022EDIDAT000000000001");

		var valuesProviderMockWithSignature = SetUpValuesProviderMock(parentBizObj, signerMock, mauCertificatePK, true);

		valuesProviderMockWithSignature.Setup(x => x.HasValidAutomaticSignature).Returns(true);
		generator = new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, valuesProviderMockWithSignature.Object);
		generatedMessage = generator.GenerateMessage();
		AssertGeneratedMessage(generatedMessage, "QUE", ExpectedUnsignedMessageText, "2022EDIDAT000000000002");

		void AssertGeneratedMessage(ITEDIMessage generatedMessage, string expectedStatus, string expectedMessageText, string expectedMessageReferenceNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", "ITH", generatedMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "ABC", generatedMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", "H1", generatedMessage.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", generatedMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", expectedStatus, generatedMessage.EM_Status);
				AssertXMLEquals("EM_MessageText", expectedMessageText, generatedMessage.EM_MessageText);
				AssertSame("EM_LinkedObject", parentBizObj, generatedMessage.EM_LinkedObject);
				AssertEquals("MessageReferenceNumber", expectedMessageReferenceNumber, generatedMessage.MessageNumberStrategy.GetMessageReferenceNumber());
				AssertEquals("EM_GP", mauCertificatePK, generatedMessage.EM_GP);
				AssertEquals("EM_ApplicationReference", "XXX", generatedMessage.EM_ApplicationReference);
			});
		}
	}

	[TestDate(2021, 3, 2)]
	public void TestDifferentLocalRefNumbersAreGeneratedForEachNewMessage()
	{
		var parentBizObj = Factory.New<DummyBusinessObject>();

		var mauCertificatePK = Guid.NewGuid();
		var signerMock = SetUpSignerMock();
		var valuesProviderMock = SetUpValuesProviderMock(parentBizObj, signerMock, mauCertificatePK);

		var firstMessageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, valuesProviderMock.Object);
		var secondMessageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory.CreateNewFactory(), valuesProviderMock.Object);

		var firstMessage = firstMessageGenerator.GenerateMessage();
		var secondMessage = secondMessageGenerator.GenerateMessage();

		AssertEquals("LRN Number for First Message", "2021EDIDAT000000000001", firstMessage.MessageNumberStrategy.GetMessageReferenceNumber());
		AssertEquals("LRN Number for Second Message", "2021EDIDAT000000000002", secondMessage.MessageNumberStrategy.GetMessageReferenceNumber());

		TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
		var thirdMessageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory.CreateNewFactory(), valuesProviderMock.Object);
		var thirdMessage = thirdMessageGenerator.GenerateMessage();
		AssertEquals("After changing current time, MessageReferenceNumber", "2021EDIDAT000000000003", thirdMessage.MessageNumberStrategy.GetMessageReferenceNumber());
	}

	public void TestZDateTimeUtcNowKind()
	{
		var now = ZDateTime.UtcNow;

		CombineAssertions(() =>
		{
			AssertEquals("ZDateTime.UtcNow Kind", DateTimeKind.Utc, now.Kind);
			AssertEquals("ZDateTime.UtcNow.ToDateTime Kind", DateTimeKind.Utc, now.ToDateTime().Kind);
		});
	}

	public void TestCryptoApiClientFromObjectFactory()
	{
		var terminalService = new Mock<TerminalService>();
		terminalService.Setup(x => x.IsWTSSession).Returns(true);
		terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);
		ObjectFactory.Substitute(terminalService.Object);
		AssertEquals("CryptoApiClient", ObjectFactory.Get<ICryptoApi>().GetType().Name);
	}

	public void TestGenerateMessage_InvalidContext()
	{
		var messageGenerationContextMock = new Mock<IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider>();
		var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, messageGenerationContextMock.Object);
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Parent", () => messageGenerator.GenerateMessage());
			messageGenerationContextMock.Setup(c => c.Parent).Returns(Factory.New<DummyBusinessObject>());

			AssertExceptionThrown<ArgumentNullException>("XML Signer", () => messageGenerator.GenerateMessage());
			messageGenerationContextMock.Setup(c => c.XmlSigner).Returns(new Mock<IAidaXmlSigner>().Object);

			AssertExceptionThrown<ArgumentNullException>("CryptokiCertificate", () => messageGenerator.GenerateMessage());
		});
	}

	Mock<IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider> SetUpValuesProviderMock(DummyBusinessObject parentBizObj
		, Mock<IAidaXmlSigner> signerMock
		, ZGuid mauCertificatePk, bool automaticSignatureEnabled = false)
	{
		var testSerializedMessage =
			FormattableString.Invariant($"serialized message-{ITEDIMessage.ITMessageNumberPlaceholder}");

		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(x => x.PK).Returns(mauCertificatePk);
		mauCertificateMock.Setup(m => m.DeclarantTaxNumber).Returns("1234567890");

		var cryptokiCertificateMock = new Mock<ICryptokiGlbExternalPassword>();
		cryptokiCertificateMock.Setup(c => c.GP_Name).Returns("BIT4ID");
		cryptokiCertificateMock.Setup(c => c.GP_CertificateSerialNumber).Returns("SR123");
		cryptokiCertificateMock.Setup(c => c.TokenPinStore.GetPin()).Returns("PIN1");

		var valuesProviderMock = new Mock<IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider>();
		valuesProviderMock.Setup(x => x.ServiceTypeNamespace).Returns("ns");
		valuesProviderMock.Setup(x => x.ServiceTypePrefix).Returns("prf");
		valuesProviderMock.Setup(x => x.MauCertificate).Returns(mauCertificateMock.Object);

		if (automaticSignatureEnabled)
		{
			valuesProviderMock.Setup(x => x.CryptokiCertificate).Throws(new InvalidOperationException("Cannot get the Cryptoki certificate for the current user"));
		}
		else
		{
			valuesProviderMock.Setup(x => x.CryptokiCertificate).Returns(cryptokiCertificateMock.Object);
		}
		valuesProviderMock.Setup(x => x.XmlSigner).Returns(signerMock.Object);
		valuesProviderMock.Setup(x => x.GetApplicationReference()).Returns("XXX");
		valuesProviderMock.Setup(x => x.GetSubType()).Returns("H1");
		valuesProviderMock.Setup(x => x.CustomsMessageText).Returns(testSerializedMessage);
		valuesProviderMock.Setup(x => x.Parent).Returns(parentBizObj);
		valuesProviderMock.Setup(x => x.GetMessageType()).Returns("ABC");
		valuesProviderMock.Setup(x => x.GetServiceId()).Returns("SRVID");
		valuesProviderMock.Setup(x => x.LocalReferenceNumberGenerator).Returns(new LocalReferenceNumberGenerator(Factory));
		return valuesProviderMock;
	}

	Mock<IAidaXmlSigner> SetUpSignerMock()
	{
		var signerMock = new Mock<IAidaXmlSigner>();
		signerMock
			.Setup(x => x.Sign(It.IsAny<byte[]>(), It.IsAny<ICryptokiGlbExternalPassword>(), It.IsAny<DateTime>()))
			.Returns((byte[] input, ICryptokiGlbExternalPassword password, DateTime signatureTime) => ApplyFakeSignature(input, signatureTime));
		return signerMock;
	}

	byte[] ApplyFakeSignature(byte[] input, DateTime signatureTime)
	{
		var inputXml = Encoding.UTF8.GetString(input);
		var applySignature = string.Concat(inputXml, "<Signature><SigningTime>", signatureTime.ToString("yyyy-MM-dd HH:mm:ss"), "</SigningTime></Signature>");
		return Encoding.UTF8.GetBytes(applySignature);
	}

	const string ExpectedSignedMessageText = @"<soapenv:Envelope xmlns:prf=""ns"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <prf:Input>
      <prf:serviceId>SRVID</prf:serviceId>
      <prf:data>
        <prf:xml>c2VyaWFsaXplZCBtZXNzYWdlLTIwMjJFRElEQVQwMDAwMDAwMDAwMDE8U2lnbmF0dXJlPjxTaWduaW5nVGltZT4yMDIyLTAxLTAyIDAwOjAwOjAwPC9TaWduaW5nVGltZT48L1NpZ25hdHVyZT4=</prf:xml>
        <prf:dichiarante>1234567890</prf:dichiarante>
      </prf:data>
    </prf:Input>
  </soapenv:Body>
</soapenv:Envelope>";

	const string ExpectedUnsignedMessageText = @"<soapenv:Envelope xmlns:prf=""ns"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <prf:Input>
      <prf:serviceId>SRVID</prf:serviceId>
      <prf:data>
        <prf:xml>c2VyaWFsaXplZCBtZXNzYWdlLTIwMjJFRElEQVQwMDAwMDAwMDAwMDI=</prf:xml>
        <prf:dichiarante>1234567890</prf:dichiarante>
      </prf:data>
    </prf:Input>
  </soapenv:Body>
</soapenv:Envelope>";
}
