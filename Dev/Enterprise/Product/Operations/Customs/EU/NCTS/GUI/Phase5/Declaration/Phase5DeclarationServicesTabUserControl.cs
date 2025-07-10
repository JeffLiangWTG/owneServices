using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5DeclarationServicesTabUserControl : ZUserControl
	{
		public Phase5DeclarationServicesTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.DeclarationServicePanelLayoutWithGrid;
			SetServicesGrid(panelLayoutWithGrid);
			SetServicesLayout(panelLayoutWithGrid);
		}

		void SetServicesGrid(IPanelLayoutWithGridProvider panelLayoutWithGrid)
		{
			var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			ServicesSplitContainer.Panel1.Controls.Add(userControl);
			BindingSource.SetBindingMember(userControl, ".");
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		void SetServicesLayout(IPanelLayoutWithGridProvider panelLayoutWithGrid)
		{
			ServiceDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
		}
	}
}
