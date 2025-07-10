using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCIncomingMessageProcessingHubTest : TestCaseWithFactory
{
	public void TestProcessIrispMessages()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var headerMessageText = "0020            4JEZ0102.XB9170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       ";
		var bodyMessageText = @" RICEVUTO  19/01/17 09:39,4JEZ0102.RB9
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      IN ATTESA DI ESITO                                                                                          ";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, headerMessageText, bodyMessageText, SADConstants.CustomsInterchangeType.IrispX, ApplicationCodeList.Codes.ITCustoms);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "IRISP");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "IRISP", ApplicationCodeList.Codes.ITCustoms);
	}

	public void TestProcessIvistoMessages()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var headerMessageText = "0RON            0RON0401.Q11200005181710028100    01824540346     001 00003    INVIO IN AMBIENTE REALE       ";
		var bodyMessageText = @"Data:01/04/2020  Ora:12:54:01
TIVISTO  20ITQVG1T0008901T0IT028100IT275105PONTE CHIASSO                      01042020Uscita conclusa                         
";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, headerMessageText, bodyMessageText, SADConstants.CustomsInterchangeType.Ivisto, ApplicationCodeList.Codes.ITCustoms);
		entryHeader.MovementReferenceNumberSetter("20ITQVG1T0008901T0");
		Factory.Save();
		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "IVISTO");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "IVISTO", ApplicationCodeList.Codes.ITCustoms);
	}

	public void TestProcessSingleWindowPositiveAckResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText: ZString.Empty, interchangeType: "WSA", ApplicationCodeList.Codes.ITCustoms);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "Single Window Acknowledgement");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Single Window Acknowledgement", ApplicationCodeList.Codes.ITCustoms, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessSingleWindowNegativeAckResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = "<error code from WS>";
		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, interchangeType: "WSE", ApplicationCodeList.Codes.ITCustoms);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "Single Window Acknowledgement");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Single Window Acknowledgement", ApplicationCodeList.Codes.ITCustoms, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessSingleWindowXmlResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
						<esito_bolletta>
							<codice_file>190019563757</codice_file>
							<estremi_dichiarazione>
								<num_reg>64252</num_reg>
								<cod_uff_dog>371101</cod_uff_dog>
								<cod_reg>4 T</cod_reg>
								<anno_reg>2019</anno_reg>
							</estremi_dichiarazione>
							<dichiarazione_registrata>2019-12-09T14:17:56.000+01:00</dichiarazione_registrata>
							<dichiarazione_convalidata>2019-12-09T14:17:56.000+01:00</dichiarazione_convalidata>
							<svincolo>
								<cod_svincolo>VL9NSW</cod_svincolo>
								<data_codice_svincolo>2019-12-09T14:17:57.000+01:00</data_codice_svincolo>
								<flag_svincolo_forzato>0</flag_svincolo_forzato>
							</svincolo>
							<prospetto_svincolo>SI</prospetto_svincolo>
							<sportello_unico>
								<amm_coinvolta>1</amm_coinvolta>
								<data_ultimo_controllo>2019-12-09T12:50:31.000+01:00</data_ultimo_controllo>
								<contributo>Controllo completato positivamente</contributo>
							</sportello_unico>
							<controllo_doganale>
								<flag_ctrl_dog>CA</flag_ctrl_dog>
								<esito_ctrl_dog>Eseguito</esito_ctrl_dog>
								<data_esito_ctrl_dog>2019-12-09T14:17:56.000+01:00</data_esito_ctrl_dog>
							</controllo_doganale>
							<controllo_sicurezza>
								<esito_ctrl_sic>Svincolabile</esito_ctrl_sic>
							</controllo_sicurezza>
						</esito_bolletta>";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "SWS", ApplicationCodeList.Codes.ITCustoms);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "Single Window XML");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Single Window XML", ApplicationCodeList.Codes.ITCustoms, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessSingleWindowPdfResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = @"<AttachedDocumentCollection> 
						<AttachedDocument> 
							<Filename>Filename_Clearance.pdf</Filename> 
							<ImageData>base64DataForPdf</ImageData> 
							<Type> 
								<Code>CLR</Code> 
								<Description>Clearance Document</Description>
							</Type> 
						</AttachedDocument> 
					</AttachedDocumentCollection>";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "PDF", ApplicationCodeList.Codes.ITCustoms);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "Single Window PDF");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Single Window PDF", ApplicationCodeList.Codes.ITCustoms, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6PositiveAckResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://importservice.domest.sogei.it"">
		 <ns2:IUT>20220307D11000328189</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>20</ns2:codice>
			<ns2:messaggio>Acquisito a sistema</ns2:messaggio>
		 </ns2:esito>
		 <ns2:dataRegistrazione>2022-03-07+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "ACK", ApplicationCodeList.Codes.ITCustomsXTrade);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "UCC6 Positive ACK");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Positive ACK", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6NegativeAckResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://importservice.domest.sogei.it"">
		 <ns2:IUT>20220307D11000328189</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>0</ns2:codice>
			<ns2:messaggio>Servizio non disponibile</ns2:messaggio>
		 </ns2:esito>
		 <ns2:dataRegistrazione>2022-03-07+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "ACK", ApplicationCodeList.Codes.ITCustomsXTrade);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "UCC6 Negative ACK");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Negative ACK", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6NegativeResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <soapenv:Body>
	  <ns3:recuperaEsitoResponse xmlns:ns3=""http://service.ws.sogei.it"">
		 <recuperaEsitoReturn>
			<IUT>20220114D11000069297</IUT>
			<esito>
			   <codice>198</codice>
			</esito>
			<data>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9InllcyI/PjxkYXRhPjxEYXRhT3JhSW5pRWxhYj48L0RhdGFPcmFJbmlFbGFiPjxEYXRhT3JhRmluRWxhYj48L0RhdGFPcmFGaW5FbGFiPjxTdGF0bz4wPC9TdGF0bz48THJuPjIwMjJGR0JSSUgxNzhRWTAwMDAxMTc8L0xybj48SW5mb3JtYXppb25lPjxDb2RpY2U+MjIwPC9Db2RpY2U+PERlc2NyaXppb25lPkVycm9yZSAyMjA6IFtDb2RpY2VUaXBEb2NJZGVudGlmaWNhdGl2b10gIERvY3VtZW50byBvYmJsaWdhdG9yaW8gbm9uIHByZXNlbnRlIE5vdGU6IGluZGljYXJlIGRvY3VtZW50byBOOTU0L044NjQvVTA0NS9BMDIyIG8gTjAxOCBvIE44NjIgbyBVMDY1IG8gVTExMC9VMTExL1UxMTIgbyBVMTE2L1UxMTcvVTExODwvRGVzY3JpemlvbmU+PExpdmVsbG8+QTwvTGl2ZWxsbz48TnVtZXJvQXJ0aWNvbG8+MTwvTnVtZXJvQXJ0aWNvbG8+PE5vbWVFbGVtZW50bz5Db2RpY2VUaXBEb2NJZGVudGlmaWNhdGl2bzwvTm9tZUVsZW1lbnRvPjwvSW5mb3JtYXppb25lPjwvZGF0YT4=</data>
			<dataRegistrazione>2022-01-14+01:00</dataRegistrazione>
		 </recuperaEsitoReturn>
	  </ns3:recuperaEsitoResponse>
   </soapenv:Body>
