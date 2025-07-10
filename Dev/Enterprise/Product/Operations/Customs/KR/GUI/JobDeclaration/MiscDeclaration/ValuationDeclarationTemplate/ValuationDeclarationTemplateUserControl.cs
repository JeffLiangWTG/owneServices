using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDeclarationTemplateUserControl : ZUserControl
	{
		public ValuationDeclarationTemplateUserControl()
		{
			InitializeComponent();

			SupplierOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
			ImporterOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			entryDetailPanel.UpdateLayout(new ValuationEntryDetailsLayout());
			DetailsPanel.UpdateLayout(new ValuationDeclarationTemplateDetailsLayout());
			AuthorPanel.UpdateLayout(new AuthorLayout());
			ResponsiblePersonPanel.UpdateLayout(new ResponsiblePersonLayout());
			HookControlVisibilityChangeEvents();
			HandleDeclarationControlVisibilityChanged();
		}

		JobDeclaration JobDeclaration => (JobDeclaration)BindingSource.DataSource;

		void HookControlVisibilityChangeEvents()
		{
			var invoice = JobDeclaration?.Invoices.FirstOrDefault();
			if (invoice != null)
			{
				invoice.JZ_ValuationCodeInfo.ValueChanged += JobDeclaration_ControlVisibilityChanged;
			}
		}

		void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
		{
			HandleDeclarationControlVisibilityChanged();
		}

		void HandleDeclarationControlVisibilityChanged()
		{
			switch (JobDeclaration?.Invoices.FirstOrDefault()?.JZ_ValuationCode)
			{
				default:
				case ValuationCodeList.Codes.MethodOne:
					ValuationMethodCUserControl.Visible = true;
					ValuationMethodDUserControl.Visible = false;
					break;
				case ValuationCodeList.Codes.MethodTwo:
				case ValuationCodeList.Codes.MethodThree:
				case ValuationCodeList.Codes.MethodFourA:
				case ValuationCodeList.Codes.MethodFourB:
				case ValuationCodeList.Codes.MethodFive:
				case ValuationCodeList.Codes.MethodSix:
					ValuationMethodCUserControl.Visible = false;
					ValuationMethodDUserControl.Visible = true;
					break;
			}
		}

		Func<BusinessObjectFactory, OrgAddress, AddressFormatter> GetAddressFormatter()
		{
			return (factory, orgAddress) => new KRAddressFormatter(factory, orgAddress);
		}
	}
}
