using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class B3ResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestErrorMessageText1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "1", "Client Supplied Request ID Submission cannot currently be amended", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributeTypes.Codes.MessageNum, "V26");
			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:67897'ERC+V26'DOC+961'CST++1+1+0'UNT+9+1'";
			const string expectedErrorMessageText = "Client Supplied Request ID Submission cannot currently be amended";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedErrorMessageText, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000067897", expectedErrorMessageText, messageText);
		}

		public void TestMessageLinkByBatchNumber()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++835+9'DTM+137:20131010:102'ERP+:X11'RFF+ABO:835'ERC+942282'ERP+:X11'RFF+ABO:835'ERC+943643'DOC+961'CST++0+0+0'UNT+12+1'";

			#region Expected Email Body

			const string expectedBody = @"Reference Number : 000067897<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942282</td><td>INVALID PASSWORD</td><td>X11 - Batch Control</td><td>Batch Number: &#39;835&#39;</td></tr><tr><td>943643</td><td>BATCH REJECTED</td><td>X11 - Batch Control</td><td>Batch Number: &#39;835&#39;</td></tr></table>";

			#endregion

			var ediMessage = GetB3Message(messageText);
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-10);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000067897", expectedBody, messageText);
		}

		public void TestMessageWithNoLinkedObject()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:216'ERC+943152'ERP+:I99'RFF+ABO:216'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";
			const string errorMessage = "Could not find an associated business object (Job) for document reference = '000000216'";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "B3/B3X Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestI11AndI99_CNF_CLO()
		{
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++758+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:67897'ERC+942861'ERP+:I99'RFF+ABO:12663'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";
			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", B3EntryStatusList.Codes.Confirmed, ediMessage.EM_MessageSubType);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Confirmed, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
		}

		public void TestOnlyI99_ERR_ERO()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:67897'ERC+943152'ERP+:I99'RFF+ABO:67897'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", B3EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
		}

		public void TestAcceptedMessageWithoutAccountSecurityNum()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:67897'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";

			#region Expected Email Body

			const string expectedBody = @"</strong>A 'Message Accepted' response has been received from the CBSA for a B3 CUSDEC.<br />
