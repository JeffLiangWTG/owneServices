using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using CusHAWB = Enterprise.Customs.AU.Declaration.Business.CusHAWB;
using CusMAWB = Enterprise.Customs.AU.Declaration.Business.CusMAWB;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoShipmentMenuTest : TestCaseWithFactory
	{
		public void TestReleaseConsignmentsFromBondStore()
		{
			Registry.Business.HVLVDataRegistry.HasHVLVClearance = false;
			AssertNull(Menu.ReleaseConsignmentsFromBondStore);
			Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
			Env.Security.ReleaseConsignmentsFromBondStore.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu = null;
			Menu.OnPopup(EventArgs.Empty);
			Menu.ReleaseConsignmentsFromBondStore.PerformClick();
			AssertEquals(Env.Security.ReleaseConsignmentsFromBondStore.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			Env.Security.ReleaseConsignmentsFromBondStore.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu = null;
			Menu.OnPopup(EventArgs.Empty);
			Menu.ReleaseConsignmentsFromBondStore.PerformClick();
			AssertContains("Releasing Cleared Consignment from Bond Store completed", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!CusHAWB.CS_IsHeldAtOutturn);
		}

		public void TestWhenSendMessageHVLVShipment_ThenDisplayWarning()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			CusHAWB.CS_JS = shipment.PK;
			CusHAWB.CS_RL_NKDestination = "AUSYD";
			CusHAWB.CS_RL_NKOrigin = "NZAKL";
			Menu.OnPopup(EventArgs.Empty);
			Menu.SendMessages.PerformClick();
			AssertContains("In order to create the AirCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV AirCargo Report", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenSendMessageNonHVLVShipment_ThenDoNotDisplayWarning()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			CusHAWB.CS_JS = shipment.PK;
			CusHAWB.CS_RL_NKDestination = "AUSYD";
			CusHAWB.CS_RL_NKOrigin = "NZAKL";
			Menu.OnPopup(EventArgs.Empty);
			Menu.SendMessages.PerformClick();
			AssertNotContains("In order to create the AirCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV AirCargo Report", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWhenSendMessageNonShipment_NoExpectionThrown()
		{
			Assert("precondition : CusHAWB has no shipment", CusHAWB.CS_JS.IsEmpty);
			AssertNoExceptionThrown(() =>
			{
				Menu.OnPopup(EventArgs.Empty);
				Menu.SendMessages.PerformClick();
			});
		}

		public void TestWhenSendMessageNullHouseBill_NoExpectionThrown()
		{
			using (TestAirCargoShipmentMenu overriddenMenu = new TestAirCargoShipmentMenu(new CusHAWBMessageManager(null)))
			{
				AssertNull("housebill is null", overriddenMenu.Manager.HAWB);
				AssertNoExceptionThrown(() =>
				{
					overriddenMenu.OnPopup(EventArgs.Empty);
					overriddenMenu.SendMessages.PerformClick();
				});
			}
		}

		public void TestOverriddenNewDelegate()
		{
			TestAirCargoShipmentMenu.RegisterThisSubTypeOverride();
			using (AirCargoShipmentMenu overriddenMenu = AirCargoShipmentMenu.New(new CusHAWBMessageManager(() => CusHAWB)))
			{
				AssertEquals("New should be returning our overridden test class now", typeof(TestAirCargoShipmentMenu), overriddenMenu.GetType());
			}
		}

		public void TestTypedManagerProperty()
		{
			AssertNotNull("Protected Manager property should return the message manager", Menu.Manager);
		}

		// Air Cargo Menu is always visible
		public void TestExportMenu()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusHAWB.CS_JS = shipment.PK;
			CusHAWB.CS_RL_NKDestination = "AUSYD";
			CusHAWB.CS_RL_NKOrigin = "SGSIN";
			AssertEquals("Air Cargo Menu Visible", true, Menu.Visible);
		}

		public void TestVisibility()
		{
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals(true, Menu.SendMessages.Visible);
			AssertEquals(true, Menu.WithdrawMessages.Visible);
			AssertEquals(true, Menu.ResetToOriginal.Visible);
			AssertEquals(true, Menu.RefreshAirCargoDataMenuItem.Visible);
			AssertEquals(true, Menu.CreateContingencyMenuItem.Visible);
		}

		public void TestAddExtraMenusWithDeleteCusHAWB()
		{
			CusHAWB.Delete();
			AssertNoExceptionThrown(() => Menu.OnPopup(EventArgs.Empty));
		}

		CusHAWB cusHAWB;
		CusHAWB CusHAWB
		{
			get
			{
				if (cusHAWB == null)
				{
					var cusMAWB = Factory.New<CusMAWB>();
					cusHAWB = cusMAWB.ChildBills.AddNew();
				}

				return cusHAWB;
			}
		}

		TestAirCargoShipmentMenu menu;
		TestAirCargoShipmentMenu Menu
		{
			get
			{
				if (menu == null)
				{
					menu = new TestAirCargoShipmentMenu(MessageManager);
				}

				return menu;
			}
		}

		CusHAWBMessageManager messageManager;
		CusHAWBMessageManager MessageManager => messageManager ?? (messageManager = new CusHAWBMessageManager(() => CusHAWB));

		protected override void SetUp()
		{
			base.SetUp();
			cusHAWB = null;
			menu = null;
			messageManager = null;
		}

		protected override void TearDown()
		{
			base.TearDown();
			menu?.Dispose();
		}

		sealed class TestAirCargoShipmentMenu : AirCargoShipmentMenu
		{
			public TestAirCargoShipmentMenu(CusHAWBMessageManager manager) : base(manager)
			{
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
			}

			static AirCargoShipmentMenu OverriddenNew(CusHAWBMessageManager manager) => new TestAirCargoShipmentMenu(manager);

			public new void OnPopup(EventArgs e) => base.OnPopup(e);

			public new CusHAWBMessageManager Manager => base.Manager;

			public MenuItem SendMessages => sendMessages;

			public MenuItem AmendMessages => amendMessages;

			public MenuItem WithdrawMessages => withdrawMessages;

			public MenuItem ResetToOriginal => resetToOriginal;
		}
	}
}
