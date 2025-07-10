using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static NUnit.Framework.Assertion;
using static NUnit.Framework.AssertionWithHtml;

namespace Enterprise.Customs.ES.Business.MessageSending.Testing
{
	static class TestingData
	{
		internal static ESEDIMessage CreateOutboundMessage(this BusinessObjectFactory factory, ZString messageType, ZString messageText
			, ZString certificateName
			, ZGuid certificatePK
			, string applicationCode = ApplicationCodeList.Codes.ESCustomsMessage
			, string direction = EDIMessage.Direction.Transmit
			, string status = EDIMessage.Status.Queued
)
		{
			var message = factory.New<ESEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageText = messageText;
			message.EM_IsTestMessage = true;
			message.EM_ApplicationReference = certificateName;
			message.EM_GP = certificatePK;
			return message;
		}

		internal static ZString XMLMessageText => @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Ent.xsd"">
  <SegmentosDeServicio Id = ""ES2001091613234560"" fecha=""20200109"" hora=""161323"" Test=""S"" />
  <NumeroDeReferencia>21ES00999912345678</NumeroDeReferencia>
  <DatosEnATC>S</DatosEnATC>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>";

		internal static ZString NPEMessageText => @"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adex/ws/predua/NotifPreDUAV1.wsdl"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>NIF22222222</NifDeclarante>
    <NombreDeclarante>Declarant Full Name</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>";

		internal static ZString EdifactMessageText => "UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200109:1513+1++&EE++++1'UNH+1+CUSDEC:1:921:UN:ECSR02'BGM+EAL+1234123444'CST++++++11ES00280110000101'LOC+42+ES::141:000801'LOC+43+0811::148+BCN010::148'DTM+128:20190815:102'NAD+1+1210244B::148+MIDIRECCION.CORREO.EN.CASTILLAYLEON:@MIXMAIL.COM+GUTIERREZ S.A.'UNT+8+1'UNZ+1+1'";

