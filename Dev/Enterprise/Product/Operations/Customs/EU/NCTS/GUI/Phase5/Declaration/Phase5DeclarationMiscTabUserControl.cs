using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5DeclarationMiscTabUserControl : ZUserControl
	{
		public Phase5DeclarationMiscTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetDeclarationDetailsLayout();
		}

		void SetDeclarationDetailsLayout()
		{
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			DynamicMiscOptionsPanel.UpdateLayout(provider.MiscellanousOptionsPanelLayout);
		}
	}
}
