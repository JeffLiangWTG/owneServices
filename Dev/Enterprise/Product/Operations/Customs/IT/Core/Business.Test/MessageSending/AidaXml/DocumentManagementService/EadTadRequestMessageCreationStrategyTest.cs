using System;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class EadTadRequestMessageCreationStrategyTest
	: DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<EadTadRequestMessageCreationStrategy<CusEntryHeader, CusEntryHeader>, CusEntryHeader, CusEntryHeader>
{
	public void TestConstructor_WithInvalidMessageType()
	{
		var mockContext = new Mock<IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader>>();
		AssertExceptionThrown<ArgumentException>("When message type is not EAD or TAD", () => new EadTadRequestMessageCreationStrategy<CusEntryHeader, CusEntryHeader>(Factory, mockContext.Object, "XXX"));
	}

	protected override string ExpectedXmlMessage => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestEadRequestMessage.xml");

	protected override string ExpectedMessageType => "EAD";

	protected override string ExpectedMessageSubType => "EAD";

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSaWNoaWVzdGFEYWVEYXQgeG1sbnM9Imh0dHA6Ly9kb2N1bWVudGkudHJhY2NpYXRpLnhzZC5mYXNjaWNvbG9lbGUuZG9tZXN0LmRvZ2FuZS5maW5hbnplLml0Ij4NCiAgPGlucHV0IHhtbG5zPSIiPg0KICAgIDxkYXRpRGljaGlhcmF6aW9uZT4NCiAgICAgIDxtcm4+MTIzNDU2Nzg5PC9tcm4+DQogICAgPC9kYXRpRGljaGlhcmF6aW9uZT4NCiAgPC9pbnB1dD4NCjwvUmljaGllc3RhRGFlRGF0Pg==";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFEYWVEYXQgeG1sbnM6cTE9Imh0dHA6Ly9kb2N1bWVudGkudHJhY2NpYXRpLnhzZC5mYXNjaWNvbG9lbGUuZG9tZXN0LmRvZ2FuZS5maW5hbnplLml0Ij4NCiAgPGlucHV0Pg0KICAgIDxkYXRpRGljaGlhcmF6aW9uZT4NCiAgICAgIDxtcm4+MTIzNDU2Nzg5PC9tcm4+DQogICAgPC9kYXRpRGljaGlhcmF6aW9uZT4NCiAgPC9pbnB1dD4NCjwvcTE6UmljaGllc3RhRGFlRGF0Pg==";
#endif

	protected override string ExpectedUnsignedMessageText => $@"<soapenv:Envelope xmlns:type=""http://ponimport.ssi.sogei.it/type/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>SERVICE_ID</type:serviceId>
      <type:data>
        <type:xml>{encryptedXmlContent}</type:xml>
        <type:dichiarante>123454555</type:dichiarante>
      </type:data>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

	protected override CusEntryHeader GetBusinessObject()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("123456789");
		return entryHeader;
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageGenerationContext)
		=> new EadTadRequestMessageCreationStrategy<CusEntryHeader, CusEntryHeader>(Factory, messageGenerationContext, "EAD");

	protected override CusEntryHeader GetMessageParent() => BusinessObject;
}
