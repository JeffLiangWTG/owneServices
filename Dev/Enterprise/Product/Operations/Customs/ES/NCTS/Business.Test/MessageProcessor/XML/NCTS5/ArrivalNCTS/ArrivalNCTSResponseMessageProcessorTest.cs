using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class ArrivalNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<ArrivalNCTSResponseMessageProcessor, ArrivalNCTSMessagePrettyFormatter, Cc007Cv1Sal>
	{
		public void TestProcessAcceptedMessageGreenResponseCodeRStatusRE()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeRStatusRE(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>21-11-2020, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>99980000521</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySUM = new ZQuery(CusEntryNumSchema.CE_EntryNum, SummaryReferenceNumber);
			querySUM.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySUM.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySUM.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = message.Factory.Load<CusEntryNumber>(querySUM).Single();

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.GREEN, mrnIssueDate: AdmissionDate, summaryEntryType: "SUM", summaryEntryNum: "99980000521", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageGreenResponseCodeUStatusRE()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeUStatusRE(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>21-11-2020, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>99980000521</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.GREEN, mrnIssueDate: AdmissionDate, summaryEntryType: "SUM", summaryEntryNum: "99980000521", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageGreenResponseCodeRStatusUL()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeRStatusUL(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>21-11-2020, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>99980000521</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySUM = new ZQuery(CusEntryNumSchema.CE_EntryNum, SummaryReferenceNumber);
			querySUM.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySUM.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySUM.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = message.Factory.Load<CusEntryNumber>(querySUM).Single();

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.GREEN, mrnIssueDate: AdmissionDate, summaryEntryType: "SUM", summaryEntryNum: "99980000521", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageResponseCodeS()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceCircuitResponseCodeS(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>Requesting data to Customs Departure. Please, send message again after 15 minutes</td></tr></table>";

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: OriginalEntryStatus, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageOrange()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceOrangeCircuitResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>21-11-2020, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>99980000521</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.ORANGE, mrnIssueDate: AdmissionDate, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageRed()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceRedCircuitResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>21-11-2020, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>99980000521</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			AssertNCTSDeclarationArrival(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.RED, mrnIssueDate: AdmissionDate, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestSetArrivalStatus()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithAutomaticCompletionOrangeCircuit = CreateNewEDIMessage(ApplicationReference, GetAcceptanceOrangeCircuitResponse(), InterchangeID);
				ProcessMessageForTest(messageARVWithAutomaticCompletionOrangeCircuit);
				AssertEquals("For ARV with automaticCompletion, orange circuit and code R, the result BM_CustomsStatus is C01", ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithAutomaticCompletionGreenCircuitCodeRStatusRE = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeRStatusRE(), InterchangeID);
				ProcessMessageForTest(messageARVWithAutomaticCompletionGreenCircuitCodeRStatusRE);
				AssertEquals("For ARV with automaticCompletion, green circuit, code R adn status RE, the result BM_CustomsStatus is UAP", ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithAutomaticCompletionGreenCircuitCodeRStatusUL = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeRStatusUL(), InterchangeID);
				ProcessMessageForTest(messageARVWithAutomaticCompletionGreenCircuitCodeRStatusUL);

				AssertEquals("For ARV with automaticCompletion, green circuit, code R adn status UL, the result BM_CustomsStatus is CL1", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithAutomaticCompletionGreenCircuitCodeS = CreateNewEDIMessage(ApplicationReference, GetAcceptanceCircuitResponseCodeS(), InterchangeID);
				ProcessMessageForTest(messageARVWithAutomaticCompletionGreenCircuitCodeS);
				AssertEquals("For ARV with automaticCompletion, green circuit and code S, the result BM_CustomsStatus is original status (not changed)", OriginalEntryStatus, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithAutomaticCompletionRedCircuitCodeR = CreateNewEDIMessage(ApplicationReference, GetAcceptanceRedCircuitResponse(), InterchangeID);
				ProcessMessageForTest(messageARVWithAutomaticCompletionRedCircuitCodeR);
				AssertEquals("For ARV with automaticCompletion, red circuit and code R, the result BM_CustomsStatus is C01", ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
				var messageARVWithRejection = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);
				ProcessMessageForTest(messageARVWithRejection);
				AssertEquals("For ARV with rejection, the result BM_CustomsStatus is original status (not changed)", OriginalEntryStatus, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			});
		}

		public void TestSetReleaseDate()
		{
			CombineAssertions(() =>
			{
				var testFileMessage = GetAcceptanceGreenCircuitResponseCodeRStatusUL();
				var message = CreateNewEDIMessage(ApplicationReference, testFileMessage, InterchangeID);
				ProcessMessageForTest(message);
				AssertEquals("BM_CustomsStatus is CL1", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("ReleaseDate is set", new ZDateTime(2020, 11, 21, 0, 0, 0), nctsHeader.ReleaseDate);

				nctsHeader.SummaryEntryNumber.CE_IssueDate = ZDateTime.Empty;
				testFileMessage = testFileMessage.Replace("<circuitoRecepcion>V</circuitoRecepcion>", "<circuitoRecepcion>A</circuitoRecepcion>");
				message = CreateNewEDIMessage(ApplicationReference, testFileMessage, InterchangeID);
				ProcessMessageForTest(message);
				AssertEquals("BM_CustomsStatus is CL1 when circuitoRecepcion is not V", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("ReleaseDate is empty when circuitoRecepcion is not V", ZDateTime.Empty, nctsHeader.ReleaseDate);
			});
		}

		protected void AssertNCTSDeclarationArrival(TestEdiMessage message, string messageSubType, string expectedMessageInterpretation = "", string commonCustomsStatus = "", string emStatus = EDIMessage.Status.Received, string messageStatus = "", string messageNum = MessageNum, CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberClearance = null, CusEntryNumber cusEntryNumberSummary = null, string mrnEntrynum = "", string mrnEntryStatus = "", string clearanceReferenceNumber = "", ZDateTime? clearanceIssueDate = null, ZDateTime? mrnIssueDate = null, ZDateTime? clearanceExpiryDate = null, string summaryEntryType = "", string summaryEntryNum = "", NctsArrivalMovementHeader arrivalMovementHeader = null, string phaseStatus = InitialPhaseStatus, string placeOfUnloadingCode = "")
		{
			AssertEquals("BM_UnloadingDate", mrnIssueDate ?? ZDateTime.Empty, nctsHeader.ArrivalMovementHeader.BM_UnloadingDate.ToZDateTime());
			AssertNCTSDeclaration(message, messageSubType, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: messageNum, commonCustomsStatus: commonCustomsStatus, emStatus: emStatus, messageStatus: messageStatus, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, cusEntryNumberSummary: cusEntryNumberSummary, mrnEntrynum: mrnEntrynum, mrnEntryStatus: mrnEntryStatus, clearanceReferenceNumber: clearanceReferenceNumber, clearanceIssueDate: clearanceIssueDate, mrnIssueDate: mrnIssueDate, clearanceExpiryDate: clearanceExpiryDate, summaryEntryType: summaryEntryType, summaryEntryNum: summaryEntryNum, arrivalMovementHeader: arrivalMovementHeader, phaseStatus: phaseStatus, placeOfUnloadingCode: placeOfUnloadingCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = initialUnloadingDate.ToDateTimeOffset(GlbBranch.CurrentBranch.HomePort);
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Arrival Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification };

		string GetAcceptanceGreenCircuitResponseCodeRStatusRE() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageGreenResponseCodeRStatusRE.txt");

		string GetAcceptanceGreenCircuitResponseCodeUStatusRE() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageGreenResponseCodeUStatusRE.txt");

		string GetAcceptanceGreenCircuitResponseCodeRStatusUL() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageGreenResponseCodeRStatusUL.txt");

		string GetAcceptanceCircuitResponseCodeS() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageResponseCondeS.txt");

		string GetAcceptanceOrangeCircuitResponse() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageOrange.txt");

		string GetAcceptanceRedCircuitResponse() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageRed.txt");

		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "RejectedMessage.txt");

		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "ErrorMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString TypeDeclaration => NctsMovementType.Codes.Arrival;

		protected override ArrivalNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ArrivalNCTSResponseMessageProcessor(logger);
		protected new readonly ZDateTime AdmissionDate = new ZDateTime(2020, 11, 21);
		readonly ZDateTime initialUnloadingDate = new ZDateTime(2021, 10, 01, 01, 30, 50);
		readonly ZString mrnEntryNumber = "23ES005001500004J1";
	}
}
