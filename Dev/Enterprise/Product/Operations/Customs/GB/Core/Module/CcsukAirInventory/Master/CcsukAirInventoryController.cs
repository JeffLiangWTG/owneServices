using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryController : BaseAirCargoController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is CusMAWB)
			{
				return new CcsukAirInventoryForm(businessEntity as CusMAWB);
			}
			else
			{
				IZForm result = new ConsolForm(businessEntity as ForwardingConsol);
				((ConsolForm)result).PlugInIDToSelectOnLoaded = ControllerIDs.Customs.GB.CcsukAirInventory;
				ZFormMenuStrategy.SetMenuItemVisible((ConsolForm)result, ZFormMenuStrategy.FileNewMenuItemName, false);
				return result;
			}
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AirCcsukMasterDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AirCcsukMasterEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukMasterNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukMasterView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventory; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusMAWB); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.GB.Module.Res.GetData("PlugInTabPage|CcsukAirInventory", "CCS-UK"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CcsukConsolMultiMawbPlugin((ForwardingConsol)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventory; }
		}
	}
}
