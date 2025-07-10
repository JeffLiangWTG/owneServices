using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(ACIForwarderCloseMessageManager))]
	sealed class ACIForwarderCloseMessageManagerTest : CAEManifestForwarderMessageManagerTestCase<ACIForwarderCloseMessageManager>
	{
		public void TestRefreshDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			consol.JK_MasterBillNum = "MB1";

			master.BP_ParentID = consol.PK;
			master.BP_MasterBill = "NO1";
			var house = master.HouseBills.AddNew();
			house.BW_ParentID = shipment.PK;

			manager.RefreshDetails();
			AssertEquals("CusSCAHouse details should be refreshed from Shipment", "MB1", master.BP_MasterBill);
		}

		public override void TestBusinessObject()
		{
			AssertEquals(dataProvider.TopLevelBusinessObject, manager.BusinessObject);
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder", typeof(ACIForwarderCloseMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "ACI eManifest Forwarder Manifest Close Report for Previous CCN 8XXXPRIMARYCCN", manager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			var house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.BW_Weight = 1;
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

		public void TestDefineActionCodeIfUndefined()
		{
			var actionCode = MessageSubTypes.Undefined;
			AssertEquals("actionCode", MessageSubTypes.Undefined, actionCode);
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			master.BP_ATA = new ZDateTime(2021, 2, 1);
			manager.DefineActionCodeIfUndefinedExposed(ref actionCode);
			AssertEquals("actionCode", MessageSubTypes.Request, actionCode);
		}

		public void TestShouldSendWithAmendment()
		{
			AssertEquals("ShouldSendWithAmendment", false, manager.ShouldSendWithAmendment_Exposed);
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			master.BP_ATA = new ZDateTime(2021, 2, 1);
			AssertEquals("ShouldSendWithAmendment", true, manager.ShouldSendWithAmendment_Exposed);
		}

		public override void TestCanSendThisMessage()
		{
			ZString messageText;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(messageText.Contains("Job not yet saved, Please save before sending."));
			Factory.Save();
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertContains("there are no valid related house CCNs entered for this job, please check the house bills have been reported.", messageText);
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "999912356";
			house.Items.AddNew();
			Factory.Save();
			GetMessageManager();
			manager.SetHouseInstruction("999912356", true);
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertContains("not all the house bills have been accepted.", messageText);
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Factory.Save();
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
			Assert(messageText.Contains("the Manifest has not been reported yet."));

			manager.ActionPurpose = ActionPurpose.Change;
			Assert(!manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert(messageText.Contains("the Amendment reason and ATA are required for Amend message, please check them have been entered."));
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			master.BP_ATA = new ZDateTime(2021, 02, 01);
			Factory.Save();
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));

			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Factory.Save();
			Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
		}

		public void TestResetToOriginal()
		{
			var message = (EDIMessage)dataProvider.Messages.AddNew();
			dataProvider.JobStatus = EManifestForwarderJobStatusList.Codes.Error;
			dataProvider.MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			AssertEquals("Pre-condition", 0, master.Logs.GetAllLogs().Count);
			Assert("Pre-condition", ZString.Empty != dataProvider.JobStatus);
			Assert("Pre-condition", ZString.Empty != dataProvider.MessageStatus);
			Assert("Pre-condition", EDIMessage.Status.Discarded != message.EM_Status);
			manager.ResetToOriginal(notification);
			AssertEquals("1 log added", 1, master.Logs.GetAllLogs().Count);
			AssertEquals("Shipment status cleared", ZString.Empty, dataProvider.JobStatus);
			AssertEquals("Message status cleared", ZString.Empty, dataProvider.MessageStatus);
			AssertEquals("Messages discarded", EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestGetNotificationsForSendingAnOriginal()
		{
			var ccn = "999912356";
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn;
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.Items.AddNew();
			Factory.Save();
			master.Messages.RemoveAndDeleteAll();
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			manager.SetHouseInstruction(ccn, true);
			manager.GenerateOriginalMessages(master);
			Factory.Save();
			Assert(manager.GetNotificationsForSendingAnOriginal().ContainsWarning("There are messages waiting for responses, are you sure you wish to re-send now?"));

			GetMessageManager();
			manager.SetHouseInstruction(ccn, true);
			manager.GenerateOriginalMessages(master);
			Factory.Save();
			Assert(manager.GetNotificationsForSendingAnOriginal().ContainsError("There are messages waiting for responses, please do not send it again."));

			ccn = "999912357";
			house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn;
			Factory.Save();
			GetMessageManager();
			manager.SetHouseInstruction(ccn, true);
			Assert("True", manager.GetNotificationsForSendingAnOriginal().ContainsWarning(string.Format(
				"The House Bill CCN: {0} is not in an active customs status nor included in this sending batch, are you sure you want to include this CCN in the close report?", ccn)));
		}

		public void TestGetCommonNotificationsForSending()
		{
			var mQwarningText = CAMessageManager.MQWarningMessage;
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP");
				manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
				Assert("should contain mQwarningText", manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));

				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW");
				manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
				Assert("should not contain mQwarningText", !manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
				Assert("should not contain mQwarningText", !manager.GetCommonNotificationsForSending_Exposed.ContainsWarning(mQwarningText));
			}
		}

		public void TestGetCommonNotificationsForSending_WeightIsZero()
		{
			var ccn = "999912356";
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn;
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.Items.AddNew();
			Factory.Save();
			master.Messages.RemoveAndDeleteAll();
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			manager.GenerateOriginalMessages(master);
			Factory.Save();
			Assert(!manager.GetCommonNotificationsForSending_Exposed.ContainsError(CusCAeMHHouseValidation.WeightIsMandatory));
			manager.SetHouseInstruction(ccn, true);
			Assert(manager.GetCommonNotificationsForSending_Exposed.ContainsError(CusCAeMHHouseValidation.WeightIsMandatory));
		}

		public void TestGenerateOriginalMessagesCore()
		{
			var message = Factory.New<ACIForwarderCloseMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.NeedAutoCloseReport = true;
			message.EM_LinkedObject = master;
			Assert("NeedAutoCloseReport", message.NeedAutoCloseReport);
			GetMessageManager();
			manager.GenerateOriginalMessages(master);
			Assert("NeedAutoCloseReport", !message.NeedAutoCloseReport);

			manager.GenerateOriginalMessages(master);
			Assert("NeedAutoCloseReport", !message.NeedAutoCloseReport);
		}

		public void TestGetNotificationsForSendingAWithdrawal()
		{
			master.Messages.RemoveAndDeleteAll();
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			manager.GenerateWithdrawalMessages(master);
			Factory.Save();
			Assert(manager.GetNotificationsForSendingAWithdrawal().ContainsWarning("There are messages waiting for responses, are you sure you wish to re-send now?"));
			Assert(manager.GetNotificationsForSendingAWithdrawal().ContainsWarning("No Close message has been accepted yet, we strongly recommend not to sent a cancel message now."));

			GetMessageManager();
			manager.GenerateWithdrawalMessages(master);
			Assert(manager.GetNotificationsForSendingAWithdrawal().ContainsError("There are messages waiting for responses, please do not send it again."));
			Assert(!manager.GetNotificationsForSendingAWithdrawal().ContainsWarning("No Close message has been accepted yet, we strongly recommend not to sent a cancel message now."));
		}

		public void TestSendWithMessageErrors()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;

			var ccn = "999912356";
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn;
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.Items.AddNew();
			Factory.Save();
			master.Messages.RemoveAndDeleteAll();
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper, true);
			master.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			manager.SetHouseInstruction(ccn, true);
			manager.GenerateOriginalMessages(master);
			Factory.Save();
			Assert(manager.GetNotificationsForSendingAnOriginal().ContainsError(SingleMessageManager.MessageErrorsExistWithNoSecurityRight));

			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			Assert(!manager.GetNotificationsForSendingAnOriginal().ContainsError(SingleMessageManager.MessageErrorsExistWithNoSecurityRight));
		}

		#region Implementation

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "8XXXPRIMARYCCN";
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			dataProvider = closeWrapper;
			return dataProvider;
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			manager = new ACIForwarderCloseMessageManagerForTesting(closeWrapper);
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

		CusCAeMHMaster master;
		ACIForwarderCloseMessageManagerForTesting manager;
		IACIForwarderCloseProvider dataProvider;

		#region ACIForwarderCloseMessageManagerForTesting

		public class ACIForwarderCloseMessageManagerForTesting : ACIForwarderCloseMessageManager
		{
			public ACIForwarderCloseMessageManagerForTesting(IACIForwarderCloseProvider master)
				: base(master, new TestUserNotification())
			{
			}

			public ACIForwarderCloseMessageManagerForTesting(IACIForwarderCloseProvider master, bool isForce = false)
				: base(master, new TestUserNotification(), isForce)
			{
			}

			public bool ShouldSendWithAmendment_Exposed
			{
				get { return ShouldSendWithAmendment; }
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

			public bool OverrideCanSendThisMessage { private get; set; }

			public void SetHouseInstruction(string ccn, bool shouldSend)
			{
				foreach (var houseCCNInstruction in (this.DataWrapper as CusCAeMHMasterCloseWrapper).AllCCNs)
				{
					if (houseCCNInstruction.CCN == ccn)
					{
						houseCCNInstruction.IsShouldSend = shouldSend;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
