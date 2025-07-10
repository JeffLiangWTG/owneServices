using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Organisations.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceKeyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LicenceKeyController()
		{
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Can't create a new LicenseKey");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Can't delete a new LicenseKey");
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.LicenseKey; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.LicenseKey; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(LicenceHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var licence = (LicenceHeader)businessEntity;
			var ediOrgForm = new EDIOrganisationForm(licence.Company.Header, new EdiOrgViewController());
			ediOrgForm.OrganisationsTabControl.SelectTab("LicenceKeyBuilderTabPage");
			ediOrgForm.OrganisationsTabControl_SelectedIndexChanging(null, EventArgs.Empty);
			ediOrgForm.BuilderTabPage.SelectedLicenceHeader = licence;

			return ediOrgForm;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}
	}
}
