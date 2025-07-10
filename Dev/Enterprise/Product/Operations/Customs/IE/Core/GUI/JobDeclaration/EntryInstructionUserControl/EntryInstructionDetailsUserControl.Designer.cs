namespace Enterprise.Customs.IE.GUI
{
	partial class EntryInstructionDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.EntryInstructionTabControl.SuspendLayout();
			this.AuthorisationsTabPage.SuspendLayout();
			this.DetailsUserControl.SuspendLayout();
			this.EntryInstructionGridUserControl.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.SpecialProceduresTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntryInstructionTabControl
			// 
			this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1519, 495, true);
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1511, 468, true);
			this.DetailsUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryInstructionDetailBasicUserControl);
			// 
			// EntryInstructionGridUserControl
			// 
			this.EntryInstructionGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1519, 332, true);
			this.EntryInstructionGridUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryInstructionGridUserControl);
			// 
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.UserControlType = typeof(Enterprise.Customs.IE.GUI.IEEntryInstructionGuaranteesUserControl);
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			// 
			// SpecialProceduresTabPage
			// 
			this.SpecialProceduresTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SpecialProceduresTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobDeclaration);
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1519, 831, true);
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.AuthorisationsTabPage.ResumeLayout(false);
			this.AuthorisationsTabPage.PerformLayout();
			this.DetailsUserControl.ResumeLayout(true);
			this.DetailsUserControl.PerformLayout();
			this.EntryInstructionGridUserControl.ResumeLayout(true);
			this.EntryInstructionGridUserControl.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.SpecialProceduresTabPage.ResumeLayout(false);
			this.SpecialProceduresTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
