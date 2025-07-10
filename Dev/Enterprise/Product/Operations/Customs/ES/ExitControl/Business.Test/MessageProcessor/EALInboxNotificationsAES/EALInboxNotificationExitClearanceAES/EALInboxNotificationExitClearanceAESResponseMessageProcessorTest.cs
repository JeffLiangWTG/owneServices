using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteSalidaV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class EALInboxNotificationExitClearanceAESResponseMessageProcessorTest : EALCommonResponseMessageProcessorTest<EALInboxNotificationExitClearanceAESResponseMessageProcessor, EALInboxNotificationExitClearanceAESMessagePrettyFormatter, ComunicaLevanteSalidaV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			AddMessageProcessAndAssertResult();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearance, AddMessageProcessAndAssertResult);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			AddMessageProcessAndAssertResult();

			CombineAssertions(() =>
			{
				report.Messages.Reload(true);
				var docMessages = report.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCodeClearance + "_E_AEAT_EAL_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)report).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearance + "_E_AEAT_EAL_CLR.pdf", "CLR");
			docManagerInfo.Save();

			AddMessageProcessAndAssertResult();

			report.Messages.Reload(true);
			var docMessages = report.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void AddMessageProcessAndAssertResult()
		{
			var message = CreateNewEDIMessage(MRNCodeClearance, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLEVSA - Exit Clearance Information</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>27-10-2022, 10:12:18</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B9</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>DBGZ6TBAS32GJ76E</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>14-09-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";

			CombineAssertions(() =>
			{
				var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
				queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusExitReport));
				queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, report.PK);
				var clrEntryNumber = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

				AssertEquals("CE_IssueDate", new ZDateTime(2022, 09, 14), clrEntryNumber.CE_IssueDate);
				AssertEquals("CE_EntryNum", CsvClearance, clrEntryNumber.CE_EntryNum);
				AssertEquals("ClearanceReferenceNumber", CsvClearance, report.ClearanceReferenceNumber);

				AssertEALCommonDeclaration(message, AESEntryStatusList.Codes.ReleasedForExit, expectedMessageInterpretation: expectedMessageInterpretationText, messageNum: MessageNum);
			});
		}

		const string TestFilePath = "Enterprise.Customs.ES.ExitControl.Business.Testing.MessageProcessor.TestFiles.EALInboxNotificationExitClearanceAES";
		string GetAcceptanceTestFile() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCodeClearance = "22ES000101100171B9";
		const string CsvClearance = "DBGZ6TBAS32GJ76E";

		protected override ZString GetMRNCode() => MRNCodeClearance;

		protected override ZString MessageStatusForWrongXML => ZString.Empty;

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Exit Clearance Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification };

		protected override EALInboxNotificationExitClearanceAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new EALInboxNotificationExitClearanceAESResponseMessageProcessor(logger);

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			consignment.CXC_MovementReference = MRNCodeClearance;

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

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
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

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
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}
		protected override ZString GetLoggerMessagesWhenProcessMessageWrongXML() => "Unable to find business object for message";
		protected override ZString GetLoggerMessagesWhenProcessMessageError() => "Message Text is empty so can't continue with processing";
	}
}
