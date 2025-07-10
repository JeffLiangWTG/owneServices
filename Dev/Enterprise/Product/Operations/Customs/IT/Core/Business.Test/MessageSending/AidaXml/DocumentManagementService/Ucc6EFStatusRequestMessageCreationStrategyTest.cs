using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class Ucc6EFStatusRequestMessageCreationStrategyTest : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<Ucc6EFStatusRequestMessageCreationStrategy>
{
	protected override string ExpectedXmlMessage => @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:RichiestaDocumentiDichiarazione xmlns:q1=""http://documenti.tracciati.xsd.fascicoloele.domest.dogane.finanze.it"">
  <input>
    <richiesta>
      <mrn>123456789</mrn>
    </richiesta>
  </input>
</q1:RichiestaDocumentiDichiarazione>";

	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ElectronicFolderQuery;

	protected override string ExpectedMessageSubType => EDIMessageTypeList.Codes.ElectronicFolderQuery;

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSaWNoaWVzdGFEb2N1bWVudGlEaWNoaWFyYXppb25lIHhtbG5zPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dCB4bWxucz0iIj4NCiAgICA8cmljaGllc3RhPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L3JpY2hpZXN0YT4NCiAgPC9pbnB1dD4NCjwvUmljaGllc3RhRG9jdW1lbnRpRGljaGlhcmF6aW9uZT4=";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFEb2N1bWVudGlEaWNoaWFyYXppb25lIHhtbG5zOnExPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dD4NCiAgICA8cmljaGllc3RhPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L3JpY2hpZXN0YT4NCiAgPC9pbnB1dD4NCjwvcTE6UmljaGllc3RhRG9jdW1lbnRpRGljaGlhcmF6aW9uZT4=";
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
		=> new Ucc6EFStatusRequestMessageCreationStrategy(Factory, messageGenerationContext);
}
