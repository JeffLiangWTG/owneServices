namespace Enterprise.Customs.BR.GUI
{
	partial class SuspensionDrawbackImportEntryDocumentUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SuspensionDrawbackImportEntryDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SuspensionDrawbackImportEntryDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackImportEntryDocumentGrid)).BeginInit();
			this.SuspensionDrawbackImportEntryDocumentGrid.SuspendLayout();
			this.SuspensionDrawbackImportEntryDocumentGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.SuspensionDrawback);
			// 
			// SuspensionDrawbackImportEntryDocumentGrid
			// 
			this.SuspensionDrawbackImportEntryDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SuspensionDrawbackImportEntryDocumentGrid, "SuspensionDrawbackImportEntryDocumentCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawbackImportEntryDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackImportEntryDocumentCollection)).SyncRoot)).CSI_Value)));
			this.SuspensionDrawbackImportEntryDocumentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.MaxValue = 999;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Decimals = 5;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SuspensionDrawbackImportEntryDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SuspensionDrawbackImportEntryDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SuspensionDrawbackImportEntryDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SuspensionDrawbackImportEntryDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SuspensionDrawbackImportEntryDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SuspensionDrawbackImportEntryDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackImportEntryDocumentGrid.GridId = "0b877a07-196d-4d2e-9d7a-37a1c1890a42";
			this.SuspensionDrawbackImportEntryDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SuspensionDrawbackImportEntryDocumentGrid.LayoutKey = "SuspensionDrawbackImportEntryDocumentGrid";
			this.SuspensionDrawbackImportEntryDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SuspensionDrawbackImportEntryDocumentGrid.Name = "SuspensionDrawbackImportEntryDocumentGrid";
			this.SuspensionDrawbackImportEntryDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 88, true);
			this.SuspensionDrawbackImportEntryDocumentGrid.TabIndex = 0;
			// 
			// SuspensionDrawbackImportEntryDocumentGroupBox
			// 
			this.SuspensionDrawbackImportEntryDocumentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("D4F6E432-8060-4A53-B424-78CF73EE04A1", "Import Entry Document");
			this.SuspensionDrawbackImportEntryDocumentGroupBox.Controls.Add(this.SuspensionDrawbackImportEntryDocumentGrid);
			this.SuspensionDrawbackImportEntryDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackImportEntryDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SuspensionDrawbackImportEntryDocumentGroupBox.Name = "SuspensionDrawbackImportEntryDocumentGroupBox";
			this.SuspensionDrawbackImportEntryDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 107, true);
			this.SuspensionDrawbackImportEntryDocumentGroupBox.TabIndex = 7;
			this.SuspensionDrawbackImportEntryDocumentGroupBox.TabStop = false;
			// 
			// SuspensionDrawbackImportEntryDocumentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SuspensionDrawbackImportEntryDocumentGroupBox);
			this.Name = "SuspensionDrawbackImportEntryDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 107, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackImportEntryDocumentGrid)).EndInit();
			this.SuspensionDrawbackImportEntryDocumentGrid.ResumeLayout(false);
			this.SuspensionDrawbackImportEntryDocumentGrid.PerformLayout();
			this.SuspensionDrawbackImportEntryDocumentGroupBox.ResumeLayout(false);
			this.SuspensionDrawbackImportEntryDocumentGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SuspensionDrawbackImportEntryDocumentGrid;
		private ZArchitecture.GUI.ZGroupBox SuspensionDrawbackImportEntryDocumentGroupBox;
	}
}
