using System;
using CargoWise.EntityFramework;
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
	public class AUCustomsAirCargoConsolController : JobConsolController
	{
		protected override string GetIDForFormCache(IBusiness businessEntity) => ControllerIDs.JobConsol.ToString();

		public override ControllerID ID => ControllerIDs.Customs.AU.AirCargoConsolController;

		protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
		{
			var result = new ConsolForm(businessEntity);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.AirCargo;
			ZFormMenuStrategy.SetMenuItemVisible(result, ZFormMenuStrategy.FileNewMenuItemName, false);
			return result;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			CusMAWB mAWB = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as CusMAWB;
			if (mAWB == null)
			{
				return null;
			}
			else
			{
				return mAWB.Consol;
			}
		}

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusMAWB);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new AirCargoConsolPlugIn((ForwardingConsol)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ACAMasterImportView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ACAMasterImportModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ACAMasterImportNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ACAMasterImportDelete;
	}
}
