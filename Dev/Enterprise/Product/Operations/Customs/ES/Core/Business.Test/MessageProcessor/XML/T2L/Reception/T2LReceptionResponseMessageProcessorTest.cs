using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LReceptionResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LReceptionResponseMessageProcessor, T2LrecepcionV1Sal>
	{
		[TestDate(2020, 05, 10, 14, 50, 32)]
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lReceptionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, acceptanceDate: AcceptanceDate);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeExisting);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lReceptionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, movementReferenceNumber: MRNCodeExisting, acceptanceDate: AcceptanceDate);
		}

		public void TestProcessAcceptedMessageNotValidDate()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFileNotValidDate(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lReceptionMessage(message, entryHeader, acceptanceDate: ZDateTime.Empty, messageNum: "AA200507145055128085");
		}

		public void TestTriggerAnnexSendingAllButTheLastAnnexMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lReceptionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, chStatus: "AWR", acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to false because there are 2 annex documents declared but only one sent", "indicadorFinDeAnexado>N", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ForEach(x => x.Messages.Reload(true));
				var msgPivot = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault(x => x.Message != null).Message;
				AssertEquals("New EDIMessage is associated to the sent annex", msgPivot, msg);
			});
		}

		public void TestTriggerAnnexSendingLastAnnexMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lReceptionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, chStatus: "AWR", acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to true", "indicadorFinDeAnexado>S", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ForEach(x => x.Messages.Reload(true));
				var msgPivot = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault(x => x.Message != null).Message;
				AssertEquals("New EDIMessage is associated to the sent annex", msgPivot, msg);
			});
		}

		void AssertT2lReceptionMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string chStatus = "RCV", string movementReferenceNumber = MRNCode, ZDateTime? acceptanceDate = null, string messageNum = MessageNum)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: messageNum, messageSubType: "ACC", chStatus: chStatus, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, movementReferenceNumber: movementReferenceNumber, acceptanceDate: acceptanceDate);
		}

		string GetAcceptedTestFileNotValidDate() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionTestFilePath, "InvalidAcceptedMessage.txt");
		string GetAcceptedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionTestFilePath, "AcceptedMessage.txt");

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionTestFilePath, "RejectedMessage.txt");

		protected override T2LReceptionResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LReceptionResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Reception Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lReception };

		protected override ZString RejectedMRN => ZString.Empty;

		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>3004</td><td>Ya existe un documento T2L con el nº de referencia indicado.</td></tr>" +
						"</table>";

		ZString AcceptedAndClearedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento de Alta Indirecta de T2L admitido.</td></tr></table>";
	}
}
