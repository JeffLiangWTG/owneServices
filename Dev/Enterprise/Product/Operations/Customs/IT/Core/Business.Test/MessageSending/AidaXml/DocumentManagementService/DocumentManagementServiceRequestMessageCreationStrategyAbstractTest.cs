using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

public abstract class DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<TStrategy, TBusiness, TMessageParent> : TestCaseWithFactory
	where TStrategy : DocumentManagementServiceRequestMessageCreationStrategy<TBusiness, TMessageParent>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	[TestDate(2022, 10, 01)]
	public void TestGenerateMessage()
	{
		var mauCertificatePK = Guid.NewGuid();
		var messageGenerationContext = CreateMessageGenerationContext(mauCertificatePK);
		var messageGenerator = GetMessageCreationStrategy(messageGenerationContext);
		var generatedMessage = messageGenerator.GenerateMessage();

		AssertGeneratedMessage(mauCertificatePK, generatedMessage, "QUE", ExpectedSignedMessageText);
	}

	[TestDate(2022, 10, 01)]
	public void TestGenerateMessage_WhenUserHasAutomaticSignature()
	{
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		var mauCertificatePK = Guid.NewGuid();
		var messageGenerationContext = CreateMessageGenerationContext(mauCertificatePK);
		var messageGenerator = GetMessageCreationStrategy(messageGenerationContext);
		var generatedMessage = messageGenerator.GenerateMessage();

		AssertGeneratedMessage(mauCertificatePK, generatedMessage, "QUE", ExpectedUnsignedMessageText);
	}

	public void TestCreateXmlMessage()
	{
		IDocumentManagementServiceRequestMessageCreationStrategy messageGenerator = (TStrategy)GetMessageCreationStrategy(CreateMessageGenerationContext(Guid.NewGuid()));
		var expectedContent = XmlUtil.HandleNamespaces(ExpectedXmlMessage);
		var actualContent = messageGenerator.CreateXmlMessage();
		AssertMultilineASCIIEquals(expectedContent, IgnoreMeaninglessDiffs(actualContent));
	}

	string IgnoreMeaninglessDiffs(string xml)
	{
		return xml.Replace("<input xmlns=\"\">", "<input>");
	}

	protected abstract string ExpectedXmlMessage { get; }

	protected abstract string ExpectedMessageType { get; }

	protected abstract string ExpectedMessageSubType { get; }

	protected abstract IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(IDocumentManagementServiceRequestContext<TBusiness, TMessageParent> messageGenerationContext);

	protected IGlbMauExternalPassword CreatelbMauExternalPassword(Guid mauCertificatePk)
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(mauCertificatePk);
		mauCertificateMock.Setup(m => m.DeclarantTaxNumber).Returns("123454555");
		return mauCertificateMock.Object;
	}

	protected ICryptokiGlbExternalPassword CreateCryptokiGlbExternalPassword()
	{
		var cryptokeiCertificateMock = new Mock<ICryptokiGlbExternalPassword>();
		cryptokeiCertificateMock.Setup(c => c.GP_Name).Returns("BIT4ID");
		cryptokeiCertificateMock.Setup(c => c.GP_CertificateSerialNumber).Returns("SR123");
		cryptokeiCertificateMock.Setup(c => c.TokenPinStore.GetPin()).Returns("PIN1");
		return cryptokeiCertificateMock.Object;
	}

	protected IAidaXmlSigner CreateAidaXmlSigner()
	{
		var signedXmlBytesForTest = Encoding.UTF8.GetBytes("<soap>MESSAGE</soap>");
		var xmlSigner = new Mock<IAidaXmlSigner>();
		xmlSigner
			.Setup(x => x.Sign(It.IsAny<byte[]>(), It.IsAny<ICryptokiGlbExternalPassword>(), It.IsAny<DateTime>()))
			.Returns(signedXmlBytesForTest);
		return xmlSigner.Object;
	}

	protected virtual IDocumentManagementServiceRequestContext<TBusiness, TMessageParent> CreateMessageGenerationContext(Guid mauCertificatePk)
	{
		var contextMock = new Mock<IDocumentManagementServiceRequestContext<TBusiness, TMessageParent>>();
		contextMock.Setup(ctx => ctx.BusinessObject).Returns(BusinessObject);
		contextMock.Setup(ctx => ctx.MessageParent).Returns(MessageParent);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(CreatelbMauExternalPassword(mauCertificatePk));
		contextMock.Setup(ctx => ctx.CryptokiCertificate).Returns(CreateCryptokiGlbExternalPassword());
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(CreateAidaXmlSigner());
		contextMock.Setup(ctx => ctx.ServiceId).Returns("SERVICE_ID");

		return contextMock.Object;
	}

	const string ExpectedSignedMessageText = @"<soapenv:Envelope xmlns:type=""http://ponimport.ssi.sogei.it/type/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>SERVICE_ID</type:serviceId>
      <type:data>
        <type:xml>PHNvYXA+TUVTU0FHRTwvc29hcD4=</type:xml>
        <type:dichiarante>123454555</type:dichiarante>
      </type:data>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

	protected abstract string ExpectedUnsignedMessageText { get; }

	protected abstract TBusiness GetBusinessObject();
	TBusiness businessObject;
	protected TBusiness BusinessObject => businessObject ??= GetBusinessObject();

	protected abstract TMessageParent GetMessageParent();
	TMessageParent messageParent;
	TMessageParent MessageParent => messageParent ??= GetMessageParent();

	void AssertGeneratedMessage(Guid mauCertificatePK, ITEDIMessage generatedMessage, ZString expectedMessageStatus, ZString expectedMessageText)
	{
		AssertNotNull("Generated Message", generatedMessage);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationCode), "ITH", generatedMessage.EM_ApplicationCode);
			AssertEquals(nameof(ITEDIMessage.EM_MessageType), ExpectedMessageType, generatedMessage.EM_MessageType);
			AssertEquals(nameof(ITEDIMessage.EM_MessageSubType), ExpectedMessageSubType, generatedMessage.EM_MessageSubType);
			AssertEquals(nameof(ITEDIMessage.EM_ReceiveTransmit), "TRX", generatedMessage.EM_ReceiveTransmit);
			AssertEquals(nameof(ITEDIMessage.EM_Status), expectedMessageStatus, generatedMessage.EM_Status);
			AssertEquals(nameof(ITEDIMessage.EM_LinkedObject), MessageParent, generatedMessage.EM_LinkedObject);
			AssertEquals(nameof(ITEDIMessage.EM_GP), mauCertificatePK, generatedMessage.EM_GP);
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationReference), ZString.Empty, generatedMessage.EM_ApplicationReference);
			AssertMultilineASCIIEquals(nameof(ITEDIMessage.EM_MessageText), expectedMessageText, generatedMessage.EM_MessageText);
		});
	}
}

public abstract class DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<TStrategy, TBusiness>
	: DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<TStrategy, TBusiness, TBusiness>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TStrategy : DocumentManagementServiceRequestMessageCreationStrategy<TBusiness>
{
	protected override TBusiness GetMessageParent() => BusinessObject;
}

public abstract class DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<TStrategy> : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<TStrategy, CusEntryHeader>
	where TStrategy : DocumentManagementServiceRequestMessageCreationStrategy<CusEntryHeader>
{
	protected override CusEntryHeader GetBusinessObject()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("123456789");
		return entryHeader;
	}
}
