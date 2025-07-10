using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ImportSyntaxErrorResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestMatchExactTransactionNumber()
		{
			#region Expected Email Body

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>ELEM TOO LONG</td><td>200.22</td><td>MOA+43:200.22</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>INVALID CHARS</td><td>200.22</td><td>MOA+43:200.22</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>ELEM TOO LONG</td><td>TRANSMISSION APPARATUS FOR RADIO-BR</td><td>GIN+PN+TRANSMISSION APPARATUS FOR RADIO-BR:OADC</td><td>GIN / L: 21, P: 2,1</td></tr><tr align=""left""><td>MAND SEG MISSING</td><td>&nbsp;</td><td>CST+1+POS+1+8525800010+13</td><td>LOC / L: 16, I: 12</td></tr><tr align=""left""><td>BLA BLA</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>
<br />
<br />
Please report this to CargoWise.";

			#endregion

			entryHeader.Messages.Add(SyntaxErrorMessageTest.GetSentEDIMessage<B3Message>(Factory, SyntaxErrorMessageTest.B3MessageText, ZDateTime.Today, "35"));
			var ediMessage = GetSyntaxErrorMessage(SyntaxErrorMessageTest.B3SyntaxErrorMessageText.Replace("\r\n", "'").Replace("12345", "23450"));
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			var errorMessage = "Could not find an associated business object (Job) for document reference = '23450000000250'";
			AssertContains(errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "Import Syntax Error Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);

			ediMessage = GetSyntaxErrorMessage(SyntaxErrorMessageTest.B3SyntaxErrorMessageText.Replace("\r\n", "'"));
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", MessageTypeList.Codes.B3CUSDEC, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.SyntaxError, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertSyntaxErrorEmail("Error B3 CUSDEC Response for 12345000000250", expectedBody, SyntaxErrorMessageTest.B3SyntaxErrorMessageText, ediMessage.SourceMessageTextWithErrorMarks);
		}

		public void TestQuerySyntaxErrorMessage()
		{
			#region Expected Email Body
			//Query Messages 

			//A Syntax Error response has been received from the CBSA.
			//Please see details below.

			//Error Message		Component Value		Source Line				Segment/Position
			//ELEM TOO LONG		85258000101			RFF+ABD:85258000101		RFF / L: 2, P: 1,2
			//ELEM TOO SHORT	990					RFF+AFG:990				RFF / L: 4, P: 1,2

			const string expectedBody = @"<strong>Query Messages</strong>
<br />
<br />
A Syntax Error response has been received from the CBSA.<br />
Please see details below.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>ELEM TOO LONG</td><td>85258000101</td><td>RFF+ABD:85258000101</td><td>RFF / L: 2, P: 1,2</td></tr><tr align=""left""><td>ELEM TOO SHORT</td><td>990</td><td>RFF+AFG:990</td><td>RFF / L: 4, P: 1,2</td></tr></table><br /><br />Please report this to CargoWise.";

			#endregion

			SyntaxErrorMessageTest.GetSentEDIMessage<QueryMessage>(Factory, SyntaxErrorMessageTest.QueryMessageText, ZDateTime.Today, "35");
			var ediMessage = GetSyntaxErrorMessage(SyntaxErrorMessageTest.QuerySyntaxErrorMessageText.Replace("\r\n", "'"));
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Message Sub Type", MessageTypeList.Codes.Query, ediMessage.EM_MessageSubType);

			//TODO: Enhance the message processor to send notifications to the user who sends Query messages from Query Messages module and remove the last parameter from the following assertion
			AssertSyntaxErrorEmail("Query Message Syntax Error Response", expectedBody, SyntaxErrorMessageTest.QuerySyntaxErrorMessageText, ediMessage.SourceMessageTextWithErrorMarks, string.Empty);
		}

		public void TestB3SyntaxErrorMessageWithNoLinkedObject()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+10207'BGM+:::000000216+020+11'DTM+137:201007071025:203'GIS+14'ERP+2:35:29'FTX+AAO+++SEGMENTMOALINE8ELE POS1,2:ELEM TOO LONG'FTX+AAO+++SEGMENTMOALINE8ELE POS1,2:INVALID CHARS'FTX+AAO+++SEGMENTNADLINE11ELE POS8,0:ELEM TOO LONG'FTX+AAO+++SEGMENTLOCLINE14ELE POS2,1:MAND ELEM MISSING'FTX+AAO+++SEGMENTMOALINE18ELE POS1,2:INVALID CHARS'UNT+11+1'";
			const string errorMessage = "Could not find an associated business object (Job) for document reference = '10207000000216'";

			var ediMessage = GetSyntaxErrorMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "Import Syntax Error Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestB3SyntaxErrorMessageWithLinkedObject()
		{
			#region Expected Email Body

			//An ERROR response message has been received from the CBSA for a B3 CUSDEC.
			//The message sent for the above mentioned job had the following errors.

			//Error Message			Component Value							Source Line											Segment/Position
			//ELEM TOO LONG			200.22									MOA+43:200.22										MOA / L: 7, P: 1,2
			//INVALID CHARS			200.22									MOA+43:200.22										MOA / L: 7, P: 1,2
			//ELEM TOO LONG			TRANSMISSION APPARATUS FOR RADIO-BR		GIN+PN+TRANSMISSION APPARATUS FOR RADIO-BR:OADC		GIN / L: 21, P: 2,1
			//MAND SEG MISSING	 											CST+1+POS+1+8525800010+13							LOC / L: 16, I: 12
			//BLA BLA	 	 	 

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>ELEM TOO LONG</td><td>200.22</td><td>MOA+43:200.22</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>INVALID CHARS</td><td>200.22</td><td>MOA+43:200.22</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>ELEM TOO LONG</td><td>TRANSMISSION APPARATUS FOR RADIO-BR</td><td>GIN+PN+TRANSMISSION APPARATUS FOR RADIO-BR:OADC</td><td>GIN / L: 21, P: 2,1</td></tr><tr align=""left""><td>MAND SEG MISSING</td><td>&nbsp;</td><td>CST+1+POS+1+8525800010+13</td><td>LOC / L: 16, I: 12</td></tr><tr align=""left""><td>BLA BLA</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>
<br />
<br />
Please report this to CargoWise.";

			#endregion

			entryHeader.Messages.Add(SyntaxErrorMessageTest.GetSentEDIMessage<B3Message>(Factory, SyntaxErrorMessageTest.B3MessageText, ZDateTime.Today, "35"));
			var ediMessage = GetSyntaxErrorMessage(SyntaxErrorMessageTest.B3SyntaxErrorMessageText.Replace("\r\n", "'"));
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", MessageTypeList.Codes.B3CUSDEC, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.SyntaxError, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertSyntaxErrorEmail("Error B3 CUSDEC Response for 12345000000250", expectedBody, SyntaxErrorMessageTest.B3SyntaxErrorMessageText, ediMessage.SourceMessageTextWithErrorMarks);
		}

		public void TestSyntaxErrorMessageLinkeToB3XDeclaration()
		{
			sentMessageX.EM_MessageNum = "35";
			Factory.Save();
			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>ELEM TOO LONG</td><td>&nbsp;</td><td>&nbsp;</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>INVALID CHARS</td><td>&nbsp;</td><td>&nbsp;</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>ELEM TOO LONG</td><td>&nbsp;</td><td>&nbsp;</td><td>GIN / L: 21, P: 2,1</td></tr><tr align=""left""><td>MAND SEG MISSING</td><td>&nbsp;</td><td>&nbsp;</td><td>LOC / L: , I: 256</td></tr><tr align=""left""><td>BLA BLA</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>
<br />
<br />
Please report this to CargoWise.";

			var messageText = SyntaxErrorMessageTest.B3XSyntaxErrorMessageText;
			var ediMessage = GetSyntaxErrorMessage(messageText.Replace("\r\n", "'"));
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", MessageTypeList.Codes.XTypeEntry, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", b3XDeclaration.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.SyntaxError, b3XDeclaration.JE_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, b3XDeclaration.JE_MessageStatus);
			AssertSyntaxErrorEmail("Error B3 CUSDEC Response for 12345500000148", expectedBody, messageText, ediMessage.SourceMessageTextWithErrorMarks);
		}

		public void TestInvalidSyntaxErrorMessage()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+10207'BGM+:::12345000000216+020+11'DTM+137:201007071025:203'GIS+1'UNT+11+1'";
			const string errorMessage = "The message processor was unable to interpret received message.";

			var ediMessage = GetSyntaxErrorMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "Import Syntax Error Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
			AssertEmail("Import Syntax Error Response Message Processor Error Report", errorMessage, messageText, string.Empty);
		}

		public void TestB3MessageStatusIsNotAwaitingReply()
		{
			#region Expected Email Body

			//An ERROR response message has been received from the CBSA for a B3 CUSDEC.
			//The message sent for the above mentioned job had the following errors.

			//Error Message			Component Value		Source Line		Segment/Position
			//ELEM TOO LONG	 	 										MOA / L: 7, P: 1,2
			//INVALID CHARS	 	 										MOA / L: 7, P: 1,2
			//ELEM TOO LONG	 	 										GIN / L: 21, P: 2,1
			//MAND SEG MISSING	 	 									LOC / L: , I: 256
			//BLA BLA	 	 	 

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a B3 CUSDEC.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Syntax Error Messages</th></tr></thead></table><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th><th>Component Value</th><th>Source Line</th><th>Segment/Position</th></tr></thead><tr align=""left""><td>ELEM TOO LONG</td><td>&nbsp;</td><td>&nbsp;</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>INVALID CHARS</td><td>&nbsp;</td><td>&nbsp;</td><td>MOA / L: 7, P: 1,2</td></tr><tr align=""left""><td>ELEM TOO LONG</td><td>&nbsp;</td><td>&nbsp;</td><td>GIN / L: 21, P: 2,1</td></tr><tr align=""left""><td>MAND SEG MISSING</td><td>&nbsp;</td><td>&nbsp;</td><td>LOC / L: , I: 256</td></tr><tr align=""left""><td>BLA BLA</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";

			#endregion

			entryHeader.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			Factory.Save();
			var ediMessage = GetSyntaxErrorMessage(SyntaxErrorMessageTest.B3SyntaxErrorMessageText.Replace("\r\n", "'"));
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", MessageTypeList.Codes.B3CUSDEC, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status not changed", ZString.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status not changed", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEquals("No notifications should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation

		SyntaxErrorMessage GetSyntaxErrorMessage(string messageText)
		{
			var message = GetEDIMessage<SyntaxErrorMessage>(messageText);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			impMessageProcessor = new IMPMessageProcessor(logger);
			declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00000025";
			declaration.JE_IsCancelled = false;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			entryHeader.Messages.Add(sentMessage);

			b3XDeclaration = (JobDeclaration)JobDeclaration.New(Factory);
			b3XDeclaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.XTypeEntry;
			b3XDeclaration.TransactionNumber.AccountSecurityCode = "12345";
			b3XDeclaration.TransactionNumber.SequentialNumber = "50000014";
			b3XDeclaration.JE_IsCancelled = false;
			b3XDeclaration.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;

			sentMessageX = Factory.New<B3Message>();
			sentMessageX.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessageX.EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+080+9'LOC+41+497'RFF+TN:050000014'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			sentMessageX.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessageX.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			sentMessageX.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			sentMessageX.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			b3XDeclaration.Messages.Add(sentMessageX);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		B3Message sentMessageX;
		JobDeclaration b3XDeclaration;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		IMPMessageProcessor impMessageProcessor;

		#endregion
	}
}
