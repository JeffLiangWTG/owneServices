using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	abstract class NCTSDepartureMessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> :
		MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, NctsDepartureMovementHeader, NctsHeader>
			where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
			where TInboundEDIMessage : InboundEDIMessage
			where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_JobReference = "B00001000";
			header.BH_GB = Branch.PK;
			var movementHeader = header.MovementHeader;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			movementHeader.Messages.Add(outgoingMessage);
			return (header, movementHeader, outgoingMessage, incomingMessage);
		}
	}

	class NCTSDepartureMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestEndToEndProcessing()
		{
			(var moveHeader, var incomingMessage, var outgoingMessage) = CreateSetupData();
			Factory.Save();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var logger = new LoggingInformation();
				var processor = new NCTSDepartureMessageProcessorForTest(logger, typeof(Cc028CType));
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("moveHeader.BM_CustomsStatus", "CLR", moveHeader.BM_CustomsStatus);
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
			Factory.Save();

			(var moveHeader, var incomingMessage, var outgoingMessage) = CreateSetupData();
			outgoingMessage.EM_SystemCreateUser = "ST1";
			incomingMessage.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE028;
			var cc521Text = InterchangeProcessorTestHelper.GetStandardCC028CText("21IEDUB11A782454R2", "21IEDUB11A782454R2");
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc521Text, includeResponseWrap: false);
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new NCTSDepartureMessageProcessorForTest(logger, typeof(Cc028CType));
			processor.ShouldSendEmailNotification = true;

			using (processor.GetEmailGroupRegistryItem().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new NctsGroupNotification(Core.Constants.EmailTo.NoEmails, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				AssertEquals("SendMode being NoEmails, no email should be created.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}

			incomingMessage.EM_Status = "QUE";
			using (processor.GetEmailGroupRegistryItem().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new NctsGroupNotification(Core.Constants.EmailTo.StaffMember, group.PK)))
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertEquals("SendMode being StaffMember, should have created an email for Staff.", "st1@email.address", email.Recipients.Cast<RecipientDef>().Single().Email);
			}

			incomingMessage.EM_Status = "QUE";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (processor.GetEmailGroupRegistryItem().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new NctsGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK)))
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

		(NctsDepartureMovementHeader moveHeader, NCTSInboundEDIMessage incomingMessage, NCTSOutboundEDIMessage outgoingMessage) CreateSetupData()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CIE";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BIE";
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_JobReference = "B00001000";
			header.BH_GB = branch.PK;
			var moveHeader = header.MovementHeader;

			const string transactionId = "E7CDA07D-7EF1-4380-8130-D584734E984B";
			var incomingMessage = Factory.New<NCTSInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE028;
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, InterchangeProcessorTestHelper.GetStandardCC028CText("21IEDUB11A782454R2", "21IEDUB11A782454R2"), includeResponseWrap: false);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = branch.PK;
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			header.Messages.Add(outgoingMessage);
			return (moveHeader, incomingMessage, outgoingMessage);
		}
	}

	class NCTSDepartureMessageProcessorForTest : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC028CProvider>
	{
		public NCTSDepartureMessageProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => "HELLO WORLD";

		public bool ShouldSendEmailNotification { get; set; }

		protected override bool NeedToSendEmailNotification(EDIMessage message) => ShouldSendEmailNotification;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, NCTSInboundEDIMessage message, CC028CProvider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			message.EM_MessageOwner = "DONE";
		}

		public new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC028CProvider provider) => "CLR";
	}
}
