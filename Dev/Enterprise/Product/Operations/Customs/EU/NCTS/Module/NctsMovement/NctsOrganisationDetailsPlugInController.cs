using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsOrganisationDetailsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public NctsOrganisationDetailsPlugInController()
		{ }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.EU.NctsOrganisationDetailsPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgDetailsViewCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.EU.NCTS.Module.Res.GetData("PlugInTabPage|NctsOrganisationDetailsPlugInController", "NCTS"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new NctsOrganisationDetailsPlugin((OrgHeader)businessEntity);
		}
	}
}
