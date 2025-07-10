using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoConsolMenuTest : TestCaseWithFactory
	{
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
			using (var plugIn = new SeaCargoConsolPlugIn(consol))
			using (var menu = new SeaCargoConsolMenuForTest(plugIn, new CusSCAOceanBillMessageManager(oceanBill)))
			{
				menu.OnPopup();
				var refreshMenuItem = SeaCargoShipmentMenuTest.GetMenuItemByName(menu, "&Refresh Sea Cargo Data");
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

		public void TestSetVisibilityCMR()
		{
			var consol = CreateImportConsol();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			using (var plugIn = new SeaCargoConsolPlugIn(consol))
			using (var menu = new SeaCargoConsolMenuForTest(plugIn, new CusSCAOceanBillMessageManager(oceanBill)))
			{
				menu.OnPopup();
				AssertEquals("SendMessages.Visible", true, menu.SendMessages.Visible);
				AssertEquals("SendMessages.Visible", true, menu.WithdrawMessages.Visible);
				AssertEquals("SendMessages.Visible", true, menu.ResetToOriginal.Visible);
				AssertEquals("MenuItemRefreshSeaCargoData.Visible", true, menu.menuItemRefreshSeaCargoData.Visible);
			}
		}

		public void TestSetVisibilityLegacy()
		{
			using (var menu = new SeaCargoConsolMenuForTest(null, new CusSCAOceanBillMessageManager(null, null)))
			{
				menu.OnPopup();
				AssertEquals("SendMessages.Visible", false, menu.SendMessages.Visible);
				AssertEquals("SendMessages.Visible", false, menu.WithdrawMessages.Visible);
				AssertEquals("SendMessages.Visible", false, menu.ResetToOriginal.Visible);
				AssertEquals("MenuItemRefreshSeaCargoData.Visible", true, menu.menuItemRefreshSeaCargoData.Visible);
			}
		}

		public void TestContingencyMenuItem()
		{
			using (var menu = new SeaCargoConsolMenuForTest(null, new CusSCAOceanBillMessageManager(null, null)))
			{
				menu.OnPopup();
				AssertCollectionContains(menu.seaCargoReportContingencyMenuItem, menu.MenuItems);
			}
		}

		public void TestExportSeaCargoContainersDataMenuItem()
		{
			using (var menu = new SeaCargoConsolMenuForTest(null, new CusSCAOceanBillMessageManager(null, null)))
			{
				menu.OnPopup();
				AssertCollectionContains(menu.exportSeaCargoContainersDataMenuItem, menu.MenuItems);
			}
		}

		public void TestConcurrencyUnderbond()
		{
			Factory.RefreshEnabled = false;
			var consol = CreateImportConsol();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var mockManager = new Mock<CusSCAOceanBillMessageManager>(oceanBill) { CallBase = true };
			mockManager.Setup(m => m.SendOriginalMessages(It.IsAny<ISendsMessagesToCustoms>()))
				.Returns(new[] { Factory.NewWithValidTestData<EDIMessage>() });
			using (var plugIn = new SeaCargoConsolPlugIn(consol))
			using (var menu = new SeaCargoConsolMenuForTest(plugIn, mockManager.Object))
			{
				menu.OnPopup();
				var underbondMentItem = menu.menuItemSendCMRUnderbondRequests;
				AssertNotNull("Failed to get Menu Item", underbondMentItem);
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				consol2.JK_BookingReference = "AAA";
				factory2.Save();
				consol.JK_BookingReference = "BBB";
				underbondMentItem.PerformClick();
				Assert("Consol should still have changes and no exception thrown", consol.HasChanges);
			}
		}

		ForwardingConsol CreateImportConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_MasterBillNum = "OCL789789234";
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			result.JK_RL_NKLoadPort = "HKHKG";
			result.JK_RL_NKDischargePort = "AUSYD";
			return result;
		}

		sealed class SeaCargoConsolMenuForTest : SeaCargoConsolMenu
		{
			public SeaCargoConsolMenuForTest(SeaCargoConsolPlugIn plugin, CusSCAOceanBillMessageManager manager) : base(plugin, manager)
			{
			}

			internal void OnPopup() => OnPopup(EventArgs.Empty);

			internal MenuItem SendMessages => sendMessages;

			internal MenuItem WithdrawMessages => withdrawMessages;

			internal MenuItem ResetToOriginal => resetToOriginal;
		}
	}
}
