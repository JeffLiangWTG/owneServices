using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ElectronicFolderQueryEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettyMessage_WithValidEncodedMessage()
	{
		message.EM_MessageText = SOAP_RESPONSE_WITH_VALID_ENCODING;
		var formattedMessage = message.EM_MessageInterpretation;

		var expectedMessage = XDocument.Parse(VALID_RESPONSE_DATA).ToString();
		var actualFormattedMessage = XDocument.Parse(formattedMessage).ToString();

		AssertNotNullOrEmpty("Prettified Text", formattedMessage);
		AssertEquals("Prettified Text", expectedMessage, actualFormattedMessage);
	}

	public void TestPrettyMessage_EmptyXmlElement()
	{
		message.EM_MessageText = SOAP_RESPONSE_WITH_EMPTY_XML;
		AssertEquals("Prettified Text", SOAP_RESPONSE_WITH_EMPTY_XML, message.EM_MessageInterpretation);
	}

	public void TestPrettyMessage_NoDataElement()
	{
		message.EM_MessageText = SOAP_RESPONSE_WITH_NO_DATA_ELEMENT;
		AssertEquals("Prettified Text", SOAP_RESPONSE_WITH_NO_DATA_ELEMENT, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.New<ITEDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.ElectronicFolderQuery;
	}

	ITEDIMessage message;

	const string SOAP_RESPONSE_WITH_VALID_ENCODING = @"<soapenv:Envelope xmlns:type=""http://ponimport.ssi.sogei.it/type/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>richiestaListaDocumentiDichiarazione</type:serviceId>
      <type:data>
        <type:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxxMTpSaWNoaWVzdGFEb2N1bWVudGlEaWNoaWFyYXppb25lIHhtbG5zOnExPSJodHRwOi8vZG9jdW1lbnRpLnRyYWNjaWF0aS54c2QuZmFzY2ljb2xvZWxlLmRvbWVzdC5kb2dhbmUuZmluYW56ZS5pdCI+DQogIDxpbnB1dD4NCiAgICA8cmljaGllc3RhPg0KICAgICAgPG1ybj4yMklUUVYwOFQwMDAzNTQ3VDU8L21ybj4NCiAgICA8L3JpY2hpZXN0YT4NCiAgPC9pbnB1dD4NCjwvcTE6UmljaGllc3RhRG9jdW1lbnRpRGljaGlhcmF6aW9uZT4=</type:xml>
        <type:dichiarante>11111111111</type:dichiarante>
      </type:data>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

	const string VALID_RESPONSE_DATA = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:RichiestaDocumentiDichiarazione xmlns:q1=""http://documenti.tracciati.xsd.fascicoloele.domest.dogane.finanze.it"">
  <input>
    <richiesta>
      <mrn>22ITQV08T0003547T5</mrn>
    </richiesta>
  </input>
</q1:RichiestaDocumentiDichiarazione>";

	const string SOAP_RESPONSE_WITH_EMPTY_XML = @"<soapenv:Envelope xmlns:type=""http://ponimport.ssi.sogei.it/type/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>richiestaListaDocumentiDichiarazione</type:serviceId>
      <type:data>
        <type:xml></type:xml>
        <type:dichiarante>11111111111</type:dichiarante>
      </type:data>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_RESPONSE_WITH_NO_DATA_ELEMENT = @"<soapenv:Envelope xmlns:type=""http://ponimport.ssi.sogei.it/type/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>richiestaListaDocumentiDichiarazione</type:serviceId>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";
}
