namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl
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
			this.AddiInfoDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// AdditionalInfosUserControl
			// 
			this.Name = "AdditionalInfosUserControl";
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
