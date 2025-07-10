using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(SupplementaryCargoReportMessageManager))]
	sealed class SupplementaryCargoReportMessageManagerTest : CAMessageManagerTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestLogCustomsCommenced()
		{
			var shipment = Factory.New<ForwardingShipment>();
			house = Factory.New<CusSCAHouse>();
			house.CA_JS = shipment.PK;
			manager = new SupplementaryCargoReportMessageManagerForTesting(house);
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Undefined, false);

			var commencedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "ACI");
			AssertNotNull("Customs Commenced event should exist on shipment", commencedEvent);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var ediReleaseManager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(entryHeader));
			ediReleaseManager.OverrideCanSendThisMessage = true;
			ediReleaseManager.SendMessage(MessageSubTypes.Create, false);

			commencedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "ACI");
			AssertNotNull("ACI Customs Commenced event should exist on shipment", commencedEvent);

			commencedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "CA Import");
			AssertNotNull("CA Import Customs Commenced event should exist on shipment", commencedEvent);
		}

		public void TestGenerateWithdrawMessage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNFUZ";
			consol.JK_RL_NKDischargePort = "USPDX";
			consol.JK_MasterBillNum = "OOLU2626134800";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "FUZ90033932E";
			var house = Factory.New<CusSCAHouse>();
			house.CA_JS = shipment.PK;
			house.CA_CB = oceanBill.PK;
			oceanBill.RegisterEditableChildObject(house);

			var manager = new SupplementaryCargoReportMessageManagerForTesting(house);
			var messages = manager.GenerateWithdrawalMessages(house);
			AssertEquals(1, messages.Length);
		}

		public override void TestBusinessObject()
		{
			AssertEquals(house, manager.BusinessObject);
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder", typeof(SupplementaryCargoReportMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			house.CA_BGMReference = "123456";
			AssertEquals("MessageFriendlyName", "ACI Supplementary Cargo Report for 123456", manager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			Assert("Pre-condition", house.Messages.Count == 0);
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Undefined);
			Assert("One message created", house.Messages.Count == 1);
			AssertEquals("Message Status", MessageStatusList.Codes.AwaitingOriginal, house.CA_MessageStatus);
			house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
			manager.SendMessage(MessageSubTypes.Undefined);
			Assert("One more message created", house.Messages.Count == 2);
			AssertEquals("Message Status", MessageStatusList.Codes.AwaitingChange, house.CA_MessageStatus);
		}

		public override void TestCanSendThisMessage()
		{
			ZString messageText;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(messageText.Contains("Job not yet saved, Please save before sending."));
			Factory.Save();
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(messageText.Contains("ACI details not initialized, please close shipment, reopen shipment and open ACI tab."));
			house.CA_BGMReference = "XXXX";
			Factory.Save();
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertContains("there are no valid Packing Lines entered for ACI job, please open ACI tab on shipment and check the packing lines.", messageText);
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = "AIR";
			var packLine = house.PackLines.AddNew();
			packLine.CV_CN = container.PK;
			packLine.CV_PackageCount = 1;
			packLine.CV_GoodsDescription = "xxx";
			Factory.Save();
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
			Assert(messageText.Contains("the ACI Supplementary Cargo has not been reported yet."));
			house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			Factory.Save();
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
		}

		public void TestResetToOriginal()
		{
			house.CA_BGMReference = "XXX";
			house.SupplementaryReferenceNumber = "ZZZ";
			var message = (EDIMessage)house.Messages.AddNew();
			house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
			house.CA_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals("Pre-condition", 0, house.Logs.GetAllLogs().Count);
			Assert("Pre-condition", ZString.Empty != house.CA_ShipmentStatus);
			Assert("Pre-condition", ZString.Empty != house.CA_MessageStatus);
			Assert("Pre-condition", ZString.Empty != house.CA_BGMReference);
			Assert("Pre-condition", ZString.Empty != house.SupplementaryReferenceNumber);
			Assert("Pre-condition", EDIMessage.Status.Discarded != message.EM_Status);
			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = false;
			manager.ResetToOriginal(notification);
			AssertEquals("1 log added", 0, house.Logs.GetAllLogs().Count);

			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = true;
			manager.ResetToOriginal(notification);
			CombineAssertions(() =>
			{
				AssertEquals("1 log added", 1, house.Logs.GetAllLogs().Count);
				AssertEquals("Shipment status cleared", ZString.Empty, house.CA_ShipmentStatus);
				AssertEquals("Message status cleared", ZString.Empty, house.CA_MessageStatus);
				AssertEquals("BGM REference cleared", ZString.Empty, house.CA_BGMReference);
				AssertEquals("Supplementary num cleared", ZString.Empty, house.SupplementaryReferenceNumber);
				AssertEquals("Messages discarded", EDIMessage.Status.Discarded, message.EM_Status);
			});
		}

		public void TestRefreshDetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			house.CA_JS = shipment.PK;
			house.isCalledFromConsolPlugin = true;
			Factory.Save();
			house.CA_RL_NK_PortOfDestination = "CAYVR";

			manager.RefreshDetails();
			AssertEquals("CusSCAHouse details should be refreshed from Shipment", "AUSYD", house.CA_RL_NK_PortOfDestination);
		}

		public void TestCacheIsCleared()
		{
			house.CA_BGMReference = "XXXX";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = "AIR";
			var packLine = house.PackLines.AddNew();
			packLine.CV_CN = container.PK;
			packLine.CV_PackageCount = 1;
			packLine.CV_GoodsDescription = "xxx";
			Factory.Save();

			manager.SendMessage(MessageSubTypes.Create);
			AssertContains("Consignee Address 1: You have not entered a Consignee Address 1.", manager.Notification.ValidationErrorsMessage);

			house.CA_ConsigneeAddress1 = "Test";
			Factory.Save();
			manager.SendMessage(MessageSubTypes.Create);
			AssertNotContains("Consignee Address 1: You have not entered a Consignee Address 1.", manager.Notification.ValidationErrorsMessage);
		}

		#region Implementation

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			oceanBill = Factory.New<CusSCAOceanBill>();
			house = oceanBill.HouseBills.AddNew();
			return house;
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			manager = new SupplementaryCargoReportMessageManagerForTesting(house);
			return manager;
		}

		CusSCAOceanBill oceanBill;
		CusSCAHouse house;
		SupplementaryCargoReportMessageManagerForTesting manager;

		#region SupplementaryCargoReportMessageManagerForTesting

		class SupplementaryCargoReportMessageManagerForTesting : SupplementaryCargoReportMessageManager
		{
			public SupplementaryCargoReportMessageManagerForTesting(ISupplementaryCargoReport supplementaryCargoReport)
				: base(supplementaryCargoReport, new TestMessageInstructionUserNotification())
			{
			}

			public bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
			{
				return CanSendThisMessage(actionCode, out messageText);
			}

			protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
			{
				messageText = string.Empty;
				return OverrideCanSendThisMessage || base.CanSendThisMessage(actionCode, out messageText);
			}

			public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
			{
				return GetMessageBuilder(actionCode);
			}

			public bool OverrideCanSendThisMessage { private get; set; }

			public TestMessageInstructionUserNotification Notification
			{
				get { return ((TestMessageInstructionUserNotification)notification); }
			}
		}

		#endregion

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			Assert(true);
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}
		#endregion
	}
}