Please see message details below.<br />
<br />
<br />
<strong>Processing Date:  </strong>12-Feb-10<br />
<br />
<hr />";

			#endregion

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Accepted, entryHeader.CH_EntryStatus);
			AssertEquals("B3 Accepted Date", ediMessage.RNSProcessingDate, entryHeader.CH_EntryReleaseDate);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Accepted B3 CUSDEC Response for 000067897", expectedBody, messageText);
		}

		public void TestMessageContentRejectedWhereAccountSecurityNumGottenFromMessage()
		{
			var impMessageProcessor = new IMPMessageProcessor(logger);
			var declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "00345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_IsCancelled = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			AddSentMessage(entryHeader);
			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'RFF+ABP:345'ERP+:I11'RFF+ABO:67892'ERC+943152'ERP+:I99'RFF+ABO:67892'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";

			#region Expected Email Body

			//An ERROR response message has been received from the CBSA for a B3 CUSDEC.
			//The message sent for the above mentioned job had the following errors.

			//Error Code	Error Message			Entry Component			Reference
			//943152		INVALID RELEASE STATUS	I11 - B3 Header			Transaction Number: '67892'
			//942991		ENTRY REJECTED			I99 - Exception	Error	Transaction Number: '67892'

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>943152</td><td>INVALID RELEASE STATUS</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;67892&#39;</td></tr><tr><td>942991</td><td>ENTRY REJECTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;67892&#39;</td></tr></table>";

			#endregion

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 00345000067892", expectedBody, messageText);
		}

		public void TestInvalidB3Message()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'RFF+ABO:67895'DOC+961'CST++1+0+1'UNT+12+1'";
			const string errorMessage = "The message processor was unable to interpret received message.";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEquals("Linked to entry", ZGuid.Empty, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status does not change", string.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Message Status does not change", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);

			AssertContains("LastLog", "B3/B3X Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
			AssertEmail("B3/B3X Response Message Processor Error Report", errorMessage, messageText, string.Empty);
		}

		public void TestMessageStatusIsNotAwaitingReply()
		{
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			Factory.Save();
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I66'RFF+ABO:1'ERC+945776'ERP+:I99'RFF+ABO:67897'ERC+942991'DOC+961'CST++1+1+0'UNT+9+1'";
			const string errorMessage = "An ERROR response message has been received from the CBSA for a B3 CUSDEC";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status not changed", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000067897", errorMessage, messageText);
		}

		public void TestAcceptedMessage_CancelAllSystemB3LateSendingWarningEvent()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:67897'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";

			var log1 = declaration.Logs.AddNew(Events.CanadianCADLateSendingWarning);
			log1.SL_GS_NKUser = User.ServiceUserCode;
			var log2 = declaration.Logs.AddNew(Events.CanadianCADLateSendingWarning);
			log2.SL_GS_NKUser = "E";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, ediMessage.EM_MessageSubType);

			Assert("System CanadianB3LateSendingWarning event log should be cancelled", log1.IsCancelled);
			Assert("Customer CanadianB3LateSendingWarning event log should not be cancelled", !log2.IsCancelled);
		}

		public void TestAcceptedMessage_CancelScheduledB3Messages()
		{
			var testMessage1 = entryHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;

			var testMessage2 = entryHeader.Messages.AddNew();
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			Factory.Save();

			CombineAssertions("Rejection Message Not Doint Anything", () =>
			{
				const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'RFF+ABP:12345'ERP+:I11'RFF+ABO:67897'ERC+943152'ERP+:I99'RFF+ABO:67897'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'";

				var ediMessage = GetB3Message(messageText);
				impMessageProcessor.ProcessMessage(ediMessage);

				AssertEquals("Errored", EDIMessage.Status.Received, ediMessage.EM_Status);
				AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);

				Assert(testMessage1.EM_IsActive);
				Assert(testMessage2.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals(EDIMessage.Status.Queued, testMessage2.EM_Status);
			});

			CombineAssertions("Accept Message will clear out the scheduled message", () =>
			{
				const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:67897'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";

				var ediMessage = GetB3Message(messageText);
				impMessageProcessor.ProcessMessage(ediMessage);

				AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
				AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, ediMessage.EM_MessageSubType);

				Assert(testMessage1.EM_IsActive);
				Assert(!testMessage2.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals(EDIMessage.Status.Cancelled, testMessage2.EM_Status);
			});
		}

		public void TestAcceptedB3MessageForMultipleDeclarations()
		{
			var declaration1 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "12345";
			declaration1.TransactionNumber.SequentialNumber = "00050314";
			declaration1.JE_IsCancelled = false;
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader1);

			var declaration2 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "12345";
			declaration2.TransactionNumber.SequentialNumber = "00050315";
			declaration2.JE_IsCancelled = false;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader2);

			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++544+9'DTM+137:20160304:102'ERP+:I99'RFF+ABO:503156'ERC+942992'ERP+:I99'RFF+ABO:503145'ERC+942992'DOC+961'CST++2+2+0'UNT+12+1'";

			#region Expected Email Body

			const string expectedBody = @"</strong>A 'Message Accepted' response has been received from the CBSA for a B3 CUSDEC.<br />
