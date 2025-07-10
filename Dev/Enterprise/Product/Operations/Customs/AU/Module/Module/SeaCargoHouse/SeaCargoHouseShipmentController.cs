using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoHouseShipmentController : JobShipmentController
	{
		public override ControllerID ID => ControllerIDs.Customs.AU.SeaCargoHouseShipmentController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.HouseSeaCargo;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusSCAHouse);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|AUCustomsSeaCargo", "Sea Cargo", "The Sea Cargo tab.");

		protected override string GetIDForFormCache(IBusiness businessEntity) => ControllerIDs.JobShipment.ToString();

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var result = new ShipmentForm(businessEntity as ForwardingShipment);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
			return result;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var scaHouse = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as CusSCAHouse;
			return scaHouse?.Shipment;
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new SeaCargoShipmentPlugIn((ForwardingShipment)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AUCustomsSCA;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AUCustomsSCAImportModify;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AUCustomsSCAImportModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AUCustomsSCAImportModify;
	}
}
