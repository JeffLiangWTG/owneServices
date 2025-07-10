using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PresentaMercanciasV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationImportResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<InboxNotificationImportResponseMessageProcessor, PresentaMercanciasV1Sal>
	{
		public void TestProcessAcceptedGreenCircuitCLPMessageEntryInstructionC()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessAcceptedGreenCircuitCLPMessageEntryInstructionZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessAcceptedGreenCircuitCLPMessageEntryInstructionB()
		{
			entryHeader.TotalAmount = 1.15;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr>" +
				"<tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>-0.05</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>1.05</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>0.10</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.15</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>-0.15</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr>" +
				"<tr><td>ATC Proof of Payment Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>06-06-2079</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>1.15</td><td>0</td><td>0</td></tr>" +
				"<tr><td>ATC</td><td>18ESCGL9980000060</td><td>1.31</td><td>0</td><td>0</td></tr></table>" +
				"<br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>MA</td><td>42.560</td><td>2.700000 %</td><td>1.15</td><td>1.15</td></tr><tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr><tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 %</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>363.90</td><td>363.90</td></tr>" +
				"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>43.710</td><td>3.000000 €/KN</td><td>1.31</td><td>1.31</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>1.31</td><td>1.31</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N853, C657, C678, 1405, 1413, C640</td></tr></table>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 42.560M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", 2.700000M, entryLine1.Fees[0].CF_Rate);
				AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", "%", entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 1.15M, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "3IG", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 43.710M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 3.000000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "KGM", entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 1.31M, entryLine1.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

				AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "A00", entryLine2.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
				AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
				AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "B00", entryLine2.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[1].CF_BaseValue", 1481.380M, entryLine2.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[1].CF_Rate", 10.000000M, entryLine2.Fees[1].CF_Rate);
				AssertEquals("EntryLine2.Fee[1].MaxMin", ZString.Empty, entryLine2.Fees[1].MaxMin);
				AssertEquals("EntryLine2.Fee[1].G4_RateDuty", "%", entryLine2.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[1].CF_ChargeAmount", 148.14M, entryLine2.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessAcceptedGreenCircuitCLPEntryInstructionBDifferedMessage()
		{
			entryHeader.TotalAmount = ZDecimal.Zero;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr>" +
				"<tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>-0.05</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>1.05</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>0.10</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.15</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>-0.15</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr>" +
				"<tr><td>ATC Proof of Payment Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>06-06-2079</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>1.15</td><td>0</td><td>0</td></tr>" +
				"<tr><td>ATC</td><td>18ESCGL9980000060</td><td>1.31</td><td>0</td><td>0</td></tr></table>" +
				"<br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>MA</td><td>42.560</td><td>2.700000 %</td><td>1.15</td><td>1.15</td></tr><tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr><tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 %</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>363.90</td><td>363.90</td></tr>" +
				"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>43.710</td><td>3.000000 €/KN</td><td>1.31</td><td>1.31</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>1.31</td><td>1.31</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N853, C657, C678, 1405, 1413, C640</td></tr></table>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);
		}

		public void TestProcessAcceptedGreenCircuitCLRMessageEntryInstructionNotBorCorZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared, parallel: true);
		}

		public void TestProcessAcceptedRedCircuitMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileRedCircuit(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedRedAndYellowEntryInstruction(message, entryHeader, circuit: CircuitCodeList.Codes.RED, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, parallel: true);
		}

		public void TestProcessAcceptedYellowCircuitMessage()
		{
			AddMessageProcessAndAssertResult_YellowCircuit();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCode, AddMessageProcessAndAssertResult_YellowCircuit);
		}

		public void TestProcessRejectedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(ExpectedMovementReferenceNumberReject, AddMessageProcessAndAssertResult_RejectedMessage);
		}

		void AddMessageProcessAndAssertResult_YellowCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuit(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedRedAndYellowEntryInstruction(message, entryHeader, circuit: CircuitCodeList.Codes.YELLOW, entryStatusCode: EntryStatusCodes.ClearedWithPendingDocuments, parallel: true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);
		}

		string GetAcceptanceTestFileYellowCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedYellowCircuitMessage.txt");
		string GetAcceptanceTestFileRedCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedRedCircuitMessage.txt");

		protected override string AcceptanceTestFileWithClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessage.txt");
		protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "RejectedMessage.txt");
		protected override string WrongXMLTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "WrongXML.txt");
		protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = AcceptanceTestFileWithClearance;

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = AcceptanceTestFileWithClearance;

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongXML()
		{
			AssertContains("logger", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
		}

		protected override void AssertLoggerMessagesWhenMessageProcessingError()
		{
			base.AssertLoggerMessagesWhenMessageProcessingError();

			AssertContains("logger exception", "Message Text is empty so can't continue with processing", GetAllConcatenatedUserLogStrings());
		}

		protected override string ExpectedEntryStatusForInstructionC => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		protected override List<(ZString, ZString)> ExpectedDocumentsAEATInstructionC => new List<(ZString, ZString)> { (MRNCode + "_I_AEAT_CLR.pdf", "TEST444444444444"),
																															(MRNCode + "_I_AEAT_CER.pdf", "CRTF444444444444"),
																															(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																															(MRNCode + "_I_AEAT_J031.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountAEATInstructionC => 4;
		protected override List<(ZString, ZString)> ExpectedDocumentsATCInstructionC => new List<(ZString, ZString)> { (MRNCode + "_I_AEAT_CLR.pdf", "TEST444444444444"),
																														(MRNCode + "_I_AEAT_CER.pdf", "CRTF444444444444"),
																														(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																														(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
																														(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
																														(MRNCode + "_I_AEAT_J032.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;
		protected override InboxNotificationImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Import inbox Notification Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport };

		protected override ZString RejectedMessageNum => MessageNum;
		protected override ZString RejectedEntryStatus => EntryStatusCodes.PreDeclarationAccepted;
		protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>1001</td><td>1.401<br>Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS.C40ClaseDocumento.KK</td></tr>" +
					"<tr><td>1001</td><td>2.401<br>Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS.C40ClaseDocumento.KK</td></tr></table>";
		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		void AssertAcceptedEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = CircuitCodeList.Codes.GREEN, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, paymentProofNumber: ExpectedPaymentProofNumber, limitPaymentDate: ExpectedLimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, circuitCan: CircuitCodeList.Codes.GREEN, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: ExpectedExportMRN);
		}
		void AssertAcceptedRedAndYellowEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string circuit = CircuitCodeList.Codes.RED, string entryStatusCode = "", bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: "RCV", chStatus: "RCV", messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, acceptanceDate: AcceptanceDate, circuit: circuit, limitPaymentDate: ExpectedLimitPaymentDate, paymentProofNumber: ExpectedPaymentProofNumber, circuitCan: CircuitCodeList.Codes.GREEN, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: ExpectedExportMRN);
		}

		protected override bool IsImportQuery => false;
		protected override string ExpectedCircuit => CircuitCodeList.Codes.GREEN;
		protected override ZDateTime ExpectedLimitPaymentDate => LimitPaymentDate;
		protected override ZDateTime ExpectedATCLimitPaymentDate => ZDateTime.Empty;
		protected override string ExpectedAtcPaymentProofNumber => AtcPaymentProofNumber;
		protected override string ExpectedPaymentProofNumber => PaymentProofNumber;
		protected override string ExpectedExportMRN => ExportMRN;
		protected override string ExpectedCircuitCan => CircuitCodeList.Codes.GREEN;
		protected override string ExpectedMovementReferenceNumberReject => MRNCode;
		protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => MovementReferenceNumberIssueDate;
		protected override string ExpectedentryStatusCode => EntryStatusCodes.Cleared;
	}
}
