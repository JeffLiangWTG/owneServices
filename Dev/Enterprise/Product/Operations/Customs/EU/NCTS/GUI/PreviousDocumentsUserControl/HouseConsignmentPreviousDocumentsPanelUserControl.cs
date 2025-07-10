using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class HouseConsignmentPreviousDocumentsPanelUserControl : ZUserControl
	{
		public HouseConsignmentPreviousDocumentsPanelUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.HouseConsignmentPreviousDocumentsLayout;
			SetGrid();
			SetLayout();

			void SetGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				SplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetLayout()
			{
				DynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
