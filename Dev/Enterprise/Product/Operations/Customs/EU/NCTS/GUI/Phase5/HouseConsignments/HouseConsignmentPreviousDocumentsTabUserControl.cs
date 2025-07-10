using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentPreviousDocumentsTabUserControl : ZUserControl
	{
		public HouseConsignmentPreviousDocumentsTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.HouseConsignmentPreviousDocumentPanelLayoutWithGrid;
			SetPreviousDocumentsGrid();
			SetPreviousDocumentLayout();

			void SetPreviousDocumentsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				PreviousDocumentsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetPreviousDocumentLayout()
			{
				PreviousDocumentDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
