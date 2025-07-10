namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceHeaderExportPreviousDocumentsUserControl
	{
		void InitializeComponent()
		{
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("5DD26A54-9100-4CAD-99E5-12B0BA85C8FD", "Previous Document");
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 45, true);
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 20, true);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 19, true);
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 20, true);
			// 
			// InvoiceHeaderExportPreviousDocumentsUserControl
			// 
			this.Name = "InvoiceHeaderExportPreviousDocumentsUserControl";
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
