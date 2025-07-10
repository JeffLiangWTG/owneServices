using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public sealed partial class TransactionDetailsTabUserControl : ZUserControl
	{
		public TransactionDetailsTabUserControl()
		{
			InitializeComponent();
		}

		new CusIntrastatHeader DataSource => (CusIntrastatHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			SetDeclarationDetailsLayout();
		}

		void SetDeclarationDetailsLayout()
		{
			DynamicOrganisationDetailsPanel.UpdateLayout(LayoutProvider.OrganisationDetailsPanelLayout);
			DynamicTransactionDetailsPanel.UpdateLayout(LayoutProvider.TransactionDetailsPanelLayout);
		}

		IIntrastatLayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = IntrastatLayoutProvider.GetLayoutProvider(DataSource?.CountryCode));
		IIntrastatLayoutProvider layoutProvider;
	}
}
