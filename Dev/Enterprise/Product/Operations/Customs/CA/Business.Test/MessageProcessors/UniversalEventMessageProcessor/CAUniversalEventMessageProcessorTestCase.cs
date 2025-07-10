using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	class CAUniversalEventMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestReportEmail_IncludeNotifyInfo()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";
			branch.GB_BranchName = "Test Branch 1";
			var user = CreateUser(Factory, "ABC", "abc@where.com");
			var notifyEmailGroup = Factory.NewWithValidTestData<GlbGroup>();
			notifyEmailGroup.GG_Code = "TES";
			notifyEmailGroup.GG_Desc = "Test Group";
			Factory.Save();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "CAM20230308001";
			var message = Factory.New<UniversalEventMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = branch.PK;
			message.EM_SystemCreateUser = user.GS_Code;
			message.EM_LinkedObject = master;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master)
			{
				NotifyEmailGlbGroup = notifyEmailGroup
			};
			processor.Process();
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Notify Group: TES - Test Group<br />" + System.Environment.NewLine + "Notify Mode: ESG - Email Staff Member and Nominated Group<br />", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestSetMessageBranch_CusCAeMHMaster()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "BBB";
			branch1.GB_BranchName = "Test Branch 1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "CCC";
			branch2.GB_BranchName = "Test Branch 2";
			Factory.Save();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "CAM20230308001";
			var message = Factory.New<UniversalEventMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = branch1.PK;
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			Factory.Save();
			AssertNotEquals(master.Branch.PK, message.EM_GB);
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master);
			processor.Process();
			AssertEquals(master.Branch.PK, message.EM_GB);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			var message1 = master.Messages.AddNew();
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message1.EM_GB = branch2.PK;
			Factory.Save();
			AssertNotEquals(message1.EM_GB, message.EM_GB);
			processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master);
			processor.Process();
			AssertEquals(message1.EM_GB, message.EM_GB);
		}

		public void TestSetMessageBranch_CusCAeMHHouse()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "BBB";
			branch1.GB_BranchName = "Test Branch 1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "CCC";
			branch2.GB_BranchName = "Test Branch 2";
			Factory.Save();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "CAM20230308001";
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "CAH20230308001";
			var message = Factory.New<UniversalEventMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = branch1.PK;
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			Factory.Save();
			AssertNotEquals(master.Branch.PK, message.EM_GB);
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, house);
			processor.Process();
			AssertEquals(master.Branch.PK, message.EM_GB);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			var message1 = house.Messages.AddNew();
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message1.EM_GB = branch2.PK;
			Factory.Save();
			AssertNotEquals(message1.EM_GB, message.EM_GB);
			processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, house);
			processor.Process();
			AssertEquals(message1.EM_GB, message.EM_GB);
		}

		public void TestGetUserToNotify_CusCAeMHMaster_FallbackHouseBills()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var user1 = CreateUser(factory1, "!U1", "user1@where.com");
			var user2 = CreateUser(factory1, "!U2", "user2@where.com");
			var master = factory1.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";
			SetupIgnoreEDIMessageForGetUserToNotifyTest(master, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var master2 = factory2.Load<CusCAeMHMaster>(master.PK);
			var message = factory2.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master2);
			var result = processor.GetUserToNotify_Exposed(master2);
			AssertNull("No outgoing message matched", result);

			CreateMessage(house, user1.GS_Code, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();

			result = processor.GetUserToNotify_Exposed(master2);
			AssertEquals("should match user1", user1.PK, result.PK);

			var house2 = master.HouseBills.AddNew();
			house2.BW_MessageReference = "CAH0000005";
			CreateMessage(house2, user2.GS_Code, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();

			factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			master2 = factory2.Load<CusCAeMHMaster>(master.PK);
			message = factory2.New<UniversalEventMessage>();
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master2);
			result = processor.GetUserToNotify_Exposed(master2);
			AssertEquals("should match the latest sent user", user2.PK, result.PK);
		}

		public void TestGetUserToNotify_CusCAeMHMaster()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var user1 = CreateUser(factory1, "!U1", "user1@where.com");
			var user2 = CreateUser(factory1, "!U2", "user2@where.com");
			var master = factory1.New<CusCAeMHMaster>();
			master.BP_SystemCreateUser = user2.GS_Code;
			SetupIgnoreEDIMessageForGetUserToNotifyTest(master, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var master2 = factory2.Load<CusCAeMHMaster>(master.PK);
			var message = factory2.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master2);
			var result = processor.GetUserToNotify_Exposed(master2);
			AssertNull("No outgoing message matched", result);

			CreateMessage(master, user1.GS_Code, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();

			result = processor.GetUserToNotify_Exposed(master2);
			AssertEquals("should match user1", user1.PK, result.PK);
		}

		public void TestGetUserToNotify_CusCAeMHHouse()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var user1 = CreateUser(factory1, "!U1", "user1@where.com");
			var user2 = CreateUser(factory1, "!U2", "user2@where.com");
			var master = factory1.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";
			master.BP_SystemCreateUser = user2.GS_Code;
			SetupIgnoreEDIMessageForGetUserToNotifyTest(house, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var house2 = factory2.Load<CusCAeMHHouse>(house.PK);
			var message = factory2.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, house2);
			var result = processor.GetUserToNotify_Exposed(house2);
			AssertNull("No outgoing message matched", result);

			CreateMessage(house, user1.GS_Code, EDIMessage.ApplicationCodes.CAACI);
			factory1.Save();

			result = processor.GetUserToNotify_Exposed(house2);
			AssertEquals("should match user1", user1.PK, result.PK);
		}

		public void TestGetUserToNotify_CusEntryHeader()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var user1 = CreateUser(factory1, "!U1", "user1@where.com");
			var user2 = CreateUser(factory1, "!U2", "user2@where.com");
			var user3 = CreateUser(factory1, "!U3", "user3@where.com");
			var declaration = factory1.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = user3.GS_Code;
			declaration.JE_SystemCreateUser = user2.GS_Code;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			SetupIgnoreEDIMessageForGetUserToNotifyTest(entry, EDIMessage.ApplicationCodes.CAIMP);
			factory1.Save();
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var entry2 = factory2.Load<CusEntryHeader>(entry.PK);
			var message = factory2.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, entry2);
			var result = processor.GetUserToNotify_Exposed(entry2);
			AssertEquals("Should use CusAgent", user3.PK, result.PK);

			user2.GS_EmailAddress = ZString.Empty;
			factory1.Save();

			factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			entry2 = factory2.Load<CusEntryHeader>(entry.PK);
			result = processor.GetUserToNotify_Exposed(entry2);
			AssertEquals("Should use sysyrm create user", user3.PK, result.PK);

			CreateMessage(entry, user1.GS_Code, EDIMessage.ApplicationCodes.CAIMP);
			factory1.Save();

			result = processor.GetUserToNotify_Exposed(entry2);
			AssertEquals("should match user1", user1.PK, result.PK);
		}

		public void TestGetAssociatedBusinessObjectDescription()
		{
			var expectedAssociatedBusinessObjectDescription = "Response Type For Testing has been received from the CBSA for a(n) Test IID Declaration.";
			var house = Factory.New<CusCAeMHHouse>();
			var message = Factory.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, house);
			AssertEquals("GetAssociatedBusinessObjectDescription", expectedAssociatedBusinessObjectDescription, processor.AssociatedBusinessObjectDescription_Exposed);
		}

		void SetupIgnoreEDIMessageForGetUserToNotifyTest(BusinessObject bizObj, ZString applicationCode)
		{
			var factory = bizObj.Factory;
			var batchProcessorStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
			batchProcessorStaffMember.GS_EmailAddress = "batch@where.com";
			CreateMessage(bizObj, User.ServiceUserCode, applicationCode);
			var interchangeUserCodeStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.InterchangeUserCode);
			interchangeUserCodeStaffMember.GS_EmailAddress = "intercahnge@where.com";
			CreateMessage(bizObj, User.InterchangeUserCode, applicationCode);
			var webUserCodeStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			webUserCodeStaffMember.GS_EmailAddress = "web@where.com";
			CreateMessage(bizObj, User.WebUserCode, applicationCode);
			var unKnownUserCodeStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.UnKnownUserCode);
			unKnownUserCodeStaffMember.GS_EmailAddress = "unknown@where.com";
			CreateMessage(bizObj, User.UnKnownUserCode, applicationCode);
			var userWithoutEmail = CreateUser(factory, "*U1", ZString.Empty);
			userWithoutEmail.GS_FullName = "USER WITHOUT EMAIL";
			CreateMessage(bizObj, userWithoutEmail.GS_Code, applicationCode);
			var userForDiffApplicationCode = CreateUser(factory, "*U2", "diffApplicationCode@where.com");
			CreateMessage(bizObj, userForDiffApplicationCode.GS_Code, "Z@#");
		}

		void CreateMessage(BusinessObject parent, ZString createUser, ZString applicationCode)
		{
			var newMessage = parent.Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			newMessage.EM_LinkedObject = parent;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_ApplicationCode = applicationCode;
			newMessage.EM_SystemCreateUser = createUser;
			newMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			newMessage.MessageNumberStrategy = MessageNumberStrategy;
		}

		MockMessageNumberStrategy MessageNumberStrategy => strategy ?? (strategy = new MockMessageNumberStrategy());
		MockMessageNumberStrategy strategy;

		class MockMessageNumberStrategy : IMessageNumberStrategy
		{
			public string GetMessageReferenceNumber() => Guid.NewGuid().ToString("N");
		}

		GlbStaff CreateUser(BusinessObjectFactory factory, ZString code, ZString emailAddress)
		{
			var user = factory.New<GlbStaff>();
			user.GS_Code = code;
			user.GS_LoginName = code;
			user.GS_FullName = code + " FULLNAME";
			user.GS_EmailAddress = emailAddress;
			return user;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
		}

		IXmlSessionTracker logger;

		protected class CustomsManifestStatusMessageProcessorForTesting : CAUniversalEventMessageProcessor
		{
			public CustomsManifestStatusMessageProcessorForTesting(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, BusinessObject businessObject)
				: base(logger, universalEvent, message, businessObject)
			{ }

			public GlbStaff GetUserToNotify_Exposed(BusinessObject parent) => GetUserToNotify(parent);

			public ZGuid NotifyEmailGroup_Exposed => NotifyEmailGroup;

			public ZString NotifyEmailMode_Exposed => NotifyEmailMode;

			public ZString AssociatedBusinessObjectDescription_Exposed => base.GetAssociatedBusinessObjectDescription();

			protected override ZString GetMessageTypeDescription() => "Message Type For Testing";

			protected override ZString GetResponseTypeDescription() => "Response Type For Testing";

			public GlbGroup NotifyEmailGlbGroup;

			protected override ZGuid NotifyEmailGroup => NotifyEmailGlbGroup?.PK ?? ZGuid.Empty;

			protected override ZString NotifyEmailMode => Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
		}
	}
}
