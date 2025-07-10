using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ESCInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestCreateMesssage_Ehub()
		{
			entryHeader.Reload();

			var interchanges = new List<(ESEDIInterchange interchange, ZString messageType)>();
			var correctEDIFACTAnswerWithUNB = "UNB+UNOA:1+AEATADUE:ZZ+BUZON:ZZ+200102:1100+02110053624233++&EE'UNH+1+CUSRES:1:921:UN:ECS001'";
			var correctEDIFACTAnswer = "UNH+1+CUSRES:1:921:UN:ECS001'";
			var interchangeWithCorrectEDIFACTAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForEhub(correctEDIFACTAnswerWithUNB), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), true);
			interchanges.Add((interchangeWithCorrectEDIFACTAnswer, "EDIFACT"));

			var interchangeWithCorrectXMLAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, BodyTextSoap("ResponseWithCorrectResponse.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.DvdH2, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), true);
			interchanges.Add((interchangeWithCorrectXMLAnswer, "XML"));
			Factory.Save();
			CombineAssertions(() =>
			{
				var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
				processor.ExecuteBatch();

				foreach (var data in interchanges)
				{
					var updatedInterchange = data.interchange;
					updatedInterchange.Reload();
					AssertNull(updatedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, updatedInterchange.EI_Status);
					AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, updatedInterchange.ContainedMessages.Count);
					if (data.interchange.EI_BodyText.Contains(correctEDIFACTAnswer))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, correctEDIFACTAnswer, updatedInterchange, data.interchange.EI_InterchangeType, entryHeader.CH_BGMReference, data.interchange.EI_InterchangeNum, true); //EDIFACTMessage
					}
					else
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextSoap("ResponseWithCorrectResponse.xml"), updatedInterchange, data.interchange.EI_InterchangeType, entryHeader.CH_BGMReference, data.interchange.EI_InterchangeNum, true); //XMLMessage
					}
				}
			});
		}

		public void TestCreateMesssage_DirectxT_CorrectResponse()
		{
			entryHeader.Reload();

			var interchanges = new List<(ESEDIInterchange interchange, ZString messageType)>();

			var correctEDIFACTAnswer = "UNH+1+CUSRES:1:921:UN:ECS001'";
			var interchangeWithCorrectEDIFACTAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWithData.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), false);
			interchanges.Add((interchangeWithCorrectEDIFACTAnswer, "EDIFACT"));

			var interchangeWithCorrectXMLAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithCorrectResponse.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.DvdH2, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), false);
			interchanges.Add((interchangeWithCorrectXMLAnswer, "XML"));

			var interchangeWithCorrectDOCAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, "JVBERi0x", entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.EsDocumentRequest, new ZGuid("33059348-BB7A-4E89-B17D-27760CE8145A"), false);
			interchangeWithCorrectDOCAnswer.EI_HeaderText = "{\"custom.OriginalMsgId\":\"2357535\",\"custom.ES.DocumentFileDescription\":\"Clearance Document\",\"custom.ES.DocumentFilename\":\"23ES009999101500B4_E_AEAT_CLR.pdf\",\"custom.ES.DocumentType\":\"CLR\",}";
			interchanges.Add((interchangeWithCorrectDOCAnswer, "DOC"));

			var interchangeWithCorrectDOCTestAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, "JVBERi0x", entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.EsDocumentRequest, new ZGuid("33059348-BB7A-4E89-B17D-27760CE8145A"), false);
			interchangeWithCorrectDOCTestAnswer.EI_HeaderText = "{\"custom.OriginalMsgId\":\"2357535\",\"custom.ES.DocumentFileDescription\":\"Clearance Document\",\"custom.ES.DocumentFilename\":\"23ES009999101500B4_E_AEAT_CLR.pdf\",\"custom.ES.DocumentType\":\"CLR\",}";
			interchanges.Add((interchangeWithCorrectDOCTestAnswer, "DOCTest"));

			var interchangeWithUniversalEventDocErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, BodyTextSoap("ResponseWithUniversalEventError.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest, new ZGuid("642360B8-F691-4B1F-828F-3BB77C9EC320"), false);
			interchanges.Add((interchangeWithUniversalEventDocErrorSoap, "DOC UniversalEvent Error"));

			var interchangeWithUniversalEventDocTestErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, BodyTextSoap("ResponseWithUniversalEventError.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest, new ZGuid("642360B8-F691-4B1F-828F-3BB77C9EC320"), false);
			interchanges.Add((interchangeWithUniversalEventDocTestErrorSoap, "DOCTest UniversalEvent Error"));

			Factory.Save();
			CombineAssertions(() =>
			{
				var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
				processor.ExecuteBatch();

				foreach (var data in interchanges)
				{
					var updatedInterchange = data.interchange;
					updatedInterchange.Reload();
					AssertNull(updatedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, updatedInterchange.EI_Status);
					AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, updatedInterchange.ContainedMessages.Count);

					if (data.interchange.EI_BodyText.Contains(correctEDIFACTAnswer))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, correctEDIFACTAnswer, updatedInterchange, data.interchange.EI_InterchangeType, ZString.Empty, data.interchange.EI_InterchangeNum, false); //EDIFACTMessage
					}
					else if (data.interchange.EI_BodyText.Contains("JVBERi0x"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, ExpectedBodyTextDOC, updatedInterchange, data.interchange.EI_InterchangeType, ZString.Empty, data.interchange.EI_InterchangeNum, false); //DOCMessage
					}
					else if (data.messageType.Contains("UniversalEvent"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, ExpectedBodyTextSoapUniversalEventError, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //DOCErrorMessage
					}
					else
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, ExpectedBodyTextSoap, updatedInterchange, data.interchange.EI_InterchangeType, ZString.Empty, data.interchange.EI_InterchangeNum, false); //XMLMessage
					}
				}
			});
		}

		public void TestCreateMesssage_DirectxT_ErrorResponse_EDIFACT()
		{
			entryHeader.Reload();

			var interchanges = new List<(ESEDIInterchange interchange, ZString messageType)>();
			var interchangeWithErrorEDIFACT = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWithDataError.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), false);
			interchanges.Add((interchangeWithErrorEDIFACT, "EDIFACT Error"));

			var interchangeWithServerErrorEDIFACT = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWith500Error.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), false);
			interchanges.Add((interchangeWithServerErrorEDIFACT, "EDIFACT Server Error"));
			Factory.Save();
			CombineAssertions(() =>
			{
				var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
				processor.ExecuteBatch();

				foreach (var data in interchanges)
				{
					var updatedInterchange = data.interchange;
					updatedInterchange.Reload();
					AssertNull(updatedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, updatedInterchange.EI_Status);
					AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, updatedInterchange.ContainedMessages.Count);

					if (data.messageType.Contains("Server"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextEDIFACTServerErrorProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceError, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //EDIFACTMessage
					}
					else
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextEDIFACTErrorProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.Export, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //EDIFACTMessage
					}
				}
			});
		}

		public void TestCreateMesssage_DirectxT_ErrorResponse_SOAP()
		{
			entryHeader.Reload();

			var interchanges = new List<(ESEDIInterchange interchange, ZString messageType)>();
			var interchangeWithInvalidCertificateSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithInvalidCertificate.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, new ZGuid("9B5FBF5F-B6D2-420B-A791-3E6915F489E0"), false);
			interchanges.Add((interchangeWithInvalidCertificateSoap, "Soap Invalid Certificate"));

			var interchangeWithClientFaultErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithClientFault.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, new ZGuid("BADED322-9C93-4B89-B815-13F8242108A7"), false);
			interchanges.Add((interchangeWithClientFaultErrorSoap, "Soap Client Fault"));

			var interchangeWithDocumentErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, BodyTextSoap("ResponseWithDocumentError.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest, new ZGuid("34B59ABB-4360-4540-B5F3-FB5A142A755D"), false);
			interchanges.Add((interchangeWithDocumentErrorSoap, "Soap Document Error"));

			var interchangeWithDocumentTestErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, BodyTextSoap("ResponseWithDocumentError.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest, new ZGuid("34B59ABB-4360-4540-B5F3-FB5A142A755D"), false);
			interchanges.Add((interchangeWithDocumentTestErrorSoap, "Soap TestDocument Error"));

			var interchangeWithUniversalEventErrorSoap = CreateInterchange(SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithUniversalEventError.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, new ZGuid("2DCDC804-6550-4DB0-BEEA-FACB6565378D"), false);
			interchanges.Add((interchangeWithUniversalEventErrorSoap, "Soap UniversalEvent Error"));

			Factory.Save();
			CombineAssertions(() =>
			{
				var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
				processor.ExecuteBatch();

				foreach (var data in interchanges)
				{
					var updatedInterchange = data.interchange;
					updatedInterchange.Reload();
					AssertNull(updatedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
					AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, updatedInterchange.EI_Status);
					AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, updatedInterchange.ContainedMessages.Count);

					if (data.messageType.Contains("Certificate"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextSoapInvalidCertificateProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceError, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //SOAPMessage
					}
					else if (data.messageType.Contains("TestDocument"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextSoapDocumentErrorProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceError, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //SOAPMessage
					}
					else if (data.messageType.Contains("Document"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextSoapDocumentErrorProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceError, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //SOAPMessage
					}
					else if (data.messageType.Contains("UniversalEvent"))
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, ExpectedBodyTextSoapUniversalEventError, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //SOAPMessage
					}
					else
					{
						AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, BodyTextSoapClientFaultProcessed, updatedInterchange, DeclarationMessageTypeList.Codes.CustomsServiceError, ZString.Empty, updatedInterchange.EI_InterchangeNum, false); //SOAPMessage
					}
				}
			});
		}

		public void TestCreateMessage_ImportSOAPDescriptionTrimmed()
		{
			entryHeader.Reload();

			var interchanges = new List<(ESEDIInterchange interchange, ZString messageType)>();
			var interchangeWithCorrectXMLAnswer = CreateInterchange(SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, BodyTextSoap("ResponseWithCorrectResponseIncorrectDescriptionLength.xml"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.ImportQuery, new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A"), false);
			interchanges.Add((interchangeWithCorrectXMLAnswer, "XML"));
			Factory.Save();
			CombineAssertions(() =>
			{
				var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
				processor.ExecuteBatch();

				var data = interchanges[0];
				var updatedInterchange = data.interchange;
				updatedInterchange.Reload();
				AssertNull(updatedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
				AssertEquals(data.messageType + " interchangeInDiffFactory.EI_Status", EDIInterchange.Status.Received, updatedInterchange.EI_Status);
				AssertEquals(data.messageType + " interchangeInDiffFactory.ContainedMessages.Count", 1, updatedInterchange.ContainedMessages.Count);
				AssertMessage(updatedInterchange.ContainedMessages[0], data.messageType, ExpectedBodyTextSoapDescriptionTrimmed, updatedInterchange, data.interchange.EI_InterchangeType, ZString.Empty, data.interchange.EI_InterchangeNum, false);
			});
		}

		public void TestCreateMessagesFromConcurrentEhubAndXtInterchanges()
		{
			var edifactAnswer = "UNH+1+CUSRES:1:921:UN:ECS001'";
			var ehubInterchange = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForEhub(edifactAnswer), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), isEHub: true);
			var xtInterchange = CreateInterchange(SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, CreateBodyTextEdiFactForDirectxT("ResponseWithData.html"), entryHeader.CH_BGMReference, DeclarationMessageTypeList.Codes.Export, new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6"), isEHub: false);
			Factory.Save();

			var processor = new ESCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ESCustomsMessage });
			processor.ExecuteBatch();

			CombineAssertions("Message Count", () =>
			{
				AssertEquals("eHub", 1, ehubInterchange.ContainedMessages.Count);
				AssertEquals("xT", 1, xtInterchange.ContainedMessages.Count);
			});

			CombineAssertions("Interchange Logs, Statuses and Unwrapped Messages", () =>
			{
				AssertInterchangeAndContainedMessage("eHub", ehubInterchange, edifactAnswer, expectedAppReference: entryHeader.CH_BGMReference, expectedIsTest: true);
				AssertInterchangeAndContainedMessage("xT", xtInterchange, edifactAnswer, expectedAppReference: ZString.Empty, expectedIsTest: false);
			});
		}

		void AssertInterchangeAndContainedMessage(string msgExchangeSystem, ESEDIInterchange interchange, ZString expectedBody, ZString expectedAppReference, ZBool expectedIsTest)
		{
			interchange.Reload();
			AssertNull(interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport));
			AssertEquals(msgExchangeSystem + " Interchange => EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertMessage(interchange.ContainedMessages[0], msgExchangeSystem, expectedBody, interchange, interchange.EI_InterchangeType, expectedAppReference, interchange.EI_InterchangeNum, expectedIsTest);
		}

		void AssertMessage(EDIMessage message, ZString messageType, ZString bodyText, EDIInterchange interchange, ZString interchangeType, ZString bGMReference, ZString messageNum, bool isTest)
		{
			AssertEquals(messageType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(messageType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(messageType + " message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals(messageType + " message.EM_MessageType", interchangeType, message.EM_MessageType);
			AssertEquals(messageType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(messageType + " message.EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
			AssertEquals(messageType + " message.EM_EI", interchange.PK, message.EM_EI);
			AssertEquals(messageType + " message.EM_GB", interchange.EI_GB, message.EM_GB);
			AssertEquals(messageType + " message.EM_ApplicationReference", bGMReference, message.EM_ApplicationReference);
			AssertEquals(messageType + " message.EM_IsActive", true, message.EM_IsActive);
			AssertEquals(messageType + " message.EM_IsTestMessage", isTest, message.EM_IsTestMessage);
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageText = textReader.ReadToEnd();
				AssertEquals(messageType + " message.EM_MessageText", bodyText, messageText);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B00183715";
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
		}
		CusEntryHeader entryHeader;

		ESEDIInterchange CreateInterchange(ZString from, ZString bodyText, ZString entryRefNum, ZString interchangeType, ZGuid sessionGuid, bool isEHub)
		{
			var interchange = Factory.New<ESEDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = from;
			interchange.EI_To = "TEST";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_BodyText = bodyText;
			interchange.EI_SessionGUID = sessionGuid;
			if (isEHub)
			{
				interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
				interchange.EI_HeaderText = ZString.Format(@"
<Headers>
	<BrokerCode>AZ</BrokerCode>
	<CertificateName>CertName</CertificateName>
	<CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
	<EntryReferenceNumber>{0}</EntryReferenceNumber>
	<TestMessage>Y</TestMessage>
	<SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>
", entryRefNum);
			}
			else
			{
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			}
			return interchange;
		}

		ZString CreateBodyTextEdiFactForEhub(ZString answer)
		{
			return ZString.Format(@"
<Response>
	<DeclarationStatus>0</DeclarationStatus>
	<Respuesta>{0}</Respuesta>
</Response>
", answer);
		}

		ZString CreateBodyTextEdiFactForDirectxT(ZString fileName)
		{
			using (var stream = typeof(EdifactResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.EdifactMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		ZString BodyTextSoap(ZString fileName)
		{
			using (var stream = typeof(SoapResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.SoapMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		string ExpectedBodyTextSoap => @"<?xml version=""1.0""?><env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header /><env:Body Id=""Body""><DVDH2V1Sal xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDH2V1Sal.xsd"" xmlns:cau=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/cau/ws/CAUTiposDeDatos.xsd"" xmlns:dvdt=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/addv/h2uc/ws/DVDTiposDeDatos.xsd""><Mensaje><SegmentosDeServicio><dvdt:Id>20220923121419948549</dvdt:Id><dvdt:Remitente>ES.AEAT</dvdt:Remitente><dvdt:IdCorr>LSV230920221663927974</dvdt:IdCorr><dvdt:FechaPreparacion>20220923</dvdt:FechaPreparacion><dvdt:HoraPreparacion>121254</dvdt:HoraPreparacion><dvdt:IndicadorTest>S</dvdt:IndicadorTest></SegmentosDeServicio></Mensaje><Respuesta><CodigoRespuesta>A</CodigoRespuesta><CodigoOperacion>2</CodigoOperacion><ED_2_5_NRL>TEST_PWS</ED_2_5_NRL><ED_3_16_NumIdentifDepositante>ESA78587268</ED_3_16_NumIdentifDepositante><ED_3_18_NumIdentifDeclarante>ESA78587268</ED_3_18_NumIdentifDeclarante><TipoDeDeclaracion>DVD</TipoDeDeclaracion><MRN>22ES009999D04136R3</MRN><Circuito>V</Circuito><FechaPresentacion>20220923</FechaPresentacion><HoraPresentacion>121254</HoraPresentacion><FechaAdmision>20220923</FechaAdmision><HoraAdmision>121254</HoraAdmision><CSV_DeclaracionElectronica>PNS6NA3WMAUC4J8W</CSV_DeclaracionElectronica></Respuesta></DVDH2V1Sal></env:Body></env:Envelope>";

		string ExpectedBodyTextSoapDescriptionTrimmed => @"<?xml version=""1.0"" encoding=""UTF-8""?><soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><soapenv:Header /><soapenv:Body><imp:ConsultaImportacionV2Sal xmlns:imp=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Sal.xsd""><SegmentosDeServicio Id=""TARIC20210415134138722000"" fecha=""20210415"" hora=""134140"" /><CodigoRespuesta>0</CodigoRespuesta><NumeroDeReferencia>20ES00999930006184</NumeroDeReferencia><Administracion>AEAT</Administracion><TipoDeDeclaracion>DUA</TipoDeDeclaracion><DescripTipoDeDeclaracion>Declaración Completa de Importación</DescripTipoDeDeclaracion><Circuito>V</Circuito><EstadoDelDespacho>L</EstadoDelDespacho><DescripEstadoDelDespacho>Accepted with Customs Clearance</DescripEstadoDelDespacho><EstadoContable>CO</EstadoContable><DescripEstadoContable>Contractado</DescripEstadoContable><TienePendenciasSinUltimar>N</TienePendenciasSinUltimar><DescripPendenciasSinUltimar>No tiene</DescripPendenciasSinUltimar><RequiereCertificadosNoAduaneros>N</RequiereCertificadosNoAduaneros><NumeroDeReferenciaExportacion>20EXP0999930006184</NumeroDeReferenciaExportacion><FechaPresentacion>20210415</FechaPresentacion><HoraPresentacion>134050</HoraPresentacion><FechaAdmision>20210415</FechaAdmision><FechaLevante>20201001</FechaLevante><FechaLimitePago>20220415</FechaLimitePago><CSVdeLevante>TEST444444444444</CSVdeLevante><CSVdelCertificadoDeImportacion>CRTF444444444444</CSVdelCertificadoDeImportacion><NumJustificantePago>1234</NumJustificantePago><CAaduana>009998</CAaduana><C011EstatutoMercancias>IM</C011EstatutoMercancias><C012ProcedimientoSolicitado>A</C012ProcedimientoSolicitado><InformacionDePartida><C32NumeroDePartida>001</C32NumeroDePartida><C31EmpaquetamientoInterno><C31EmpaqInternoClase>FR</C31EmpaqInternoClase><C31EmpaqInternoMarcas>BASTIDORES</C31EmpaqInternoMarcas><C31EmpaqInternoNumeroBultos>6</C31EmpaqInternoNumeroBultos><C31EmpaqInternoNumeroPiezas>0</C31EmpaqInternoNumeroPiezas></C31EmpaquetamientoInterno><C31DescripcionDeLaMercancia>DescripciÃ³n con mÃ¡s de 250 caracteres, con tildes. This is a description with more than 250 characters. DescripciÃ³n con mÃ¡s de 250 caracteres, con tildes. This is a description with more than 250 characters. This is the last of the 250 characters</C31DescripcionDeLaMercancia><C3312CodigoPosicionTaric>2402201000</C3312CodigoPosicionTaric><C34PaisOrigen>US</C34PaisOrigen><C35MasaBrutaEnKg>100.000</C35MasaBrutaEnKg><C36Preferencia>1</C36Preferencia><C36Reduccion>00</C36Reduccion><C37RegimenAduanero><C371RegimenSolicitado>40</C371RegimenSolicitado><C371RegimenPrecedente>00</C371RegimenPrecedente></C37RegimenAduanero><C38MasaNetaEnKg>100.000</C38MasaNetaEnKg><C42ValorFactura>300.00</C42ValorFactura><C46ValorEstadistico>300.00</C46ValorEstadistico><C47ImporteTotal>641.25</C47ImporteTotal></InformacionDePartida></imp:ConsultaImportacionV2Sal></soapenv:Body></soapenv:Envelope>";

		string ExpectedBodyTextDOC => @$"<AttachedDocument {XMLTestFileConstants.XmlnsLinkAttributes}>
  <FileName>23ES009999101500B4_E_AEAT_CLR.pdf</FileName>
  <Type>
    <Code>CLR</Code>
    <Description>Clearance Document</Description>
  </Type>
  <ImageData>JVBERi0x</ImageData>
  <IsPublished>true</IsPublished>
</AttachedDocument>";

		ZString BodyTextEDIFACTServerErrorProcessed => ZString.Format(@$"<CommonCustomsServiceError {XMLTestFileConstants.XmlnsLinkAttributes} xmlns=""http://cargowise.com/xhub/products/Customs"">
  <ErrorType>500</ErrorType>
  <ErrorDescription>ERROR 500</ErrorDescription>
</CommonCustomsServiceError>");

		ZString BodyTextEDIFACTErrorProcessed => ZString.Format(@$"<Response {XMLTestFileConstants.XmlnsLinkAttributes}>
  <DeclarationStatus>50052</DeclarationStatus>
  <ErrorStatus>Error Traducción:Segmento (TPL) Mensaje erroneo</ErrorStatus>
</Response>");

		string BodyTextSoapInvalidCertificateProcessed => string.Format($@"<CommonCustomsServiceError {XMLTestFileConstants.XmlnsLinkAttributes} xmlns=""http://cargowise.com/xhub/products/Customs"">
  <ErrorType>401</ErrorType>
  <ErrorDescription>403: Digital certificate not detected or not selected correctly.</ErrorDescription>
</CommonCustomsServiceError>");

		string BodyTextSoapClientFaultProcessed => string.Format($@"<CommonCustomsServiceError {XMLTestFileConstants.XmlnsLinkAttributes} xmlns=""http://cargowise.com/xhub/products/Customs"">
  <ErrorType>400</ErrorType>
  <ErrorDescription>UserException</ErrorDescription>
</CommonCustomsServiceError>");

		string BodyTextSoapDocumentErrorProcessed => string.Format($@"<CommonCustomsServiceError {XMLTestFileConstants.XmlnsLinkAttributes} xmlns=""http://cargowise.com/xhub/products/Customs"">
  <ErrorType>400</ErrorType>
  <ErrorDescription>El código que ha utilizado en el cotejo no es un CSV válido:  8M6A78YWSYTPPZ5G. ERROR: El CSV no existe. </ErrorDescription>
</CommonCustomsServiceError>");

		string ExpectedBodyTextSoapUniversalEventError => @"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>HYECM2</SenderID>
    <RecipientID>ESCustomsAsync</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
      <Event>
        <EventTime>2022-08-09 03:50:11.903</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <Reason>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</Reason>
          <MessageType>NPI</MessageType>
        </EventParameters>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";
	}
}
