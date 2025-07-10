using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5UnloadingDifferencesPreviousDocumentsUserControl : ZUserControl
	{
		public Phase5UnloadingDifferencesPreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var panelGridType = provider.UnloadingDifferencesPreviousDocumentPanelGridUserControlType;
			SetPreviousDocumentsGrid();

			void SetPreviousDocumentsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(panelGridType);
				PreviousDocumentGroupBox.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, "PreviousDocuments");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}
	}
}
