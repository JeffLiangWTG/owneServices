namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseResponseMessageForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ImportLicensesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FileContentGroupBox.SuspendLayout();
			this.LogDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ImportLicensesGrid)).BeginInit();
			this.ImportLicensesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// OpenFileDialog
			// 
			this.OpenFileDialog.Multiselect = false;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 15, true);
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 8, true);
			// 
			// ImportButton
			// 
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(517, 519, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 519, true);
			// 
			// FileContentGroupBox
			// 
			this.FileContentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("F8C1E5C8-CF84-4E36-8C52-3CF3D52DDB18", "Import Licenses");
			this.FileContentGroupBox.Controls.Add(this.ImportLicensesGrid);
			this.FileContentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 328, true);
			// 
			// LogDetailsGroupBox
			// 
			this.LogDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 370, true);
			this.LogDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 139, true);
			// 
			// LogListBox
			// 
			this.LogDetailsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 102, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 548, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent);
			// 
			// ImportLicensesGrid
			// 
			this.ImportLicensesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ImportLicensesGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseResponseObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)).SyncRoot)).ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseResponseObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseResponseObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)).SyncRoot)).RegistrationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseResponseObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)).SyncRoot)).Diagnosis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseResponseObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent)(null)).Collection)).SyncRoot)).Status)));
			this.ImportLicensesGrid.CaptionText = "Teste";
			this.ImportLicensesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "RegistrationDate";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "Diagnosis";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ImportLicensesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportLicensesGrid.GridId = "418e5cb0-f389-45e4-adb8-be227d7d8f07";
			this.ImportLicensesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportLicensesGrid.LayoutKey = "ImportLicensesGrid";
			this.ImportLicensesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.ImportLicensesGrid.Name = "ImportLicensesGrid";
			this.ImportLicensesGrid.ReadOnly = true;
			this.ImportLicensesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 313, true);
			this.ImportLicensesGrid.TabIndex = 8;
			// 
			// ImportLicenseResponseMessageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 572, true);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.IImportLicenseResponseObjectParent);
			this.Name = "ImportLicenseResponseMessageForm";
			this.FileContentGroupBox.ResumeLayout(false);
			this.FileContentGroupBox.PerformLayout();
			this.LogDetailsGroupBox.ResumeLayout(false);
			this.LogDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ImportLicensesGrid)).EndInit();
			this.ImportLicensesGrid.ResumeLayout(false);
			this.ImportLicensesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ImportLicensesGrid;
	}
}
