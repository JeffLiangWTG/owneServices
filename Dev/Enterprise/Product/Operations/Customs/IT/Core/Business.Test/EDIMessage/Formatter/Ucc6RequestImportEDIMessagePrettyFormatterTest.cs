using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6RequestImportEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettyMessage_WithValidEncodedMessage_IMP_NEW() => AssertPrettyFormatedMessageText(OUTPUT_DATA_REQUEST_MESSAGE_NEW_IMP, SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_NEW_IMP);

	public void TestPrettyMessage_WithValidEncodedMessage_IMP_CAN() => AssertPrettyFormatedMessageText(OUTPUT_DATA_REQUEST_MESSAGE_CAN_IMP, SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_CAN_IMP);

	public void TestPrettyMessage_WithNoDataElement_IMP_NEW() => AssertPrettyFormatedMessageText(SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_IMP, SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_IMP);

	public void TestPrettyMessage_WithNoDataElement_IMP_CAN() => AssertPrettyFormatedMessageText(SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_IMP, SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_IMP);

	public void TestPrettyMessage_WithAnEmptyDataElement_IMP_NEW() => AssertPrettyFormatedMessageText(SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_IMP, SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_IMP);

	public void TestPrettyMessage_WithAnEmptyDataElement_IMP_CAN() => AssertPrettyFormatedMessageText(SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_IMP, SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_IMP);

	#region Implementation

	void AssertPrettyFormatedMessageText(string expectedMessage, string toBeFormatedMessageWithEnvolope)
	{
		var message = Factory.New<ITEDIMessage>();
		message.EM_ApplicationReference = "IMP";
		message.EM_MessageType = EDIMessageTypeList.Codes.NewDeclaration;
		message.EM_MessageText = toBeFormatedMessageWithEnvolope;
		var parsedFormattedMessage = XDocument.Parse(message.EM_MessageInterpretation).ToString();

		var parsedExpectedMessage = XDocument.Parse(expectedMessage).ToString();

		AssertEquals("Prettyfied Text", parsedExpectedMessage, parsedFormattedMessage);
	}

	const string SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_NEW_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>invioDichiarazione</imp:serviceId>
			<imp:data>
				<imp:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxNZXNzYWdnaW8+DQogIDxEaWNoaWFyYXppb25lSDE+DQogICAgPERpY2hDb21wbD5TPC9EaWNoQ29tcGw+DQogICAgPFVsdGltb0ludmlvPlM8L1VsdGltb0ludmlvPg0KICAgIDxEYXRpSDE+DQogICAgICA8SW50ZXN0YXppb25lSDE+DQogICAgICAgIDxJbmZvcm1hemlvbmlNZXNzYWdnaW8+DQogICAgICAgICAgPFRpcG9EaWNoaWFyYXppb25lPklNPC9UaXBvRGljaGlhcmF6aW9uZT4NCiAgICAgICAgICA8VGlwb0RpY2hpYXJhemlvbmVTdXBwbGVtZW50YXJlPkE8L1RpcG9EaWNoaWFyYXppb25lU3VwcGxlbWVudGFyZT4NCiAgICAgICAgPC9JbmZvcm1hemlvbmlNZXNzYWdnaW8+DQogICAgICAgIDxSaWZlcmltZW50aU1lc3NhZ2dpRG9jdW1lbnRpQ2VydGlmaWNhdGlBdXRvcml6emF6aW9uaT4NCiAgICAgICAgICA8SW5mb3JtYXppb25pU3VwcGxlbWVudGFyaT4NCiAgICAgICAgICAgIDxEZXNjcml6aW9uZUluZm9TdXBwbD5uZXNzdW5hIGRlbGxlIHByZWNlZGVudGk8L0Rlc2NyaXppb25lSW5mb1N1cHBsPg0KICAgICAgICAgIDwvSW5mb3JtYXppb25pU3VwcGxlbWVudGFyaT4NCiAgICAgICAgICA8Q29kaWNlVGlwRG9jSWRlbnRpZmljYXRpdm8+DQogICAgICAgICAgICA8Q29kaWNlPjMzWVk8L0NvZGljZT4NCiAgICAgICAgICAgIDxJZGVudGlmaWNhdGl2b0RvY3VtZW50bz4tPC9JZGVudGlmaWNhdGl2b0RvY3VtZW50bz4NCiAgICAgICAgICA8L0NvZGljZVRpcERvY0lkZW50aWZpY2F0aXZvPg0KICAgICAgICAgIDxOdW1SaWZVQ1I+MklUMTMxNDk2MDAxNTAtQjAwMTc1ODYzPC9OdW1SaWZVQ1I+DQogICAgICAgICAgPExSTj4yMDIyV1RMUkYyMDAwMDAwMDAwMDE5PC9MUk4+DQogICAgICAgIDwvUmlmZXJpbWVudGlNZXNzYWdnaURvY3VtZW50aUNlcnRpZmljYXRpQXV0b3Jpenphemlvbmk+DQogICAgICAgIDxQYXJ0aT4NCiAgICAgICAgICA8RGljaGlhcmFudGU+DQogICAgICAgICAgICA8Tm9tZT5BQ08gWE1MIFRFU1Q8L05vbWU+DQogICAgICAgICAgICA8VmlhTnVtZXJvPlZJQSBQQUxBVElOTyAxNTwvVmlhTnVtZXJvPg0KICAgICAgICAgICAgPFBhZXNlPklUPC9QYWVzZT4NCiAgICAgICAgICAgIDxDb2RQb3N0YWxlPjIwMTQ4PC9Db2RQb3N0YWxlPg0KICAgICAgICAgICAgPENpdHRhPk1JTEFOTzwvQ2l0dGE+DQogICAgICAgICAgPC9EaWNoaWFyYW50ZT4NCiAgICAgICAgICA8SWRlbnRpZmljYXRpdm9EaWNoaWFyYW50ZT5JVDEzMTQ5NjAwMTUwPC9JZGVudGlmaWNhdGl2b0RpY2hpYXJhbnRlPg0KICAgICAgICAgIDxSYXBwcmVzZW50YW50ZSAvPg0KICAgICAgICAgIDxRdWFsaWZpY2FSYXBwcmVzZW50YW50ZT4yPC9RdWFsaWZpY2FSYXBwcmVzZW50YW50ZT4NCiAgICAgICAgPC9QYXJ0aT4NCiAgICAgICAgPEluZm9ybWF6aW9uaVZhbG9yZUltcG9zdGU+DQogICAgICAgICAgPFVuaXRhTW9uZXRhcmlhSW50ZXJuYT5FVVI8L1VuaXRhTW9uZXRhcmlhSW50ZXJuYT4NCiAgICAgICAgICA8VGFzc29DYW1iaW8+MDwvVGFzc29DYW1iaW8+DQogICAgICAgIDwvSW5mb3JtYXppb25pVmFsb3JlSW1wb3N0ZT4NCiAgICAgICAgPERhdGVUZW1waVBlcmlvZGlMdW9naGkgLz4NCiAgICAgICAgPElkZW50aWZpY2F6aW9uZU1lcmNpPg0KICAgICAgICAgIDxNYXNzYUxvcmRhPjA8L01hc3NhTG9yZGE+DQogICAgICAgICAgPFRvdGFsZUNvbGxpPjA8L1RvdGFsZUNvbGxpPg0KICAgICAgICA8L0lkZW50aWZpY2F6aW9uZU1lcmNpPg0KICAgICAgICA8SW5mb3JtYXppb25pVHJhc3BvcnRpPg0KICAgICAgICAgIDxDb250YWluZXI+MDwvQ29udGFpbmVyPg0KICAgICAgICAgIDxNb2RvRGlUcmFzcG9ydG9Gcm9udGllcmE+MDwvTW9kb0RpVHJhc3BvcnRvRnJvbnRpZXJhPg0KICAgICAgICA8L0luZm9ybWF6aW9uaVRyYXNwb3J0aT4NCiAgICAgICAgPEFsdHJpRGF0aT4NCiAgICAgICAgICA8TmF0dXJhVHJhbnNhemlvbmU+MDwvTmF0dXJhVHJhbnNhemlvbmU+DQogICAgICAgIDwvQWx0cmlEYXRpPg0KICAgICAgPC9JbnRlc3RhemlvbmVIMT4NCiAgICAgIDxBcnRpY29sb0gxPg0KICAgICAgICA8SW5mb3JtYXppb25pTWVzc2FnZ2lvPg0KICAgICAgICAgIDxOdW1lcm9BcnRpY29sbz4xPC9OdW1lcm9BcnRpY29sbz4NCiAgICAgICAgPC9JbmZvcm1hemlvbmlNZXNzYWdnaW8+DQogICAgICAgIDxSaWZlcmltZW50aU1lc3NhZ2dpRG9jdW1lbnRpQ2VydGlmaWNhdGlBdXRvcml6emF6aW9uaT4NCiAgICAgICAgICA8SW5mb3JtYXppb25pU3VwcGxlbWVudGFyaT4NCiAgICAgICAgICAgIDxEZXNjcml6aW9uZUluZm9TdXBwbD5uZXNzdW5hIGRlbGxlIHByZWNlZGVudGk8L0Rlc2NyaXppb25lSW5mb1N1cHBsPg0KICAgICAgICAgIDwvSW5mb3JtYXppb25pU3VwcGxlbWVudGFyaT4NCiAgICAgICAgPC9SaWZlcmltZW50aU1lc3NhZ2dpRG9jdW1lbnRpQ2VydGlmaWNhdGlBdXRvcml6emF6aW9uaT4NCiAgICAgICAgPFBhcnRpIC8+DQogICAgICAgIDxJbmZvcm1hemlvbmlWYWxvcmVJbXBvc3RlPg0KICAgICAgICAgIDxUb3RhbGVJbXBvc3RhPjA8L1RvdGFsZUltcG9zdGE+DQogICAgICAgICAgPEluZGljYXRvcmlWYWx1dGF6aW9uZT4wMDAwPC9JbmRpY2F0b3JpVmFsdXRhemlvbmU+DQogICAgICAgICAgPFByZXp6b0FydGljb2xvPjA8L1ByZXp6b0FydGljb2xvPg0KICAgICAgICAgIDxNZXRvZG9WYWx1dGF6aW9uZT4wPC9NZXRvZG9WYWx1dGF6aW9uZT4NCiAgICAgICAgICA8UHJlZmVyZW56ZT4wPC9QcmVmZXJlbnplPg0KICAgICAgICA8L0luZm9ybWF6aW9uaVZhbG9yZUltcG9zdGU+DQogICAgICAgIDxEYXRlVGVtcGlQZXJpb2RpTHVvZ2hpIC8+DQogICAgICAgIDxJZGVudGlmaWNhemlvbmVNZXJjaT4NCiAgICAgICAgICA8TWFzc2FOZXR0YT4wPC9NYXNzYU5ldHRhPg0KICAgICAgICAgIDxNYXNzYUxvcmRhPjA8L01hc3NhTG9yZGE+DQogICAgICAgICAgPERlc2NyaXppb25lTWVyY2k+QTE8L0Rlc2NyaXppb25lTWVyY2k+DQogICAgICAgIDwvSWRlbnRpZmljYXppb25lTWVyY2k+DQogICAgICAgIDxJbmZvcm1hemlvbmlUcmFzcG9ydGkgLz4NCiAgICAgICAgPEFsdHJpRGF0aT4NCiAgICAgICAgICA8TmF0dXJhVHJhbnNhemlvbmU+MDwvTmF0dXJhVHJhbnNhemlvbmU+DQogICAgICAgICAgPFZhbG9yZVN0YXRpc3RpY28+MDwvVmFsb3JlU3RhdGlzdGljbz4NCiAgICAgICAgPC9BbHRyaURhdGk+DQogICAgICA8L0FydGljb2xvSDE+DQogICAgPC9EYXRpSDE+DQogIDwvRGljaGlhcmF6aW9uZUgxPg0KPC9NZXNzYWdnaW8+</imp:xml>
				<imp:dichiarante>0132456789A</imp:dichiarante>
			</imp:data>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_CAN_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>annullaDichiarazione</imp:serviceId>
			<imp:data>
				<imp:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxBbm51bGxhbWVudG8+DQogIDxDYXVzYWxlPkI8L0NhdXNhbGU+DQogIDxSaWZOb3JtYT4zPC9SaWZOb3JtYT4NCjwvQW5udWxsYW1lbnRvPg==</imp:xml>
				<imp:dichiarante>01324567890</imp:dichiarante>
			</imp:data>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string OUTPUT_DATA_REQUEST_MESSAGE_NEW_IMP = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Messaggio>
	<DichiarazioneH1>
		<DichCompl>S</DichCompl>
		<UltimoInvio>S</UltimoInvio>
		<DatiH1>
			<IntestazioneH1>
				<InformazioniMessaggio>
					<TipoDichiarazione>IM</TipoDichiarazione>
					<TipoDichiarazioneSupplementare>A</TipoDichiarazioneSupplementare>
				</InformazioniMessaggio>
				<RiferimentiMessaggiDocumentiCertificatiAutorizzazioni>
					<InformazioniSupplementari>
						<DescrizioneInfoSuppl>nessuna delle precedenti</DescrizioneInfoSuppl>
					</InformazioniSupplementari>
					<CodiceTipDocIdentificativo>
						<Codice>33YY</Codice>
						<IdentificativoDocumento>-</IdentificativoDocumento>
					</CodiceTipDocIdentificativo>
					<NumRifUCR>2IT13149600150-B00175863</NumRifUCR>
					<LRN>2022WTLRF2000000000019</LRN>
				</RiferimentiMessaggiDocumentiCertificatiAutorizzazioni>
				<Parti>
					<Dichiarante>
						<Nome>ACO XML TEST</Nome>
						<ViaNumero>VIA PALATINO 15</ViaNumero>
						<Paese>IT</Paese>
						<CodPostale>20148</CodPostale>
						<Citta>MILANO</Citta>
					</Dichiarante>
					<IdentificativoDichiarante>IT13149600150</IdentificativoDichiarante>
					<Rappresentante />
					<QualificaRappresentante>2</QualificaRappresentante>
				</Parti>
				<InformazioniValoreImposte>
					<UnitaMonetariaInterna>EUR</UnitaMonetariaInterna>
					<TassoCambio>0</TassoCambio>
				</InformazioniValoreImposte>
				<DateTempiPeriodiLuoghi />
				<IdentificazioneMerci>
					<MassaLorda>0</MassaLorda>
					<TotaleColli>0</TotaleColli>
				</IdentificazioneMerci>
				<InformazioniTrasporti>
					<Container>0</Container>
					<ModoDiTrasportoFrontiera>0</ModoDiTrasportoFrontiera>
				</InformazioniTrasporti>
				<AltriDati>
					<NaturaTransazione>0</NaturaTransazione>
				</AltriDati>
			</IntestazioneH1>
			<ArticoloH1>
				<InformazioniMessaggio>
					<NumeroArticolo>1</NumeroArticolo>
				</InformazioniMessaggio>
				<RiferimentiMessaggiDocumentiCertificatiAutorizzazioni>
					<InformazioniSupplementari>
						<DescrizioneInfoSuppl>nessuna delle precedenti</DescrizioneInfoSuppl>
					</InformazioniSupplementari>
				</RiferimentiMessaggiDocumentiCertificatiAutorizzazioni>
				<Parti />
				<InformazioniValoreImposte>
					<TotaleImposta>0</TotaleImposta>
					<IndicatoriValutazione>0000</IndicatoriValutazione>
					<PrezzoArticolo>0</PrezzoArticolo>
					<MetodoValutazione>0</MetodoValutazione>
					<Preferenze>0</Preferenze>
				</InformazioniValoreImposte>
				<DateTempiPeriodiLuoghi />
				<IdentificazioneMerci>
					<MassaNetta>0</MassaNetta>
					<MassaLorda>0</MassaLorda>
					<DescrizioneMerci>A1</DescrizioneMerci>
				</IdentificazioneMerci>
				<InformazioniTrasporti />
				<AltriDati>
					<NaturaTransazione>0</NaturaTransazione>
					<ValoreStatistico>0</ValoreStatistico>
				</AltriDati>
			</ArticoloH1>
		</DatiH1>
	</DichiarazioneH1>
</Messaggio>";

	const string OUTPUT_DATA_REQUEST_MESSAGE_CAN_IMP = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Annullamento>
	<Causale>B</Causale>
	<RifNorma>3</RifNorma>
</Annullamento>";

	const string SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>invioDichiarazione</imp:serviceId>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>annullaDichiarazione</imp:serviceId>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>invioDichiarazione</imp:serviceId>
			<imp:data>
			</imp:data>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_IMP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:imp=""http://importservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<imp:Input>
			<imp:serviceId>annullaDichiarazione</imp:serviceId>
			<imp:data>
			</imp:data>
		</imp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	#endregion
}
