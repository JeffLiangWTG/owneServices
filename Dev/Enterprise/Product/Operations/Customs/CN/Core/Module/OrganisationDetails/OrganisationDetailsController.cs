using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CN.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.Module
{
	public class OrganisationDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.OrgConfigModifyCountryDefaults;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.OrgDetailsViewCountryDefaults;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("No form");

		public override ControllerID ID => ControllerIDs.Customs.CN.OrganisationDetailsPlugIn;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|CNOrganisationDetailsController", "China");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationDetailsPlugIn((OrgHeader)businessEntity);
	}
}
