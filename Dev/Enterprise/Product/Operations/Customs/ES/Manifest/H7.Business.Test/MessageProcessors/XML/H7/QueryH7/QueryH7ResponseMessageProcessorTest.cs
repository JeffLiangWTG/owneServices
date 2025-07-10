using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class QueryH7ResponseMessageProcessorTest : H7CommonResponseMessageProcessorTest<QueryH7ResponseMessageProcessor, QueryH7MessagePrettyFormatter, ConsultaH7V1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			SetupOutgoingMessageWithCertificate(message);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<H3>Declaration Information</H3>" +
				"<table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AN: Annulled.</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Presentation:</td><td>&nbsp;&nbsp;</td><td>16-01-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>15-01-2025, 19:00:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES123456A0000112</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101211111</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Documentation Required:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>" +
				"<table border=\"0\"><tr><td>Risk Analysis Result Code:</td><td>&nbsp;&nbsp;</td><td>N: <strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Release:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 20:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV1357911222222</td></tr></table>" +
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

			AssertDeclarationH7AcceptedResults(message, bill, expectedBillStatus: "CAN", expectedReleaseDate: releaseDate, expectedMrnEntryStatus: "6",  expectedDocumentationRequired: "S", expectedReleaseCsvId: releaseCsvId, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		void AssertDeclarationH7AcceptedResults(TestEdiMessage responseMessage, AsycudaBill bill, ZString expectedBillStatus, ZDateTime expectedReleaseDate, string expectedMrnEntryStatus = "", string expectedDocumentationRequired = "", string expectedReleaseCsvId = "", string expectedMessageInterpretation = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("Message Status", "ACC", bill.ABL_MessageStatus);
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

				var cusEntryNumberMRN = GetCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, null, ZDateTime.Empty, expectedMrnEntryStatus);
				AssertNotNull("MRN Entry Number", cusEntryNumberMRN);
			});
		}

		public void TestProcessAcceptedMessage_WillSendDocumentRequest()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			SetupOutgoingMessageWithCertificate(message);
			ProcessMessageForTest(message);
#if NET
			var messageText = ZString.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ESCustoms xmlns=\"http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest\">\r\n  <File>\r\n    <name>{0}</name>\r\n    <url_parameter>{1}</url_parameter>\r\n  </File>\r\n</ESCustoms>", "23ES123456A0000112_H7_AEAT_CLR.pdf", releaseCsvId);
#else
			var messageText = ZString.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<q1:ESCustoms xmlns:q1=\"http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest\">\r\n  <q1:File>\r\n    <q1:name>{0}</q1:name>\r\n    <q1:url_parameter>{1}</q1:url_parameter>\r\n  </q1:File>\r\n</q1:ESCustoms>", "23ES123456A0000112_H7_AEAT_CLR.pdf", releaseCsvId);
