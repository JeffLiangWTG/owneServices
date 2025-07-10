using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class SummaryProspectusDownloadMessageCreationStrategyTest : DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<SummaryProspectusDownloadMessageCreationStrategy>
{
	protected override string ExpectedXmlMessage => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestAccountingSummaryDownload.xml");

	protected override string ExpectedMessageType => "SPD";

	protected override string ExpectedMessageSubType => "SPD";

#if NET
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxEb3dubG9hZFByb3NwZXR0byB4bWxucz0iaHR0cDovL2RvY3VtZW50aS50cmFjY2lhdGkueHNkLmZhc2NpY29sb2VsZS5kb21lc3QuZG9nYW5lLmZpbmFuemUuaXQiPg0KICA8aW5wdXQgeG1sbnM9IiI+DQogICAgPGRhdGlEaWNoaWFyYXppb25lPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L2RhdGlEaWNoaWFyYXppb25lPg0KICAgIDxyaWNoaWVzdGFQcm9zcGV0dG8+DQogICAgICA8SVVUPjIwMjQwNzI1RDEyMDUwMTM4MTEyPC9JVVQ+DQogICAgPC9yaWNoaWVzdGFQcm9zcGV0dG8+DQogIDwvaW5wdXQ+DQo8L0Rvd25sb2FkUHJvc3BldHRvPg==";
#else
	readonly string encryptedXmlContent = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpEb3dubG9hZFByb3NwZXR0byB4bWxuczpxMT0iaHR0cDovL2RvY3VtZW50aS50cmFjY2lhdGkueHNkLmZhc2NpY29sb2VsZS5kb21lc3QuZG9nYW5lLmZpbmFuemUuaXQiPg0KICA8aW5wdXQ+DQogICAgPGRhdGlEaWNoaWFyYXppb25lPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L2RhdGlEaWNoaWFyYXppb25lPg0KICAgIDxyaWNoaWVzdGFQcm9zcGV0dG8+DQogICAgICA8SVVUPjIwMjQwNzI1RDEyMDUwMTM4MTEyPC9JVVQ+DQogICAgPC9yaWNoaWVzdGFQcm9zcGV0dG8+DQogIDwvaW5wdXQ+DQo8L3ExOkRvd25sb2FkUHJvc3BldHRvPg==";
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
		var entryHeader = base.GetBusinessObject();
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = "SPR";
		message.IsTransmitMessage = false;
		message.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestAccountingSummaryRequestReceived.xml");
		return entryHeader;
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> messageGenerationContext)
		=> new SummaryProspectusDownloadMessageCreationStrategy(Factory, messageGenerationContext);
}
