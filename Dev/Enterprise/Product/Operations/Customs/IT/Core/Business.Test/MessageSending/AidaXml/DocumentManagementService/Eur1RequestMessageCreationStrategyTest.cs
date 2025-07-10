using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class Eur1RequestMessageCreationStrategyTest : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<Eur1RequestMessageCreationStrategy>
{
	protected override string ExpectedXmlMessage => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestEur1RequestMessage.xml");

	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.Eur1Request;

	protected override string ExpectedMessageSubType => EDIMessageTypeList.Codes.Eur1Request;

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSaWNoaWVzdGFFdXIxIHhtbG5zPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dCB4bWxucz0iIj4NCiAgICA8ZGF0aURpY2hpYXJhemlvbmU+DQogICAgICA8bXJuPjEyMzQ1Njc4OTwvbXJuPg0KICAgIDwvZGF0aURpY2hpYXJhemlvbmU+DQogIDwvaW5wdXQ+DQo8L1JpY2hpZXN0YUV1cjE+";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFFdXIxIHhtbG5zOnExPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dD4NCiAgICA8ZGF0aURpY2hpYXJhemlvbmU+DQogICAgICA8bXJuPjEyMzQ1Njc4OTwvbXJuPg0KICAgIDwvZGF0aURpY2hpYXJhemlvbmU+DQogIDwvaW5wdXQ+DQo8L3ExOlJpY2hpZXN0YUV1cjE+";
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
		=> new Eur1RequestMessageCreationStrategy(Factory, messageGenerationContext);
}