		internal static ZString EdifactInterchangeText => "VIA=WTGCW1ES&DAT=UNB%2BUNOA:1%2B1210244B:ZZ%2BAEATADUE:ZZ%2B200109:1513%2B1%2B%2B%26EE%2B%2B%2B%2B1'UNH%2B1%2BCUSDEC:1:921:UN:ECSR02'BGM%2BEAL%2B1234123444'CST%2B%2B%2B%2B%2B%2B11ES00280110000101'LOC%2B42%2BES::141:000801'LOC%2B43%2B0811::148%2BBCN010::148'DTM%2B128:20190815:102'NAD%2B1%2B1210244B::148%2BMIDIRECCION.CORREO.EN.CASTILLAYLEON:@MIXMAIL.COM%2BGUTIERREZ S.A.'UNT%2B8%2B1'UNZ%2B1%2B1'&FIR=MIIKHAYJKoZIhvcNAQcCoIIKDTCCCgkCAQExDzANBglghkgBZQMEAgEFADALBgkqhkiG9w0BBwGggghUMIIIUDCCBzigAwIBAgIQPrFOSV04sUde8zyHf7ujEzANBgkqhkiG9w0BAQsFADBNMQswCQYDVQQGEwJFUzERMA8GA1UECgwIRk5NVC1SQ00xDjAMBgNVBAsMBUNFUkVTMRswGQYDVQQDDBJBQyBSZXByZXNlbnRhY2nDs24wHhcNMjAwNjI0MTE0NDA3WhcNMjIwNjI0MTE0NDA3WjCB4zE4MDYGA1UEDQwvUmVmOkFFQVQvQUVBVDAwMzAvUFVFU1RPIDEvMzcxMDQvMjQwNjIwMjAxMzQwNTQxGDAWBgNVBAUTD0lEQ0VTLVk3ODU3MzYxVDEOMAwGA1UEKgwFUEVURVIxETAPBgNVBAQMCFdJTExJQU1TMTAwLgYDVQQDDCdZNzg1NzM2MVQgUEVURVIgV0lMTElBTVMgKFI6IEE3ODU4NzI2OCkxGDAWBgNVBGEMD1ZBVEVTLUE3ODU4NzI2ODERMA8GA1UECgwIVEFSSUMgU0ExCzAJBgNVBAYTAkVTMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAq6FPf9anhM0rDPh5WyZXT8xZuWeWMiHm0tPPUhLXfp36SB2jBG%2F3PhvvMpLZI%2BnUS%2BsPR2lsRPpj0BTuGwQQTG7J558k4foWGiM4azgC859LmMA%2B%2FW547D08SVLtk2qOQRFEwlV9JLt4qW6yWtrwdwVRR2lt8W8DC0MgTkbbdTXPLirGfmFO%2F%2F8m6zm%2BAmZN%2F66tRmNb1h%2FgR5azIh5ONdBwb1DdBwblzPIzoVhT%2BmoFafUV4f9pMnT2OqbMHDDXVJDl6N4kGyuoRjBZPR10Kf8IF7TrjUZG51Ow%2BgHlS8uyBDKL0MmT8r5XLr8Q%2Fc%2Br18RdC8ZEw3rXtw9CZeIS0QIDAQABo4IEkzCCBI8wgbwGA1UdEQSBtDCBsYEhUGV0ZXIuV2lsbGlhbXNAd2lzZXRlY2hnbG9iYWwuY29tpIGLMIGIMR4wHAYJKwYBBAGsZgEHDA9WQVRFUy1BNzg1ODcyNjgxFzAVBgkrBgEEAaxmAQYMCFRBUklDIFNBMR4wHAYJKwYBBAGsZgEEDA9JRENFUy1ZNzg1NzM2MVQxFzAVBgkrBgEEAaxmAQIMCFdJTExJQU1TMRQwEgYJKwYBBAGsZgEBDAVQRVRFUjAMBgNVHRMBAf8EAjAAMA4GA1UdDwEB%2FwQEAwIF4DAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwQwgYIGCCsGAQUFBwEBBHYwdDA9BggrBgEFBQcwAYYxaHR0cDovL29jc3ByZXAuY2VydC5mbm10LmVzL29jc3ByZXAvT2NzcFJlc3BvbmRlcjAzBggrBgEFBQcwAoYnaHR0cDovL3d3dy5jZXJ0LmZubXQuZXMvY2VydHMvQUNSRVAuY3J0MB0GA1UdDgQWBBSGL4nRDdQWK9tNkFvxSwHarmx0cjCCATwGA1UdIASCATMwggEvMIIBFQYKKwYBBAGsZgMLAjCCAQUwKQYIKwYBBQUHAgEWHWh0dHA6Ly93d3cuY2VydC5mbm10LmVzL2RwY3MvMIHXBggrBgEFBQcCAjCBygyBx0NlcnRpZmljYWRvIGN1YWxpZmljYWRvIGRlIHJlcHJlc2VudGFudGUgZGUgcC4ganVyw61kaWNhIGVuIHN1cyByZWxhY2lvbmVzIGNvbiBsYXMgQUFQUC4gU3VqZXRvIGEgY29uZGljaW9uZXMgZGUgdXNvIHNlZ8O6biBsYSBEUEMgZGUgRk5NVC1SQ00sIE5JRjogUTI4MjYwMDQtSiAoQy9Kb3JnZSBKdWFuIDEwNi0yODAwOS1NYWRyaWQtRXNwYcOxYSkwCQYHBACL7EABADAJBgdghVQBAwUIMIGnBggrBgEFBQcBAwSBmjCBlzAIBgYEAI5GAQEwEwYGBACORgEGMAkGBwQAjkYBBgEwaQYGBACORgEFMF8wLRYnaHR0cHM6Ly93d3cuY2VydC5mbm10LmVzL3Bkcy9QRFNfZXMucGRmEwJlczAuFihodHRwczovL3d3dy5jZXJ0LmZubXQuZXMvcGRzL1BEU19lbi5wZGYgEwJlbjALBgYEAI5GAQMCAQ8wHwYDVR0jBBgwFoAU3FCWn9cxickR5O%2BWX%2FZfglJGYlMwgeEGA1UdHwSB2TCB1jCB06CB0KCBzYaBnWxkYXA6Ly9sZGFwcmVwLmNlcnQuZm5tdC5lcy9DTj1DUkwxMDkwLE9VPUFDJTIwUmVwcmVzZW50YWNpb24sT1U9Q0VSRVMsTz1GTk1ULVJDTSxDPUVTP2NlcnRpZmljYXRlUmV2b2NhdGlvbkxpc3Q7YmluYXJ5P2Jhc2U%2Fb2JqZWN0Y2xhc3M9Y1JMRGlzdHJpYnV0aW9uUG9pbnSGK2h0dHA6Ly93d3cuY2VydC5mbm10LmVzL2NybHNyZXAvQ1JMMTA5MC5jcmwwDQYJKoZIhvcNAQELBQADggEBACZtF55kLhnEUXDsGdnuE5xEyHvUfxMa86TGwVz9Tw8BbAuoJXycf3QCu%2FPbe4Fm%2BYTIqiOQj9%2BiKeVEuU%2FyVYcUO%2FBbMAcB5L20x1Q%2B241f%2Bq8nvOgfYWoNjgo%2FR6mp7cxxIhuCcXhrT6xA9QfsDn%2F5OPOukZ53O1hgXaDQSET8iCUdbpHOAPCb8pzL%2FgXH0DV4q4tH1pV%2B5Ls8AixWsOn8J%2BJekXX1cWq%2BiAIujfHGkFIMnIYUdPAnYYvbKrPAb2c4btPuqYABvrxGDl78DOIReGFCwj30NJh01fsjb5ebGrDkJtYQmH%2B89nSSeT0TfWmfucaD4mRs7ogYwmOvwCAxggGMMIIBiAIBATBhME0xCzAJBgNVBAYTAkVTMREwDwYDVQQKDAhGTk1ULVJDTTEOMAwGA1UECwwFQ0VSRVMxGzAZBgNVBAMMEkFDIFJlcHJlc2VudGFjacOzbgIQPrFOSV04sUde8zyHf7ujEzANBglghkgBZQMEAgEFADANBgkqhkiG9w0BAQEFAASCAQA7p2v%2FLiDchF9TzEYQxtFGvtHCJophR53kq2jl40rj0hCywnXMIKRMvDNsrXErOFOnzqn1cq%2B7%2BCmHkfPEStY1E%2BY6ELH24YKypl%2B7whLPa1bJ6gw8BYJ1fhHcYl5E03MLOfe%2FLYcYMmabzb1aNHyo0eQmhnAL7ZHzKRX9sOs7TNowauBOMhjJVXELE9EEvN8uwgkPVJFz5ShjpFgwlyXDCC%2BG9GzmmI%2BsUsZDL0FhSdYXpSIENHxogcSic5pywwAv2ILVOdh5gYovlI%2FuXnvVRWv7eYN7LeG9djEJEf%2BSMgeMCePpeGlP43pJFfiGoGnaHrxsYBz%2B%2BDWFPRtzCBWd";

