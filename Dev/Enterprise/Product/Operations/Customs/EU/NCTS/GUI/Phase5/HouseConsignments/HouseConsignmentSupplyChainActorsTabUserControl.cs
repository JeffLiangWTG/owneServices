using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentSupplyChainActorsTabUserControl : ZUserControl
	{
		public HouseConsignmentSupplyChainActorsTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.HouseConsignmentSupplyChainActorPanelLayoutWithGrid;
			SetSupplyChainActorsGrid();
			SetSupplyChainActorsLayout();

			void SetSupplyChainActorsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				SupplyChainActorsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetSupplyChainActorsLayout()
			{
				SupplyChainActorDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
