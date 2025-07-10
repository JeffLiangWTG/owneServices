using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Organisations.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices
{
	public class BillingPricesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var priceItem = ((ClientLicencePriceItem)businessEntity);
			var ediOrgForm = new EDIOrganisationForm(priceItem.Parent.LicCompany.Header, new EdiOrgViewController());

			ediOrgForm.Load += (sender, e) =>
			{
				ediOrgForm.OrganisationsTabControl.SelectTab(ediOrgForm.BuilderTabPage.Name);
				ediOrgForm.OrganisationsTabControl_SelectedIndexChanging(null, EventArgs.Empty);

				ediOrgForm.BuilderTabPage.LicenceControl.SelectPriceItemTab();
				ediOrgForm.BuilderTabPage.LicenceControl.SelectPriceHeadersGrid(priceItem);
			};

			return ediOrgForm;
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.BillingPrices; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.BillingPrices; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ClientLicencePriceItem); }
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		#endregion

	}
}
