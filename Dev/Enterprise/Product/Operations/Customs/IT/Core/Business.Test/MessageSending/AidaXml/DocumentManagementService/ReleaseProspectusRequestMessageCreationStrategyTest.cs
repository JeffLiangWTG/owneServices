using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class ReleaseProspectusRequestMessageCreationStrategyTest : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<ReleaseProspectusRequestMessageCreationStrategy>
{
	protected override string ExpectedXmlMessage => @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:RichiestaProspettoSvincolo xmlns:q1=""http://documenti.tracciati.xsd.fascicoloele.domest.dogane.finanze.it"">
  <input>
    <datiDichiarazione>
      <mrn>123456789</mrn>
    </datiDichiarazione>
  </input>
</q1:RichiestaProspettoSvincolo>";

	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ReleaseProspectusRequest;

	protected override string ExpectedMessageSubType => EDIMessageTypeList.Codes.ReleaseProspectusRequest;

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSaWNoaWVzdGFQcm9zcGV0dG9TdmluY29sbyB4bWxucz0iaHR0cDovL2RvY3VtZW50aS50cmFjY2lhdGkueHNkLmZhc2NpY29sb2VsZS5kb21lc3QuZG9nYW5lLmZpbmFuemUuaXQiPg0KICA8aW5wdXQgeG1sbnM9IiI+DQogICAgPGRhdGlEaWNoaWFyYXppb25lPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L2RhdGlEaWNoaWFyYXppb25lPg0KICA8L2lucHV0Pg0KPC9SaWNoaWVzdGFQcm9zcGV0dG9TdmluY29sbz4=";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFQcm9zcGV0dG9TdmluY29sbyB4bWxuczpxMT0iaHR0cDovL2RvY3VtZW50aS50cmFjY2lhdGkueHNkLmZhc2NpY29sb2VsZS5kb21lc3QuZG9nYW5lLmZpbmFuemUuaXQiPg0KICA8aW5wdXQ+DQogICAgPGRhdGlEaWNoaWFyYXppb25lPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L2RhdGlEaWNoaWFyYXppb25lPg0KICA8L2lucHV0Pg0KPC9xMTpSaWNoaWVzdGFQcm9zcGV0dG9TdmluY29sbz4=";
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
		=> new ReleaseProspectusRequestMessageCreationStrategy(Factory, messageGenerationContext);
}
