using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3PresV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3DeclarationResponseMessageProcessorTest : G3CommonResponseMessageProcessorTest<G3DeclarationResponseMessageProcessor, G3DeclarationMessagePrettyFormatter, G3PresV1Sal>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "G3 Declaration Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G3DeclarationOfGoods };

		protected override G3DeclarationResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new G3DeclarationResponseMessageProcessor(logger);

		public void TestProcessMessage_AcceptsAndSendsH7Q()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessage = "<H3>Accepted Declaration</H3><br>" +
				"<table border=\"0\"><tr><td>Declaration Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144150</td></tr></table>" +
				"<table border=\"0\"><tr><td>Presentation Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144151</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>28SQ7LWDBC72MRKP</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code Effective Date Time</td><td>&nbsp;&nbsp;</td><td>20200401124513</td></tr></table><br>" +
				"<strong>Master Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Previous Document Type</td><td>&nbsp;&nbsp;</td><td>337: 20200104AIR78456</td></tr></table>" +
				"<table border=\"0\"><tr><td>Transport Document Type</td><td>&nbsp;&nbsp;</td><td>N740: 99103033726</td></tr></table>" +
				"<table border=\"0\"><tr><td>Receptacle</td><td>&nbsp;&nbsp;</td><td>991030337260103668</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Transport Document</td><td>&nbsp;&nbsp;</td><td>5025: IR231113997HK</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Transport Document</td><td>&nbsp;&nbsp;</td><td>5025: MA280212006HK</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>LN</td></tr></table>";

			var messageMrn = "20ESG3B000000001Q6";
			CombineAssertions(() =>
			{
				AssertMessage(message, DeclarationMessageSubTypeList.Codes.AcceptedResponse, EDIMessageStatusList.Codes.Received, expectedMessage);

				AssertEquals("referencedBill1 G3 Mrn is set from message", messageMrn, referencedBill1.G3MovementReferenceNumber);
				AssertEquals("referencedBill2 G3 Mrn is set from message", messageMrn, referencedBill2.G3MovementReferenceNumber);

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

		string GetAcceptedMessage() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3DeclarationTestFilePath, "AcceptedMessage.xml");
		string GetRejectedMessageWithInvalidCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3DeclarationTestFilePath, "RejectedMessageWithInvalidCode.xml");
		string GetRejectedMessageWithErrorCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3DeclarationTestFilePath, "RejectedMessageWithErrorCode.xml");
		string GetRejectedMessageWithInvalidAndErrorCode() => ESH7TestFileReader.GetEmbeddedFileText(G3MessageProcessorTestFileConstants.G3DeclarationTestFilePath, "RejectedMessageWithInvalidAndErrorCode.xml");
	}
}
