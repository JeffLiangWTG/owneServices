using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class DepartureMovementsTabUserControl : ZUserControl
	{
		public DepartureMovementsTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			SetDeclarationDetailsLayout();
		}

		void SetDeclarationDetailsLayout()
		{
			DynamicDeclarationDetailsPanel.UpdateLayout(LayoutProvider.DeclarationDetailsPanelLayout);
			TransportDepartureDynamicLayoutPanel.UpdateLayout(LayoutProvider.TransportDeparturePanelLayout);
			TransportBorderDynamicLayoutPanel.UpdateLayout(LayoutProvider.TransportBorderPanelLayout);
			TransportAndPackagingDynamicLayoutPanel.UpdateLayout(LayoutProvider.TransportAndPackagingPanelLayout);
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ??= NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
		INctsPhase5LayoutProvider layoutProvider;
	}
}
