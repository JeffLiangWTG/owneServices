using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3RevokeResponseMessageProcessorTest : G3CommonResponseMessageProcessorTest<G3RevokeResponseMessageProcessor, G3RevokeMessagePrettyFormatter, G3RevokeV1Sal>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "G3 Revoke Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G3RevocationOfGoods };

		protected override G3RevokeResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new G3RevokeResponseMessageProcessor(logger);

		public void TestProcessMessage_AcceptsAndSendsH7Q()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessage = "<H3>Accepted Declaration</H3><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>RZSURPDJ8S62KF7U</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertMessage(message, DeclarationMessageSubTypeList.Codes.AcceptedResponse, EDIMessageStatusList.Codes.Received, expectedMessage);

				AssertEquals("referencedBill1 should be Prelodged", AISEntryStatusList.Codes.Prelodged, referencedBill1.ABL_BillStatus);
				AssertEquals("referencedBill2 should be Prelodged", AISEntryStatusList.Codes.Prelodged, referencedBill2.ABL_BillStatus);

				AssertEquals("20ESG3B000000001Q6", referencedBill1.G3RevokedMovementReferenceNumber);
				AssertEquals("20ESG3B000000001Q6", referencedBill2.G3RevokedMovementReferenceNumber);

				AssertReferencedBillsMessageStatus(LogicalStatusList.Codes.Sent);
				AssertReferencedBillSendsQueryH7(referencedBill1, bill1Mrn);
				AssertReferencedBillSendsQueryH7(referencedBill2, bill2Mrn);
				AssertReferencedBillHasNoH7QuerySent(referencedBill3);
			});
		}

		public void TestProcessMessage_RejectsAndSetsInvalid_WhenInvalid()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithInvalidCode(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessage = "<H3>Rejected</H3><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Description</strong></td></tr>" +
				"<tr><td>899</td><td>Inval pointer</td><td>Inval desc</td></tr>" +
				"</table>";

			CombineAssertions(() =>
			{
				AssertMessage(message, DeclarationMessageSubTypeList.Codes.RejectedResponse, EDIMessageStatusList.Codes.Received, expectedMessage);
				AssertReferencedBillsMessageStatus(LogicalStatusList.Codes.Invalid);
			});
		}

		public void TestProcessMessage_RejectsAndSetsError_WhenError()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithErrorCode(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessage = "<H3>Rejected</H3><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4><" +
				"table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Description</strong></td></tr>" +
				"<tr><td>900</td><td>ErrorPointer</td><td>ErrorDesc</td></tr>" +
				"</table>";

			CombineAssertions(() =>
			{
				AssertMessage(message, DeclarationMessageSubTypeList.Codes.RejectedResponse, EDIMessageStatusList.Codes.Received, expectedMessage);
				AssertReferencedBillsMessageStatus(LogicalStatusList.Codes.Error);
			});
		}

		public void TestProcessMessage_RejectsAndSetsError_WhenErrorAndInvalid()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithInvalidAndErrorCode(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessage = "<H3>Rejected</H3><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Description</strong></td></tr>" +
				"<tr><td>899</td><td>Inval pointer</td><td>Inval desc</td></tr><tr><td>900</td><td>Err pointer</td><td>Err desc</td></tr>" +
				"</table>";

			CombineAssertions(() =>
			{
				AssertMessage(message, DeclarationMessageSubTypeList.Codes.RejectedResponse, EDIMessageStatusList.Codes.Received, expectedMessage);
				AssertReferencedBillsMessageStatus(LogicalStatusList.Codes.Error);
			});
		}

		string GetAcceptedMessage() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3RevokeTestFilePath, "AcceptedMessage.xml");
		string GetRejectedMessageWithInvalidCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3RevokeTestFilePath, "RejectedMessageWithInvalidCode.xml");
		string GetRejectedMessageWithErrorCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3RevokeTestFilePath, "RejectedMessageWithErrorCode.xml");
		string GetRejectedMessageWithInvalidAndErrorCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3RevokeTestFilePath, "RejectedMessageWithInvalidAndErrorCode.xml");
	}
}
