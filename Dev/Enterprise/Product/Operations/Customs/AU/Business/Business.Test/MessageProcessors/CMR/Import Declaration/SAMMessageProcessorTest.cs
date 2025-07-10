using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SAMMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestSettingDeclarationStatusForSAM()
		{
			JobDeclaration drawbackDeclaration = Factory.New<JobDeclaration>();
			drawbackDeclaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawbackDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			drawbackDeclaration.JE_DeclarationReference = "B00001142";

			EDIMessage sAMMessage = drawbackDeclaration.Messages.AddNew(typeof(CMRSAMMessage));
			sAMMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKSam;
			var processor = new SAMMessageProcessor(logger);
			processor.ProcessMessage(sAMMessage);
			AssertEquals("Declaration Status", CustomsEntryStatus.Lodged.Code, drawbackDeclaration.JE_EntryStatus);
			AssertEquals("Declaration Entry Number", "AAACFRA9M", drawbackDeclaration.DeclarationNumber);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestConsolidatedEntry() => CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			otherDeclaration.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;
			Factory.Save();

			incomingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 05);
			incomingMessage.EM_MessageNum = "0001007";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.SAM;
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("RFF+ABO:B00122382/1/1", $"RFF+ABO:{consolidatedDeclaration.CRD_JobReferenceNumber}/SYD1");

			AssertEquals("Precondition: leadDeclaration status", "ATC", leadDeclaration.JE_EntryStatus);
			AssertEquals("Precondition: otherDeclaration status", "ATC", otherDeclaration.JE_EntryStatus);
			var processor = GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Message is linked to consolidated entry", consolidatedDeclaration, incomingMessage.EM_LinkedObject);
			AssertEquals("leadDeclaration status", "CLR", leadDeclaration.JE_EntryStatus);
			AssertEquals("otherDeclaration status", "CLR", otherDeclaration.JE_EntryStatus);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		});

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.SAM;

		protected override ZString GetExpectedMessageName() => "Status Advice Message (SAM)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new SAMMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRSAMMessage);
	}
}
