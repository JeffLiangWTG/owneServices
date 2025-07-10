using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class B3XResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestMessageRejectedResponse()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN'BGM++835+9'DTM+137:20131010:102'ERP+:I11'RFF+ABO:500000148'ERC+942282'ERP+:I11'RFF+ABO:500000148'ERC+943643'DOC+961'CST++0+0+0'UNT+12+1'";

			#region Expected Email Body

			const string expectedBody = @"Reference Number : 500000148<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a X Type Entry Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Code</th><th>Error Message</th><th>Entry Component</th><th>Reference</th></tr></thead><tr><td>942282</td><td>INVALID PASSWORD</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;500000148&#39;</td></tr><tr><td>943643</td><td>BATCH REJECTED</td><td>I11 - B3 Header</td><td>Transaction Number: &#39;500000148&#39;</td></tr></table>";

			#endregion

			var ediMessage = GetEDIMessage<B3Message>(messageText);
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-10);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message type updated to B3X", MessageTypeList.Codes.XTypeEntry, ediMessage.EM_MessageType);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", declaration.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EntryStatusList.Codes.Error, declaration.JE_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, declaration.JE_MessageStatus);
		}

		public void TestMessageAcceptedResponse()
		{
			const string messageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:500000148'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
			const string expectedBody = @"Reference Number : 500000148<br />
<br />
</strong>A 'Message Accepted' response has been received from the CBSA for a X Type Entry Message.<br />
Please see message details below.<br />
<br />
<br />
<strong>Processing Date:  </strong>12-Feb-10<br />";

			var ediMessage = GetEDIMessage<B3Message>(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EntryStatusList.Codes.Clear, ediMessage.EM_MessageSubType);
			AssertEquals("Message type updated to B3X", MessageTypeList.Codes.XTypeEntry, ediMessage.EM_MessageType);
			AssertEquals("Linked to entry", declaration.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EntryStatusList.Codes.Clear, declaration.JE_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, declaration.JE_MessageStatus);
			AssertEquals("Message Status", new ZDateTime(2010, 02, 12), declaration.CA_B2AcceptedDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			impMessageProcessor = new IMPMessageProcessor(logger);
			declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.XTypeEntry;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "50000014";
			declaration.JE_IsCancelled = false;
			declaration.JE_MessageStatus = "AWO";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			AddSentMessage(declaration);
			Factory.Save();
		}

		void AddSentMessage(JobDeclaration declaration)
		{
			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = string.Format("UNH+{0}+CUSDEC:S:99B:UN'BGM+:::AB+835+9'LOC+41+497'RFF+TN:050000014'RFF+ARA:123241838RM0001'RFF+AEA:0123241838'", EDIMessage.MessageNumberPlaceHolder);
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			declaration.Messages.Add(sentMessage);
		}

		JobDeclaration declaration;
		IMPMessageProcessor impMessageProcessor;
	}
}
