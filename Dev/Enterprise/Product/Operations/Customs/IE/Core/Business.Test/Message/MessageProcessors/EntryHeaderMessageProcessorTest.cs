using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	abstract class EntryHeaderMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> : MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, CusEntryHeader, JobDeclaration>
	where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
	where TInboundEDIMessage : InboundEDIMessage
	where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_GB = Branch.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			entry.Messages.Add(outgoingMessage);
			return (declaration, entry, outgoingMessage, incomingMessage);
		}

		protected (CusEntryHeader messageAttachee, TInboundEDIMessage incomingMessage) SetupAndProcessMessage(string incomingMessageText)
		{
			var (_, _, _, incomingMessage) = CreateSetupData(InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, incomingMessageText, includeResponseWrap: false));
			var processor = Processor;
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			return (incomingMessage.EM_LinkedObject as CusEntryHeader, incomingMessage);
		}

		protected (CusEntryHeader entry, AESInboundEDIMessage incomingMessage) CreateSetupDataForExitReport(JobDeclaration declaration, CusExitHeader exitHeader)
		{
			var baseTime = new ZDateTime(2024, 7, 20, 10, 0, 0);

			declaration.JE_DeclarationReference = "B00001000";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var outgoing515OriginalMessage = Factory.New<AESOutboundEDIMessage>();
			outgoing515OriginalMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			outgoing515OriginalMessage.EM_ApplicationReference = TransactionID;
			outgoing515OriginalMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoing515OriginalMessage.EM_SystemCreateTimeUtc = baseTime;
			outgoing515OriginalMessage.EM_SystemCreateUser = Staff.GS_Code;
			entryHeader.Messages.Add(outgoing515OriginalMessage);

			var entryHeader528Message = Factory.New<AESInboundEDIMessage>();
			entryHeader528Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			entryHeader528Message.EM_ApplicationReference = TransactionID;
			entryHeader528Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2"), includeResponseWrap: false);
			entryHeader528Message.EM_SystemCreateTimeUtc = baseTime.AddMinutes(10);
			entryHeader528Message.EM_Status = EDIMessage.Status.ProcessedOK;
			entryHeader.Messages.Add(entryHeader528Message);

			entryHeader.MovementReferenceNumberSetter("21IEDUB11A782454R2");

			var exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_MessageStatus = "SNT";
			var consignment = exitHeader.CusExitConsignments.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;
			consignment.CXC_MovementReference = "21IEDUB11A782454R2";

			var exitReportOutgoingMessage = Factory.New<AESOutboundEDIMessage>();
			exitReportOutgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.InformationOnNonExitedExport;
			exitReportOutgoingMessage.EM_ApplicationReference = ExitReport_TransactionID;
			exitReportOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			exitReportOutgoingMessage.EM_SystemCreateTimeUtc = baseTime.AddMinutes(20);
			exitReportOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			exitReport.Messages.Add(exitReportOutgoingMessage);

			var exitReportAckMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESInboundEDIMessage.Status.Acknowledged, ExitReport_TransactionID);
			exitReportAckMessage.EM_LinkedObject = exitReport;
			exitReportAckMessage.EM_SystemCreateTimeUtc = baseTime.AddMinutes(30);
			exitReport.Messages.Add(exitReportAckMessage);

			var exitReport556Message = Factory.New<AESInboundEDIMessage>();
			exitReport556Message.EM_ApplicationReference = ExitReport_TransactionID;
			exitReport556Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE556;
			exitReport556Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardCC556CMailboxItemText(ExitReport_TransactionID, includeResponseWrap: false);
			exitReport556Message.EM_SystemCreateTimeUtc = baseTime.AddMinutes(40);
			exitReport556Message.EM_Status = EDIMessage.Status.ProcessedOK;
			exitReport.Messages.Add(exitReport556Message);

			var incomingMessage = Factory.New<AESInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = ExitReport_TransactionID;
			incomingMessage.EM_MessageType = MessageType;
			incomingMessage.EM_LinkedObject = exitReport;
			incomingMessage.EM_SystemCreateTimeUtc = baseTime.AddMinutes(60);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(ExitReport_TransactionID, MessageText, includeResponseWrap: false);

			return (entryHeader, incomingMessage);
		}

		const string ExitReport_TransactionID = "DDEDBD67-E5CF-4C11-B1FF-83AB7A17B40C";
	}
}
