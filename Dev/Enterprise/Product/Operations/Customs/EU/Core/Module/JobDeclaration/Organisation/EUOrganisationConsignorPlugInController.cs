using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.Module
{
	public class EUOrganisationConsignorPlugInController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		public override ControllerID ID => ControllerIDs.Customs.EU.OrganisationConsignorPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrganisationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrganisationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrganisationModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrganisationDelete;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|EUOrganisationConsignorPlugInController", "Customs Defaults");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationConsignorPlugIn((OrgHeader)businessEntity);
	}
}
