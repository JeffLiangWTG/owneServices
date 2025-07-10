using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class SupplementaryCargoReportResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestContentAcceptedResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>A Content Accepted response message has been received from the CBSA for a Supplementary Cargo Report.<br />
The message has been validated and is error free.<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Original,
				"Validated Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestCancellationAcceptedResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>A Content Accepted response message has been received from the CBSA for a Supplementary Cargo Report.<br />
The message has been validated and is error free.<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingDelete;
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearDelete,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Cancellation,
				"Cancellation accepted Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestMatchedResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>A MATCHED response message has been received from the CBSA for a Supplementary Cargo Report.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>13-Feb-09 12:41:00</td></tr><tr><td>Related Cargo Control Number</td><td>9990123456TEST3</td></tr></table>
<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			AssertMessageResponse(
				MatchedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearChange,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Change,
				"Matched Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestNotMatchedResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>A NOT MATCHED response message has been received from the CBSA for a Supplementary Cargo Report.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following codes returned.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>13-Feb-09 12:41:00</td></tr><tr><td>Related Cargo Control Number</td><td>9990123456TEST3</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>No-Match Notices</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>No-Match Reason</th><th>Comments</th></tr></thead><tr align=""left""><td>02</td><td>Supplementary de-linked by a CCN decision status.</td><td>NO COMMENTS</td></tr></table>
<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				NotMatchedMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Original,
				"NOT Matched Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestRiskAssessmentNotice()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
<br />
</strong>A RISK ASSESSMENT response message has been received from the CBSA for this Supplementary Cargo Report.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following RA Comments.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>13-Feb-09 12:41:00</td></tr><tr><td>Risk Assessment Type</td><td>Do Not Load</td></tr><tr><td>Container Numbers</td><td>OCLU3213211, OCLU3213212, OCLU3213213, OCLU3213214, OCLU3213215, OCLU3213216,<br>OCLU3213217, OCLU3213218, OCLU3213219, OCLU3213210</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Risk Assessment Notices</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Notice Details</th><th>Officer Remarks</th></tr></thead><tr align=""left""><td>605</td><td>Consignee Name Inadequate:<br>The name of the Consignee is insufficient or not<br>adequate to allow CBSA to properly perform its risk<br>assessment function. If no supplementary data is<br>required, it means that the ultimate Consignee name is<br>required. When supplementary data was transmitted, the<br>information submitted does not allow CBSA to identify<br>the entity receiving the goods. The CBSA officer's<br>remarks will guide you toward what you need to transmit<br>to meet CBSA requirements.</td><td>ULTIMATE CONSIGNEE INFO</td></tr></table>
<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				RiskAssessmentNoticeMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ClearOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Original,
				"Risk Assessment Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestErrorResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a Supplementary Cargo Report.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>16-Feb-09 04:34:00</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error</th><th>Error Data Value</th></tr></thead><tr align=""left""><td>313</td><td>Consignee Postal Code: FIELD IS NOT VALID</td><td>M5P1A2XX</td></tr></table>
<br />";

			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				ValidationErrorMessageText,
				EDIMessage.Status.Received,
				MessageStatusList.Codes.ErrorOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Original,
				"Error Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
		}

		public void TestSyntaxErrorResponse()
		{
			const string expectedBody = @"Reference Number : 8010 S00001244D<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a Supplementary Cargo Report.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Date</td><td>20-Jul-11 02:02:00</td></tr></table>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>INVALID CODE</td><td>22</td><td>GEI+6+:::22</td><td>GEI / L: 8, P: 2,4</td></tr><tr align=""left""><td>SEG USE EXCEEDED</td><td>&nbsp;</td><td>UNH+55+GSMCAR:D:00A:UN:SUPRPT</td><td>CST / L: 0, I: 0</td></tr></table>
<br />";
			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			var outMessage = SyntaxErrorMessageTest.GetSentEDIMessage<SUPRPTMessage>(factory, SyntaxErrorMessageTest.SupplementaryCargoReportMessageText, ZDateTime.Now.AddDays(-1), "55");
			outMessage.EM_SendWithMessageErrors = true;
			house.Messages.Add(outMessage);

			AssertMessageResponse(
				SyntaxErrorMessageTest.SupplementaryCargoReportSyntaxErrorMessageText,
				EDIMessage.Status.Received,
				string.Empty,
				MessageTypeList.Codes.SyntaxError,
				MessageTypeList.Codes.SupplementaryCargoReport,
				"Error Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			outMessage.EM_SendWithMessageErrors = false;
			AssertMessageResponse(
				SyntaxErrorMessageTest.SupplementaryCargoReportSyntaxErrorMessageText,
				EDIMessage.Status.Received,
				string.Empty,
				MessageTypeList.Codes.SyntaxError,
				MessageTypeList.Codes.SupplementaryCargoReport,
				"Error Supplementary Cargo Report Response for 8010 S00001244D",
				expectedBody);
			AssertEquals("ACI message syntax error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidMessage()
		{
			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertMessageResponse(
				"UNH+1+CUSRES:D:00A:UN'BGM+:::687+8010S00001244D+11'BLAH BLAH",
				EDIMessage.Status.Failed,
				MessageStatusList.Codes.AwaitingOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageSubTypeCodes.Codes.Original,
				"Supplementary Cargo Response Message Processor Error Report",
				"The message processor was unable to interpret received message");
		}

		public void TestInvalidLinkedObject()
		{
			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			house.SupplementaryReferenceNumber = "BLAH";
			factory.Save();
			AssertMessageResponse(
				ContentAcceptedMessageText,
				EDIMessage.Status.Failed,
				MessageStatusList.Codes.AwaitingOriginal,
				MessageTypeList.Codes.SupplementaryCargoReport,
				MessageTypeList.Codes.SupplementaryCargoReport,
				"Supplementary Cargo Response Message Processor Error Report",
				"Could not find an associated business object (Job) for document reference",
				string.Empty);
		}

		void AssertMessageResponse(string messageText, string expectedMessageStatus, string expectedHouseMessageStatus, string expectedType, string expectedSubType,
			string expectedMailSubject, string expectedEmailBody, string recipient = "UserToNotify@blah.com", string ccRecipient = "blah@blah.com")
		{
			var message = factory.New<ACIEDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAACI;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_MessageText = messageText.Replace("'\r\n", "'").Replace("\r\n", "'");
			Env.OutgoingMailManager.EmailsCreated.Clear();
			processor.ProcessMessage(message);

			AssertEquals("Message Type", expectedType, message.EM_MessageType);
			AssertEquals("Status on EDIMessage", expectedMessageStatus, message.EM_Status);
			AssertEquals("Status on House", expectedHouseMessageStatus, house.CA_MessageStatus);
			AssertEquals("Message SubType set", expectedSubType, message.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedEmailBody, message.EM_MessageInterpretation);

			if (string.IsNullOrEmpty(expectedMailSubject))
			{
				AssertEquals("No Email Sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			else if (expectedType == MessageTypeList.Codes.SyntaxError)
			{
				var expectedSyntaxErrorText = factory.Load<SyntaxErrorMessage>(message.PK).SourceMessageTextWithErrorMarks;
				AssertSyntaxErrorEmail(expectedMailSubject, expectedEmailBody, message.EM_MessageText, expectedSyntaxErrorText, recipient, ccRecipient);
			}
			else
			{
				AssertEmail(expectedMailSubject, expectedEmailBody, message.EM_MessageText, recipient, ccRecipient);
			}
		}

		#region Message Text

		internal const string ContentAcceptedMessageText = @"UNH+1+CUSRES:D:00A:UN'
BGM+:::687+8010S00001244D+11'
DTM+9:200902131330:203'
GIS+1'
UNT+5+1'
";
		internal const string MatchedMessageText = @"UNH+1+CUSRES:D:00A:UN'
BGM+:::687+8010S00001244D+64'
DTM+9:200902131241:203'
GIS+32'
RFF+MB:9990123456TEST3'
UNT+6+1'
";
		internal const string NotMatchedMessageText = @"UNH+1+CUSRES:D:00A:UN'
BGM+:::687+8010S00001244D+64'
DTM+9:200902131241:203'
GIS+33'
RFF+MB:9990123456TEST3'
ERP+2:233'
ERC+02'
FTX+AAO+++NO COMMENTS'
UNT+8+1'
";
		internal const string RiskAssessmentNoticeMessageText = @"UNH+1+CUSRES:D:00A:UN'
BGM+:::687+8010S00001244D+11'
DTM+9:200902131241:203'
GIS+25'
ERP+2::5'
ERC+605'
FTX+AAO+++ULTIMATE CONSIGNEE INFO'
DOC+235'
EQD+CN+OCLU3213211'
EQD+CN+OCLU3213212'
EQD+CN+OCLU3213213'
EQD+CN+OCLU3213214'
EQD+CN+OCLU3213215'
EQD+CN+OCLU3213216'
EQD+CN+OCLU3213217'
EQD+CN+OCLU3213218'
EQD+CN+OCLU3213219'
EQD+CN+OCLU3213210'
UNT+10+1'
";
		internal const string ValidationErrorMessageText = @"UNH+2+CUSRES:D:00A:UN'
BGM+:::687+8010S00001244D+11'
DTM+9:200902160434:203'
GIS+14'
ERP+2:237:22'
ERC+313'
FTX+AAO+++M5P1A2XX'
UNT+8+2'
";

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var refDatahelper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "313", "Consignee Postal Code: FIELD IS NOT VALID", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "UL02", "Supplementary de-linked by a CCN decision status.", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "RA605", "Consignee Name Inadequate:\r\nThe name of the Consignee is insufficient or not\r\nadequate to allow CBSA to properly perform its risk\r\nassessment function. If no supplementary data is\r\nrequired, it means that the ultimate Consignee name is\r\nrequired. When supplementary data was transmitted, the\r\ninformation submitted does not allow CBSA to identify\r\nthe entity receiving the goods. The CBSA officer's\r\nremarks will guide you toward what you need to transmit\r\nto meet CBSA requirements.", startDate, endDate);
			Factory.Save();

			helper = new CusSCATestHelper();
			factory = helper.HelperFactory;
			processor = new ACIMessageProcessor(logger);
			house = helper.House;
			house.SupplementaryReferenceNumber = "8010S00001244D";

			var sentMessage = factory.New<SUPRPTMessage>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			house.Messages.Add(sentMessage);

			shipment = factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;

			factory.Save();
		}

		CusSCATestHelper helper;
		BusinessObjectFactory factory;
		CusSCAHouse house;
		ACIMessageProcessor processor;
		ForwardingShipment shipment;

		#endregion
	}
}
