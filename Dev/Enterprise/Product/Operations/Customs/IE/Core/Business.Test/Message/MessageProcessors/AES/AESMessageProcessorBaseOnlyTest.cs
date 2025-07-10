using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class AESMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestEndToEndProcessing()
		{
			(var entry, var incomingMessage, var outgoingMessage) = CreateSetupData();
			Factory.Save();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var logger = new LoggingInformation();
				var processor = new AESMessageProcessorForTest(logger, typeof(Cc528C));
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("entry.CH_EntryStatus", "CLR", entry.CH_EntryStatus);
					AssertEquals("incomingMessage.EM_MessageOwner", "DONE", incomingMessage.EM_MessageOwner);
				});
			}
		}

		public void TestGetEmailGroupRegistryItem()
		{
			var staff = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST1", "Staff 1", "st1@email.address");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GP1";
			group.GG_Desc = "Group 1";
			var groupStaff1 = group.Staff.AddNew();
			groupStaff1.GS_Code = "GS1";
			groupStaff1.GS_FullName = "Group Staff 1";
			groupStaff1.GS_EmailAddress = "gs1@email.address";

			(var entry, var incomingMessage, var outgoingMessage) = CreateSetupData();
			outgoingMessage.EM_SystemCreateUser = "ST1";
			incomingMessage.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			var cc528Text = AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2");
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc528Text, includeResponseWrap: false);
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new AESMessageProcessorForTest(logger, typeof(Cc528C));
			processor.ShouldSendEmailNotification = true;

			using (EUCustomsDataRegistry.Instance.SendExportAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExportGroupNotification(Core.Constants.EmailTo.NoEmails, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				AssertEquals("SendMode being NoEmails, no email should be created.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}

			incomingMessage.EM_Status = "QUE";
			using (EUCustomsDataRegistry.Instance.SendExportAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExportGroupNotification(Core.Constants.EmailTo.StaffMember, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertEquals("SendMode being StaffMember, should have created an email for Staff.", "st1@email.address", email.Recipients.Cast<RecipientDef>().Single().Email);
			}

			incomingMessage.EM_Status = "QUE";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (EUCustomsDataRegistry.Instance.SendExportAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExportGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				CombineAssertions("SendMode being Staff&Group, should have created an email for both Staff and Group.", () =>
				{
					var recipients = email.Recipients.Cast<RecipientDef>().Select(r => r.Email).ToArray();
					AssertEquals("Recipients Count", 2, recipients.Length);
					Assert("Recipients should contain Staff email.", recipients.Contains("st1@email.address"));
					Assert("Recipients should contain Group email.", recipients.Contains("gs1@email.address"));
				});
			}
		}

		(CusEntryHeader entry, AESInboundEDIMessage incomingMessage, AESOutboundEDIMessage outgoingMessage) CreateSetupData()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CIE";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BIE";
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_GB = branch.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			const string transactionId = "E7CDA07D-7EF1-4380-8130-D584734E984B";
			var incomingMessage = Factory.New<AESInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2"), includeResponseWrap: false);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = branch.PK;
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			entry.Messages.Add(outgoingMessage);
			return (entry, incomingMessage, outgoingMessage);
		}
	}

	class AESMessageProcessorForTest : AESMessageProcessor<AESInboundEDIMessage, CC528CProvider>
	{
		public AESMessageProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => "HELLO WORLD";

		public bool ShouldSendEmailNotification { get; set; }
		protected override bool NeedToSendEmailNotification(EDIMessage message) => ShouldSendEmailNotification;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AESInboundEDIMessage message, CC528CProvider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			message.EM_MessageOwner = "DONE";
		}

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC528CProvider provider) => "CLR";
	}
}
