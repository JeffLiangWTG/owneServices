namespace Enterprise.Customs.BR.GUI
{
	partial class AdditionalDetailsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DrawbackImportLicenseUserControl = new Enterprise.Customs.BR.GUI.DrawbackImportLicenseUserControl();
			this.ConsentingProcessGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConsentingProcessGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DrawbackImportLicenseUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsentingProcessGrid)).BeginInit();
			this.ConsentingProcessGrid.SuspendLayout();
			this.ConsentingProcessGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// DrawbackImportLicenseUserControl
			// 
			this.DrawbackImportLicenseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DrawbackImportLicenseUserControl, ".");
			this.DrawbackImportLicenseUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.DrawbackImportLicenseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DrawbackImportLicenseUserControl.Name = "DrawbackImportLicenseUserControl";
			this.DrawbackImportLicenseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 105, true);
			this.DrawbackImportLicenseUserControl.TabIndex = 8;
			// 
			// ConsentingProcessGrid
			// 
			this.ConsentingProcessGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsentingProcessGrid, "ConsentingProcessCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ConsentingProcessCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ConsentingProcess)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ConsentingProcessCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ConsentingProcess)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ConsentingProcessCollection)).SyncRoot)).CSI_CustomsOffice)));
			this.ConsentingProcessGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_CustomsOffice";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ConsentingProcessGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsentingProcessGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConsentingProcessGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsentingProcessGrid.GridId = "A4A40816-0F60-4603-8ED6-922E55C0A042";
			this.ConsentingProcessGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsentingProcessGrid.LayoutKey = "ConsentingProcessGrid";
			this.ConsentingProcessGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConsentingProcessGrid.Name = "ConsentingProcessGrid";
			this.ConsentingProcessGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 347, true);
			this.ConsentingProcessGrid.TabIndex = 0;
			// 
			// ConsentingProcessGroupBox
			// 
			this.ConsentingProcessGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("E98EB1CC-5A5F-4CBB-95BC-6BEE7280935E", "Consenting Process");
			this.ConsentingProcessGroupBox.Controls.Add(this.ConsentingProcessGrid);
			this.ConsentingProcessGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsentingProcessGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.ConsentingProcessGroupBox.Name = "ConsentingProcessGroupBox";
			this.ConsentingProcessGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 366, true);
			this.ConsentingProcessGroupBox.TabIndex = 7;
			this.ConsentingProcessGroupBox.TabStop = false;
			// 
			// AdditionalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsentingProcessGroupBox);
			this.Controls.Add(this.DrawbackImportLicenseUserControl);
			this.Name = "AdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 471, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DrawbackImportLicenseUserControl.ResumeLayout(true);
			this.DrawbackImportLicenseUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsentingProcessGrid)).EndInit();
			this.ConsentingProcessGrid.ResumeLayout(false);
			this.ConsentingProcessGrid.PerformLayout();
			this.ConsentingProcessGroupBox.ResumeLayout(false);
			this.ConsentingProcessGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid ConsentingProcessGrid;
		private ZArchitecture.GUI.ZGroupBox ConsentingProcessGroupBox;
		internal DrawbackImportLicenseUserControl DrawbackImportLicenseUserControl;

		#endregion
	}
}
