using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentSupportingDocumentsTabUserControl : ZUserControl
	{
		public HouseConsignmentSupportingDocumentsTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = provider.HouseConsignmentSupportingDocumentPanelLayoutWithGrid;
			SetSupportingDocumentsGrid();
			SetSupportingDocumentLayout();

			void SetSupportingDocumentsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				HouseConsignmentSupportingDocumentsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetSupportingDocumentLayout()
			{
				HouseConsignmentSupportingDocumentsDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
