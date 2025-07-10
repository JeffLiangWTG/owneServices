using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaLevanteParV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class InboxNotificationNctsDepartureClearanceMessageProcessorTest : NCTS5CommonInboxNotificationResponseMessageProcessorTest<InboxNotificationNctsDepartureClearanceResponseMessageProcessor, InboxNotificationNctsDepartureClearanceMessagePrettyFormatter, ComunicaLevanteParV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			AddMessageProcessAndAssertResult_AcceptedMessage();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearance, AddMessageProcessAndAssertResult_AcceptedMessage);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			AddMessageProcessAndAssertResult_AcceptedMessage();

			CombineAssertions(() =>
			{
				nctsHeader.Messages.Reload(true);
				var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);
				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCodeClearance + "_NCTS_AEAT_DAT.pdf", ClearanceReferenceNumber) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearance + "_NCTS_AEAT_DAT.pdf", "CAU");
			docManagerInfo.Save();

			AddMessageProcessAndAssertResult_AcceptedMessage();

			CombineAssertions(() =>
			{
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNames = new List<ZString>() { MRNCodeClearance + "_NCTS_AEAT_DAT.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				nctsHeader.Messages.Reload(true);
				var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		void AddMessageProcessAndAssertResult_AcceptedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CLETPA - Clearance Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 11:20:38</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeClearance + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>" + ExpiryDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>" + ClearanceReferenceNumber + "</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>" + ClearanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";

			CombineAssertions(() =>
			{
				var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
				queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
				var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

				var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
				queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
				queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
				var cusEntryNumberClearance = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

				AssertEquals("CusEntryNum MRN CE_EntryNum", MRNCodeClearance, cusEntryNumberMRN.CE_EntryNum);
				AssertEquals("CusEntryNum MRN CE_EntryStatus", MessageFunctionCodeList.Codes.OrangeCircuit, cusEntryNumberMRN.CE_EntryStatus);

				AssertEquals("CusEntryNum CLR CE_EntryNum", ClearanceReferenceNumber, cusEntryNumberClearance.CE_EntryNum);
				AssertEquals("CusEntryNum CLR CE_IssueDate", ClearanceDate.ToCustomsFormatDateStringddMMyyyyWithDash(), cusEntryNumberClearance.CE_IssueDate.ToCustomsFormatDateStringddMMyyyyWithDash());
				AssertEquals("CusEntryNum CLR CE_ExpiryDate", ExpiryDate, cusEntryNumberClearance.CE_ExpiryDate);
			});
			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCodeClearance, mrnEntryStatus: MessageFunctionCodeList.Codes.OrangeCircuit, mrnIssueDate: ClearanceDate, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceIssueDate: clearanceDate, clearanceExpiryDate: ExpiryDate);
		}

		protected override ZString GetMrncode() => MRNCodeClearance;

		const string MRNCodeClearance = "22ES000101500647K2";
		readonly ZDateTime clearanceDate = new ZDateTime(2020, 11, 21);

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox notification NCTS Departure clearance";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance };

		protected override ZString GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureClearanceNCTSTestFilePath, "ComunicaLevantePar.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureClearanceNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override InboxNotificationNctsDepartureClearanceResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationNctsDepartureClearanceResponseMessageProcessor(logger);
	}
}
