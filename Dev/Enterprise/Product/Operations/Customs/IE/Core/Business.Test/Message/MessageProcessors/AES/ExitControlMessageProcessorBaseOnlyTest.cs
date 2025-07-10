using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	abstract class ExitControlMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> : MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, CusExitReport, CusExitHeader>
		where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
		where TInboundEDIMessage : InboundEDIMessage
		where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (CusExitHeader declaration, CusExitReport messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			header.CXH_GB_Branch = Branch.PK;

			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";
			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			report.Messages.Add(outgoingMessage);
			return (header, report, outgoingMessage, incomingMessage);
		}
	}

	class ExitControlMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestEndToEndProcessing()
		{
			(var report, var incomingMessage, var outgoingMessage) = CreateSetupData();
			Factory.Save();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var logger = new LoggingInformation();
				var processor = new ExitControlMessageProcessorForTest(logger, typeof(Cc521C));
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("report.CER_Status", "CLR", report.CER_Status);
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

			(var report, var incomingMessage, var outgoingMessage) = CreateSetupData();
			outgoingMessage.EM_SystemCreateUser = "ST1";
			incomingMessage.EM_MessageType = AESIncomingMessageTypeList.Codes.IE521;
			var cc521Text = AESInterchangeProcessorTestHelper.GetStandardCC521CText("21IEDUB11A782454R2");
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc521Text, includeResponseWrap: false);
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new ExitControlMessageProcessorForTest(logger, typeof(Cc521C));
			processor.ShouldSendEmailNotification = true;

			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExitControlGroupNotification(Core.Constants.EmailTo.NoEmails, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				AssertEquals("SendMode being NoEmails, no email should be created.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}

			incomingMessage.EM_Status = "QUE";
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExitControlGroupNotification(Core.Constants.EmailTo.StaffMember, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertEquals("SendMode being StaffMember, should have created an email for Staff.", "st1@email.address", email.Recipients.Cast<RecipientDef>().Single().Email);
			}

			incomingMessage.EM_Status = "QUE";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ExitControlGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK)))
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

		(CusExitReport report, AESInboundEDIMessage incomingMessage, AESOutboundEDIMessage outgoingMessage) CreateSetupData()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CIE";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BIE";
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			header.CXH_GB_Branch = branch.PK;

			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";
			const string transactionId = "E7CDA07D-7EF1-4380-8130-D584734E984B";
			var incomingMessage = Factory.New<AESInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = AESIncomingMessageTypeList.Codes.IE521;
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC521CText("21IEDUB11A782454R2"), includeResponseWrap: false);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = branch.PK;
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			report.Messages.Add(outgoingMessage);
			return (report, incomingMessage, outgoingMessage);
		}
	}

	class ExitControlMessageProcessorForTest : ExitControlMessageProcessor<AESInboundEDIMessage, CC521CProvider>
	{
		public ExitControlMessageProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => "HELLO WORLD";

		public bool ShouldSendEmailNotification { get; set; }
		protected override bool NeedToSendEmailNotification(EDIMessage message) => ShouldSendEmailNotification;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AESInboundEDIMessage message, CC521CProvider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			message.EM_MessageOwner = "DONE";
		}

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC521CProvider provider) => "CLR";
	}
}
