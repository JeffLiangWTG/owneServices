using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AnnexAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<AnnexAESResponseMessageProcessor, IMessagePrettyFormatter, Ccdoccv1Sal>
	{
		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "AWR", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There are 2 extra TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.ExportAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseCHStatusRejected()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			entryHeader.CH_Status = "REJ";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseCHStatusFailed()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			entryHeader.CH_Status = "FAL";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(true);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: ZString.Empty, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "AWR", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There is 1 extra TRX EDIMessages created for the entryHeader", 1, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.ExportAnnexes));
				AssertContains("New EDIMessage has dispatch request flag to S because there 9 or less annexes so they are sent as last", "finAnexos>S", msgs[0].EM_MessageText);

				var pivotsForFirstMessage = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 3, pivotsForFirstMessage.Count());
			});
		}

		public void TestProcessAcceptedMessage_NoExtraAnnex_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);

				AssertEquals("logger has no exceptions", ZString.Empty, GetAllConcatenatedUserLogStrings());
			});
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseCHStatusRejected()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			entryHeader.CH_Status = "REJ";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseCHStatusFailed()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			entryHeader.CH_Status = "FAL";
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			AddAnnexes(false);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>NW2HYEZ92FTR5J2F</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>EDQGYLR8BHGKCVR9</td></tr></table>";
			AssertAcceptedExportAnnex(message, chStatus: ZString.Empty, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			entryHeader.Messages.Reload(true);
			var msgs = entryHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the entryHeader", 0, msgs.Count);
		}

		public void TestProcessRejectedMessage_RequestDispatchN() => ProcessAnnexRejectedMessage("N");

		public void TestProcessRejectedMessage_RequestDispatchY() => ProcessAnnexRejectedMessage("Y");

		void ProcessAnnexRejectedMessage(ZString requestDispatch)
		{
			entryHeader.ZG_RequestDispatch = requestDispatch;
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetGetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CCDOCC/AesDatosGestion/nacEstadoAES</td><td>(3500) El estado AES del MRN indicado no permite la operación.</td><td>DE</td></tr>" +
				"</table>";
			AssertRejectedAndErrorExportAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessErrorMessage_RequestDispatchN()
		{
			entryHeader.ZG_RequestDispatch = "N";
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCDOCCV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCDOCCV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"</table>";
			AssertRejectedAndErrorExportAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessErrorMessage_RequestDispatchY()
		{
			entryHeader.ZG_RequestDispatch = "Y";
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCDOCCV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCDOCCV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"</table>";
			AssertRejectedAndErrorExportAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		void AssertAcceptedExportAnnex(TestEdiMessage message, string chStatus = "RCV", string expectedMessageInterpretation = "", string requestDispatch = "N")
		{
			AssertExportAnnex(message, messageSubType: "ACC", chStatus: chStatus, expectedMessageInterpretation: expectedMessageInterpretation, requestDispatch: requestDispatch);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Received, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = entryHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertRejectedAndErrorExportAnnex(TestEdiMessage message, string expectedMessageInterpretation = "")
		{
			AssertExportAnnex(message, messageSubType: "REJ", emStatus: EDIMessage.Status.Rejected, chStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Rejected, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = entryHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertExportAnnex(TestEdiMessage message, string messageSubType, string emStatus = EDIMessage.Status.Received, string chStatus = "RCV", string expectedMessageInterpretation = "", string requestDispatch = "")
		{
			AssertEquals("ZG_RequestDispatch", requestDispatch, entryHeader.ZG_RequestDispatch);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, chStatus: chStatus, messageSubType: messageSubType, entryStatusCode: OriginalEntryStatus, movementReferenceNumber: OriginalMRNCode, messageNum: MessageNum);
		}

		void AssertNoTrigger()
		{
			ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			var msg = Factory.Load<EDIMessage>(messagesQuery).OrderByDescending(x => x.EM_SystemCreateTimeUtc).First();
			AssertNotEquals("Last EDIMessage is not transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumber = OriginalMRNCode;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.ZG_UCC6Version = 1;

			var sentInterchange = SetSentInterchange(entryHeader, InterchangeID);
			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			sentMessage = (TestEdiMessage)sentInterchange.ContainedMessages[0];

			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc1.txt", "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = pivot1.PK;
			messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = sentMessage.PK;
			messagePivot1.XX_Relation2TableCode = sentMessage.TablePrefix;

			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc2.txt", "CIV");
			var pivot2 = entryHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			var messagePivot2 = Factory.New<GenPivot>();
			messagePivot2.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot2.XX_Relation1ID = pivot2.PK;
			messagePivot2.XX_Relation1TableCode = pivot2.TablePrefix;
			messagePivot2.XX_Relation2ID = sentMessage.PK;
			messagePivot2.XX_Relation2TableCode = sentMessage.TablePrefix;

			Factory.Save();
			declaration.DocManagerInfo.Save();
			entryHeader.EDocPivotCollection.Reload(true);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		TestEdiMessage sentMessage;

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexAESTestFilePath, "AcceptedMessage.txt");
		string GetGetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexAESTestFilePath, "RejectedMessage.txt");
		string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexAESTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string OriginalMRNCode = "TestMRN";
		const string MessageNum = "20221014115635534071";

		protected override ZString GetExpectedProcessorFriendlyName() => "Export Annex Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportAnnexes };

		protected override AnnexAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new AnnexAESResponseMessageProcessor(logger);

		void AddAnnexes(ZBool addExtraAnnexDocs)
		{
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc1.txt", "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc2.txt", "CIV");
			var pivot2 = entryHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

			var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc3.txt", "CIV");
			var pivot3 = entryHeader.EDocPivotCollection.AddNew();
			pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;

			if (addExtraAnnexDocs)
			{
				for (int i = 0; i <= 10; i++)
				{
					var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice" + i + ".txt", "MSC");
					var pivot = entryHeader.EDocPivotCollection.AddNew();
					pivot.CSD_StorageDocReference = eDoc.UniqueKey;
				}
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();
			entryHeader.EDocPivotCollection.Reload(true);
		}
		void AddExtraAnnexSent()
		{
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Extra1.txt", "CIV");
			var pivot1 = entryHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var messageAnnexSent = Factory.New<TestEdiMessage>();
			messageAnnexSent.EM_MessageNum = SentMessageNumber;
			messageAnnexSent.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageAnnexSent.EM_MessageType = MessageType;
			messageAnnexSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageAnnexSent.EM_Status = EDIMessage.Status.Sent;
			messageAnnexSent.EM_MessageText = SentMessageText;
			entryHeader.Messages.Add(messageAnnexSent);

			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = pivot1.PK;
			messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = messageAnnexSent.PK;
			messagePivot1.XX_Relation2TableCode = messageAnnexSent.TablePrefix;
		}
	}
}
