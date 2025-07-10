using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class BROrganisationDetailPlugInController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.BR.OrganisationDetailsPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrgDetailsViewCountryDefaults;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrgConfigModifyCountryDefaults;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrgConfigModifyCountryDefaults;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrgConfigModifyCountryDefaults;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|BROrganisationDetailPlugIn", "Brazil");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new GUI.OrganisationDetailsPlugIn((OrgHeader)businessEntity);
	}
}

