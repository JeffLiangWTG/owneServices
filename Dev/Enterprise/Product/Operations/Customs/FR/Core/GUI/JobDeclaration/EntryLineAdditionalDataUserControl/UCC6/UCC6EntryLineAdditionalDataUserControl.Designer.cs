namespace Enterprise.Customs.FR.GUI
{
	partial class UCC6EntryLineAdditionalDataUserControl
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
			this.FeesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UCC6EntryLineCalculationResultsControl = new Enterprise.Customs.FR.GUI.UCC6EntryLineCalculationResultsControl();
			this.ExtendInfoTabControl.SuspendLayout();
			this.ExtendedInfoTabPage.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.ExtendedInfoGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FeesPanel.SuspendLayout();
			this.UCC6EntryLineCalculationResultsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtendedInfoTabPage
			// 
			this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 226, true);
			// 
			// TaxOrFeeTabPage
			// 
			this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 226, true);
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 226, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Controls.Add(this.FeesPanel);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 226, true);
			this.ExtendedInfoGroupBox.Controls.SetChildIndex(this.FeesPanel, 0);
			// 
			// DutyAndTaxDetails
			// 
			this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 220, true);
			this.DutyAndTaxDetails.UserControlType = typeof(UCC6EntryLineTaxAndConfirmedFeeUserControl);
			// 
			// EntryLineSupportingDocumentsGrid
			// 
			this.EntryLineSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 220, true);
			// 
			// FeesPanel
			// 
			this.FeesPanel.Controls.Add(this.UCC6EntryLineCalculationResultsControl);
			this.FeesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 34, true);
			this.FeesPanel.Name = "FeesPanel";
			this.FeesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 189, true);
			this.FeesPanel.TabIndex = 3;
			// 
			// UCC6EntryLineCalculationResultsControl
			// 
			this.UCC6EntryLineCalculationResultsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UCC6EntryLineCalculationResultsControl, "CustomsEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.FR.Business.Declaration.CusEntryLine)(((Enterprise.Customs.FR.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)))));
			this.UCC6EntryLineCalculationResultsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UCC6EntryLineCalculationResultsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UCC6EntryLineCalculationResultsControl.Name = "UCC6EntryLineCalculationResultsControl";
			this.UCC6EntryLineCalculationResultsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 189, true);
			this.UCC6EntryLineCalculationResultsControl.TabIndex = 0;
			// 
			// UCC6EntryLineAdditionalDataUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "UCC6EntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.ExtendedInfoTabPage.ResumeLayout(false);
			this.ExtendedInfoTabPage.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FeesPanel.ResumeLayout(false);
			this.FeesPanel.PerformLayout();
			this.UCC6EntryLineCalculationResultsControl.ResumeLayout(true);
			this.UCC6EntryLineCalculationResultsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel FeesPanel;
		private UCC6EntryLineCalculationResultsControl UCC6EntryLineCalculationResultsControl;
	}
}
