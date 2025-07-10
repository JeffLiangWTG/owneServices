using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryHouseInShipmentController : CcsukAirInventoryHouseController
	{
		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobShipment.ToString();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var shipment = businessEntity as ForwardingShipment;
			var hawb = businessEntity as CusHAWB;
			if (hawb != null && hawb.Shipment != null)
			{
				shipment = hawb.Shipment;
			}
			if (shipment != null)
			{
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				var shipmentForm = new ShipmentForm(shipment);
				shipmentForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.GB.CcsukAirInventoryHouse;
				return shipmentForm;
			}
			return base.GetForm(businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusHAWB); }
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var result = factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK) ?? factory.Load(typeof(ForwardingShipment), sourceEntityPK);
			return result;
		}

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
			var deleteForm = controller.ShowDeleteForm(sourceEntity);
			LastShownForm = controller.LastShownForm;
			return deleteForm;
		}
	}
}
