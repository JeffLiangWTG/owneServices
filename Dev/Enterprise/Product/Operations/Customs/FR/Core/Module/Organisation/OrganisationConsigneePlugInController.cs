using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.Module
{
	public class OrganisationConsigneePlugInController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		public override ControllerID ID => ControllerIDs.Customs.FR.OrganisationConsigneePlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrganisationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrganisationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrganisationModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrganisationDelete;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|FROrganisationConsigneePlugIn", "FR");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationConsigneePlugIn(businessEntity);
	}
}
