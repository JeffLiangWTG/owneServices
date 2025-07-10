using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class G7ExportResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestMessageWithNoLinkedObject()
		{
			const string messageText = @"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661+01E0012000001+11'
DTM+9:200209251015:203'
GIS+1'
RFF+ED:RC123420021100001'
UNT+6+12345600REFNBR'";

			const string errorMessage = "Could not find an associated business object (Job) for document reference = '01E0012000001'";

			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "G7 Export Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
			AssertEmail("G7 Export Response Message Processor Error Report", errorMessage, ediMessage.EM_MessageText, string.Empty);
		}

		public void TestMessageWithInvalidEDIFACTVersion()
		{
			const string messageText = @"UNH+12345600REFNBR+CUSRES:D:97B:UN'
BGM+:::661+01E0012000001+11'
DTM+9:200209251015:203'
GIS+1'
RFF+ED:RC123420021100001'
UNT+6+12345600REFNBR'";

			mailManager = Env.OutgoingCustomsMailManager;
			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEmail("CA Customs G7 Export Message Processor Error Report", "Inbound message is incorrect version", ediMessage.EM_MessageText, "blah@blah.com", string.Empty);
			ErrorReporter.Clear();
		}

		public void TestMessageWithInvalidServiceOption()
		{
			const string messageText =
@"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661XX+01E0012000001+11'
DTM+9:200209251015:203'
GIS+1'
RFF+ED:RC123420021100001'
UNT+6+12345600REFNBR'";

			mailManager = Env.OutgoingCustomsMailManager;
			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEmail("CA Customs G7 Export Message Processor Error Report", "Could not find a supporting Processor Class", ediMessage.EM_MessageText, "blah@blah.com", string.Empty);
			ErrorReporter.Clear();
		}

		public void TestAcceptedMessage()
		{
			const string messageText =
@"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661+54321X8000002+11'
DTM+9:200209251015:203'
GIS+1'
RFF+ED:RC123420021100001'
UNT+6+12345600REFNBR'";

			const string expectedBody = @"Reference Number : 54321X8000002<br />
<br />
</strong>A CLEAR response message has been received from the CBSA for a G7 Export Message.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>25-Sep-02 10:15:00</td></tr><tr><td>CERS Proof Of Report Number</td><td>RC123420021100001</td></tr></table>
<br />";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			AssertEquals("CERSProofOfReportNumber", "RC123420021100001", declaration.JE_CERSProofOfReportNumber);
			AssertEquals("Entry Status", EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Clear G7 Export Message Response for 54321X8000002", expectedBody, ediMessage.EM_MessageText);
		}

		public void TestErrorMessage()
		{
			const string messageText =
@"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661+54321X8000002+11'
DTM+9:200209251015:203'
GIS+14'
RFF+ED:RC123420021100001'
ERP+2:12345600REFNBR:29'
ERC+J22'
FTX+AAO+++0493'
ERP+2:12345600REFNBR:29'
ERC+J11'
FTX+AAO+++32'
ERP+2:12345600REFNBR:29'
ERC+200'
FTX+AAO+++AA'
UNT+6+12345600REFNBR'";

			const string expectedBody = @"Reference Number : 54321X8000002<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a G7 Export Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>25-Sep-02 10:15:00</td></tr><tr><td>CERS Proof Of Report Number</td><td>RC123420021100001</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error</th><th>Error Data Value</th></tr></thead><tr align=""left""><td>J22</td><td>Export Office: Export Office must be a valid Port office.</td><td>0493</td></tr><tr align=""left""><td>J11</td><td>Export Reason Code: Export Reason Code is not valid</td><td>32</td></tr><tr align=""left""><td>200</td><td>Country of Origin: FIELD IS NOT VALID</td><td>AA</td></tr></table>
<br />";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			AssertEquals("Entry Status does not change", EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorChange, entryHeader.CH_Status);
			AssertEmail("Error G7 Export Message Response for 54321X8000002", expectedBody, ediMessage.EM_MessageText);
		}

		public void TestMessageWithInvalidGIS()
		{
			const string messageText = @"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661+54321X8000002+11'
DTM+9:200209251015:203'
GIS+14XX'
RFF+ED:RC123420021100001'
ERP+2:12345600REFNBR:29'
ERC+J22'
FTX+AAO+++0493'
ERP+2:12345600REFNBR:29'
ERC+J11'
FTX+AAO+++32'
ERP+2:12345600REFNBR:29'
ERC+200'
FTX+AAO+++AA'
UNT+6+12345600REFNBR'";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

			var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
			expMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("Entry Status does not change", EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Message Status does not change", MessageStatusList.Codes.AwaitingChange, entryHeader.CH_Status);
			AssertEmail("G7 Export Response Message Processor Error Report", "was unable to interpret received message", ediMessage.EM_MessageText);
		}

		public void TestGenericSyntaxErrorMessage()
		{
			const string messageText =
@"UNH+1+CUSRES:D:00A:UN'
BGM+:::1000+54321X8000002'
DTM+9:200903070924:203'
GIS+14'
ERP+2:26:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTMEA-BYTE OFFSET468MAND SEG MISSING'
FTX+AAO+++SEGMENTMEALINE5ELE POS3,2ELEM TOO SHORT'
UNT+13+1'";

			const string expectedBody = @"Reference Number : 54321X8000002<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a G7 Export Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>07-Mar-09 09:24:00</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>MAND SEG MISSING</td><td>&nbsp;</td><td>MOA+39:7000.00:USD</td><td>MEA / L: 17, I: 11</td></tr><tr align=""left""><td>ELEM TOO SHORT</td><td>142</td><td>MEA+WT+AAD+KGM:142</td><td>MEA / L: 5, P: 3,2</td></tr></table>
<br />";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader.Messages.Add(SyntaxErrorMessageTest.GetSentEDIMessage<EX1STPMessage>(Factory, G7ExportMessageWrapperTest.G7ExportDeclarationMessage, ZDateTime.Now.AddDays(-1), "26"));
			var ediMessage = GetEDIMessage<EXPEDIMessage>(messageText);
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			syntaxMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.SyntaxError, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.G7Export, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			AssertEquals("Entry Status does not change", EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorChange, entryHeader.CH_Status);

			var expectedSyntaxErrorText = Factory.Load<SyntaxErrorMessage>(ediMessage.PK).SourceMessageTextWithErrorMarks;
			AssertSyntaxErrorEmail("Error G7 Export Message Response for 54321X8000002", expectedBody, ediMessage.EM_MessageText, expectedSyntaxErrorText);
			ErrorReporter.Clear();
		}

		public void TestGetLinkedObjectAndSetOnMessageWhenTwoOrMoreCACompany()
		{
			var companyCA1 = Factory.New<GlbCompany>();
			companyCA1.GC_Code = "CA1";
			companyCA1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var companyCA2 = Factory.New<GlbCompany>();
			companyCA2.GC_Code = "CA2";
			companyCA2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branchCA1 = Factory.New<GlbBranch>();
			branchCA1.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			branchCA1.GB_Code = "TL1";
			branchCA1.GB_GC = companyCA1.PK;

			var branchCA2 = Factory.New<GlbBranch>();
			branchCA2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			branchCA2.GB_Code = "TL2";
			branchCA2.GB_GC = companyCA2.PK;

			declaration.JE_GB = branchCA2.PK;
			Factory.Save();

			const string messageText =
@"UNH+12345600REFNBR+CUSRES:D:00A:UN'
BGM+:::661+54321X8000002+11'
DTM+9:200209251015:203'
GIS+1'
RFF+ED:RC123420021100001'
UNT+6+12345600REFNBR'";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

			using (DisposableEnvironment.ForBranch(branchCA1.PK.ToGuid()))
			{
				var ediMessage = GetEDIMessage<EX1STPMessage>(messageText);
				expMessageProcessor.ProcessMessage(ediMessage);
				AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
				AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			}
		}

		public void TestAlternateSyntaxErrorMessage()
		{
			const string messageText =
@"UNH+1+CUSRES:D:00A:UN'
BGM++54321X8000002+11'
DTM+9:200903070924:203'
GIS+14'
FTX+AAO+++SEGMENTDMSLINE19ELE POS1,1:ELEM TOO LONG'
ERP+2:123:29'
ERC+ZZZ'
UNT+8+1'";

			const string expectedBody = @"Reference Number : 54321X8000002<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a G7 Export Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>07-Mar-09 09:24:00</td></tr></table>
<br />
<br />
Malformed Syntax Error message from Customs, see message text for details.
<br />";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader.Messages.Add(SyntaxErrorMessageTest.GetSentEDIMessage<EX1STPMessage>(Factory, G7ExportMessageWrapperTest.G7ExportDeclarationMessage, ZDateTime.Now.AddDays(-1)));
			var ediMessage = GetEDIMessage<EXPEDIMessage>(messageText);
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var expMessageProcessor = new EXPMessageProcessor(logger);
			expMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.SyntaxError, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.G7Export, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			AssertEquals("Entry Status does not change", EntryStatusList.Codes.Clear, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorChange, entryHeader.CH_Status);

			AssertEmail("Error G7 Export Message Response for 54321X8000002", expectedBody, ediMessage.EM_MessageText);
			ErrorReporter.Clear();
		}

		protected override void SetUp()
		{
			base.SetUp();
			expMessageProcessor = new EXPMessageProcessor(logger);
			syntaxMessageProcessor = new GenericSyntaxResponseMessageProcessor(logger);
			declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B12345678";
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader.EntryNumber = "54321X8000002";

			var sentMessage = Factory.New<EX1STPMessage>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			entryHeader.Messages.Add(sentMessage);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "J22", "Export Office: Export Office must be a valid Port office.", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "J11", "Export Reason Code: Export Reason Code is not valid", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "200", "Country of Origin: FIELD IS NOT VALID", startDate, endDate);

			Factory.Save();
			mailManager.EmailsCreated.Clear();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		EXPMessageProcessor expMessageProcessor;
		GenericSyntaxResponseMessageProcessor syntaxMessageProcessor;
	}
}
