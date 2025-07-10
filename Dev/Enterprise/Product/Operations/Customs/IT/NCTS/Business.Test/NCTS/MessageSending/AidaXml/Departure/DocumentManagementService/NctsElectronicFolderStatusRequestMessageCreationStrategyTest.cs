using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsElectronicFolderStatusRequestMessageCreationStrategyTest
	: DocumentManagementServiceRequestMessageCreationStrategyAbstractTest<NctsElectronicFolderStatusRequestMessageCreationStrategy, NctsHeader, NctsDepartureMovementHeader>
{
	protected override string ExpectedXmlMessage => ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageSending.AidaXml.TestFiles.TestElectronicFolderStatusRequestMessage.xml");

	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ElectronicFolderQuery;

	protected override string ExpectedMessageSubType => EDIMessageTypeList.Codes.ElectronicFolderQuery;

	protected override string ExpectedUnsignedMessageText => "<soapenv:Envelope xmlns:type=\"http://ponimport.ssi.sogei.it/type/\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n  <soapenv:Header />\r\n  <soapenv:Body>\r\n    <type:Input>\r\n      <type:serviceId>SERVICE_ID</type:serviceId>\r\n      <type:data>\r\n        <type:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFEb2N1bWVudGlEaWNoaWFyYXppb25lIHhtbG5zOnExPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dD4NCiAgICA8cmljaGllc3RhPg0KICAgICAgPG1ybj4xMjM0NTY3ODk8L21ybj4NCiAgICA8L3JpY2hpZXN0YT4NCiAgPC9pbnB1dD4NCjwvcTE6UmljaGllc3RhRG9jdW1lbnRpRGljaGlhcmF6aW9uZT4=</type:xml>\r\n        <type:dichiarante>123454555</type:dichiarante>\r\n      </type:data>\r\n    </type:Input>\r\n  </soapenv:Body>\r\n</soapenv:Envelope>";

	protected override NctsHeader GetBusinessObject() => nctsHeader;

	protected override NctsDepartureMovementHeader GetMessageParent() => nctsHeader.MovementHeader;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> messageGenerationContext)
		=> new NctsElectronicFolderStatusRequestMessageCreationStrategy(Factory, messageGenerationContext);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "123456789";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
