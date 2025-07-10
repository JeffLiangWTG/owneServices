using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.Module
{
	public class ExitSummaryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|ExitSummaryController", "Exit Control");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var providers = ObjectFactory.Get<Hashtable>("ExitControlPlugIns");
			var providerHandle = (ObjectHandle)providers[GlbCompany.CurrentCompany.Country.Code.ToString()];

			return providerHandle != null ? (ZPlugIn)providerHandle.GetObject(businessEntity) : new ExitSummaryPlugIn(businessEntity);
		}

		public override bool IsFormShownFor(IBusiness entity) => true;

		public override ControllerID ID => ControllerIDs.Customs.EU.ExitSummaryController;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Exit Summary Controller");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This is a plugin - no Form.");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsMain;
	}
}
