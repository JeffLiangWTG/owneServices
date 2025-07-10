using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class DeclarationUserControl : BaseCustomsEntryUserControl
	{
		public DeclarationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = EMCSLayoutProvider.GetLayoutProvider(((EMCSJobDeclaration)DataSource).GetDefaultDataGroupingCode());
			SetDeclarationOrganizationsLayout();

			void SetDeclarationOrganizationsLayout()
			{
				DeclarationOrganizationsDynamicLayoutPanel.UpdateLayout(provider.DeclarationOrganizationsPanelLayout);
			}
		}
	}
}
