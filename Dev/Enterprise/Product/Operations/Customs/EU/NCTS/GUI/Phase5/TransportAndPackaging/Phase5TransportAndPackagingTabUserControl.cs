using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5TransportAndPackagingTabUserControl : ZUserControl
	{
		public Phase5TransportAndPackagingTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetTransportAndPackagingLayout();
		}

		void SetTransportAndPackagingLayout()
		{
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			BindingSource.SetBindingMember(TransportDepartureDynamicLayoutPanel, nameof(NctsHeader.MovementHeader));
			BindingSource.SetBindingMember(TransportBorderDynamicLayoutPanel, nameof(NctsHeader.MovementHeader));
			BindingSource.SetBindingMember(TransportAndPackagingDynamicLayoutPanel, nameof(NctsHeader.MovementHeader));

			TransportDepartureDynamicLayoutPanel.UpdateLayout(provider.TransportDeparturePanelLayout);
			TransportBorderDynamicLayoutPanel.UpdateLayout(provider.TransportBorderPanelLayout);
			TransportAndPackagingDynamicLayoutPanel.UpdateLayout(provider.TransportAndPackagingPanelLayout);
		}
	}
}
