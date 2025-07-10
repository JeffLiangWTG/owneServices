using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.GUI.CIN;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.Module
{
	public class CINExportConsolIntegrationController : ZController
	{
		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.FR.Module.Res.GetData("PlugInTabPage|CINExportConsolIntegrationController", "Customs"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CINExportConsolIntegrationPlugIn((ForwardingConsol)businessEntity);
		}
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.FR.CINExportConsolIntegrationController; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("CIN Export Consol Controller"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This is a plugin - no Form.");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsMain; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsMain; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsMain; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsMain; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
	}
}

