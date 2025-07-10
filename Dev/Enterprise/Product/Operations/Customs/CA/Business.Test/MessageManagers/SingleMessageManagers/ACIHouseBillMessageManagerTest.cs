using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(ACIHouseBillMessageManager))]
	sealed class ACIHouseBillMessageManagerTest : CAEManifestForwarderMessageManagerTestCase<ACIHouseBillMessageManager>
	{
		// TODO: implement with console plugin
		//public void TestLogCustomsCommenced()
		//{
		//	var shipment = Factory.New<ForwardingShipment>();
		//	house = Factory.New<CusSCAHouse>();
		//	house.CA_JS = shipment.PK;
		//	manager = new ACIHouseBillMessageManagerForTesting(house);
		//	manager.OverrideCanSendThisMessage = true;
		//	manager.SendMessage(MessageSubTypes.Undefined, false);

		//	var commencedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "ACI");
		//	AssertNotNull("Customs Commenced event should exist on shipment", commencedEvent);
		//}

		public override void TestBusinessObject()
		{
			AssertEquals(dataProvider.TopLevelBusinessObject, manager.BusinessObject);
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder", typeof(ACIHouseBillMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "ACI eManifest House Bill Report for CCN HOUSECCN", manager.MessageFriendlyName);
		}

		public void TestGetNotifications()
		{
			house.MasterBill.RunPreSaveValidation();
			var expectedNotifications = @"eManifest House - HOUSECCN: You must enter Consignee address.
eManifest House - HOUSECCN: You must enter Shipper address.
CBSA Release Port: You have not entered a value.
CBSA Release Sub Location: You have not entered a value.
House Bill: You have not entered a House Bill.
Weight UQ: You have not entered Weight UQ.
CBSA Carrier Code: You have not entered a value.
Mode Of Transport: You have not entered a value.
Primary CCN: You have not entered a value.

This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.
As Job not yet saved, Please save before sending.
";
			AssertMultilineASCIIEquals("Notifications should also include Header notifications", expectedNotifications, manager.GetNotificationsForSendingAnOriginal().NotificationsAsString());
		}

		public void TestGetCommonNotificationsForSending()
		{
			var mQwarningText = CAMessageManager.MQWarningMessage;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP");
				manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
				Assert("should contain mQwarningText", manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));

				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW");
				manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
				Assert("should not contain mQwarningText", !manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
				Assert("should not contain mQwarningText", !manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));
			}
		}

		public void TestGetCommonNotificationsForSending_WeightIsZero()
		{
			manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
			Assert(!manager.GetCommonNotificationsForSending_Exposed.ContainsError(CusCAeMHHouseValidation.WeightIsMandatory));
			house.BW_Weight = 0m;
			Assert(manager.GetCommonNotificationsForSending_Exposed.ContainsError(CusCAeMHHouseValidation.WeightIsMandatory));
		}

		public override void TestPopulateMessages()
		{
			Assert("Pre-condition", dataProvider.Messages.Count == 0);
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Undefined);
			Assert("One message created", dataProvider.Messages.Count == 1);
			AssertEquals("Message Status", MessageStatusList.Codes.AwaitingOriginal, dataProvider.MessageStatus);
			dataProvider.JobStatus = EManifestForwarderJobStatusList.Codes.Error;
			manager.SendMessage(MessageSubTypes.Undefined);
			Assert("One more message created", dataProvider.Messages.Count == 2);
			AssertEquals("Message Status", MessageStatusList.Codes.AwaitingChange, dataProvider.MessageStatus);
		}

		public override void TestCanSendThisMessage()
		{
			ZString messageText;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(messageText.Contains("Job not yet saved, Please save before sending."));
			Factory.Save();
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertContains("there are no valid Packing Lines entered for this job, please check the packing lines.", messageText);
			house.Items.AddNew();
			Factory.Save();
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
			Assert(messageText.Contains("the House Bill has not been reported yet."));
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Factory.Save();
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
		}

		[TestDate(2013, 8, 15, 10, 30, 25)]
		public void TestDefineActionCode()
		{
			MessageSubTypes actionCode = MessageSubTypes.Undefined;

			Assert(manager.DefineActionCodeIfUndefinedExposed(ref actionCode));
			AssertEquals(MessageSubTypes.Create, actionCode);

			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			actionCode = MessageSubTypes.Undefined;

			Assert(manager.DefineActionCodeIfUndefinedExposed(ref actionCode));
			AssertEquals(MessageSubTypes.Change, actionCode);

			house.MasterBill.BP_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			house.MasterBill.BP_ATA = ZDateTime.Now.AddHours(1);
			actionCode = MessageSubTypes.Undefined;

			Assert(manager.DefineActionCodeIfUndefinedExposed(ref actionCode));
			AssertEquals(MessageSubTypes.Change, actionCode);

			var message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = house.PK;
				stmAlog.SL_Table = house.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			house.MessagesForDisplay.Reload(true);

			actionCode = MessageSubTypes.Undefined;

			Assert(manager.DefineActionCodeIfUndefinedExposed(ref actionCode));
			AssertEquals(MessageSubTypes.Request, actionCode);
		}

		public void TestResetToOriginal()
		{
			var message = (EDIMessage)dataProvider.Messages.AddNew();
			dataProvider.JobStatus = EManifestForwarderJobStatusList.Codes.Error;
			dataProvider.MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals("Pre-condition", 0, house.Logs.GetAllLogs().Count);
			Assert("Pre-condition", ZString.Empty != dataProvider.JobStatus);
			Assert("Pre-condition", ZString.Empty != dataProvider.MessageStatus);
			Assert("Pre-condition", EDIMessage.Status.Discarded != message.EM_Status);
			manager.ResetToOriginal(notification);
			AssertEquals("1 log added", 1, house.Logs.GetAllLogs().Count);
			AssertEquals("Shipment status cleared", ZString.Empty, dataProvider.JobStatus);
			AssertEquals("Message status cleared", ZString.Empty, dataProvider.MessageStatus);
			AssertEquals("Messages discarded", EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestRefreshDetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB1";
			house.BW_ParentID = shipment.PK;
			house.BW_HouseBill = "NO1";

			manager.RefreshDetails();
			AssertEquals("CusSCAHouse details should be refreshed from Shipment", "HB1", house.BW_HouseBill);
		}

		public void TestGetNotificationsForSendingAWithdrawal()
		{
			var ccn = house.BW_HouseCCN;
			house.BW_IsCloseReported = true;
			GetMessageManager();
			Assert("True", manager.GetNotificationsForSendingAWithdrawal().ContainsWarning(string.Format(
				"The House Bill CCN: {0} is already marked as closed in a close report, are you sure you want to include this CCN in the close report?", ccn)));
		}

		public void TestIncludeHouseBillCCNInEDIMessage()
		{
			house.BW_HouseCCN = "IAN TEST 12 ";
			house.Items.AddNew();
			Factory.Save();

			manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
			manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals(1, dataProvider.Messages.Count);
			AssertEquals("IANTEST12", dataProvider.Messages[0].EM_ApplicationReference);
		}

		public void TestMessageManager_DoesNotCollectMessageErrorsFromChildren()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var dataProvider = master.HouseBills.AddNew();

			var houseToInclude = dataProvider;
			houseToInclude.AddRowMessageError("This message error should be included.");

			var houseToIgnore = master.HouseBills.AddNew();
			houseToIgnore.AddRowMessageError("This message error should not be included.");

			Assert("Precondition: both houses have message errors.", houseToInclude.HasMessageErrors);
			Assert("Precondition: both houses have message errors.", houseToIgnore.HasMessageErrors);

			manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
			var messageErrors = manager.GetMessageErrors_Exposed;

			var messageError = messageErrors.Single();
			AssertEquals("eManifest House - : This message error should be included.", messageError.Message);
		}

		#region Implementation

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var master = Factory.New<CusCAeMHMaster>();
			dataProvider = master.HouseBills.AddNew();
			house = dataProvider as CusCAeMHHouse;
			house.BW_HouseCCN = "HOUSECCN";
			house.BW_Weight = 1;
			return dataProvider;
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			manager = new ACIHouseBillMessageManagerForTesting(dataProvider);
			return manager;
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			Assert(true);
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}

		IACIHouseBillProvider dataProvider;
		CusCAeMHHouse house;
		ACIHouseBillMessageManagerForTesting manager;

		#region ACIHouseBillMessageManagerForTesting

		class ACIHouseBillMessageManagerForTesting : ACIHouseBillMessageManager
		{
			public ACIHouseBillMessageManagerForTesting(IACIHouseBillProvider houseBillProvider)
				: base(houseBillProvider, new TestUserNotification())
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

			public MessageSendingNotificationCollection GetCommonNotificationsForSending_Exposed => base.GetCommonNotificationsForSending();

			public bool DefineActionCodeIfUndefinedExposed(ref MessageSubTypes actionCode)
			{
				return DefineActionCodeIfUndefined(ref actionCode);
			}

			public Enterprise.Messaging.Business.EDIMessage[] PopulateMessage_Exposed(MessageSubTypes actionCode)
			{
				return PopulateMessage(actionCode);
			}

			public IEnumerable<INotification> GetMessageErrors_Exposed => base.GetMessageErrors();

			public bool OverrideCanSendThisMessage { private get; set; }
		}

		#endregion

		#endregion
	}
}
