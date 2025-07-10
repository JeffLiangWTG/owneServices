using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemSupplyChainActorsTabUserControl : ZUserControl
	{
		public Phase5GoodsItemSupplyChainActorsTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.GoodsItemSupplyChainActorPanelLayoutWithGrid;
			SetSupplyChainActorsGrid();
			SupplyChainActorDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);

			void SetSupplyChainActorsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				SupplyChainActorsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}
	}
}
