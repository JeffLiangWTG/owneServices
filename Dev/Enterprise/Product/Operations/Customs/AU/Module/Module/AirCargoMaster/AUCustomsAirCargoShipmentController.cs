using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsAirCargoShipmentController : JobShipmentController
	{
		public override ControllerID ID => ControllerIDs.Customs.AU.AirCargoShipmentController;

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusHAWB);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|AUCustomsAirCargo", "Air Cargo", "The Air Cargo tab.");

		protected override string GetIDForFormCache(IBusiness businessEntity) => ControllerIDs.JobShipment.ToString();

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var result = new ShipmentForm(businessEntity as ForwardingShipment);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.HouseAirCargo;
			return result;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var hAWB = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as CusHAWB;
			if (hAWB == null)
			{
				return null;
			}
			else
			{
				return hAWB.Shipment;
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new AirCargoConsolWithScanPlugIn((ForwardingConsol)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ACAMasterImportView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ACAMasterImportModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ACAMasterImportNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ACAMasterImportDelete;
	}
}
