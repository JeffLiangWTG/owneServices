using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PAYINVMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestShouldSendAcknowledgementReport()
		{
			AssertEquals(false, new PAYINVMessageProcessorForTest(logger).ShouldSendAcknowledgementReportExposed);
		}

		public void TestConsolidatedEntryProcessPAYINVMessage()
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
			incomingMessage.EM_MessageNum = "0001007";
			incomingMessage.EM_MessageText = TestMessages.PAYINVMessage;
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("RFF+ABO:S00004233/1/MEL1", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/MEL1");

			GetMessageProcessor().ProcessMessage(incomingMessage);
			AssertEquals("Message is linked to consolidated entry", consolidatedDeclaration, incomingMessage.EM_LinkedObject);
			AssertEquals("IsCustomsChargePaid", false, leadDeclarationEntryHeader.AddInfo.ZA_IsPAYRECAck_Hidden);
			AssertEquals("EntryNumber", "AACHJJFJG", leadDeclarationEntryHeader.EntryNumber);
			AssertEquals("Lead Declaration Message Status", "Failed Payment", leadDeclaration.JE_MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Message Status", "Failed Payment", leadDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Lead Declaration EntryHeader Customs Pay", "Pay Rejected", leadDeclarationEntryHeader.PaymentStatus);
			AssertEquals("EntryNumber", "AACHJJFJG", otherDeclarationEntryHeader.EntryNumber);
			AssertEquals("Other Declaration Message Status", "Failed Payment", otherDeclaration.JE_MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Message Status", "Failed Payment", otherDeclarationEntryHeader.MessageStatusDescription);
			AssertEquals("Other Declaration EntryHeader Customs Pay", "Pay Rejected", otherDeclarationEntryHeader.PaymentStatus);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.PAYINV;

		protected override ZString GetExpectedMessageName() => "Invalid Payment Record Advice - (PAYINV)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new PAYINVMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRPAYINVMessage);

		sealed class PAYINVMessageProcessorForTest : PAYINVMessageProcessor
		{
			public PAYINVMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public bool ShouldSendAcknowledgementReportExposed
			{
				get { return ShouldSendAcknowledgementReport; }
			}
		}
	}
}
