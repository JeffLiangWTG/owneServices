using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module.ChiefExportConsolIntegration
{
	public class ChiefExportConsolIntegrationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.GB.Module.Res.GetData("PlugInTabPage|ChiefExportConsolIntegrationController", "Customs"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ChiefExportConsolIntegrationPlugIn((ForwardingConsol)businessEntity);
		}

		public override bool IsFormShownFor(IBusiness entity)
		{
			var consol = entity as ForwardingConsol;
			return (consol != null && consol.IsAir && consol.IsExport() && GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.UnitedKingdom);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Chief Export Consol Controller"); }
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
	}
}
