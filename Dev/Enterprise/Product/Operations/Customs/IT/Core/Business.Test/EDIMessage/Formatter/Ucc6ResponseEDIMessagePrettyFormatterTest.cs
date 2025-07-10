using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ResponseEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettyMessage_WithValidEncodedMessage()
	{
		var formatter = new Ucc6ResponseEDIMessagePrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_BASE64_VALID_ENCODED_DATA_MESSAGE);

		var expectedMessage = XDocument.Parse(VALID_DATA_OUTPUT).ToString();
		var actualFormattedMessage = XDocument.Parse(formattedMessage).ToString();

		AssertEquals("Prettified Text", expectedMessage, actualFormattedMessage);
	}

	public void TestPrettyMessage_WithNoDataElement()
	{
		var formatter = new Ucc6ResponseEDIMessagePrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_WITH_NO_DATA_ELEMENT);
		AssertEquals("Prettified Text", SOAP_WITH_NO_DATA_ELEMENT, formattedMessage);
	}

	public void TestPrettyMessage_WithAnEmptyDataElement()
	{
		var formatter = new Ucc6ResponseEDIMessagePrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_WITH_EMPTY_DATA_ELEMENT);
		AssertEquals("Prettified Text", SOAP_WITH_EMPTY_DATA_ELEMENT, formattedMessage);
	}

	public void TestPrettyMessage_WithAnInvalidContent()
	{
		const string expectedMessage = "<h2>The message formatter was unable to parse received message.</h2><br /><h2>Please use \"Message Text\" tab page to inspect the raw message content.</h2>";

		var unknownResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_InvalidAcknowledgement.xml");

		var formatter = new Ucc6ResponseEDIMessagePrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(unknownResponse);
		AssertEquals("Prettified Text", expectedMessage, formattedMessage);
	}

	const string SOAP_BASE64_VALID_ENCODED_DATA_MESSAGE = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <soapenv:Body>
      <ns3:recuperaEsitoResponse xmlns:ns3=""http://service.ws.sogei.it"">
         <recuperaEsitoReturn>
            <IUT>20220114D11000069297</IUT>
            <esito>
               <codice>200</codice>
            </esito>
            <data>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9InllcyI/PjxkYXRhPjxEYXRhT3JhSW5pRWxhYj48L0RhdGFPcmFJbmlFbGFiPjxEYXRhT3JhRmluRWxhYj48L0RhdGFPcmFGaW5FbGFiPjxTdGF0bz4zPC9TdGF0bz48TXJuPjIySVRRWFQwNENFOTgxNTVSMjwvTXJuPjxMcm4+MjAyMkZHU0dSSDEzTDdNMDAwMDA0MTwvTHJuPjxJbmZvcm1hemlvbmU+PENvZGljZT4wPC9Db2RpY2U+PERlc2NyaXppb25lPkxhIGRpY2hpYXJhemlvbmUgaWRlbnRpZmljYXRhIGNvbiBMUk4gMjAyMkZHU0dSSDEzTDdNMDAwMDA0MSBlJyBzdGF0YSBhY2NldHRhdGEgY29uIGF0dHJpYnV6aW9uZSBkZWwgc2VndWVudGUgTVJOIDIySVRRWFQwNENFOTgxNTVSMi48L0Rlc2NyaXppb25lPjxMaXZlbGxvPkM8L0xpdmVsbG8+PC9JbmZvcm1hemlvbmU+PC9kYXRhPg==</data>
            <dataRegistrazione>2022-01-14+01:00</dataRegistrazione>
         </recuperaEsitoReturn>
      </ns3:recuperaEsitoResponse>
   </soapenv:Body>
</soapenv:Envelope>";

	const string VALID_DATA_OUTPUT = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<data>
  <DataOraIniElab></DataOraIniElab>
  <DataOraFinElab></DataOraFinElab>
  <Stato>3</Stato>
  <Mrn>22ITQXT04CE98155R2</Mrn>
  <Lrn>2022FGSGRH13L7M0000041</Lrn>
  <Informazione>
    <Codice>0</Codice>
    <Descrizione>La dichiarazione identificata con LRN 2022FGSGRH13L7M0000041 e' stata accettata con attribuzione del seguente MRN 22ITQXT04CE98155R2.</Descrizione>
    <Livello>C</Livello>
  </Informazione>
</data>";

	const string SOAP_WITH_NO_DATA_ELEMENT = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <soapenv:Body>
      <ns3:recuperaEsitoResponse xmlns:ns3=""http://service.ws.sogei.it"">
         <recuperaEsitoReturn>
            <IUT>20220114D11000069297</IUT>
            <esito>
               <codice>200</codice>
            </esito>
            <dataRegistrazione>2022-01-14+01:00</dataRegistrazione>
         </recuperaEsitoReturn>
      </ns3:recuperaEsitoResponse>
   </soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_EMPTY_DATA_ELEMENT = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <soapenv:Body>
      <ns3:recuperaEsitoResponse xmlns:ns3=""http://service.ws.sogei.it"">
         <recuperaEsitoReturn>
            <IUT>20220114D11000069297</IUT>
            <esito>
               <codice>200</codice>
            </esito>
			<data></data>
            <dataRegistrazione>2022-01-14+01:00</dataRegistrazione>
         </recuperaEsitoReturn>
      </ns3:recuperaEsitoResponse>
   </soapenv:Body>
</soapenv:Envelope>";
}
