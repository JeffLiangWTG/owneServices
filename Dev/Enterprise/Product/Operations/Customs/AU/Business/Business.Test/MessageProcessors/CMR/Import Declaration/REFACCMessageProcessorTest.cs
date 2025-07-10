using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class REFACCMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestUpdateConsolidatedDeclaration()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_PaymentMethod = "IMP";
			leadDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			leadDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			leadDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDeclaration.InvoiceLines);
			leadDeclarationEntryHeader.MergedLines.Add(leadDeclarationEntryHeader.AllEntryLines[0]);
			leadDeclarationEntryHeader.AQISServicePaymentAmount = 100m;
			leadDeclarationEntryHeader.CH_TotalPaid = 2500m;
			leadDeclarationEntryHeader.EntryNumber = "1";
			leadDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			otherDeclaration.JE_ApplicationCode = AUCustoms.ImportMessagingMode.ForceCMRMessages;
			otherDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			otherDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			var otherDeclarationEntryHeader = otherDeclaration.EntryHeader;
			otherDeclarationEntryHeader.EntryNumber = "1";
			otherDeclarationEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			Factory.Save();

			leadDeclarationEntryHeader.EntryPayInfos.RemoveAndDeleteAll();
			AssertEquals("No payment information", 0, entryHeader.EntryPayInfos.Count);

			var iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMDRClearWithNegative.Replace("RFF+ABO:B00122382/1/MEL2", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");
			iMDRMessage.EM_MessageNum = "0001007";
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			AssertEquals("There should be one PayInfo", 1, leadDeclarationEntryHeader.EntryPayInfos.Count);

			var payInfo = leadDeclarationEntryHeader.EntryPayInfos[0];
			AssertEquals(false, payInfo.C9_RemAdvReceived);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Pending, payInfo.C9_PaymentStatus);
			AssertEquals("0001007", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(-71m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);

			incomingMessage.EM_MessageType = "RCC";
			incomingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 05);
			incomingMessage.EM_MessageNum = "0001008";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.REFACC.Replace("RFF+ABO:B00122382/1/SYD1", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");

			var processor = GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);

			AssertEquals("There should be only one cusEntryPayInfo", 1, leadDeclarationEntryHeader.EntryPayInfos.Count);
			AssertEquals(true, payInfo.C9_RemAdvReceived);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Clear, payInfo.C9_PaymentStatus);
			AssertEquals("0001007", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(-71m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
			AssertEquals("Lead Declaration EntryHeader Customs Pay", "Refunded", leadDeclarationEntryHeader.PaymentStatus);
			AssertEquals("Other Declaration EntryHeader Customs Pay", "Refunded", otherDeclarationEntryHeader.PaymentStatus);
		}

		public void TestCusEntryPayInfoWhenIMDRComesFirst()
		{
			entryHeader.EntryPayInfos.RemoveAndDeleteAll();
			AssertEquals("No payment information", 0, entryHeader.EntryPayInfos.Count);

			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.Declaration.JE_PaymentMethod = "IMP";
			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMDRClearWithNegative;
			iMDRMessage.EM_MessageNum = "0001007";
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			AssertEquals("There should be one PayInfo", 1, entryHeader.EntryPayInfos.Count);
			CusEntryPayInfo payInfo = entryHeader.EntryPayInfos[0];

			AssertEquals(false, payInfo.C9_RemAdvReceived);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Pending, payInfo.C9_PaymentStatus);
			AssertEquals("0001007", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(-71m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);

			incomingMessage.EM_MessageType = "RCC";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			incomingMessage.EM_MessageNum = "0001008";
			var processor = GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);

			AssertEquals("There should be only one cusEntryPayInfo", 1, entryHeader.EntryPayInfos.Count);
			AssertEquals(true, payInfo.C9_RemAdvReceived);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Clear, payInfo.C9_PaymentStatus);
			AssertEquals("0001007", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(-71m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.REFACC;

		protected override ZString GetExpectedMessageName() => "Refund Acknowledgement Response (REFACC)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new REFACCMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRREFACCMessage);
	}
}
