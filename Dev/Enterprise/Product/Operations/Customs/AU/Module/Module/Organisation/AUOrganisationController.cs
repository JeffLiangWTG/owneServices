using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class AUOrganisationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AUOrganisationController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.OrganisationCustomsMessaging; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDeclarationEnquiryEdit; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsDeclarationEnquiryNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsDeclarationEnquiryDelete; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|AUOrganisationCustomsMessaging", "Customs Messaging"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrganisationPlugIn((OrgHeader)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