</soapenv:Envelope>";

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "RES", ApplicationCodeList.Codes.ITCustomsXTrade);

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "UCC6 Negative RES");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Negative RES", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessImportUcc6PositiveResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";

		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml");

		var (entryHeader, unmatchingMessage, receivedInterchange, receivedMessage) = CreateEDIInterchangeAndMessage(
			sentInterchangeSessionId,
			sentMessageNumber,
			ZString.Empty,
			bodyMessageText,
			"RES",
			ApplicationCodeList.Codes.ITCustomsXTrade,
			declarationType: "IMP");

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "UCC6 Import Positive RES");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Import Positive RES", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessExportUcc6PositiveResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";

		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponseWithClearance.xml");

		var (entryHeader, unmatchingMessage, receivedInterchange, receivedMessage) = CreateEDIInterchangeAndMessage(
			sentInterchangeSessionId,
			sentMessageNumber,
			ZString.Empty,
			bodyMessageText,
			"RES",
			ApplicationCodeList.Codes.ITCustomsXTrade,
			declarationType: "EXP");

		AssertPreConditions(unmatchingMessage, receivedInterchange, receivedMessage, messageTypeForLog: "UCC6 Export Positive RES");

		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Export Positive RES", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6EFStatusMessageProcessor()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
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

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "EFR", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 EFQ Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6XTradeErrorResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "ERR", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);
		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 XTrade Error Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6IvistoResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "IVR", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);
		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "UCC6 Ivisto Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessUcc6CustomsXTradeErrorResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222286";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ucc6CustomsXTradeErrorFile.xml");

		(var entryHeader, var unmatchingMessage, var receivedInterchange, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "XER", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		receivedInterchange.EI_TransportType = "XTT";
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);
		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Customs XHub Error Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessEadResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "2222867";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadPositiveResponseResultCode199.xml");

		var (entryHeader, unmatchingMessage, _, receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "EAD", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "EAD Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessTadResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "2222866";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_TadPositiveResponseResultCode200.xml");

		var (entryHeader, unmatchingMessage, _, receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "TAD", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "TAD Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessEur1ResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1PositiveResponseResultCode199_sample.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "EU1", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Eur1 Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessReleaseProspectusResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusPositiveResponseResultCode199.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "SVI", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Release Prospectus Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessAccountingSummaryDownloadMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AccountingSummaryDownloadPositiveResponseResultCode199.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "PRD", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Accounting Summary Download", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessAccountingSummaryResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AccountingSummaryResponsePositiveResponseResultCode199.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "PRR", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Accounting Summary Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestXTradeSignatureErrorResponseMessageProcessor()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSginatureErrorFile_ReasonContainsMessageTransmissionFailure.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "XSE", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "XTrade Signature Error Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessXTradeSignatureResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSignatureResponse.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "SGN", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "XTrade Signature Message Processor", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessSummaryProspectusResponseMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusResponsePositiveResponseResultCode199.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "SPR", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "Summary Prospectus Response", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	public void TestProcessSummaryProspectusDownloadMessage()
	{
		var sentInterchangeSessionId = ZGuid.NewZGuid();
		var sentMessageNumber = "222285";
		var bodyMessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadPositiveResponseResultCode199.xml");

		(var entryHeader, var unmatchingMessage, _, var receivedMessage) = CreateEDIInterchangeAndMessage(sentInterchangeSessionId, sentMessageNumber, ZString.Empty, bodyMessageText, "SPD", ApplicationCodeList.Codes.ITCustomsXTrade);
		var logger = new LoggingInformationForTesting();
		ITCIncomingMessageProcessingHub.ProcessMessages(logger, CancellationToken.None);

		AssertResult(sentMessageNumber, entryHeader, unmatchingMessage, receivedMessage, logger, messageTypeForLog: "SummaryProspectusDownload", ApplicationCodeList.Codes.ITCustomsXTrade, expectReceivedMessageLinkedToEntry: true);
	}

	#region Implementation

	void AssertPreConditions(EDIMessage unmatchingMessage, EDIInterchange receivedInterchange, EDIMessage receivedMessage, string messageTypeForLog)
	{
		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals("Unmatching Message Status", EDIMessage.Status.Queued, unmatchingMessage.EM_Status);
			AssertEquals($"Received {messageTypeForLog} Message Status", EDIMessage.Status.Queued, receivedMessage.EM_Status);
			AssertEquals($"Received {messageTypeForLog} Message Interchange", receivedInterchange.PK, receivedMessage.Interchange.PK);
			AssertEquals($"Received {messageTypeForLog} Interchange Status", EDIInterchange.Status.Received, receivedInterchange.EI_Status);
		});
	}

	void AssertResult(string sentMessageNumber, CusEntryHeader entryHeader, EDIMessage unmatchingMessage, EDIMessage receivedMessage, LoggingInformationForTesting logger, string messageTypeForLog, string expectedApplicationCode, bool expectReceivedMessageLinkedToEntry = false)
	{
		unmatchingMessage.Reload();
		receivedMessage.Reload();
		entryHeader.Messages.Reload(reLoadExistingRows: true);
		var message = entryHeader.Messages.Find(x => x.EM_Status == EDIInterchange.Status.Received).SingleOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("Logged Info",
				$"Information: Processing Message #{sentMessageNumber}"
				+ "\r\nInformation: Saving..."
				+ "\r\nInformation: 1 message processed",
				logger.AccumulatedLogMessages.ToString().Trim());

			AssertEquals("Unmatching Message Status", EDIInterchange.Status.Queued, unmatchingMessage.EM_Status);

			AssertEquals("Received Interchange Status", EDIInterchange.Status.Received, receivedMessage.Interchange.EI_Status);

			AssertEquals("Received Message Status", EDIMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals("Received Message EM_ApplicationCode", expectedApplicationCode, receivedMessage.EM_ApplicationCode);
			AssertEquals("Received Message EM_ApplicationReference", "", receivedMessage.EM_ApplicationReference);
			AssertEquals("Received Message EM_HeldUntilDate", ZDateTime.Empty, receivedMessage.EM_HeldUntilDate);
			if (!expectReceivedMessageLinkedToEntry)
			{
				AssertEquals("Received Message EM_LinkTable", "", receivedMessage.EM_LinkTable);
				AssertEquals("Received Message EM_LinkUniqueID", ZGuid.Empty, receivedMessage.EM_LinkUniqueID);
			}
			else
			{
				AssertEquals("Received Message EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, receivedMessage.EM_LinkTable);
				AssertEquals("Received Message EM_LinkUniqueID", entryHeader.PK, receivedMessage.EM_LinkUniqueID);
			}

			var numberOfMessagesExcludingSWR = entryHeader.Messages.Find(x => x.EM_MessageType != MessageProcessorConstants.InterchangeTypes.SingleWindowRequest).Count();
			AssertEquals("Messages count for header (1 sent, 1 received)", 2, numberOfMessagesExcludingSWR);
			AssertNotNull($"EntryHeader {messageTypeForLog} message", message);
		});

		CombineAssertions($"Entry Header {messageTypeForLog} Message", () =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_ApplicationCode", expectedApplicationCode, message.EM_ApplicationCode);
			AssertEquals("EM_ApplicationReference", "", message.EM_ApplicationReference);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
			AssertEquals("EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_ReceiveTransmit", receivedMessage.EM_ReceiveTransmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_EI", receivedMessage.EM_EI, message.EM_EI);
		});
	}

	(CusEntryHeader entryHeader, EDIMessage unmatchingMessage, EDIInterchange receivedInterchange, EDIMessage receivedMessage) CreateEDIInterchangeAndMessage(ZGuid sentInterchangeSessionId, ZString sentMessageNumber, ZString headerMessageText, ZString bodyMessageText, ZString interchangeType, ZString applicationCode, string declarationType = null)
	{
		var entryHeader = CreateRelatedEntryHeaderSentMessages(sentInterchangeSessionId, sentMessageNumber, declarationType);

		var unmatchingMessage = Factory.NewWithValidTestData<EDIMessage>();
		unmatchingMessage.EM_ApplicationCode = applicationCode;
		unmatchingMessage.EM_MessageType = "T1";
		unmatchingMessage.EM_MessageSubType = "XXX";
		unmatchingMessage.EM_MessageNum = "1111";
		unmatchingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		unmatchingMessage.EM_Status = EDIInterchange.Status.Queued;
		unmatchingMessage.EM_MessageText = "SAMPLE TEXT 1";

		var receivedInterchange = Factory.New<EDIInterchange>();
		receivedInterchange.EI_ApplicationCode = applicationCode;
		receivedInterchange.EI_InterchangeType = interchangeType;
		receivedInterchange.EI_InterchangeNum = "2";
		receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		receivedInterchange.EI_Status = EDIInterchange.Status.Received;
		receivedInterchange.EI_SessionGUID = sentInterchangeSessionId;
		receivedInterchange.EI_HeaderText = $@"<ITMessage>
	<MessageType>{interchangeType}</MessageType>
	<FileName>4JEZ0102.{interchangeType}B9</FileName>
	<eHubTrackingIDFromSentInterchange>{sentInterchangeSessionId}</eHubTrackingIDFromSentInterchange>
	<Header>{headerMessageText}</Header>
</ITMessage>";
		receivedInterchange.EI_BodyText = bodyMessageText;

		var receivedMessage = Factory.New<ITEDIMessage>();
		receivedMessage.EM_ApplicationCode = applicationCode;
		receivedMessage.EM_MessageType = interchangeType;
		receivedMessage.EM_MessageNum = sentMessageNumber;
		receivedMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(sentMessageNumber);
		receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		receivedMessage.EM_Status = EDIInterchange.Status.Queued;
		receivedMessage.EM_MessageText = $"{headerMessageText}\r\n{bodyMessageText}".TrimStart();

		receivedInterchange.ContainedMessages.Add(receivedMessage);

		Factory.Save();

		return (entryHeader, unmatchingMessage, receivedInterchange, receivedMessage);
	}

	CusEntryHeader CreateRelatedEntryHeaderSentMessages(ZGuid sessionId, ZString messageNumber, string declarationType = null)
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = declarationType ?? "EXP";

		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
		var sentMessage = entryHeader.Messages.AddNew();
		sentMessage.EM_MessageText = "HERE THERE IS THE INTERCHANGE HEADER\r\nTIM           12345600012345    A   0123456 01  02072019    HERE SOME OTHER DATA";
		sentMessage.EM_Status = EDIMessage.Status.Sent;
		sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentMessage.EM_MessageSubType = SADConstants.MessageSubTypes.IM;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber);
		sentMessage.EM_ApplicationReference = jobDeclaration.JE_MessageType;

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_SessionGUID = sessionId;
		sentEdiInterchange.ContainedMessages.Add(sentMessage);
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_HeaderText = SadIncomingCustomsMessageProcessorTestHelper.GetSentHeaderText("4JEZ0102.RB9");

		return entryHeader;
	}

	#endregion
}

public class LoggingInformationForTesting : LoggingInformation
{
	public LoggingInformationForTesting() : base()
	{
		AccumulatedLogMessages = new ZStringBuilder();
		OnLogInfoAdded += (log, logType) => AccumulatedLogMessages.AppendLine($"{logType}: {log.Trim()}");
	}

	public ZStringBuilder AccumulatedLogMessages { get; private set; }
}
