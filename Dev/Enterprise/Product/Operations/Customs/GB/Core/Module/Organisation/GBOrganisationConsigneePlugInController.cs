using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.Module
{
	public class GBOrganisationConsigneePlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GBOrganisationConsigneePlugInController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.OrganisationConsigneePlugIn; }
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
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|GBOrganisationConsigneePlugIn", "GB"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.OrganisationConsigneePlugIn((OrgHeader)businessEntity);
		}
	}
}