Please see message details below.<br />
<br />
<br />
<strong>Processing Date:  </strong>04-Mar-16<br />
<br />
<hr />";

			#endregion

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var message1 = entryHeader1.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, message1.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader1.PK, message1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, message1.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Accepted, entryHeader1.CH_EntryStatus);
			AssertEquals("B3 Accepted Date", message1.RNSProcessingDate, entryHeader1.CH_EntryReleaseDate);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader1.CH_Status);
			AssertEmail("Accepted B3 CUSDEC Response for 000503145", expectedBody, messageText);

			var message2 = entryHeader2.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, message2.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader2.PK, message2.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, message2.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Accepted, entryHeader2.CH_EntryStatus);
			AssertEquals("B3 Accepted Date", message2.RNSProcessingDate, entryHeader2.CH_EntryReleaseDate);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader2.CH_Status);
			AssertEmail("Accepted B3 CUSDEC Response for 000503156", expectedBody, messageText);
		}

		public void TestRejectedB3MessageForMultipleDeclarations()
		{
			var declaration1 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "12345";
			declaration1.TransactionNumber.SequentialNumber = "00050303";
			declaration1.JE_IsCancelled = false;
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader1);

			var declaration2 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "12345";
			declaration2.TransactionNumber.SequentialNumber = "00050302";
			declaration2.JE_IsCancelled = false;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader2);

			var declaration3 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.TransactionNumber.AccountSecurityCode = "12345";
			declaration3.TransactionNumber.SequentialNumber = "00050301";
			declaration3.JE_IsCancelled = false;
			var entryHeader3 = declaration3.ActiveEntryHeaders.AddNew();
			entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader3.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader3);

			Factory.Save();

			const string messageText = @"UNH+2+CUSRES:S:99B:UN'BGM++543+9'DTM+137:20160301:102'ERP+:I11'RFF+ABO:503032'ERC+942855'ERP+:I99'RFF+ABO:503032'ERC+942991'ERP+:I11'RFF+ABO:503021'ERC+942855'ERP+:I99'RFF+ABO:503021'ERC+942991'ERP+:I11'RFF+ABO:503010'ERC+942861'ERP+:I99'RFF+ABO:503010'ERC+942992'DOC+961'CST++3+0+3'UNT+24+2'";

			#region Expected Email Body

			const string expectedBody1 = @"Reference Number : 000503032<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942855</td><td>IMPRT NO NOT EQUAL TO IMPRTR NO OF RELEASE RECORD</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;503032&#39;</td></tr><tr><td>942991</td><td>ENTRY REJECTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;503032&#39;</td></tr></table>";

			const string expectedBody2 = @"Reference Number : 000503021<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942855</td><td>IMPRT NO NOT EQUAL TO IMPRTR NO OF RELEASE RECORD</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;503021&#39;</td></tr><tr><td>942991</td><td>ENTRY REJECTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;503021&#39;</td></tr></table>";

			const string expectedBody3 = @"Reference Number : 000503010<br />
<br />
</strong>An Entry HAS BEEN CONFIRMED response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942861</td><td>ENTRY HAS BEEN CONFIRMED</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;503010&#39;</td></tr><tr><td>942992</td><td>ENTRY ACCEPTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;503010&#39;</td></tr></table>";

			#endregion

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var message1 = entryHeader1.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, message1.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader1.PK, message1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody1, message1.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader1.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader1.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000503032", expectedBody1, messageText);

			var message2 = entryHeader2.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, message2.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader2.PK, message2.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody2, message2.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader2.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader2.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000503021", expectedBody2, messageText);

			var message3 = entryHeader3.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message3.EM_Status);
			AssertEquals("Message Sub Type", B3EntryStatusList.Codes.Confirmed, message3.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader3.PK, message3.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody3, message3.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Confirmed, entryHeader3.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedOriginal, entryHeader3.CH_Status);
			AssertEmail("Confirmed B3 CUSDEC Response for 000503010", expectedBody3, messageText);
		}

		public void TestMixedB3MessageForMultipleDeclarations()
		{
			var declaration1 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "12345";
			declaration1.TransactionNumber.SequentialNumber = "00050303";
			declaration1.JE_IsCancelled = false;
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader1);

			var declaration2 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "12345";
			declaration2.TransactionNumber.SequentialNumber = "00050302";
			declaration2.JE_IsCancelled = false;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader2);

			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++548+9'DTM+137:20160306:102'ERP+:I99'RFF+ABO:503032'ERC+942992'ERP+:I11'RFF+ABO:503021'ERC+943152'DOC+961'CST++2+1+1'UNT+15+1'";

			#region Expected Email Body

			const string expectedBody1 = @"</strong>A 'Message Accepted' response has been received from the CBSA for a B3 CUSDEC.<br />
