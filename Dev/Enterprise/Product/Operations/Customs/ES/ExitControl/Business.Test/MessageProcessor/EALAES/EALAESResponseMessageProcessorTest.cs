using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC507C_v514.CC507CV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class EALAESResponseMessageProcessorTest : EALCommonResponseMessageProcessorTest<EALAESResponseMessageProcessor, EALAESMessagePrettyFormatter, Cc507Cv1Sal>
	{
		public void TestProcessAcceptedMessage_CLR_GreenCircuit()
		{
			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileGreenCircuitResponseCodeLMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>L - Clearance</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>WBW3PATKBEJDAQ3D</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>28-06-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ReleasedForExit, circuit: CircuitCodeList.Codes.GREEN, arrivalDate: arrivalDate, entryReleaseDate: new ZDateTime(2022, 06, 28), csvClearance: CsvClearance, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CLR_RedCircuit()
		{
			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileRedCircuitResponseCodeLMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>L - Clearance</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ReleasedForExit, circuit: CircuitCodeList.Codes.RED, arrivalDate: arrivalDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CDA_OrangeCircuit_AndTriggerInboxRequest_EHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileOrangeCircuitResponseCodeBMessage(), InterchangeID);

				ProcessMessageForTest(message);
				var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
					"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
					"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
					"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>B - Goods Under Customs Control</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
					"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
				AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ControlledForExit, circuit: CircuitCodeList.Codes.ORANGE, arrivalDate: arrivalDate, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewInboxMessages(report.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification }, DeclarantId, DeclarantName, MRNCode);
			}
		}

		public void TestProcessAcceptedMessage_CDA_OrangeCircuit_AndTriggerInboxRequest_xT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(true))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileOrangeCircuitResponseCodeBMessage(), InterchangeID);

				ProcessMessageForTest(message);
				var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>B - Goods Under Customs Control</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
				AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ControlledForExit, circuit: CircuitCodeList.Codes.ORANGE, arrivalDate: arrivalDate, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewCusPollingTransaction(report.PK, report.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification }, MRNCode);
			}
		}

		public void TestProcessAcceptedMessage_EmptyStatus()
		{
			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileResponseCodeSMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>S - Requesting data to Export Customs Office</td></tr></table>" +
				"<table border=\"0\"><tr><td>Please, send message again after 15 minutes</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RZ - Rejected deviation</td></tr></table>";
			AssertEALDeclaration(message, statusCode: OriginalEntryStatus, arrivalDate: arrivalDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileGreenCircuitResponseCodeLMessage(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>L - Clearance</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>WBW3PATKBEJDAQ3D</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>28-06-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ReleasedForExit, circuit: CircuitCodeList.Codes.GREEN, arrivalDate: arrivalDate, entryReleaseDate: new ZDateTime(2022, 06, 28), csvClearance: CsvClearance, expectedMessageInterpretation: expectedMessageInterpretationText);

			CombineAssertions(() =>
			{
				report.Messages.Reload(true);
				var docMessages = report.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_EAL_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)report).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_EAL_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFileGreenCircuitResponseCodeLMessage(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>27-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>L - Clearance</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>WBW3PATKBEJDAQ3D</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>28-06-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>5GAUWM2NJGVXVRSU</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			AssertEALDeclaration(message, statusCode: AESEntryStatusList.Codes.ReleasedForExit, circuit: CircuitCodeList.Codes.GREEN, arrivalDate: arrivalDate, entryReleaseDate: new ZDateTime(2022, 06, 28), csvClearance: CsvClearance, expectedMessageInterpretation: expectedMessageInterpretationText);

			report.Messages.Reload(true);
			var docMessages = report.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		public void TestProcessRejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(MRNCode, GetGetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC507C/ExportOperation/MRN</td><td>(1162) No existe declaración para el valor del MRN indicado.</td><td>22ES000101100171B5</td></tr>" +
				"</table>";
			AssertEALDeclaration(responseMessage, statusCode: OriginalEntryStatus, LogicalStatusList.Codes.Error, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessErrorMessage()
		{
			var responseMessage = CreateNewEDIMessage(MRNCode, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC507CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC507CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"</table>";
			AssertEALDeclaration(responseMessage, statusCode: OriginalEntryStatus, LogicalStatusList.Codes.Error, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		void AssertEALDeclaration(TestEdiMessage message, string statusCode, string cerMessageStatus = LogicalStatusList.Codes.Accepted, string messageStatus = EDIMessage.Status.Received, string messageSubType = "ACC", string messageNum = MessageNum, string circuit = "", ZDateTime? arrivalDate = null, string csvClearance = "", ZDateTime? entryReleaseDate = null, string loggerDesc = "", string expectedMessageInterpretation = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("CER_DateTime", arrivalDate == null ? ZDateTimeOffset.Empty : arrivalDate.Value.ToOffset(), report.CER_DateTime);

				if (!string.IsNullOrEmpty(circuit))
				{
					var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
					queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusExitReport));
					queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, report.PK);
					var clrEntryNumber = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

					AssertEquals("CE_IssueDate", entryReleaseDate == null ? ZDateTime.Empty : entryReleaseDate, clrEntryNumber.CE_IssueDate);
					AssertEquals("CE_EntryNum", csvClearance, clrEntryNumber.CE_EntryNum);
					AssertEquals("CE_EntryStatus", circuit, clrEntryNumber.CE_EntryStatus);
					AssertEquals("ClearanceReferenceNumber", csvClearance, report.ClearanceReferenceNumber);
				}

				AssertEALCommonDeclaration(message, statusCode, cerMessageStatus, expectedMessageInterpretation: expectedMessageInterpretation, messageStatus: messageStatus, messageNum: messageNum, messageSubType: messageSubType, loggerDesc: loggerDesc);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			header.CXH_GS_NKCustomsAgent = staffWithCertificateHelperTest.Staff.GS_Code;
			header.CXH_CustomsProfile = staffWithCertificateHelperTest.Certificate.CertificateName;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			header.CXH_OA_Carrier = declarant.MainAddress.PK;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		const string TestFilePath = "Enterprise.Customs.ES.ExitControl.Business.Testing.MessageProcessor.TestFiles.EALAES";
		string GetAcceptanceTestFileGreenCircuitResponseCodeLMessage() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedGreenCircuitLMessage.txt");
		string GetAcceptanceTestFileRedCircuitResponseCodeLMessage() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedRedCircuitLMessage.txt");
		string GetAcceptanceTestFileOrangeCircuitResponseCodeBMessage() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedOrangeCircuitBMessage.txt");
		string GetAcceptanceTestFileResponseCodeSMessage() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedSMessage.txt");
		string GetGetRejectedTestFile() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "RejectedMessage.txt");
		string GetErrorTestFile() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		readonly ZDateTime arrivalDate = new ZDateTime(2022, 06, 27);
		const string DeclarantId = "NIF22222222";
		const string DeclarantName = "Declarant Full Name";
		const string CsvClearance = "WBW3PATKBEJDAQ3D";

		protected override ZString GetExpectedProcessorFriendlyName() => "Export EAL Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6 };

		protected override EALAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new EALAESResponseMessageProcessor(logger);
	}
}
