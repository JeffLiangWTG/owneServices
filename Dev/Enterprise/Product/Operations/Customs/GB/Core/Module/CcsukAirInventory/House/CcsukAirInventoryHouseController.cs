using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryHouseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobShipment.ToString();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CcsukAirInventoryFormHouse(businessEntity as CusHAWB);
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AirCcsukHouseDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AirCcsukHouseEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukHouseNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukHouseView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventoryHouse; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusHAWB); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.GB.Module.Res.GetData("PlugInTabPage|CcsukAirInventoryHouse", "CCS-UK"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var shipment = (ForwardingShipment)businessEntity;
			return shipment.IsDirectShipment ? new CcsukShipmentMultiHawbPluginForDirectConsols(shipment) : new CcsukShipmentMultiHawbPlugin(shipment);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse; }
		}
	}
}
