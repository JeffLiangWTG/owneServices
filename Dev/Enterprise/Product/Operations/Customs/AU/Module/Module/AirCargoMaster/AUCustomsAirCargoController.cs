using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsAirCargoController : BaseAirCargoController
	{
		public AUCustomsAirCargoController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.AirCargo;

		public override ControllerID ID => ControllerIDs.Customs.AU.AirCargo;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusMAWB);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|AUCustomsAirCargo", "Air Cargo", "The Air Cargo tab.");

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => Factory.New<CusMAWB>();

		protected override IZForm GetForm(IBusiness businessEntity) => new AirCargoMasterForm((CusMAWB)businessEntity);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new AirCargoConsolWithScanPlugIn((ForwardingConsol)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ACAMasterImportView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ACAMasterImportModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ACAMasterImportNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ACAMasterImportDelete;
	}
}
