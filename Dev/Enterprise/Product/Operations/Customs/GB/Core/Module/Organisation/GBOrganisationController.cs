using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.GUI.Organisation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module
{
	public class GBOrganisationController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.GB.OrganisationCustomsMessaging;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|GBOrganisationCustomsMessaging", "Customs Messaging");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrganisationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrganisationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrganisationModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrganisationDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrganisationPlugIn((OrgHeader)businessEntity);
	}
}
