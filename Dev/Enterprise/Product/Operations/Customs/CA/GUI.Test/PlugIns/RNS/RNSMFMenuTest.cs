using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSMFMenuTest : TestCaseWithFactory
	{
		public void TestLoadListMenuItemsVisibility()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var menu = new RNSMFMenuForTesting(new RNSPlugInSupportLoadListWrapper(loadList));

			Assert("Send messages menu should be visible", menu.SendMessagesMenuItem.Visible);
			Assert("Withdraw messages menu should be invisible", !menu.WithdrawMessagesMenuItem.Visible);
			Assert("Reset to original menu should be invisible", !menu.ResetToOriginalMenuItem.Visible);

			AssertEquals("Send messages menu text", "Send RNS Status Request", menu.SendMessagesMenuItem.Text);

			AssertNotNull("Attach/Create Shipments from Forwarded Manifests menu should be visible", menu.MenuItems.FindByText("Attach/Create Shipments from Forwarded Manifests"));
		}

		public void TestTallyMenuItemsVisibility()
		{
			var tally = Factory.New<TallyContainer>();
			var menu = new RNSMFMenuForTesting(new RNSPlugInSupportTallyWrapper(tally));

			Assert("Send messages menu should be visible", menu.SendMessagesMenuItem.Visible);
			Assert("Withdraw messages menu should be invisible", !menu.WithdrawMessagesMenuItem.Visible);
			Assert("Reset to original menu should be invisible", !menu.ResetToOriginalMenuItem.Visible);

			AssertEquals("Send messages menu text", "Send RNS Status Request", menu.SendMessagesMenuItem.Text);
			AssertNotNull("Send Arrival Certification for Arrived Cargo menu should be visible", menu.MenuItems.FindByText("Send Arrival Certification for Arrived Cargo"));
		}

		public void TestCreateShipmentsWithWithLoadListChanged()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var rnsMFMenu = new RNSMFMenuForTesting(new RNSPlugInSupportLoadListWrapper(loadList));
			var menuItem = rnsMFMenu.MenuItems.FindByText("Attach/Create Shipments from Forwarded Manifests");

			loadList.HasChanges = true;

			menuItem.PerformClick();

			AssertEquals("Save changes prompt", string.Format("Please save {0} before attach/create shipments from Forwarded Manifests.", loadList.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAttachAndCreateShipmentsWithNothingToDo()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var rnsMFMenu = new RNSMFMenuForTesting(new RNSPlugInSupportLoadListWrapper(loadList));
			var menuItem = rnsMFMenu.MenuItems.FindByText("Attach/Create Shipments from Forwarded Manifests");

			menuItem.PerformClick();

			AssertEquals("No matching Forwarded Manifests prompt", "There are no new matching Forwarded Manifests.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendArrivalCertificationForArrivedCargo()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			var tally = Factory.New<TallyContainer>();
			var shipment1 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment1);
			shipment1.OuterPackLines.AddNew().JL_Outturn = 10;
			shipment1.JS_RL_NKOrigin = "USAAA";
			shipment1.JS_RL_NKDestination = "CABBB";
			var shipment2 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment2);
			shipment2.OuterPackLines.AddNew().JL_Outturn = 10;
			shipment2.JS_RL_NKOrigin = "USAAA";
			shipment2.JS_RL_NKDestination = "CABBB";
			Factory.Save();

			var wrapper = new RNSPlugInSupportTallyWrapperForTesting(tally);
			var rnsMFMenu = new RNSMFMenuForTesting(wrapper);
			var menuItem = rnsMFMenu.MenuItems.FindByText("Send Arrival Certification for Arrived Cargo");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object dialog) =>
			{
				if (dialog is MessagesChooserDialog dialog1)
				{
					dialog1.SendPressed = true;
				}
				if (dialog is RNSRequestForm)
				{
					var rnsRequestBO = (RNSRequestBO)((ZForm)dialog).BusinessEntity;
					rnsRequestBO.DateOfArrival = new ZDateTime(2015, 8, 4);
					rnsRequestBO.OfficeCode = "0497";
					rnsRequestBO.SubLocationCode = "3252";
				}
			});
			menuItem.PerformClick();
			Assert("A RNSRequestForm should be showed", ZFormModaliser.LastFormShownDialogForTest is RNSRequestForm);
			AssertEquals("Should have created 2 SingleMessageManager", 2, wrapper.RNSMultiMessageManager.MessagesToSendManagers_Exposed.Length);
			var rnsSMessageManager1 = (RNSMessageManager)wrapper.RNSMultiMessageManager.MessagesToSendManagers_Exposed[0];
			AssertEquals("DateOfArrival", new ZDateTime(2015, 8, 4), rnsSMessageManager1.DataWrapper.DateOfArrival);
			AssertEquals("OfficeCode", "0497", rnsSMessageManager1.DataWrapper.OfficeCode);
			var rnsSMessageManager2 = (RNSMessageManager)wrapper.RNSMultiMessageManager.MessagesToSendManagers_Exposed[1];
			AssertEquals("DateOfArrival", new ZDateTime(2015, 8, 4), rnsSMessageManager2.DataWrapper.DateOfArrival);
			AssertEquals("OfficeCode", "0497", rnsSMessageManager2.DataWrapper.OfficeCode);
			AssertEquals("Should have sent 2 messages", ediMessageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		public void TestSendRNSStatusRequest()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			var tally = Factory.New<TallyContainer>();
			var shipment1 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment1);
			shipment1.OuterPackLines.AddNew().JL_Outturn = 10;
			shipment1.JS_RL_NKOrigin = "USAAA";
			shipment1.JS_RL_NKDestination = "CABBB";
			var shipment2 = tally.PackUnpackShipments.AddNew();
			CFSShipmentRNSStatusProviderTest.AddREJMessages(shipment2);
			shipment2.OuterPackLines.AddNew().JL_Outturn = 10;
			shipment2.JS_RL_NKOrigin = "USAAA";
			shipment2.JS_RL_NKDestination = "CABBB";
			Factory.Save();

			var wrapper = new RNSPlugInSupportTallyWrapperForTesting(tally);
			var rnsMFMenu = new RNSMFMenuForTesting(wrapper);
			var menuItem = rnsMFMenu.MenuItems.FindByText("Send RNS Status Request");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object dialog) =>
			{
				if (dialog is MessagesChooserDialog)
				{
					((MessagesChooserDialog)dialog).SendPressed = true;
					var chooser = (MessageChooserNonPersistent)((MessagesChooserDialog)dialog).DataSource;
					chooser.SelectAll();
				}
			});
			menuItem.PerformClick();
			Assert("RNSRequestForm should not be showed", ZFormModaliser.LastFormShownDialogForTest is MessagesChooserDialog);
			AssertEquals("Should have created 2 SingleMessageManager", 2, wrapper.RNSMultiMessageManager.MessagesToSendManagers_Exposed.Length);
			AssertEquals("Should have sent 2 messages", ediMessageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		sealed class RNSPlugInSupportTallyWrapperForTesting : IRNSPlugInSupport
		{
			public RNSPlugInSupportTallyWrapperForTesting(TallyContainer container)
			{
				this.container = container;
			}

			ZDateTime IRNSRequestData.DateOfArrival => ZDateTime.Now;

			ZString IRNSRequestData.CargoControlNumber => ZString.Empty;

			ZString IRNSRequestData.HouseBillNumber => ZString.Empty;

			ZString IRNSRequestData.TransactionNumber => ZString.Empty;

			ZString IRNSRequestData.OfficeCode => ZString.Empty;

			ZString IRNSRequestData.SubLocationCode => ZString.Empty;

			BusinessObject IRNSPlugInSupport.Master => container;

			Logs IRNSPlugInSupport.Logs => container.Logs;

			bool IRNSPlugInSupport.ReleaseStatusEventsSupported => true;

			bool IRNSPlugInSupport.PlugInVisible => true;

			event EventHandler IRNSPlugInSupport.PlugInVisibilityDataChanged
			{
				add { }
				remove { }
			}

			RNSMultiMessageManager IRNSPlugInSupport.GetRNSMultiMessageManager()
			{
				RNSMultiMessageManager = new RNSMultiMessageManagerForTesting(RNSParentTallyWrapper.Load(container));
				return RNSMultiMessageManager;
			}

			internal RNSMultiMessageManagerForTesting RNSMultiMessageManager;
			readonly TallyContainer container;
		}

		sealed class RNSMultiMessageManagerForTesting : RNSMultiMessageManager
		{
			public RNSMultiMessageManagerForTesting(IRNSRequestParent parent)
				: base(parent)
			{
			}

			protected override IList<Enterprise.Messaging.Business.EDIMessage> SendOriginal(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
			{
				MessagesToSendManagers_Exposed = messagesToSendManagers;
				return base.SendOriginal(sender, messagesToSendManagers);
			}

			internal SingleMessageManager[] MessagesToSendManagers_Exposed { get; private set; }
		}
	}
}
