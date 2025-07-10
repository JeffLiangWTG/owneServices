using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SoapMessageOutputPrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettyMessage_WithValidEncodedMessage()
	{
		var formatter = new SoapMessageOutputPrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_RESPONSE_WITH_VALID_ENCODING);

		var expectedMessage = XDocument.Parse(VALID_RESPONSE_DATA).ToString();
		var actualFormattedMessage = XDocument.Parse(formattedMessage).ToString();

		AssertNotNullOrEmpty("Prettified Text", formattedMessage);
		AssertEquals("Prettified Text", expectedMessage, actualFormattedMessage);
	}

	public void TestPrettyMessage_EmptyDataElement()
	{
		var formatter = new SoapMessageOutputPrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_RESPONSE_WITH_EMPTY_DATA);
		AssertEquals("Prettified Text", SOAP_RESPONSE_WITH_EMPTY_DATA, formattedMessage);
	}

	public void TestPrettyMessage_NoDataElement()
	{
		var formatter = new SoapMessageOutputPrettyFormatter(Factory);
		var formattedMessage = formatter.GetFormattedText(SOAP_RESPONSE_WITH_NO_DATA_ELEMENT);
		AssertEquals("Prettified Text", SOAP_RESPONSE_WITH_NO_DATA_ELEMENT, formattedMessage);
	}

	const string SOAP_RESPONSE_WITH_VALID_ENCODING = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://ponimport.ssi.sogei.it/type/"">
         <ns2:IUT>20220530D12000451314</ns2:IUT>
         <ns2:esito>
            <ns2:codice>199</ns2:codice>
            <ns2:messaggio>Elaborazione OK: completata senza esito finale</ns2:messaggio>
         </ns2:esito>
         <ns2:data>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPG5zMDpSaWNoaWVzdGFEb2N1bWVudGlEaWNoaWFyYXppb25lIHhtbG5zOm5zMD0iaHR0cDovL2RvY3VtZW50aS50cmFjY2lhdGkueHNkLmZhc2NpY29sb2VsZS5kb21lc3QuZG9nYW5lLmZpbmFuemUuaXQiPgogICA8b3V0cHV0PgogICAgICA8ZGljaGlhcmF6aW9uZT4KICAgICAgICAgPGxybj4yMDIyWFRBQ09IMTAwMTAwMDAwMDc8L2xybj4KICAgICAgICAgPHV0ZW50ZUludmlvPjEzMTQ5NjAwMTUwPC91dGVudGVJbnZpbz4KICAgICAgICAgPGRpY2hpYXJhbnRlPklUMTMxNDk2MDAxNTA8L2RpY2hpYXJhbnRlPgogICAgICAgICA8aW1wb3J0YXRvcmU+SVQwMTAwMTA1MDI1OTwvaW1wb3J0YXRvcmU+CiAgICAgICAgIDxmaXJtYXRhcmlvPlJCTENTVDcyTDIxRjIwNVE8L2Zpcm1hdGFyaW8+CiAgICAgICAgIDxtcm4+MjJJVFEwQjA0QUEwMzEwMlI0PC9tcm4+CiAgICAgICAgIDxtb2RhbGl0YUFjcXVpc2l6aW9uZT5VU1I8L21vZGFsaXRhQWNxdWlzaXppb25lPgogICAgICA8L2RpY2hpYXJhemlvbmU+CiAgICAgIDxhcnRpY29saT4KICAgICAgICAgPHNpbmdvbG8+MTwvc2luZ29sbz4KICAgICAgICAgPGNvZGljZUFydGljb2xvPjgyMDE5MDAwMDA8L2NvZGljZUFydGljb2xvPgogICAgICAgICA8c3RhdG9GYXNjaWNvbG8+QUMuPC9zdGF0b0Zhc2NpY29sbz4KICAgICAgICAgPGNvZGljZUVzaXRvQ0RDPlZNPC9jb2RpY2VFc2l0b0NEQz4KICAgICAgICAgPGNvbXBsZXRhdG8+ZmFsc2U8L2NvbXBsZXRhdG8+CiAgICAgIDwvYXJ0aWNvbGk+CiAgICAgIDxkb2N1bWVudGlTaW5nb2xpPgogICAgICAgICA8Y29kaWNlPk4zODA8L2NvZGljZT4KICAgICAgICAgPGlkZW50aWZpY2F0aXZvRG9jdW1lbnRvPjIwMjEtQ04tVE9ELzE4NzAzPC9pZGVudGlmaWNhdGl2b0RvY3VtZW50bz4KICAgICAgICAgPGFubm8+MjAyMjwvYW5ubz4KICAgICAgICAgPHNpbmdvbGk+MTwvc2luZ29saT4KICAgICAgICAgPHJpY2hpZXN0bz50cnVlPC9yaWNoaWVzdG8+CiAgICAgICAgIDxwcmVzZW50ZT5mYWxzZTwvcHJlc2VudGU+CiAgICAgICAgIDxhbHRybz5mYWxzZTwvYWx0cm8+CiAgICAgIDwvZG9jdW1lbnRpU2luZ29saT4KICAgICAgPGRvY3VtZW50aVNpbmdvbGk+CiAgICAgICAgIDxjb2RpY2U+WTAyNDwvY29kaWNlPgogICAgICAgICA8aWRlbnRpZmljYXRpdm9Eb2N1bWVudG8+MjAyMS1JVC1BRU9GMTIzNDU1PC9pZGVudGlmaWNhdGl2b0RvY3VtZW50bz4KICAgICAgICAgPGFubm8+MjAyMjwvYW5ubz4KICAgICAgICAgPHNpbmdvbGk+MTwvc2luZ29saT4KICAgICAgICAgPHJpY2hpZXN0bz5mYWxzZTwvcmljaGllc3RvPgogICAgICAgICA8cHJlc2VudGU+ZmFsc2U8L3ByZXNlbnRlPgogICAgICAgICA8YWx0cm8+ZmFsc2U8L2FsdHJvPgogICAgICA8L2RvY3VtZW50aVNpbmdvbGk+CiAgICAgIDxkb2N1bWVudGlTaW5nb2xpPgogICAgICAgICA8Y29kaWNlPk45MzQ8L2NvZGljZT4KICAgICAgICAgPGlkZW50aWZpY2F0aXZvRG9jdW1lbnRvPi08L2lkZW50aWZpY2F0aXZvRG9jdW1lbnRvPgogICAgICAgICA8YW5ubz4yMDIyPC9hbm5vPgogICAgICAgICA8c2luZ29saT4xPC9zaW5nb2xpPgogICAgICAgICA8cmljaGllc3RvPnRydWU8L3JpY2hpZXN0bz4KICAgICAgICAgPHByZXNlbnRlPmZhbHNlPC9wcmVzZW50ZT4KICAgICAgICAgPGFsdHJvPmZhbHNlPC9hbHRybz4KICAgICAgPC9kb2N1bWVudGlTaW5nb2xpPgogICAgICA8ZXNpdG8+CiAgICAgICAgIDxjb2RpY2VFcnJvcmU+MDwvY29kaWNlRXJyb3JlPgogICAgICAgICA8bWVzc2FnZ2lvRXJyb3JlPmRpY2hpYXJhemlvbmUgdHJvdmF0YTwvbWVzc2FnZ2lvRXJyb3JlPgogICAgICA8L2VzaXRvPgogICA8L291dHB1dD4KPC9uczA6UmljaGllc3RhRG9jdW1lbnRpRGljaGlhcmF6aW9uZT4K</ns2:data>
         <ns2:dataRegistrazione>2022-05-30+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string VALID_RESPONSE_DATA = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ns0:RichiestaDocumentiDichiarazione xmlns:ns0=""http://documenti.tracciati.xsd.fascicoloele.domest.dogane.finanze.it"">
   <output>
      <dichiarazione>
         <lrn>2022XTACOH10010000007</lrn>
         <utenteInvio>13149600150</utenteInvio>
         <dichiarante>IT13149600150</dichiarante>
         <importatore>IT01001050259</importatore>
         <firmatario>RBLCST72L21F205Q</firmatario>
         <mrn>22ITQ0B04AA03102R4</mrn>
         <modalitaAcquisizione>USR</modalitaAcquisizione>
      </dichiarazione>
      <articoli>
         <singolo>1</singolo>
         <codiceArticolo>8201900000</codiceArticolo>
         <statoFascicolo>AC.</statoFascicolo>
         <codiceEsitoCDC>VM</codiceEsitoCDC>
         <completato>false</completato>
      </articoli>
      <documentiSingoli>
         <codice>N380</codice>
         <identificativoDocumento>2021-CN-TOD/18703</identificativoDocumento>
         <anno>2022</anno>
         <singoli>1</singoli>
         <richiesto>true</richiesto>
         <presente>false</presente>
         <altro>false</altro>
      </documentiSingoli>
      <documentiSingoli>
         <codice>Y024</codice>
         <identificativoDocumento>2021-IT-AEOF123455</identificativoDocumento>
         <anno>2022</anno>
         <singoli>1</singoli>
         <richiesto>false</richiesto>
         <presente>false</presente>
         <altro>false</altro>
      </documentiSingoli>
      <documentiSingoli>
         <codice>N934</codice>
         <identificativoDocumento>-</identificativoDocumento>
         <anno>2022</anno>
         <singoli>1</singoli>
         <richiesto>true</richiesto>
         <presente>false</presente>
         <altro>false</altro>
      </documentiSingoli>
      <esito>
         <codiceErrore>0</codiceErrore>
         <messaggioErrore>dichiarazione trovata</messaggioErrore>
      </esito>
   </output>
</ns0:RichiestaDocumentiDichiarazione>";

	const string SOAP_RESPONSE_WITH_EMPTY_DATA = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://ponimport.ssi.sogei.it/type/"">
         <ns2:IUT>20220530D12000451314</ns2:IUT>
         <ns2:esito>
            <ns2:codice>199</ns2:codice>
            <ns2:messaggio>Elaborazione OK: completata senza esito finale</ns2:messaggio>
         </ns2:esito>
         <ns2:data></ns2:data>
         <ns2:dataRegistrazione>2022-05-30+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_RESPONSE_WITH_NO_DATA_ELEMENT = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://ponimport.ssi.sogei.it/type/"">
         <ns2:IUT>20220530D12000451314</ns2:IUT>
         <ns2:esito>
            <ns2:codice>199</ns2:codice>
            <ns2:messaggio>Elaborazione OK: completata senza esito finale</ns2:messaggio>
         </ns2:esito>
         <ns2:dataRegistrazione>2022-05-30+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";
}
