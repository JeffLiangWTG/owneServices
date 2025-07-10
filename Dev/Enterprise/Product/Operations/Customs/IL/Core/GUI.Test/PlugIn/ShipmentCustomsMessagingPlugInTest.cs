using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class ShipmentCustomsMessagingPlugInTest : CustomsMessagingPlugInTestBase
	{
		public void TestCustomsMessagingMenuItems_AllEnabled()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var plugin = GetPlugInToTest())
			{
				var customsMessagingPlugInMenu = plugin.TopLevelMenu;
				AssertNotNull("TopLevelMenu exist", customsMessagingPlugInMenu);
				AssertType<ZMenuItem>(customsMessagingPlugInMenu);
				AssertEquals("the menu text", MainMenu, customsMessagingPlugInMenu.Text);
				AssertEquals("Menu items count", 2, customsMessagingPlugInMenu.MenuItems.Count);

				var deliveryOrderMenu = customsMessagingPlugInMenu.MenuItems[0];
				AssertNotNull("deliveryOrderMenu exist", deliveryOrderMenu);
				AssertType<ZMenuItem>(deliveryOrderMenu);
				AssertEquals("the DeliveryOrderMenu text", DeliveryOrderMenu, deliveryOrderMenu.Text);
				AssertContainsExactElementsInAnyOrder("There should be expected menu items", new[] { NoMessagesMenu, DeliveryOrderSendMenu }, deliveryOrderMenu.MenuItems.ToList<ZMenuItem>().Select(s => s.Caption.GetUnresolvedString()));

				var gatepassMovementMenu = customsMessagingPlugInMenu.MenuItems[1];
				AssertNotNull("gatepassMovementMenu exist", gatepassMovementMenu);
				AssertType<ZMenuItem>(gatepassMovementMenu);
				AssertEquals("the GatepassMovementMenu text", GatepassMovementMenu, gatepassMovementMenu.Text);
				AssertContainsExactElementsInAnyOrder("There should be expected menu items (Reload DocumentsComplete.xml from file into DB)", new[] { NoMessagesMenu, GatepassMovementSendMenu }, gatepassMovementMenu.MenuItems.ToList<ZMenuItem>().Select(s => s.Caption.GetUnresolvedString()));
			}
		}

		public void TestCustomsMessagingMenuItems_WhenILGatePassMovementsIsDisable()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var plugin = GetPlugInToTest())
			{
				var customsMessagingPlugInMenu = plugin.TopLevelMenu;
				AssertNotNull("TopLevelMenu exist", customsMessagingPlugInMenu);
				AssertType<ZMenuItem>(customsMessagingPlugInMenu);
				AssertEquals("The menu text", MainMenu, customsMessagingPlugInMenu.Text);
				AssertEquals("When ILGatePassMovements Is Disable, GatePassMovementSendMenu Is Not Available, and Menu items count", 1, customsMessagingPlugInMenu.MenuItems.Count);

				var deliveryOrderMenu = customsMessagingPlugInMenu.MenuItems[0];
				AssertNotNull("DeliveryOrderMenu exist", deliveryOrderMenu);
				AssertType<ZMenuItem>(deliveryOrderMenu);
				AssertEquals("The DeliveryOrderMenu text", DeliveryOrderMenu, deliveryOrderMenu.Text);
			}
		}

		public void TestMenuItemVisibility()
		{
			using (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.SetTemporaryValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				const string DeliveryOrderPath = "Customs Messaging|Delivery Order";
				const string SendDeliveryOrderPath = DeliveryOrderPath + "|Send Delivery Order";
				const string GatepassMovementPath = "Customs Messaging|Gatepass Movement";
				const string SendGatepassMovementPath = GatepassMovementPath + "|Send Gatepass Movement";
				var factory = Factory;
				var shipment = factory.NewWithValidTestData<ForwardingShipment>();
				var israeliConsignee = factory.NewWithValidTestData<OrgHeader>();
				israeliConsignee.OH_RL_NKClosestPort = "ILASH";
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = israeliConsignee.PK;

				var italianConsignee = factory.NewWithValidTestData<OrgHeader>();
				italianConsignee.OH_RL_NKClosestPort = "ITMIL";

				using (var form = new FormForTest(shipment))
				{
					form.Show();

					CombineAssertions("When CountryCode == Israel, And transportMode not in (Road,Sea)", () =>
					{
						AssertMenuItemVisible(form, DeliveryOrderPath, false);
						AssertMenuItemVisible(form, SendDeliveryOrderPath, false);

						AssertMenuItemVisible(form, GatepassMovementPath, true);
						AssertMenuItemVisible(form, SendGatepassMovementPath, true);
					});

					shipment.JS_TransportMode = "ROA";
					CombineAssertions("When CountryCode == Israel, And transportMode in (Road,Sea)", () =>
					{
						AssertMenuItemVisible(form, DeliveryOrderPath, true);
						AssertMenuItemVisible(form, SendDeliveryOrderPath, true);

						AssertMenuItemVisible(form, GatepassMovementPath, true);
						AssertMenuItemVisible(form, SendGatepassMovementPath, true);
					});

					shipment.ConsigneeDocumentaryAddress.OrganisationPK = italianConsignee.PK;
					CombineAssertions("When Country != Israel", () =>
					{
						AssertMenuItemVisible(form, DeliveryOrderPath, false);
						AssertMenuItemVisible(form, SendDeliveryOrderPath, false);

						AssertMenuItemVisible(form, GatepassMovementPath, false);
						AssertMenuItemVisible(form, SendGatepassMovementPath, false);
					});
				}
			}
		}

		public void TestMenuItemEnabled()
		{
			var factory = Factory;
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			var consignee = factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			using (var plugIn = new ShipmentCustomsMessagingPlugIn(shipment))
			{
				AssertEquals(true, plugIn.Enabled);
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertEquals(false, plugIn.Enabled);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new ShipmentCustomsMessagingPlugIn(Factory.NewWithValidTestData<ForwardingShipment>());

		protected override ICustomsMessagingPlugInBaseForTest GetPlugInForTest() => new ShipmentCustomsMessagingPlugInBaseForTest(Factory.NewWithValidTestData<ForwardingShipment>());

		const string GatepassMovementMenu = "Gatepass Movement";

		const string GatepassMovementSendMenu = "Send Gatepass Movement";

		const string DeliveryOrderMenu = "Delivery Order";

		const string DeliveryOrderSendMenu = "Send Delivery Order";

		const string NoMessagesMenu = "No Messages Available";
	}

	public sealed class ShipmentCustomsMessagingPlugInBaseForTest : ShipmentCustomsMessagingPlugIn, ICustomsMessagingPlugInBaseForTest
	{
		public ShipmentCustomsMessagingPlugInBaseForTest(ForwardingShipment shipment) : base(shipment)
		{
		}

		string ICustomsMessagingPlugInBaseForTest.Name => base.Name;

		bool ICustomsMessagingPlugInBaseForTest.CanDelete => base.CanDelete;

		bool ICustomsMessagingPlugInBaseForTest.HasUserControl => base.HasUserControl;

		LicenceCheckpoint ICustomsMessagingPlugInBaseForTest.LicenceCheckPoint => base.LicenceCheckPoint;

		IBusiness ICustomsMessagingPlugInBaseForTest.BusinessEntity => base.BusinessEntity;
	}
}
