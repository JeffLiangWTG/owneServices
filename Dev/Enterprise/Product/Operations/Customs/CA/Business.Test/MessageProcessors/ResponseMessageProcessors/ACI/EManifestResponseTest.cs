using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class EManifestResponseTest : EDIFACTMessageProcessorTest
	{
		public void TestContentAcceptedResponse()
		{
			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>A Content Accepted response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
The message has been validated and is error free.<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Original,
				"Message content accepted ACI eManifest House Bill Message Response for 8036X555",
				expectedBody);
		}

		public void TestErrorResponse()
		{
			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>An ERROR response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>07-Aug-13 21:36:00</td></tr><tr><td>Comment</td><td>22-Conformance</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error</th></tr></thead><tr align=""left""><td>W64</td><td>Primary CCN: Carrier cannot be a Freight Forwarder</td></tr></table>
<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				ValidationErrorMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ErrorOriginal,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Original,
				"Error ACI eManifest House Bill Message Response for 8036X555",
				expectedBody);
		}

		public void TestCancellationAcceptedResponse()
		{
			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>A Content Accepted response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
The message has been validated and is error free.<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingDelete;
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearDelete,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Cancellation,
				"Cancellation accepted ACI eManifest House Bill Message Response for 8036X555",
				expectedBody);
		}

		public void TestMatchedResponse()
		{
			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>A MATCHED response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>07-Aug-13 22:38:00</td></tr></table>
<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			AssertMessageResponse(
				MatchedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearChange,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Change,
				"Matched ACI eManifest House Bill Message Response for 8036X555",
				expectedBody);
		}

		public void TestNotMatchedResponse()
		{
			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>A NOT MATCHED response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following codes returned.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>07-Aug-13 22:38:00</td></tr></table>
<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				NotMatchedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Original,
				"NOT Matched ACI eManifest House Bill Message Response for 8036X555",
				expectedBody);
		}

		public void TestSyntaxErrorResponse()
		{
			const string expectedBody = @"Reference Number : 80363647474C<br />
<br />
</strong>An ERROR response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>06-Aug-13 01:33:00</td></tr><tr><td>Comment</td><td>UMRN(377)SEGMENTPACLINE21ELEM7064(3.4)MAND ELEM MISSING</td></tr><tr><td>Comment</td><td>UMRN(377)SEGMENTCNTLINE27ELEM6411(1.3)MAND ELEM MISSING</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>MAND ELEM MISSING</td><td>&nbsp;</td><td>PAC+5</td><td>PAC / L: 21, P: 3,4</td></tr><tr align=""left""><td>MAND ELEM MISSING</td><td>&nbsp;</td><td>CNT+7:121</td><td>CNT / L: 27, P: 1,3</td></tr></table>
<br />";
			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			house.BW_MessageReference = "803636474747";
			var outMessage = SyntaxErrorMessageTest.GetSentEDIMessage<ACIHouseBillMessage>(Factory, SyntaxErrorMessageTest.EManifestHouseBillMessageText, ZDateTime.Now.AddDays(-1));
			outMessage.EM_SendWithMessageErrors = true;
			outMessage.EM_MessageNum = "377";
			house.Messages.Add(outMessage);

			AssertMessageResponse(
				SyntaxErrorMessageTest.EManifestHouseBillSyntaxErrorMessageText,
				EDIMessage.Status.Received,
				string.Empty,
				MessageTypeList.Codes.SyntaxError,
				MessageTypeList.Codes.ACIHouseBill,
				"Error ACI eManifest House Bill Message Response for 80363647474C",
				expectedBody);
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			outMessage.EM_SendWithMessageErrors = false;
			AssertMessageResponse(
				SyntaxErrorMessageTest.EManifestHouseBillSyntaxErrorMessageText,
				EDIMessage.Status.Received,
				string.Empty,
				MessageTypeList.Codes.SyntaxError,
				MessageTypeList.Codes.ACIHouseBill,
				"Error ACI eManifest House Bill Message Response for 80363647474C",
				expectedBody);
			AssertEquals("ACI eManifest Forwarding message syntax error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidMessage()
		{
			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				"UNH+1+GOVCBR:D:11B:UN'BGM+23+8036X555'DTM+9:201308072136:203'RFF+AGO:HBL-CAH0000004'RCS+11'",
				EDIMessage.Status.Failed,
				MessageStatusList.Codes.AwaitingOriginal,
				MessageTypeList.Codes.ACIHouseBill,
				MessageSubTypeCodes.Codes.Original,
				"eManifest Forwarding Response Message Processor Error Report",
				"The message processor was unable to interpret received message");
		}

		public void TestInvalidLinkedObject()
		{
			house.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			house.BW_MessageReference = "BLAH";
			Factory.Save();
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Failed,
				MessageStatusList.Codes.AwaitingOriginal,
				MessageTypeList.Codes.ACIHouseBill,
				MessageTypeList.Codes.ACIHouseBill,
				"eManifest Forwarding Response Message Processor Error Report",
				"Could not find an associated business object (Job) for document reference",
				string.Empty);
		}

		public void TestUpdateHouseBillIsCloseReported_VIC()
		{
			var testMaster = Factory.New<CusCAeMHMaster>();
			testMaster.BP_MessageReference = "C10001000";
			testMaster.BP_PrimaryCCN = "C10001000CCN";
			testMaster.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			var testHouse1 = testMaster.HouseBills.AddNew();
			testHouse1.BW_MessageReference = "CAH1000001";
			testHouse1.BW_HouseCCN = "8036 CAH1000001";
			var testHouse2 = testMaster.HouseBills.AddNew();
			testHouse2.BW_MessageReference = "CAH1000002";
			testHouse2.BW_HouseCCN = "8036CAH1000002";
			var testHouse3 = testMaster.HouseBills.AddNew();
			testHouse3.BW_MessageReference = "CAH1000003";
			testHouse3.BW_HouseCCN = "8036CAH1000003";

			var sntMessage = Factory.New<ACIForwarderMessage>();
			sntMessage.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			sntMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			sntMessage.EM_LinkTable = testMaster.TableName;
			sntMessage.EM_LinkUniqueID = testMaster.PK;
			sntMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sntMessage.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			var rcvMessage = Factory.New<ACIForwarderMessage>();
			rcvMessage.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			rcvMessage.EM_MessageSubType = string.Empty;
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_Status = "QUE";
			rcvMessage.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+312+8036081-14073001+11
DTM+9:201407300108:203
RFF+AGO:CLS-C10001000
RCS+11
GEI+5+1
UNS+D
HYN+3
UNS+S
UNT+10+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();

			processor.ProcessMessage(rcvMessage);

			CombineAssertions("TestOfUpdatingCloseReported for successfulresponse", () =>
			{
				AssertEquals(MessageSubTypeCodes.Codes.Original, rcvMessage.EM_MessageSubType);
				AssertEquals(true, testHouse1.BW_IsCloseReported);
				AssertEquals(false, testHouse2.BW_IsCloseReported);
				AssertEquals(false, testHouse3.BW_IsCloseReported);
			});

			var sntMessage2 = Factory.New<ACIForwarderMessage>();
			sntMessage2.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			sntMessage2.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			sntMessage2.EM_LinkTable = testMaster.TableName;
			sntMessage2.EM_LinkUniqueID = testMaster.PK;
			sntMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sntMessage2.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000002
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			var rcvMessage2 = Factory.New<ACIForwarderMessage>();
			rcvMessage2.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			rcvMessage2.EM_MessageSubType = string.Empty;
			rcvMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage2.EM_Status = "QUE";
			rcvMessage2.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+8036081-53451134+11
DTM+9:201407292127:203
RFF+AGO:CLS-C10001000
RCS+11
FTX+AAO+++29
GEI+5+14
ERC+463
ERP+2:973:29
UNS+D
HYN+3
UNS+S
UNT+13+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();

			processor.ProcessMessage(rcvMessage2);

			CombineAssertions("TestOfUpdatingCloseReported for Failure", () =>
			{
				AssertEquals(MessageSubTypeCodes.Codes.Original, rcvMessage2.EM_MessageSubType);
				AssertEquals(true, testHouse1.BW_IsCloseReported);
				AssertEquals(false, testHouse2.BW_IsCloseReported);
				AssertEquals(false, testHouse3.BW_IsCloseReported);
			});

			testMaster.BP_MessageStatus = MessageStatusList.Codes.AwaitingDelete;
			testHouse3.BW_IsCloseReported = true;
			var rcvMessage3 = Factory.New<ACIForwarderMessage>();
			rcvMessage3.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			rcvMessage3.EM_MessageSubType = string.Empty;
			rcvMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage3.EM_Status = "QUE";
			rcvMessage3.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+312+80369165ABC12342+11
DTM+9:201408132033:203
RFF+AGO:CLS-C10001000
RCS+11
GEI+5+1
UNS+D
HYN+3
UNS+S
UNT+10+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();
			processor.ProcessMessage(rcvMessage3);

			CombineAssertions("TestOfUpdatingCloseReported for Cancellation", () =>
			{
				AssertEquals(MessageSubTypeCodes.Codes.Cancellation, rcvMessage3.EM_MessageSubType);
				AssertEquals(false, testHouse1.BW_IsCloseReported);
				AssertEquals(false, testHouse2.BW_IsCloseReported);
				AssertEquals(false, testHouse2.BW_IsCloseReported);
			});
		}

		public void TestMatchedResponseByCCN()
		{
			string messageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+70SX020418
DTM+9:201510270833:203
RFF+AGO:123029118RM0003
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1
";

			const string expectedBody = @"Reference Number : 70SX020418<br />
<br />
</strong>A MATCHED response message has been received from the CBSA for an ACI eManifest House Bill Message.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>27-Oct-15 08:33:00</td></tr></table>
<br />";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = new BusinessObjectFactory().New<JobDeclaration>();
				declaration.CargoControlNumbers.AddNew("70SX020417");
				declaration.CargoControlNumbers.AddNew("70SX020418");
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var sentMessage = entryHeader.Messages.AddNew();
				sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
				declaration.Factory.Save();

				AssertMessageResponse(
					messageText,
					EDIMessage.Status.Received,
					"",
					MessageTypeList.Codes.ACIHouseBill,
					MessageTypeList.Codes.ACIHouseBill,
					"Matched ACI eManifest House Bill Message Response for 70SX020418",
					expectedBody);
			}
		}

		public void TestIsReadyToClose()
		{
			var processor = new EManifestResponse(logger);
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Assert("ReadyToClose", master.ReadyToClose);
			master.HouseBills.AddNew();
			Assert("ReadyToClose", !master.ReadyToClose);
		}

		public void TestDoProcessingReturningStatus_IsNeedAutoCloseReport()
		{
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "C10001000";
			master.BP_PrimaryCCN = "C10001000CCN";
			master.BP_MessageStatus = "AWD";
			var outgoingMessage = Factory.New<ACIForwarderCloseMessage>();
			outgoingMessage.EM_LinkedObject = master;
			outgoingMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			outgoingMessage.NeedAutoCloseReport = true;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			var incomingMessage1 = Factory.New<ACIForwarderCloseMessage>();
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_Status = "QUE";
			incomingMessage1.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+312+8036081-14073001+11
DTM+9:201407300108:203
RFF+AGO:CLS-C10001000
RCS+11
GEI+5+1
UNS+D
HYN+3
UNS+S
UNT+10+1".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();
			processor.ProcessMessage(incomingMessage1);
			AssertContains("LastLog", "The auto close report messages have been generated for C10001000", logger.DebugLogStrings[0]);

			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH1000001";
			house.BW_HouseCCN = "8036 CAH1000001";
			var incomingMessage2 = Factory.New<ACIForwarderMessage>();
			incomingMessage2.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_Status = "QUE";
			incomingMessage2.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:201308072238:203
RFF+AGO:HBL-CAH1000001
STS++2:::0002
UNS+D
HYN+3
UNS+S
UNT+9+1
".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();
			processor.ProcessMessage(incomingMessage2);
			AssertContains("LastLog", "The auto close report messages have been generated for C10001000", logger.DebugLogStrings[1]);
		}

		public void TestMessageSyntaxError_IncludeSentMessageText()
		{
			var processor = new EManifestResponse(logger);
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "C10001000";
			master.BP_PrimaryCCN = "C10001000CCN";
			master.BP_MessageStatus = "AWD";

			var requestEHubID = ZGuid.NewZGuid();
			var requestInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			requestInterchange.EI_HeaderText = "RequestHEADER";
			requestInterchange.EI_BodyText = "BODY";
			requestInterchange.EI_FooterText = "FOOTER";
			requestInterchange.EI_InterchangeType = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.XMS;
			requestInterchange.EI_Status = "FAL";
			requestInterchange.EI_SessionGUID = requestEHubID;

			var responseEHubID = ZGuid.NewZGuid();
			var responseInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			responseInterchange.EI_HeaderText = "ResponseHEADER";
			responseInterchange.EI_BodyText = "BODY";
			responseInterchange.EI_FooterText = "FOOTER";
			responseInterchange.EI_InterchangeType = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.XMS;
			responseInterchange.EI_Status = "FAL";
			responseInterchange.EI_SessionGUID = responseEHubID;

			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "SYULSI026500";
			var outgoingMessage = Factory.New<ACIForwarderCloseMessage>();
			outgoingMessage.EM_LinkedObject = master;
			outgoingMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			outgoingMessage.NeedAutoCloseReport = true;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageType = "AHB";
			outgoingMessage.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("\r\n", "'");
			outgoingMessage.EM_EI = requestInterchange.PK;
			house.Messages.Add(outgoingMessage);
			Factory.Save();
			outgoingMessage = Factory.Load<ACIForwarderCloseMessage>(outgoingMessage.PK);
			var incomingMessage = Factory.New<ACIForwarderCloseMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = $@"UNH+1+GOVCBR:D:11B:UN'
BGM+961+8476SYULSI026500+11'
DTM+9:201901031712:203'
RFF+AGO:HBL-SYULSI026500'
RCS+11'
FTX+AAO+++UMRN({outgoingMessage.EM_MessageNum})SEGMENTSELLINE26ELEM9308(1.0)ELEM TOO LONG'
GEI+5+14'
UNS+D'
HYN+3'
UNS+S'
UNT+11+1'".Replace("'\r\n", "'");
			incomingMessage.EM_EI = responseInterchange.PK;
			Factory.Save();

			processor.ProcessMessage(incomingMessage);

			AssertEquals($@"Can response message's linked object be found? 'Y';
Can last request message be found? 'Y';
Did last request message sent with message errors? 'N';
Request message eHub Tracking ID: '{requestEHubID.ToString()}';
Response message eHub Tracking ID: '{responseEHubID.ToString()}';
<Request>{outgoingMessage.EM_MessageText}</Request><Response>{incomingMessage.EM_MessageText}</Response>;
<RequestInterChange>RequestHEADER</RequestInterChange><ResponseInterChange>ResponseHEADER</ResponseInterChange>", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAutoCloseSetting()
		{
			CACustomsDataRegistry.Instance.AutoSendCloseMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "C10001000";
			master.BP_PrimaryCCN = "C10001000CCN";
			master.BP_MessageStatus = "AWD";
			var outgoingMessage = Factory.New<ACIForwarderCloseMessage>();
			outgoingMessage.EM_LinkedObject = master;
			outgoingMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			outgoingMessage.NeedAutoCloseReport = true;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH1000001";
			house.BW_HouseCCN = "8036 CAH1000001";
			var incomingMessage1 = Factory.New<ACIForwarderMessage>();
			incomingMessage1.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_Status = "QUE";
			incomingMessage1.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:201308072238:203
RFF+AGO:HBL-CAH1000001
STS++2:::0002
UNS+D
HYN+3
UNS+S
UNT+9+1
".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();

			processor.ProcessMessage(incomingMessage1);

			AssertEquals("Close message should link to master", 2, master.Messages.Count);
			AssertEquals("Log created for close message", 1, logger.DebugLogStrings.Count);
			AssertContains("LastLog", "The auto close report messages have been generated for C10001000", logger.DebugLogStrings[0]);

			CACustomsDataRegistry.Instance.AutoSendCloseMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var house2 = master.HouseBills.AddNew();
			house2.BW_MessageReference = "CAH1000002";
			house2.BW_HouseCCN = "8036 CAH1000002";
			var incomingMessage2 = Factory.New<ACIForwarderMessage>();
			incomingMessage2.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_Status = "QUE";
			incomingMessage2.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:201308072238:203
RFF+AGO:HBL-CAH1000002
STS++2:::0002
UNS+D
HYN+3
UNS+S
UNT+9+1
".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();

			processor.ProcessMessage(incomingMessage2);

			AssertEquals("Close message should not be generated.", 2, master.Messages.Count);
		}

		public void TestGenerateAutoCloseReport()
		{
			var processor = new EManifestResponse(logger);
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "CAM01";
			var house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			var (isError, logText) = processor.GenerateAutoCloseReport(master);
			AssertEquals("Count", 1, master.Messages.Count);
			AssertContains("MessageText", "CLS-CAM01", master.Messages[0].EM_MessageText);
			AssertEquals("EM_SendWithMessageErrors", true, master.Messages[0].EM_SendWithMessageErrors);
			AssertContains("LastLog", "The auto close report messages have been generated for CAM01", logText);
		}

		public void TestSendEmailToHouseSenderOnMasterResponse()
		{
			var sentMasterMessage = Factory.New<ACIForwarderMessage>();
			sentMasterMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMasterMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMasterMessage.EM_SystemCreateUser = "BP~";
			master.Messages.Add(sentMasterMessage);

			Factory.Save();

			const string expectedBody = @"Reference Number : 8036X555<br />
<br />
</strong>A MATCHED response message has been received from the CBSA for an ACI eManifest Forwarder Close Message.<br />";

			house.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				MasterMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.ACIForwarderClose,
				MessageSubTypeCodes.Codes.Original,
				"Matched ACI eManifest Forwarder Close Message Response for 8036X555",
				expectedBody);
		}

		void AssertMessageResponse(string messageText, string expectedMessageStatus, string expectedHouseMessageStatus, string expectedType, string expectedSubType,
					string expectedMailSubject, string expectedEmailBody, string recipient = "UserToNotify@blah.com", string ccRecipient = "blah@blah.com")
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAACI;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_MessageText = messageText.Replace("'\r\n", "'").Replace("\r\n", "'");
			Env.OutgoingMailManager.EmailsCreated.Clear();
			processor.ProcessMessage(message);

			AssertEquals("Message Type", expectedType, message.EM_MessageType);
			AssertEquals("Status on EDIMessage", expectedMessageStatus, message.EM_Status);
			AssertEquals("Status on House", expectedHouseMessageStatus, house.BW_MessageStatus);
			AssertEquals("Message SubType set", expectedSubType, message.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedEmailBody, message.EM_MessageInterpretation);

			if (string.IsNullOrEmpty(expectedMailSubject))
			{
				AssertEquals("No Email Sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			else if (expectedType == MessageTypeList.Codes.SyntaxError)
			{
				var expectedSyntaxErrorText = Factory.Load<SyntaxErrorMessage>(message.PK).SourceMessageTextWithErrorMarks;
				AssertSyntaxErrorEmail(expectedMailSubject, expectedEmailBody, message.EM_MessageText, expectedSyntaxErrorText, recipient, ccRecipient);
			}
			else
			{
				AssertEmail(expectedMailSubject, expectedEmailBody, message.EM_MessageText, recipient, ccRecipient);
			}
		}

		#region Message Text

		internal const string DataErrorMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+8036X555+11
DTM+9:201308072136:203
RFF+AGO:HBL-CAH0000004
RCS+11
FTX+AAO+++29
GEI+5+14
ERC+H11
ERP+2:420:29
UNS+D
HYN+3
UNS+S
UNT+13+1
";

		internal const string ValidationErrorMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+8036X555+11
DTM+9:201308072136:203
RFF+AGO:HBL-CAH0000004
RCS+11
FTX+AAO+++22
GEI+5+14
ERC+W64
ERP+2:387:22
UNS+D
HYN+3
UNS+S
UNT+13+1
";

		internal const string ContentAcceptedMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+312+8036X555+11
DTM+9:201308072239:203
RFF+AGO:HBL-CAH0000004
RCS+11
GEI+5+1
UNS+D
HYN+3
UNS+S
UNT+10+1
";

		internal const string NotMatchedMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:201308072238:203
RFF+AGO:HBL-CAH0000004
STS++2:::0002
UNS+D
HYN+3
UNS+S
UNT+9+1
";

		internal const string MatchedMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:201308072238:203
RFF+AGO:HBL-CAH0000004
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1
";

		internal const string MasterMessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+23+8036X555
DTM+9:202011091515:203
RFF+AGO:CLS-CAH0000004
STS++2:::0001
UNS+D
HYN+3
UNS+S
UNT+9+1

";

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			//processor = new EManifestResponse(Logger);
			processor = new ACIMessageProcessor(logger);
			master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "CAH0000004";
			house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";

			var sentMessage = Factory.New<ACIHouseBillMessage>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			house.Messages.Add(sentMessage);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "W64", "Primary CCN: Carrier cannot be a Freight Forwarder", startDate, endDate);

			//shipment = Factory.New<ForwardingShipment>();
			//house.CA_JS = shipment.PK;
			Factory.Save();
		}

		CusCAeMHMaster master;
		CusCAeMHHouse house;
		//EManifestResponse processor;
		ACIMessageProcessor processor;

		//ForwardingShipment shipment;

		#endregion
	}
}
