namespace Enterprise.Customs.GB.H7.GUI
{
	public partial class H7AdditionalInfoUserControl
	{
		void InitializeComponent()
		{
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.AdditionalInfosPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
			this.AdditionalInfosGrid.SuspendLayout();
			this.AddInfoTypeCodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AddiInfoDescriptionTextBox
			// 
			this.AddiInfoDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			// 
			// AddInfoTypeCodeDropEdit
			// 
			this.AddInfoTypeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			// 
			// H7AdditionalInfoUserControl
			// 
			this.Name = "H7AdditionalInfoUserControl";
			this.AdditionalInfosGroupBox.ResumeLayout(false);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.AdditionalInfosPanel.ResumeLayout(false);
			this.AdditionalInfosPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
			this.AdditionalInfosGrid.ResumeLayout(false);
			this.AdditionalInfosGrid.PerformLayout();
			this.AddInfoTypeCodeDropEdit.ResumeLayout(true);
			this.AddInfoTypeCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