#endif
			CombineAssertions("Document request message should be sent automatically", () =>
			{
				bill.Messages.Reload(true);
				var message = bill.Messages.LastMessage;

				AssertEquals("MessageType", "DOC", message.EM_MessageType);
				AssertEquals("MessageSubType", "DOC", message.EM_MessageSubType);
				AssertEquals("MessageStatus", "QUE", message.EM_Status);
				AssertEquals("MessageText", messageText, message.EM_MessageText);
			});
		}

		public void TestStatusDeclarationToBillStatusMapping()
		{
			SetupTestData();
			var expectedMapping = new Dictionary<string, string>
			{
				{ "PR", "PRE" },
				{ "PD", "CON" },
				{ "DE", "REG" },
				{ "LE", "REL" },
				{ "AN", "CAN" },
				{ "H1", "SUP" },
				{ "CP", "REL" },
				{ "NL", "NOT" },
				{ "DV", "REX" },
				{ "IN", "INV" },
				{ "U1", "REL" },
				{ "AD", "ABD" },
				{ "A1", "GEN" },
				{ "RA", "CAR" },
			};

			CombineAssertions(() =>
			{
				foreach (var mapping in expectedMapping)
				{
					bill.ABL_BillStatus = string.Empty;
					var statusDeclaration = mapping.Key;
					var expectedBillStatus = mapping.Value;

					var messageWithStatus = string.Format(GetAcceptedTemplateMessage_DocumentationRequired_riskAnalysis_statusDeclaration(), "S", "L", statusDeclaration);
					var message = CreateNewEDIMessage(ApplicationReference, messageWithStatus, InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
					SetupOutgoingMessageWithCertificate(message);
					ProcessMessageForTest(message);
					AssertEquals(statusDeclaration, expectedBillStatus, bill.ABL_BillStatus);
				}
			});
		}

		public void TestRiskAnalysisResultCodeToH7MRNEntryStatusMapping()
		{
			SetupTestData();
			var expectedMapping = new Dictionary<string, string>
			{
				{ "L", "7" },
				{ "V", "4" },
				{ "R", "5" },
				{ "N", "6" },
			};

			CombineAssertions(() =>
			{
				foreach (var mapping in expectedMapping)
				{
					h7Mrn.CE_EntryStatus = string.Empty;
					var riskAnalysisResultCode = mapping.Key;
					var expectedEntryStatus = mapping.Value;

					var messageWithRiskAnalysisResultCode = string.Format(GetAcceptedTemplateMessage_DocumentationRequired_riskAnalysis_statusDeclaration(), "S", riskAnalysisResultCode, "PR");
					var message = CreateNewEDIMessage(ApplicationReference, messageWithRiskAnalysisResultCode, InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
					SetupOutgoingMessageWithCertificate(message);
					ProcessMessageForTest(message);
					AssertEquals(riskAnalysisResultCode, expectedEntryStatus, h7Mrn.CE_EntryStatus);
				}
			});
		}

		public void TestSetDocumentationRequired()
		{
			SetupTestData();
			var expectedResult = new Dictionary<string, string>
			{
				{ "S", "S" },
				{ "N", "N" },
				{ "", "O" } // When empty, leave unchanged from Original
			};

			CombineAssertions(() =>
			{
				foreach (var mapping in expectedResult)
				{
					bill.DocumentationRequired = "O";
					var messageDocumentationRequired = mapping.Key;
					var expectedDocumentationRequired = mapping.Value;

					var messageWithDocumentationRequired = string.Format(GetAcceptedTemplateMessage_DocumentationRequired_riskAnalysis_statusDeclaration(), messageDocumentationRequired, "L", "PR");
					var message = CreateNewEDIMessage(ApplicationReference, messageWithDocumentationRequired, InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
					SetupOutgoingMessageWithCertificate(message);
					ProcessMessageForTest(message);
					AssertEquals(messageDocumentationRequired, expectedDocumentationRequired, bill.DocumentationRequired);
				}
			});
		}

		public void TestProcessAcceptedMessage_NoMRNMatch()
		{
			SetupTestData();
			h7Mrn.CE_EntryNum = "NOMATCH";

			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			AssertNoExceptionThrown(() => ProcessMessageForTest(message));
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Query H7 Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.H7Query };

		protected override QueryH7ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new QueryH7ResponseMessageProcessor(logger);

		string GetAcceptedMessage() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "AcceptedMessage.xml");
		string GetAcceptedTemplateMessage_DocumentationRequired_riskAnalysis_statusDeclaration() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "AcceptedMessageNoTaxes_Template_DocumentationRequired_riskAnalysis_statusDeclaration.xml");
		protected override string GetRejectedMessageWithOneErrorTypeN() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "RejectedMessageWithOneErrorTypeN.xml");
		protected override string GetRejectedMessageWithOneErrorTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "RejectedMessageWithOneErrorTypeF.xml");
		protected override string GetRejectedMessageWithMultipleErrorsTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "RejectedMessageWithMultipleErrorsTypeF.xml");
		protected override string GetRejectedMessageWithMultipleErrors() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.QueryH7TestFilePath, "RejectedMessageWithMultipleErrors.xml");

		protected override void SetupTestData()
		{
			base.SetupTestData();

			h7Mrn = bill.CustomsEntryNumbers.AddNew();
			h7Mrn.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			h7Mrn.CE_EntryLineReference = H7EntryLineReference;
			h7Mrn.CE_EntryNum = mrn;
		}

		ABLEntryNum h7Mrn;
		ZDateTime releaseDate => new ZDateTime("17-01-2025, 20:00:00");
		string mrn => "23ES123456A0000112";
		string releaseCsvId => "XIV1357911222222";
	}
}
