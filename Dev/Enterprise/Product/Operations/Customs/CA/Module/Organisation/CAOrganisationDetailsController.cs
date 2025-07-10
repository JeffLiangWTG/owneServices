using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CAOrganisationDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgDetailsViewCountryDefaults; }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.OrganisationDetailsPlugIn; }
		}

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.OrganisationDetailsPlugIn((OrgHeader)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.CA.Module.Res.GetData("PlugInTabPage|CAOrganisationDetailsController", "Canada"); }
		}
	}
}
