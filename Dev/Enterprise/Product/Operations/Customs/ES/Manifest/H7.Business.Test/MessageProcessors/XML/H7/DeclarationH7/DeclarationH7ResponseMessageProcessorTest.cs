using System;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestDate(2025, 5, 26, 11, 48, 00)]
	public class DeclarationH7ResponseMessageProcessorTest : H7CommonResponseMessageProcessorTest<DeclarationH7ResponseMessageProcessor, DeclarationH7MessagePrettyFormatter, AltaH7V1Sal>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "Declaration H7 Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.H7Declaration };

		protected override DeclarationH7ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DeclarationH7ResponseMessageProcessor(logger);

		public void TestProcessAcceptedMessage_Type0_DocRequiredN_RiskV()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType0DocRequiredNRiskV(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: H7 admission</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>V: <strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: mrn, expectedMrnIssueDate: presentationDate, expectedMrnEntryStatus: CircuitCodeList.Codes.GREEN, expectedDocumentationRequired: DocumentationRequiredN, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type0_DocRequiredS_RiskL_ReleaseCsvId_OneTax()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType0DocRequiredSRiskLReleaseCSVIdOneTax(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			SetupOutgoingMessageWithCertificate(message);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: H7 admission</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Documentation Required:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>L: ENS in latency period</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV1357911222222</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>tax1</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>1</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>1</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: mrn, expectedMrnIssueDate: presentationDate, expectedMrnEntryStatus: CircuitCodeList.Codes.YELLOW, expectedDocumentationRequired: DocumentationRequiredS, expectedReleaseCsvId: releaseCsvId, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type0_RiskN_Taxes_NoPresentationDate()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType0RiskNTaxesNoPresentationDate(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: H7 admission</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>N: <strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IVA</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>100</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>300</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IGIC</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>-10000000000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>-10000000000000000</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>AIEM</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>10000000000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>10000000000000000</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: mrn, expectedMrnIssueDate: ZDateTime.Empty, expectedMrnEntryStatus: CircuitCodeList.Codes.ORANGE, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type0_RiskR_NoMrn()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType0RiskRNoMrn(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: H7 admission</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>R: <strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: string.Empty, expectedMrnIssueDate: ZDateTime.Empty, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type1_DocRequiredN_RiskR_OneTax()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1DocRequiredNRiskROneTax(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>1: Pre-H7 presentation</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>R: <strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IVA</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>100</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>300</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: mrn, expectedMrnIssueDate: presentationDate, expectedMrnEntryStatus: CircuitCodeList.Codes.RED, expectedDocumentationRequired: DocumentationRequiredN, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type1_RiskN_Taxes()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1RiskNTaxes(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>1: Pre-H7 presentation</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>N: <strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IVA</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>100</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>300</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>IGIC</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>-10000000000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>-10000000000000000</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Currency:</td><td>&nbsp;&nbsp;</td><td>USD</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Type:</td><td>&nbsp;&nbsp;</td><td>AIEM</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Amount Payable:</td><td>&nbsp;&nbsp;</td><td>10000000000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Tax Base:</td><td>&nbsp;&nbsp;</td><td>10000000000000000</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: releaseDate, expectedMrn: mrn, expectedMrnIssueDate: presentationDate, expectedMrnEntryStatus: CircuitCodeList.Codes.ORANGE, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type1_RiskL_ReleaseCsvId_NoReleaseDate()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1RiskLReleaseCsvIdNoReleaseDate(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			SetupOutgoingMessageWithCertificate(message);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>1: Pre-H7 presentation</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000011</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>L: ENS in latency period</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV1357911222222</td></tr></table>";

			AssertDeclarationH7AcceptedResults(message, bill, expectedReleaseDate: ZDateTime.Empty, expectedMrn: mrn, expectedMrnIssueDate: presentationDate, expectedMrnEntryStatus: CircuitCodeList.Codes.YELLOW, expectedReleaseCsvId: releaseCsvId, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_WithReleaseCsvId_ShouldSendDocumentRequest()
		{
			SetupTestData();
			bill.MovementReferenceNumber = mrn;
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1RiskLReleaseCsvIdNoReleaseDate(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			SetupOutgoingMessageWithCertificate(message);
			ProcessMessageForTest(message);
#if NET
			var messageText = ZString.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ESCustoms xmlns=\"http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest\">\r\n  <File>\r\n    <name>{0}</name>\r\n    <url_parameter>{1}</url_parameter>\r\n  </File>\r\n</ESCustoms>", "23ES123456A0000011_H7_AEAT_CLR.pdf", releaseCsvId);
#else
			var messageText = ZString.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<q1:ESCustoms xmlns:q1=\"http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest\">\r\n  <q1:File>\r\n    <q1:name>{0}</q1:name>\r\n    <q1:url_parameter>{1}</q1:url_parameter>\r\n  </q1:File>\r\n</q1:ESCustoms>", "23ES123456A0000011_H7_AEAT_CLR.pdf", releaseCsvId);
#endif
			CombineAssertions("Document request message should be sent automatically", () =>
			{
				bill.Messages.Reload(true);
				var message = bill.Messages.OfType<EDIMessage>().Single(m => m.EM_MessageType == "DOC");

				AssertEquals("MessageSubType", "DOC", message.EM_MessageSubType);
				AssertEquals("MessageStatus", "QUE", message.EM_Status);
				AssertEquals("MessageText", messageText, message.EM_MessageText);
			});
		}

		public void TestProcessAcceptedMessage_UpdateExistingH7Mrn()
		{
			SetupTestData();
			var cusEntryNumber = bill.CustomsEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = "MRN";
			cusEntryNumber.CE_EntryNum = "ESMRNH70001";
			cusEntryNumber.CE_EntryLineReference = "H7";
			Factory.Save();

			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1RiskNTaxes(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			ProcessMessageForTest(message);

			var h7MrnEntryNumbers = bill.CustomsEntryNumbers.OfType<ABLEntryNum>().Where(x =>
				x.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && x.CE_EntryLineReference == H7EntryLineReference).ToList();

			AssertEquals(1, h7MrnEntryNumbers.Count);
			AssertEquals(mrn, h7MrnEntryNumbers.First().CE_EntryNum);
		}

		public void TestProcessAcceptedMessage_DoNotUpdateExistingG3Mrn()
		{
			SetupTestData();
			var g3Mrn = "ESMRNG30001";
			var cusEntryNumber = bill.CustomsEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_EntryNum = g3Mrn;
			cusEntryNumber.CE_EntryLineReference = G3EntryLineReference;
			Factory.Save();

			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType1RiskNTaxes(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			ProcessMessageForTest(message);

			var g3MrnEntryNumber = bill.CustomsEntryNumbers.OfType<ABLEntryNum>().FirstOrDefault(x =>
				x.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && x.CE_EntryLineReference == G3EntryLineReference);

			var h7MrnEntryNumber = bill.CustomsEntryNumbers.OfType<ABLEntryNum>().FirstOrDefault(x =>
				x.CE_EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && x.CE_EntryLineReference == H7EntryLineReference);

			AssertEquals("G3 MRN should not be updated", g3Mrn, g3MrnEntryNumber.CE_EntryNum);
			AssertEquals("H7 MRN should be added", mrn, h7MrnEntryNumber.CE_EntryNum);
		}

		void AssertDeclarationH7AcceptedResults(TestEdiMessage responseMessage, AsycudaBill bill, ZDateTime expectedReleaseDate, string expectedMrn, ZDateTime expectedMrnIssueDate, string expectedMrnEntryStatus = "", string expectedDocumentationRequired = "", string expectedReleaseCsvId = "", string expectedMessageInterpretation = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("Release Date", expectedReleaseDate.Date, bill.ABL_ReleaseDate);
				AssertEquals("Documentation Required", expectedDocumentationRequired, bill.DocumentationRequired);

				AssertEquals("Message Sub Type", DeclarationMessageSubTypeList.Codes.AcceptedResponse, responseMessage.EM_MessageSubType);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("Message Interpretation", expectedMessageInterpretation, responseMessage.EM_MessageInterpretation);

				if (!string.IsNullOrEmpty(expectedReleaseCsvId))
				{
					var cusEntryNumberCSV = GetCusEntryNumber(bill, CusEntryNumberTypes.Spain.ClearanceCSV, expectedReleaseCsvId, expectedReleaseDate);
					AssertNotNull("CLR Entry Number", cusEntryNumberCSV);
				}
				else
				{
					var cusEntryNumberCSV = GetCusEntryNumber(bill, CusEntryNumberTypes.Spain.ClearanceCSV, entryNum: null, issueDate: ZDateTime.Empty, entryLineReference: null);
					AssertNull("CLR Entry Number", cusEntryNumberCSV);
				}

				var sentMessage = bill.Messages.Where(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.H7Query).FirstOrDefault();

				if (!string.IsNullOrEmpty(expectedMrn))
				{
					var cusEntryNumberMRN = GetCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, expectedMrn, expectedMrnIssueDate, expectedMrnEntryStatus);
					AssertNotNull("MRN Entry Number", cusEntryNumberMRN);

					var delayTime = new DateTime(2025, 5, 26, 11, 50, 30);
					AssertNotNull("Query H7 message should be sent", sentMessage);
					Assert("Query H7 message should include MRN", sentMessage.EM_MessageText.Contains($"<MRN_H7>{expectedMrn}</MRN_H7>"));
					AssertEquals("Query H7 message's EM_HeldUntilDate as EM_SystemCreateTimeUTC plus 150 seconds.", delayTime, sentMessage.EM_HeldUntilDate);
					AssertEquals("Message Status should be SNT", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				}
				else
				{
					var cusEntryNumberMRN = GetCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNum: null, issueDate: ZDateTime.Empty, entryLineReference: null);
					AssertNull("MRN Entry Number", cusEntryNumberMRN);

					AssertNull("Query H7 message should not be sent", sentMessage);
					AssertEquals("Message Status should be ACC", LogicalStatusList.Codes.Accepted, bill.ABL_MessageStatus);
				}
			});
		}

		string GetAcceptedMessageWithType0DocRequiredNRiskV() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType0DocRequiredNRiskV.xml");
		string GetAcceptedMessageWithType0DocRequiredSRiskLReleaseCSVIdOneTax() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType0DocRequiredSRiskLReleaseCsvIdOneTax.xml");
		string GetAcceptedMessageWithType0RiskNTaxesNoPresentationDate() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType0RiskNTaxesNoPresentationDate.xml");
		string GetAcceptedMessageWithType0RiskRNoMrn() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType0RiskRNoMrn.xml");
		string GetAcceptedMessageWithType1DocRequiredNRiskROneTax() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType1DocRequiredNRiskROneTax.xml");
		string GetAcceptedMessageWithType1RiskNTaxes() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType1RiskNTaxes.xml");
		string GetAcceptedMessageWithType1RiskLReleaseCsvIdNoReleaseDate() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessageWithType1RiskLReleaseCsvIdNoReleaseDate.xml");
		protected override string GetRejectedMessageWithOneErrorTypeN() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "RejectedMessageWithOneErrorTypeN.xml");
		protected override string GetRejectedMessageWithOneErrorTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "RejectedMessageWithOneErrorTypeF.xml");
		protected override string GetRejectedMessageWithMultipleErrorsTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "RejectedMessageWithMultipleErrorsTypeF.xml");
		protected override string GetRejectedMessageWithMultipleErrors() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "RejectedMessageWithMultipleErrors.xml");

		ZDateTime releaseDate => new ZDateTime("17-01-2025, 20:00:00");
		ZDateTime presentationDate => new ZDateTime("16-01-2025, 18:00:00");
		string mrn => "23ES123456A0000011";
		string releaseCsvId => "XIV1357911222222";
	}
}
