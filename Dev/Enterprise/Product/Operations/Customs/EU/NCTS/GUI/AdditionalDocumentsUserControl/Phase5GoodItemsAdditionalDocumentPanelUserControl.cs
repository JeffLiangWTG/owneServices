using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodItemsAdditionalDocumentPanelUserControl : ZUserControl
	{
		public Phase5GoodItemsAdditionalDocumentPanelUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelLayoutWithGrid = (DataSource?.IsArrivalMovement ?? false) ? provider.ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid : provider.DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid;
			SetAdditionalDocumentsGrid();
			SetAdditionalDocumentLayout();

			void SetAdditionalDocumentsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
				AdditionalDocumentsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, ".");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetAdditionalDocumentLayout()
			{
				AdditionalDocumentDynamicLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			}
		}
	}
}
