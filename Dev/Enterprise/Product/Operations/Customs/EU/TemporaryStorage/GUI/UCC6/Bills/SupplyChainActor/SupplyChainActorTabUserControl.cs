using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class SupplyChainActorTabUserControl : ZUserControl
	{
		public SupplyChainActorTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var panelLayoutWithGrid = new SupplyChainActorLayout();
			SetSupplyChainActorGrid();
			SetSupplyChainActorLayout();

			void SetSupplyChainActorGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				SupplyChainActorSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetSupplyChainActorLayout()
			{
				SupplyChainActorDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
