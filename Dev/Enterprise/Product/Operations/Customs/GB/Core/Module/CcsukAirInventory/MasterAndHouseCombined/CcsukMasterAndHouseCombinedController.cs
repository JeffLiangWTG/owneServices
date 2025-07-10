using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukMasterAndHouseCombinedController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override string GetIDForFormCache(IBusiness workerOrRealHouse)
		{
			return GetBusinessObjectForTopLevel(workerOrRealHouse) switch
			{
				ForwardingConsol => ControllerIDs.JobConsol.ToString(),
				ForwardingShipment => ControllerIDs.JobShipment.ToString(),
				CusMAWB => ControllerIDs.Customs.GB.CcsukAirInventory.ToString(),
				CusHAWB => ControllerIDs.Customs.GB.CcsukAirInventoryHouse.ToString(),
				_ => throw new ArgumentException($"Type of business object {workerOrRealHouse?.GetType()?.FullName ?? "null"} is not supported"),
			};
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var result = factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK);
			result ??= factory.Load(typeof(CusMAWB), sourceEntityPK);
			result ??= factory.Load(typeof(ForwardingShipment), sourceEntityPK);
			result ??= factory.Load(typeof(ForwardingConsol), sourceEntityPK);
			return result;
		}

		protected override IZForm GetForm(IBusiness workerOrRealHouse)
		{
			var bo = GetBusinessObjectForTopLevel(workerOrRealHouse);
			switch (bo)
			{
				case ForwardingConsol consol:
					var consolForm = new ConsolForm(consol);
					consolForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.GB.CcsukAirInventory;
					return consolForm;
				case ForwardingShipment shipment:
					ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
					var shipmentForm = new ShipmentForm(shipment);
					shipmentForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.GB.CcsukAirInventoryHouse;
					return shipmentForm;
				case CusMAWB mawb:
					return new CcsukAirInventoryForm(mawb);
				case CusHAWB hawb:
					return new CcsukAirInventoryFormHouse(hawb);
				default:
					throw new ArgumentException($"Type of business object {workerOrRealHouse?.GetType()?.FullName ?? "null"} is not supported");
			}
		}

		protected override Guid GetIDForBusinessEntity(IBusiness workerOrRealHouse)
		{
			var bo = GetBusinessObjectForTopLevel(workerOrRealHouse);
			return bo?.PK.ToGuid() ?? Guid.Empty;
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AirCcsukAllAWBs; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AirCcsukAllAWBs; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukAllAWBs; }  // irrelevant
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukAllAWBsView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukMasterAndHouseCombined; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusHAWB); }  // for initial load of BizO
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined; }
		}

		BusinessObject GetBusinessObjectForTopLevel(IBusiness bo)
		{
			switch (bo)
			{
				case ForwardingConsol consol:
					return consol;
				case ForwardingShipment shipment:
					return shipment;
				case CusMAWB mawb:
					return (BusinessObject)mawb.Consol ?? mawb;
				case CusHAWB hawb:
					if (hawb.CS_IsMasterHouse)
					{
						return (BusinessObject)hawb.MAWB.Consol ?? hawb.MAWB;
					}
					else
					{
						return (BusinessObject)hawb.Shipment ?? hawb;
					}
				default:
					return null;
			}
		}
	}
}
