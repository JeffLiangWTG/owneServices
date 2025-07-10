using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class AccountingSummaryRequestMessageCreationStrategyTest : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<AccountingSummaryRequestMessageCreationStrategy>
{
	protected override string ExpectedXmlMessage => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestProspectusRequest.xml");

	protected override string ExpectedMessageType => "PRR";

	protected override string ExpectedMessageSubType => "PRR";

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSaWNoaWVzdGFQcm9zcGV0dG8geG1sbnM9Imh0dHA6Ly9kb2N1bWVudGkudHJhY2NpYXRpLnhzZC5mYXNjaWNvbG9lbGUuZG9tZXN0LmRvZ2FuZS5maW5hbnplLml0Ij4NCiAgPGlucHV0IHhtbG5zPSIiPg0KICAgIDxkYXRpRGljaGlhcmF6aW9uZT4NCiAgICAgIDxtcm4+MTIzNDU2Nzg5PC9tcm4+DQogICAgPC9kYXRpRGljaGlhcmF6aW9uZT4NCiAgPC9pbnB1dD4NCjwvUmljaGllc3RhUHJvc3BldHRvPg==";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFQcm9zcGV0dG8geG1sbnM6cTE9Imh0dHA6Ly9kb2N1bWVudGkudHJhY2NpYXRpLnhzZC5mYXNjaWNvbG9lbGUuZG9tZXN0LmRvZ2FuZS5maW5hbnplLml0Ij4NCiAgPGlucHV0Pg0KICAgIDxkYXRpRGljaGlhcmF6aW9uZT4NCiAgICAgIDxtcm4+MTIzNDU2Nzg5PC9tcm4+DQogICAgPC9kYXRpRGljaGlhcmF6aW9uZT4NCiAgPC9pbnB1dD4NCjwvcTE6UmljaGllc3RhUHJvc3BldHRvPg==";
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

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageGenerationContext)
		=> new AccountingSummaryRequestMessageCreationStrategy(Factory, messageGenerationContext);
}
