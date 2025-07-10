using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class AnnexNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<AnnexNCTSResponseMessageProcessor, AnnexNCTSMessagePrettyFormatter, Ccdotcv1Sal>
	{
		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchN()
		{
			nctsHeader.RequestDispatch = "N";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText);
			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(true);
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, messageStatus: LogicalStatusList.Codes.Sent, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				nctsHeader.Messages.Reload(true);
				var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There are 2 extra TRX EDIMessages created for the nctsHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = nctsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = nctsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseMessageStatusRejected()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(true);
			nctsHeader.MovementHeader.BM_MessageStatus = "REJ";
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";
			AssertAcceptedNCTSAnnex(message, messageStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_LastResponseMessageStatusFailed()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(true);
			nctsHeader.MovementHeader.BM_MessageStatus = "FAL";
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";
			AssertAcceptedNCTSAnnex(message, messageStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_NotLastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(true);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";
			AssertAcceptedNCTSAnnex(message, messageStatus: LogicalStatusList.Codes.Sent, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchN()
		{
			nctsHeader.RequestDispatch = "N";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText);
			AssertNoTrigger();
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(false);
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, messageStatus: LogicalStatusList.Codes.Sent, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				nctsHeader.Messages.Reload(true);
				var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There is 1 extra TRX EDIMessages created for the nctsHeader", 1, msgs.Count);
				AssertEquals("New EDIMessages are type EDA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes));
				AssertContains("New EDIMessage has dispatch request flag to S because there 9 or less annexes so they are sent as last", "finAnexos>S", msgs[0].EM_MessageText);

				var pivotsForFirstMessage = nctsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 3, pivotsForFirstMessage.Count());
			});
		}

		public void TestProcessAcceptedMessage_NoExtraAnnex_RequestDispatchY()
		{
			nctsHeader.RequestDispatch = "Y";
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			CombineAssertions(() =>
			{
				nctsHeader.Messages.Reload(true);
				var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
				AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);

				AssertEquals("logger has no exceptions", ZString.Empty, GetAllConcatenatedUserLogStrings());
			});
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseMessageStatusRejected()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(false);
			nctsHeader.MovementHeader.BM_MessageStatus = "REJ";
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, messageStatus: "REJ", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_LastResponseMessageStatusFailed()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(false);
			nctsHeader.MovementHeader.BM_MessageStatus = "FAL";
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, messageStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessAcceptedMessage_LastAnnex_RequestDispatchY_AnnexWaitingForResponse()
		{
			nctsHeader.RequestDispatch = "Y";
			AddAnnexes(false);
			AddExtraAnnexSent();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YJB4K9UXA5JFPMXC</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>referencia1</td><td>S8R9XYANYM9T9NT6</td></tr></table>";

			AssertAcceptedNCTSAnnex(message, messageStatus: LogicalStatusList.Codes.Sent, expectedMessageInterpretation: expectedMessageInterpretationText, requestDispatch: "Y");

			nctsHeader.Messages.Reload(true);
			var msgs = nctsHeader.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX" && x.EM_MessageText != SentMessageText).ToList();
			AssertEquals("There are no extra TRX EDIMessages created for the nctsHeader", 0, msgs.Count);
		}

		public void TestProcessRejectedMessage_RequestDispatchN()
		{
			nctsHeader.RequestDispatch = "N";
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr>" +
				"</table>";
			AssertRejectedAndErrorNCTSAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessRejectedMessage_RequestDispatchY()
		{
			nctsHeader.RequestDispatch = "Y";
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr>" +
				"</table>";
			AssertRejectedAndErrorNCTSAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessErrorMessage_RequestDispatchN()
		{
			nctsHeader.RequestDispatch = "N";
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
				"</table>";
			AssertRejectedAndErrorNCTSAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessErrorMessage_RequestDispatchY()
		{
			nctsHeader.RequestDispatch = "Y";
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
				"</table>";
			AssertRejectedAndErrorNCTSAnnex(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestMessageProcessingErrorHeaderNotDeparture()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			responseMessage.EM_LinkedObject = nctsHeader;

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretation = string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Header type is A so can't process Departure response message</H4>", responseMessage.EM_MessageNum, responseMessage.EM_MessageType, responseMessage.EM_MessageSubType, responseMessage.EM_ApplicationReference);

			AssertNCTSDeclaration(responseMessage, messageSubType: "AAA", emStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretation, commonCustomsStatus: OriginalEntryStatus, messageStatus: "FAL");
		}

		void AssertAcceptedNCTSAnnex(TestEdiMessage message, string messageStatus = "", string expectedMessageInterpretation = "", string requestDispatch = "N")
		{
			AssertNCTSAnnex(message, messageSubType: "ACC", messageStatus: messageStatus, expectedMessageInterpretation: expectedMessageInterpretation, requestDispatch: requestDispatch);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Received, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = nctsHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertRejectedAndErrorNCTSAnnex(TestEdiMessage message, string expectedMessageInterpretation = "")
		{
			AssertNCTSAnnex(message, messageSubType: "REJ", messageStatus: "REJ", emStatus: EDIMessage.Status.Rejected, expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Rejected, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessages = nctsHeader.Factory.Load<GenPivot>(pivotQuery);
				AssertEquals("Received message is related to sent annexes through new GenPivot", 2, genPivotResponseMessages.Length);
			});
		}

		void AssertNCTSAnnex(TestEdiMessage message, string messageSubType, string messageStatus = EDIMessageStatusList.Codes.Received, string emStatus = EDIMessage.Status.Received, string expectedMessageInterpretation = "", string requestDispatch = "")
		{
			AssertEquals("RequestDispatch", requestDispatch, nctsHeader.RequestDispatch);
			AssertNCTSDeclaration(message, messageSubType: messageSubType, messageStatus: messageStatus, emStatus: emStatus, expectedMessageInterpretation: expectedMessageInterpretation, commonCustomsStatus: OriginalEntryStatus, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
		}

		void AssertNoTrigger()
		{
			ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, nctsHeader.PK);
			var msg = Factory.Load<EDIMessage>(messagesQuery).OrderByDescending(x => x.EM_SystemCreateTimeUtc).First();
			AssertNotEquals("Last EDIMessage is not transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			sentMessage = (TestEdiMessage)sentInterchange.ContainedMessages[0];

			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc1.txt", "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = pivot1.PK;
			messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = sentMessage.PK;
			messagePivot1.XX_Relation2TableCode = sentMessage.TablePrefix;

			var eDoc2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "sentDoc2.txt", "CIV");
			var pivot2 = nctsHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			var messagePivot2 = Factory.New<GenPivot>();
			messagePivot2.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot2.XX_Relation1ID = pivot2.PK;
			messagePivot2.XX_Relation1TableCode = pivot2.TablePrefix;
			messagePivot2.XX_Relation2ID = sentMessage.PK;
			messagePivot2.XX_Relation2TableCode = sentMessage.TablePrefix;

			Factory.Save();
			nctsHeader.DocManagerInfo.Save();
			nctsHeader.EDocPivotCollection.Reload(true);
		}
		TestEdiMessage sentMessage;

		protected override ZString MessageStatusWhenRejectedOrError => "REJ";

		protected override ZString EMStatusWhenRejectedOrError => EDIMessage.Status.Rejected;

		protected override ZString PhaseStatusWhenErrorOrRejected => ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Annex Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes };

		string GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexNCTSTestFilePath, "AcceptedMessage.txt");
		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexNCTSTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexNCTSTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override AnnexNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new AnnexNCTSResponseMessageProcessor(logger);

		void AddAnnexes(ZBool addExtraAnnexDocs)
		{
			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc1.txt", "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var eDoc2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc2.txt", "CIV");
			var pivot2 = nctsHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

			var eDoc3 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Doc3.txt", "CIV");
			var pivot3 = nctsHeader.EDocPivotCollection.AddNew();
			pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;

			if (addExtraAnnexDocs)
			{
				for (int i = 0; i <= 10; i++)
				{
					var eDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice" + i + ".txt", "MSC");
					var pivot = nctsHeader.EDocPivotCollection.AddNew();
					pivot.CSD_StorageDocReference = eDoc.UniqueKey;
				}
			}

			Factory.Save();
			nctsHeader.DocManagerInfo.Save();
			nctsHeader.EDocPivotCollection.Reload(true);
		}
		void AddExtraAnnexSent()
		{
			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Extra1.txt", "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var messageAnnexSent = Factory.New<TestEdiMessage>();
			messageAnnexSent.EM_MessageNum = SentMessageNumber;
			messageAnnexSent.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageAnnexSent.EM_MessageType = MessageType;
			messageAnnexSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageAnnexSent.EM_Status = EDIMessage.Status.Sent;
			messageAnnexSent.EM_MessageText = SentMessageText;
			nctsHeader.Messages.Add(messageAnnexSent);

			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = pivot1.PK;
			messagePivot1.XX_Relation1TableCode = pivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = messageAnnexSent.PK;
			messagePivot1.XX_Relation2TableCode = messageAnnexSent.TablePrefix;
		}
	}
}
