namespace Enterprise.Customs.ES.GUI
{
	public partial class SupportingInformationControl
	{
		void InitializeComponent()
		{
			this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.additionalDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SupportingInformationTabControl.SuspendLayout();
			this.SupportingDocumentTabPage.SuspendLayout();
			this.AdditionalDocumentsTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentTabPage.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// AdditionalDocumentsTabPage
			// 
			this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("78FFBCA2-E72B-4D85-871C-24BF33682D63", "Additional Documents");
			this.AdditionalDocumentsTabPage.Controls.Add(this.additionalDocumentsUserControl);
			this.AdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDocumentsTabPage.Name = "AdditionalDocumentsTabPage";
			this.AdditionalDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			this.AdditionalDocumentsTabPage.TabIndex = 1;
			this.AdditionalDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// additionalDocumentsUserControl
			// 
			this.additionalDocumentsUserControl.AllowDrop = true;
			this.additionalDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.additionalDocumentsUserControl.Name = "additionalDocumentsUserControl";
			this.additionalDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
			this.additionalDocumentsUserControl.TabIndex = 1;
			// 
			// SupportingInformationControl
			// 
			this.Name = "SupportingInformationControl";
			this.SupportingInformationTabControl.ResumeLayout(false);
			this.SupportingInformationTabControl.PerformLayout();
			this.SupportingDocumentTabPage.ResumeLayout(false);
			this.SupportingDocumentTabPage.PerformLayout();
			this.AdditionalDocumentsTabPage.ResumeLayout(false);
			this.AdditionalDocumentsTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentTabPage.ResumeLayout(false);
			this.PreviousDocumentTabPage.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalDocumentsUserControl;
	}
}