Please see message details below.<br />
<br />
<br />
<strong>Processing Date:  </strong>06-Mar-16<br />
<br />
<hr />";

			const string expectedBody2 = @"Reference Number : 000503021<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>943152</td><td>INVALID RELEASE STATUS</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;503021&#39;</td></tr></table>";

			#endregion

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var message1 = entryHeader1.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, message1.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader1.PK, message1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody1, message1.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Accepted, entryHeader1.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader1.CH_Status);
			AssertEmail("Accepted B3 CUSDEC Response for 000503032", expectedBody1, messageText);

			var message2 = entryHeader2.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, message2.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader2.PK, message2.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody2, message2.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader2.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader2.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000503021", expectedBody2, messageText);
		}

		public void TestMultipleRejectedB3MessageForMultipleDeclaration()
		{
			var declaration1 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "12345";
			declaration1.TransactionNumber.SequentialNumber = "00050302";
			declaration1.JE_IsCancelled = false;
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader1);

			var declaration2 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "12345";
			declaration2.TransactionNumber.SequentialNumber = "00050303";
			declaration2.JE_IsCancelled = false;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader2);

			Factory.Save();

			#region Expected Email Body

			const string expectedBody1 = @"Reference Number : 000503021<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942930</td><td>EXCISE TAX AMT INPUT NOT = TO CALCULATED EXCISE TAX AMT</td><td>I51 - Classification Line 3</td><td>Line Number: &#39;2&#39;</td></tr><tr><td>945578</td><td>VALUE FOR TAX INPUT NOT = CALCULATED VALUE FOR TAX</td><td>I51 - Classification Line 3</td><td>Line Number: &#39;2&#39;</td></tr><tr><td>945580</td><td>AMOUNT INPUT NOT = CALCULATED GST AMOUNT</td><td>I51 - Classification Line 3</td><td>Line Number: &#39;2&#39;</td></tr><tr><td>942991</td><td>ENTRY REJECTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;503021&#39;</td></tr></table>";

			const string expectedBody2 = @"Reference Number : 000503032<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942925</td><td>DUTY RATE INPUT NOT EQUAL TO DUTY RATE ON FILE</td><td>I41 - Classification Line 2</td><td>Line Number: &#39;2&#39;</td></tr><tr><td>942927</td><td>DUTY AMOUNT INPUT NOT EQUAL TO CALCULATED DUTYAMOUNT</td><td>I41 - Classification Line 2</td><td>Line Number: &#39;2&#39;</td></tr><tr><td>942991</td><td>ENTRY REJECTED</td><td>I99 - Exception Error</td><td>Transaction Number: &#39;503032&#39;</td></tr></table>";

			#endregion

			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++581+9'DTM+137:20160416:102'ERP+:I41'RFF+ABO:2'ERC+942925'ERP+:I41'RFF+ABO:2'ERC+942927'ERP+:I99'RFF+ABO:503032'ERC+942991'ERP+:I51'RFF+ABO:2'ERC+942930'ERP+:I51'RFF+ABO:2'ERC+945578'ERP+:I51'RFF+ABO:2'ERC+945580'ERP+:I99'RFF+ABO:503021'ERC+942991'DOC+961'CST++1+0+1'UNT+24+1'";

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var message1 = entryHeader1.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, message1.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader1.PK, message1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody1, message1.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader1.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader1.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000503021", expectedBody1, messageText);

			var message2 = entryHeader2.Messages[1];

			AssertEquals("Received", EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, message2.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader2.PK, message2.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody2, message2.EM_MessageInterpretation);
			AssertEquals("Entry Status", B3EntryStatusList.Codes.Error, entryHeader2.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader2.CH_Status);
			AssertEmail("Error B3 CUSDEC Response for 000503032", expectedBody2, messageText);
		}

		public void TestSaveB3AndCCIReportsToEDocs()
		{
			var declaration = GetJobDeclaration();

			using (ObjectFactory.Substitute(MockCDGServiceTasks()))
			{
				const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++484+9'DTM+137:20160428:102'RFF+ABO:12345'ERP+:I99'RFF+ABO:503032'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
				var receiveMessage = GetB3Message(messageText);
				receiveMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
				impMessageProcessor.ProcessMessage(receiveMessage);
				Factory.Save();
				var cdgProcessor = new Customs.Business.BatchProcessor.CustomsDocumentGeneratorProcessor(new Enterprise.BatchProcessor.LoggingInformation());
				cdgProcessor.ExecuteBatch();
				var storageMain = declaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, Core.Constants.DocManagerCodes.JobDeclaration);
				AssertEquals("count of the files", 3, storageMain.Files.Count);

				var b3ImportDocumentType = GetActualDocumentType("B3ImportDocument", ".B3ImportEntry");
				var invoiceDocumentType = GetActualDocumentType("CACustomsInvoice", "CommercialInvoice");
				var file1 = storageMain.Files[0];
				AssertEquals("FileName", "B3ImportDocument.pdf", file1.FileName);
				AssertEquals("DocType should be " + b3ImportDocumentType, b3ImportDocumentType, file1.DocType);
				var file2 = storageMain.Files[1];
				AssertEquals("FileName", "CACustomsInvoice.pdf", file2.FileName);
				AssertEquals("DocType should be " + invoiceDocumentType, invoiceDocumentType, file2.DocType);
				var file3 = storageMain.Files[2];
				AssertEquals("FileName", "CACustomsInvoice[2].pdf", file3.FileName);
				AssertEquals("DocType should be " + invoiceDocumentType, invoiceDocumentType, file3.DocType);
			}
		}

		public void TestSaveB3AndCCIReportsToEDocs_CdgNotActive()
		{
			var declaration = GetJobDeclaration();

			using (ObjectFactory.Substitute(MockCDGServiceTasks(false)))
			{
				const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++484+9'DTM+137:20160428:102'RFF+ABO:12345'ERP+:I99'RFF+ABO:503032'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
				var receiveMessage = GetB3Message(messageText);
				receiveMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
				impMessageProcessor.ProcessMessage(receiveMessage);
				Factory.Save();
				var cdgProcessor = new Customs.Business.BatchProcessor.CustomsDocumentGeneratorProcessor(new Enterprise.BatchProcessor.LoggingInformation());
				cdgProcessor.ExecuteBatch();
				var storageMain = declaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, Core.Constants.DocManagerCodes.JobDeclaration);
				AssertEquals("count of the files", 0, storageMain.Files.Count);
			}
		}

		JobDeclaration GetJobDeclaration()
		{
			var declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00050303";
			declaration.JE_IsCancelled = false;
			declaration.Invoices.AddNew();
			declaration.Invoices.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_MessageText = string.Format("UNH+2287+CUSDEC:S:99B:UN'BGM+:::AB+454+9'CST++I'LOC+41+497'LOC+11+497'RFF+TN:000001831'RFF+ARA:105759013RM0001'TDT+11++2++9165'DOC+785+803609238364B'DTM+204:20160120:102'MOA+43:4000'UNS+D'DMS+1'NAD+SE++GHJ LTD. INT?'L  ?+?:??@'DOC+935'DTM+129:20160119:102'LOC+27+AU+AU'PAT+1+CONSIGN:::02'MOA+6::CAD'CST+1+POS+1+8544700090+23'MOA+40:400000'MOA+43:400000'MOA+125:400000'RFF+LI:1:0'MOA+38:400000'TAX+7+VAT++5.0'MOA+1:20000'GIR+1+1'MEA+AAR++MTR:2134'TAX+5+++0.00'MOA+155:000'UNS+S'TAX+7+:::K90'MOA+1:20000'TAX+4+:::K90'MOA+176:20000'UNT+37+2287'");
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			entryHeader.Messages.Add(sentMessage);
			var sentInterchange = Factory.New<CAEDIInterchange>();
			sentInterchange.EI_InterchangeNum = "1";
			sentMessage.EM_EI = sentInterchange.PK;
			Factory.Save();
			return declaration;
		}

		IServiceManagerQuerier MockCDGServiceTasks(bool active = true)
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			var serviceTaskStatus = active ? ServiceTaskStatus.AtLeastOneHostIsRunningHealthily : ServiceTaskStatus.ServiceTaskIsInactive;
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("CDG")).Returns(serviceTaskStatus);
			return mockQuerier.Object;
		}

		public void TestAcceptedB3MessageForDuplicatedTransactionNumbers()
		{
			var declaration1 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "10207";
			declaration1.TransactionNumber.SequentialNumber = "50000000";
			declaration1.JE_IsCancelled = false;
			var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader1);
			entryHeader1.Messages[0].EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+123+9'LOC+41+497'RFF+TN:500000001'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			Factory.Save();

			const string messageText1 = @"UNH+1+CUSRES:S:99B:UN'BGM++123+9'DTM+137:20160304:102'ERP+:I99'RFF+ABO:500000001'ERC+942992'DOC+961'CST++2+2+0'UNT+9+1'";
			var ediMessage1 = GetB3Message(messageText1);
			impMessageProcessor.ProcessMessage(ediMessage1);
			AssertEquals("Linked to entry", entryHeader1.PK, ediMessage1.EM_LinkUniqueID);

			var declaration2 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "27896";
			declaration2.TransactionNumber.SequentialNumber = "50000000";
			declaration2.JE_IsCancelled = false;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader2);
			entryHeader2.Messages[0].EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+456+9'LOC+41+497'RFF+TN:500000001'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			Factory.Save();

			const string messageText2 = @"UNH+1+CUSRES:S:99B:UN'BGM++456+9'DTM+137:20160304:102'ERP+:I99'RFF+ABO:500000001'ERC+942992'DOC+961'CST++2+2+0'UNT+9+1'";
			var ediMessage2 = GetB3Message(messageText2);
			impMessageProcessor.ProcessMessage(ediMessage2);
			AssertEquals("Linked to entry", entryHeader2.PK, ediMessage2.EM_LinkUniqueID);

			var declaration3 = (JobDeclaration)JobDeclaration.New(Factory);
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.TransactionNumber.AccountSecurityCode = "87296";
			declaration3.TransactionNumber.SequentialNumber = "50000000";
			declaration3.JE_IsCancelled = false;
			var entryHeader3 = declaration3.ActiveEntryHeaders.AddNew();
			entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader3.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader3);
			entryHeader3.Messages[0].EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+456+9'LOC+41+497'RFF+TN:500000001'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			Factory.Save();

			var ediMessage3 = GetB3Message(messageText2);
			ediMessage3.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			impMessageProcessor.ProcessMessage(ediMessage3);
			AssertEquals("Linked to entry", entryHeader3.PK, ediMessage3.EM_LinkUniqueID);

			entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader3.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Factory.Save();
			var ediMessage4 = GetB3Message(messageText2);
			impMessageProcessor.ProcessMessage(ediMessage4);
			Assert("not linked to entry", !ediMessage4.EM_LinkUniqueID.IsValid);
		}

		public void TestConfirmedAndErrorMessageDeclarations()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:500000001'ERC+942861'ERP+:I99'RFF+ABO:500000001'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1";

			var declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "87296";
			declaration.TransactionNumber.SequentialNumber = "50000000";
			declaration.JE_IsCancelled = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AddSentMessage(entryHeader);
			Factory.Save();

			var ediMessage = GetB3Message(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Message Sub Type", B3EntryStatusList.Codes.Confirmed, ediMessage.EM_MessageSubType);
		}

		ZString GetActualDocumentType(ZString templateName, ZString dataContext)
		{
			var templateQuery = new ZDBOnlySubQuery(typeof(StmTemplate), StmMenuTemplatePivotSchema.SI_SO);
			templateQuery.AddToFilter(StmTemplateSchema.SO_Name, templateName);
			templateQuery.AddToFilter(StmTemplateSchema.SO_DataContext, dataContext);
			templateQuery.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, true);
			var pivotQuery = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			pivotQuery.AddSubQuery(templateQuery, JoinCondition.And);
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsSystemDefined, true);

			return Factory.LoadTop1<StmMenuTemplatePivot>(pivotQuery)?.DocType.RT_DocType ?? ZString.Empty;
		}

		#region Implementation

		B3Message GetB3Message(string messageText)
		{
			return GetEDIMessage<B3Message>(messageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			impMessageProcessor = new IMPMessageProcessor(logger);
			declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_IsCancelled = false;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			AddSentMessage(entryHeader);
			Factory.Save();
		}

		void AddSentMessage(CusEntryHeader entryHeader)
		{
			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+835+9'LOC+41+497'RFF+TN:000067897'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			entryHeader.Messages.Add(sentMessage);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		IMPMessageProcessor impMessageProcessor;

		#endregion
	}
}