		internal static ZString GetHeaderTextXML(ZString entryReferenceNumber, ZString messageNum) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>{0}</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <Service>ConsultaImportacionV2Service</Service>
  <Operation>ConsultaImportacionV2</Operation>
  <SentEDIMessageNumber>{1}</SentEDIMessageNumber>
</Headers>", entryReferenceNumber, messageNum);

		internal static ZString GetHeaderTextEdifactAndDOC(ZString entryReferenceNumber, ZString messageNum) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>{0}</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <SentEDIMessageNumber>{1}</SentEDIMessageNumber>
</Headers>", entryReferenceNumber, messageNum);

		internal static ZString GetHeaderTextNPEForEHub(ZString entryReferenceNumber, ZString messageNum) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>{0}</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <SentEDIMessageNumber>{1}</SentEDIMessageNumber>
</Headers>", entryReferenceNumber, messageNum);

		internal static ZString GetHeaderTextNPEForXT(ZString mrn) => "{\"custom.ES.MRNToRequest\":\"" + mrn + "\"}";

		internal static void AssertOutgoingInterchange(ZString declarationType, EDIInterchanges.ESEDIInterchange interchange, string expectedMessageType, string expectedDestination
, string expectedTransportType, string expectedStatus, ZGuid expectedMessageGP, string expectedHeader = null, string expectedBody = null)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"{declarationType} interchange.EI_TransportType", expectedTransportType, interchange.EI_TransportType);
				AssertEquals($"{declarationType} interchange.EI_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, interchange.EI_ApplicationCode);
				AssertEquals($"{declarationType} interchange.EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals($"{declarationType} interchange.EI_InterchangeType", expectedMessageType, interchange.EI_InterchangeType);
				AssertEquals($"{declarationType} interchange.EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals($"{declarationType} interchange.EI_To", expectedDestination, interchange.EI_To);
				AssertEquals($"{declarationType} interchange.EI_Status", expectedStatus, interchange.EI_Status);
				AssertEquals($"{declarationType} interchange.EI_Priority", "HGH", interchange.EI_Priority);
				AssertNotEquals($"{declarationType} interchange.EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
				AssertEquals($"{declarationType} interchange.EI_GP", expectedMessageGP, interchange.EI_GP);
				if (expectedHeader != null)
				{
					AssertMultilineASCIIEquals($"{declarationType} interchange.EI_HeaderText", expectedHeader, interchange.EI_HeaderText);
				}
				if (expectedBody != null)
				{
					AssertEquals($"{declarationType} interchange.EI_BodyText", expectedBody, interchange.EI_BodyText);
				}
				AssertEquals($"{declarationType} interchange.EI_FooterText", ZString.Empty, interchange.EI_FooterText);
			});
		}
	}
}
