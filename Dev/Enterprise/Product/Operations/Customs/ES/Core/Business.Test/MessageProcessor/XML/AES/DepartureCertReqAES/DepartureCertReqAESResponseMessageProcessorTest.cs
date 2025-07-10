using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCCSEC_v514.CCCSECV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DepartureCertReqAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<DepartureCertReqAESResponseMessageProcessor, Cccsecv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Exit Certificate:</td><td>&nbsp;&nbsp;</td><td>EKNG6NZX5BNYX2NM</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5ANTZWJRXQDWMP4M</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			AssertDepartureCertReq(message, messageSubType: "ACC", csvExitCertificate: "EKNG6NZX5BNYX2NM", expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		void AssertDepartureCertReq(TestEdiMessage message, string messageSubType, string csvExitCertificate = "", string expectedMessageInterpretation = "")
		{
			AssertEquals("ZG_CSVExitCertificate", csvExitCertificate, entryHeader.ZG_CSVExitCertificate);

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, movementReferenceNumber: MRNCode, acceptanceDate: AcceptanceDate, messageSubType: messageSubType, entryStatusCode: OriginalEntryStatus, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.MovementReferenceNumberSetter(MRNCode, AcceptanceDate);
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureCertReqAESTestFilePath, "AcceptedMessage.txt");
		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureCertReqAESTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureCertReqAESTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureCertReqAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString RejectedMRN => MRNCode;

		protected override ZDateTime RejectedAcceptanceDate => AcceptanceDate;

		protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

		protected override ZString ErrorMRN => MRNCode;

		protected override ZDateTime ErrorAcceptanceDate => AcceptanceDate;

		protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCCSECV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCCSECV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"</table>";
		protected override ZString GetExpectedProcessorFriendlyName() => "Export Exit Certificate Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.RequestExportExitCertificate };

		protected override DepartureCertReqAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DepartureCertReqAESResponseMessageProcessor(logger);
	}
}
