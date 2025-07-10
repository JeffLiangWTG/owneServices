using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PAYEXCMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestMessageSubTypeSetToREJ()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_DeclarationReference = "S00007901";

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00007901/3";

			CMRPAYEXCMessage incomingMessage = Factory.New<CMRPAYEXCMessage>();
			incomingMessage.EM_MessageText = response;
			incomingMessage.EM_MessageSubType = "";

			PAYEXCMessageProcessor processor = (PAYEXCMessageProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Message sub type should be REJ to trigger Entry's status to be Payment Failed", "REJ", incomingMessage.EM_MessageSubType);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 0, processor.AcknowledgementEmailSendCount);
		}

		public void TestConsolidatedEntryProcessPAYEXCMessage()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			leadDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDeclaration.InvoiceLines);
			leadDeclarationEntryHeader.MergedLines.Add(leadDeclarationEntryHeader.AllEntryLines[0]);
			leadDeclarationEntryHeader.AQISServicePaymentAmount = 100m;
			leadDeclarationEntryHeader.CH_TotalPaid = 2500m;
			leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			otherDeclaration.JE_ApplicationCode = AUCustoms.ImportMessagingMode.ForceCMRMessages;
			otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			otherDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			var otherDeclarationEntryHeader = otherDeclaration.EntryHeader;
			otherDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			var outgoingMessage = Factory.New<CMRPAYSTDMessage>();
			outgoingMessage.EM_MessageText = TestMessages.PAYSTDMessage.Replace("BGM+481:::PAYSTD+B00001019/8/SYD1", $"BGM+481:::PAYSTD+{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 01);
			consolidatedDeclaration.Messages.Add(outgoingMessage);

			incomingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 05);
			incomingMessage.EM_MessageText = response;
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("RFF+ABO:S00007901/3/MEL1", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/MEL1");

			GetMessageProcessor().ProcessMessage(incomingMessage);
			AssertEquals("Message is linked to consolidated entry", consolidatedDeclaration, incomingMessage.EM_LinkedObject);
			AssertEquals("IsCustomsChargePaid", false, leadDeclarationEntryHeader.AddInfo.ZA_IsPAYRECAck_Hidden);
			AssertEquals("EntryNumber", "AAEGLR4TK", leadDeclarationEntryHeader.EntryNumber);
			AssertEquals("Lead Declaration Message Status", "Failed Payment", leadDeclaration.JE_MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Message Status", "Failed Payment", leadDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Customs Pay", "Pay Rejected", leadDeclarationEntryHeader.PaymentStatus);
			AssertEquals("EntryNumber", "AAEGLR4TK", otherDeclarationEntryHeader.EntryNumber);
			AssertEquals("Other Declaration Message Status", "Failed Payment", otherDeclaration.JE_MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Message Status", "Failed Payment", otherDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Customs Pay", "Pay Rejected", otherDeclarationEntryHeader.PaymentStatus);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.PAYEXC;

		protected override ZString GetExpectedMessageName() => "Exceeded Payment Limit Advice - (PAYEXC)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new PAYEXCMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.CustomsEntryHeaders.AddNew();
			outgoingMessage = declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(CMRPAYEXCMessage));
			outgoingMessage.EM_MessageSubType = "ORG";
		}

		protected override Type IncomingMessageType => typeof(CMRPAYEXCMessage);

		readonly string response = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYEXC+4457 6DB7 8GAG:1+11'
NAD+MR+FGG393E::95'
NAD+IM+25059022963::95'
NAD+VT+AA67WH::95'
NAD+COQ+033063::215'
NAD+AO+163182::215'
RFF+ABO:S00007901/3/MEL1::1'
RFF+ABQ:N/A'
RFF+ADU:S00007901/3 S-7901'
RFF+ABT:AAEGLR4TK'
TAX+4+TOT'
MOA+128:0000000027619.04'
TAX+6+OTH'
MOA+206:0000000000000.00'
UNT+16+000001'".Replace("\r\n", "");
	}
}
