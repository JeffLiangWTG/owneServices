using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.IL.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IL.GUI
{
	public class ShipmentCustomsMessagingPlugIn : CustomsMessagingPlugInBase
	{
		public ShipmentCustomsMessagingPlugIn(ForwardingShipment shipment) : base(shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			shipment.ConsigneeDocumentaryAddressValueChanged += OnChangeTheVisibilityRequired;
			shipment.TransportModeValueChanged += OnChangeTheVisibilityRequired;
			ChangeTheVisibility();
		}

		protected override IEnumerable<MenuItem> GetMenuItems()
		{
			var deliveryOrderMenuItem = new ZMenuItem(DeliveryOrderName);
			deliveryOrderMenuItem.AddFormsMenuItems(shipment, ModuleIdentifier, CreateSendDeliveryOrderMenuItemInfos());

			yield return deliveryOrderMenuItem;

			if (ILCustomsDataRegistry.Instance.EnableILGatePassMovements.Value)
			{
				var gatePassMovementMenuItem = new ZMenuItem(GatepassMovementName);
				gatePassMovementMenuItem.AddFormsMenuItems(shipment, ModuleIdentifier, CreateSendGatePassMovementMenuItemInfos());

				yield return gatePassMovementMenuItem;
			}
		}

		protected override bool IsMenuItemVisible(MenuItem subMenuItem, bool visible)
		{
			var isDeliveryOrderNotSeaOrRoad = subMenuItem.Text == DeliveryOrderName && !(shipment.IsSea || shipment.IsRoad);
			return !isDeliveryOrderNotSeaOrRoad && visible;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				shipment.ConsigneeDocumentaryAddressValueChanged -= OnChangeTheVisibilityRequired;
				shipment.TransportModeValueChanged -= OnChangeTheVisibilityRequired;
			}
		}

		protected override bool IsEnabled => shipment.ConsigneeDocumentaryAddress?.Country?.Code.ToString() == Core.Constants.CountryCodes.Israel;

		protected override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobShipment;

		IEnumerable<IMenuItemInfo> CreateSendDeliveryOrderMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = ShipmentSystemFormMenuItems.SendDeliveryOrderPK
			};
		}

		IEnumerable<IMenuItemInfo> CreateSendGatePassMovementMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = ShipmentSystemFormMenuItems.SendGatePassMovementPK
			};
		}

		static string DeliveryOrderName => ResString.GetMultilingualString("7FBC6183-923A-4A14-8F56-13B19857664C", "Delivery Order");
		static string GatepassMovementName => ResString.GetMultilingualString("8D5BB593-ECE0-4A24-9B72-E2D1BA0260B2", "Gatepass Movement");

		readonly ForwardingShipment shipment;
	}
}
