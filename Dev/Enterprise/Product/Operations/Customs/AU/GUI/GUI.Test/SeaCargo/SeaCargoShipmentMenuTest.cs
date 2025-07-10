using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoShipmentMenuTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (SeaCargoShipmentMenu menu = GetMenu())
			{
				AssertNotNull("Sea Cargo Menu", menu);
			}
		}

		public void TestMenuItems()
		{
			using (SeaCargoShipmentMenu menu = GetMenu())
			{
				try
				{
					typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
				}
				catch
				{
					// is only a test, so any error should not be a failure
				}

				foreach (string expectedMenuItemString in GetExpectedMenuItemStrings())
				{
					Assert(expectedMenuItemString + " was not part of menu", IsMenuIncluded(menu.MenuItems, expectedMenuItemString));
				}
			}
		}

		public void TestRefreshSeaCargoDataMenuItem()
		{
			var consol = CreateImportConsol();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "BILL1";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "BILL2";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill1 = oceanBill.HouseBills.AddNew();
			houseBill1.CA_JS = shipment1.PK;
			houseBill1.CA_HouseBill = shipment1.JS_HouseBill;
			var houseBill2 = oceanBill.HouseBills.AddNew();
			houseBill2.CA_JS = shipment2.PK;
			houseBill2.CA_HouseBill = shipment2.JS_HouseBill;
			using (var plugIn = new SeaCargoShipmentPlugIn(shipment1))
			using (var menu = new SeaCargoShipmentMenuForTest(plugIn, new CusSCAHouseMessageManager(houseBill1)))
			{
				menu.InitializeMenuInternal();
				var refreshMenuItem = GetMenuItemByName(menu, "&Refresh SeaCargo Data");
				AssertNotNull(refreshMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				refreshMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				houseBill1.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
				houseBill1.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				refreshMenuItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("House Bill BILL1 will not be synchronised, is currently Original Accepted\r\nSynchronise remaining House Bills?", UnitTestUserNotification.Instance.LastMessage.Text);
				houseBill2.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
				houseBill2.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				refreshMenuItem.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("House Bill BILL1 will not be synchronised, is currently Original Accepted\r\nHouse Bill BILL2 will not be synchronised, is currently Original Accepted\r\nNo House Bills are suitable for synchronising.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestContingencyMenuItem()
		{
			using (SeaCargoShipmentMenu menu = GetMenu())
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
				AssertCollectionContains(menu.seaCargoReportContingencyMenuItem, menu.MenuItems);
			}
		}

		public void TestConcurrencyOnPreAlert()
		{
			Factory.RefreshEnabled = false;
			ForwardingConsol consol = CreateImportConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Factory.Save();
			var mockManager = new Mock<CusSCAHouseMessageManager>(HouseBill) { CallBase = true };
			mockManager.Setup(m => m.SendOriginalMessages(It.IsAny<ISendsMessagesToCustoms>()))
				.Returns(new[] { Factory.NewWithValidTestData<EDIMessage>() });
			using (var menu = new SeaCargoShipmentMenuForTest(Plugin, mockManager.Object))
			{
				menu.InitializeMenuInternal();
				MenuItem preAlertMentItem = GetMenuItemByName(menu, "&Pre-Alert House");
				AssertNotNull("Failed to get Menu Item", preAlertMentItem);
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
				shipment2.JS_ActualWeight = 1m;
				factory2.Save();
				shipment.JS_ActualWeight = 2m;
				preAlertMentItem.PerformClick();
				Assert("Shipment should still have changes and no exception thrown", shipment.HasChanges);
			}
		}

		public void TestConcurrencySendUnderbond()
		{
			Factory.RefreshEnabled = false;
			ForwardingConsol consol = CreateImportConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Factory.Save();
			var mockManager = new Mock<CusSCAHouseMessageManager>(HouseBill) { CallBase = true };
			mockManager.Setup(m => m.SendOriginalMessages(It.IsAny<ISendsMessagesToCustoms>()))
				.Returns(new[] { Factory.NewWithValidTestData<EDIMessage>() });
			using (var menu = new SeaCargoShipmentMenuForTest(Plugin, mockManager.Object))
			{
				menu.InitializeMenuInternal();
				MenuItem preAlertMentItem = GetMenuItemByName(menu, "Send &Underbond Requests");
				AssertNotNull("Failed to get Menu Item", preAlertMentItem);
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
				shipment2.JS_ActualWeight = 1m;
				factory2.Save();
				shipment.JS_ActualWeight = 2m;
				preAlertMentItem.PerformClick();
				Assert("Shipment should still have changes and no exception thrown", shipment.HasChanges);
			}
		}

		public void TestMustHaveSecurityRightsToSeeMessagingAdminMenu()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			Env.Security.AUCustomsSCAImportMessagingAdmin.IsAllowed = false;
			ForwardingConsol consol = CreateImportConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm testForm = new ShipmentForm(shipment))
			{
				testForm.PlugInIDToSelectOnLoaded = ZArchitecture.Modules.ControllerIDs.Customs.AU.SeaCargo;
				testForm.Show();
				UserIdleWorker.Flush();
				SeaCargoShipmentPlugIn plugIn = (SeaCargoShipmentPlugIn)testForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.AU.SeaCargo);
				AssertNotNull("Failed to get Plug In", plugIn);
				AssertNull(GetMenuItemByName(testForm.Menu, "&Accept Current as Acknowledged"));
			}

			Env.Security.AUCustomsSCAImportMessagingAdmin.IsAllowed = true;
			AssertAcceptVisible(shipment);
			GlbStaff.CurrentUser.GS_IsController = true;
			Env.Security.AUCustomsSCAImportMessagingAdmin.IsAllowed = false;
			AssertAcceptVisible(shipment);
		}

		public void TestWhenSendMessageHVLVShipment_ThenDisplayWarning()
		{
			var consol = CreateImportConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_HouseBill = "BILL1";
			HouseBill.CA_JS = shipment.PK;
			HouseBill.CA_HouseBill = shipment.JS_HouseBill;
			using (var plugIn = new SeaCargoShipmentPlugIn(shipment))
			using (var menu = new SeaCargoShipmentMenuForTest(plugIn, new CusSCAHouseMessageManager(HouseBill)))
			{
				menu.OnPopupInternal(EventArgs.Empty);
				menu.SendMessagesInternal.PerformClick();
				AssertContains("In order to create the SeaCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV SeaCargo Report", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhenSendMessageNonHVLVShipment_ThenDoNotDisplayWarning()
		{
			var consol = CreateImportConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "BILL1";
			HouseBill.CA_JS = shipment.PK;
			HouseBill.CA_HouseBill = shipment.JS_HouseBill;
			using (var plugIn = new SeaCargoShipmentPlugIn(shipment))
			using (var menu = new SeaCargoShipmentMenuForTest(plugIn, new CusSCAHouseMessageManager(HouseBill)))
			{
				menu.OnPopupInternal(EventArgs.Empty);
				menu.SendMessagesInternal.PerformClick();
				AssertNotContains("In order to create the SeaCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV SeaCargo Report", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions()]
		public void TestMenuItemClicks()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			using (SeaCargoShipmentPlugIn plugIn = new SeaCargoShipmentPlugIn(shipment))
			{
				using (SeaCargoShipmentMenu menu = new SeaCargoShipmentMenu(plugIn, new CusSCAHouseMessageManager(null, null)))
				{
					menu.MenuItemPreAlertCMRHouseBill_Click(null, EventArgs.Empty);
					menu.MenuItemSendCMRUnderbondRequests_Click(null, EventArgs.Empty);
					menu.AcceptCurrentAsAcknowledgedMenuItem_Click(null, EventArgs.Empty);
				}
			}
		}

		public void TestMenuItemVisibility()
		{
			using (var menu = new SeaCargoShipmentMenuForTest(Plugin, new CusSCAHouseMessageManager(HouseBill)))
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });

				Assert("SendMessagesInternal", menu.SendMessagesInternal.Enabled);
				Assert("WithdrawMessagesInternal", menu.WithdrawMessagesInternal.Enabled);
				Assert("ResetToOriginalInternal", menu.ResetToOriginalInternal.Enabled);
			}
		}

		public void TestMenuItemVisibility_Legacy()
		{
			using (var menu = new SeaCargoShipmentMenuForTest(Plugin, new CusSCAHouseMessageManager(null)))
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });

				AssertEquals("SendMessagesInternal", false, menu.SendMessagesInternal.Enabled);
				AssertEquals("WithdrawMessagesInternal", false, menu.WithdrawMessagesInternal.Enabled);
				AssertEquals("ResetToOriginalInternal", false, menu.ResetToOriginalInternal.Enabled);
			}
		}

		void AssertAcceptVisible(ForwardingShipment shipment)
		{
			using (ShipmentForm testForm = new ShipmentForm(shipment))
			{
				testForm.PlugInIDToSelectOnLoaded = ZArchitecture.Modules.ControllerIDs.Customs.AU.SeaCargo;
				testForm.Show();
				UserIdleWorker.Flush();
				SeaCargoShipmentPlugIn plugIn = (SeaCargoShipmentPlugIn)testForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.AU.SeaCargo);
				AssertNotNull("Failed to get Plug In", plugIn);
				AssertNotNull(GetMenuItemByName(testForm.Menu, "&Accept Current as Acknowledged"));
			}
		}

		bool IsMenuIncluded(Menu.MenuItemCollection items, string expectedMenuItemString)
		{
			foreach (MenuItem item in items)
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, item, new object[] { EventArgs.Empty });
				if ((item.MenuItems != null && IsMenuIncluded(item.MenuItems, expectedMenuItemString)))
				{
					return true;
				}
				else if (item.Text == expectedMenuItemString)
				{
					return true;
				}
			}

			return false;
		}

		ArrayList GetExpectedMenuItemStrings()
		{
			ArrayList result = new ArrayList();
			result.Add("&Refresh SeaCargo Data");
			result.Add("&Messaging Admin");
			result.Add("&Accept Current as Acknowledged");
			return result;
		}

		SeaCargoShipmentMenu GetMenu() => new SeaCargoShipmentMenu(Plugin, new CusSCAHouseMessageManager(HouseBill));

		internal static MenuItem GetMenuItemByName(Menu menuToSearch, string menuText)
		{
			MenuItem result = null;
			if (menuToSearch != null)
			{
				foreach (MenuItem item in menuToSearch.MenuItems)
				{
					try
					{
						typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, item, new object[] { EventArgs.Empty });
					}
					catch
					{
					}

					if (item.Text == menuText)
					{
						result = item;
						break;
					}

					result = GetMenuItemByName(item, menuText);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		CusSCAHouse houseBill;
		CusSCAHouse HouseBill
		{
			get
			{
				if (houseBill == null)
				{
					var oceanBill = Factory.New<CusSCAOceanBill>();
					houseBill = oceanBill.HouseBills.AddNew();
				}

				return houseBill;
			}
		}

		SeaCargoShipmentPlugIn plugin;
		SeaCargoShipmentPlugIn Plugin => plugin ?? (plugin = new SeaCargoShipmentPlugIn(HouseBill));

		protected override void TearDown()
		{
			base.TearDown();
			plugin?.Dispose();
		}

		ForwardingConsol CreateImportConsol()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			result.JK_MasterBillNum = "OCL789789234";
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			result.JK_RL_NKLoadPort = "HKHKG";
			result.JK_RL_NKDischargePort = "AUSYD";
			Transport transport = result.Transports[0];
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty)).RV_Code;
			transport.JW_VoyageFlight = "4938";
			transport.JW_ETA = ZDateTime.Today.AddDays(2);
			CommonContainer container = result.Containers.AddNew();
			container.JC_ContainerNum = "NLFU3399201";
			return result;
		}

		sealed class SeaCargoShipmentMenuForTest : SeaCargoShipmentMenu
		{
			public SeaCargoShipmentMenuForTest(SeaCargoShipmentPlugIn plugin, CusSCAHouseMessageManager manager) : base(plugin, manager)
			{
			}

			internal void InitializeMenuInternal() => InitializeMenu();

			internal void OnPopupInternal(EventArgs e) => OnPopup(e);

			internal MenuItem SendMessagesInternal => sendMessages;

			internal MenuItem WithdrawMessagesInternal => withdrawMessages;

			internal MenuItem ResetToOriginalInternal => resetToOriginal;
		}
	}
}
