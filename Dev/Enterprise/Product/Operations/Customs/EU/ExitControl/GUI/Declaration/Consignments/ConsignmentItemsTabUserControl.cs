using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentItemsTabUserControl : ZUserControl
	{
		public ConsignmentItemsTabUserControl()
		{
			InitializeComponent();
		}

		new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = ExitControlLayoutProvider.GetLayoutProvider(DataSource.CountryCode);
			var panelLayoutWithGrid = provider.ConsignmentItemPanelLayoutWithGrid;
			SetConsignmentItemsGrid();
			SetConsignmentItemPackingAndContainerLayout();

			void SetConsignmentItemsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				ConsignmentItemsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetConsignmentItemPackingAndContainerLayout()
			{
				ConsignmentItemPackingAndContainerDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
