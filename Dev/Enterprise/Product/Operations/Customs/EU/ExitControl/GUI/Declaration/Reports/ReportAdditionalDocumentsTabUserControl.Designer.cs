namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportAdditionalDocumentsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReportAdditionalDocumentsGridUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ReportAdditionalDocumentsGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportAdditionalDocumentsGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.IAdditionalInfoCollection<Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo>);
			// 
			// ReportAdditionalDocumentsGridUserControl
			// 
			this.ReportAdditionalDocumentsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportAdditionalDocumentsGridUserControl, ".");
			this.ReportAdditionalDocumentsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportAdditionalDocumentsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportAdditionalDocumentsGridUserControl.Name = "ReportAdditionalDocumentsGridUserControl";
			this.ReportAdditionalDocumentsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			this.ReportAdditionalDocumentsGridUserControl.TabIndex = 0;
			// 
			// ReportAdditionalDocumentsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportAdditionalDocumentsGridUserControl);
			this.Name = "ReportAdditionalDocumentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportAdditionalDocumentsGridUserControl.ResumeLayout(true);
			this.ReportAdditionalDocumentsGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ReportAdditionalDocumentsGridUserControl ReportAdditionalDocumentsGridUserControl;
	}
}
