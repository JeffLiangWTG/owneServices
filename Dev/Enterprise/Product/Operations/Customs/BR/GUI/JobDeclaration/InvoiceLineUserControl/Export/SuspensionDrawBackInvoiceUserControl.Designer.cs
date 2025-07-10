namespace Enterprise.Customs.BR.GUI
{
	partial class SuspensionDrawbackInvoiceUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SuspensionDrawbackInvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SuspensionDrawbackInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackInvoiceGrid)).BeginInit();
			this.SuspensionDrawbackInvoiceGrid.SuspendLayout();
			this.SuspensionDrawbackInvoiceGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.SuspensionDrawback);
			// 
			// SuspensionDrawbackInvoiceGrid
			// 
			this.SuspensionDrawbackInvoiceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SuspensionDrawbackInvoiceGrid, "SuspensionDrawbackInvoiceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackInvoiceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackInvoiceCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackInvoiceCollection)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackInvoiceCollection)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawbackInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(null)).SuspensionDrawbackInvoiceCollection)).SyncRoot)).CSI_Value)));
			this.SuspensionDrawbackInvoiceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.SuspensionDrawbackInvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SuspensionDrawbackInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SuspensionDrawbackInvoiceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SuspensionDrawbackInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SuspensionDrawbackInvoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackInvoiceGrid.GridId = "0b877a07-196d-4d2e-9d7a-37a1c1890a42";
			this.SuspensionDrawbackInvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SuspensionDrawbackInvoiceGrid.LayoutKey = "SuspensionDrawbackInvoiceGrid";
			this.SuspensionDrawbackInvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SuspensionDrawbackInvoiceGrid.Name = "SuspensionDrawbackInvoiceGrid";
			this.SuspensionDrawbackInvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 88, true);
			this.SuspensionDrawbackInvoiceGrid.TabIndex = 0;
			// 
			// SuspensionDrawbackInvoiceGroupBox
			// 
			this.SuspensionDrawbackInvoiceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("59C6CFC6-AD3F-4D39-B9E0-09A6E6F68D8A", "Invoice");
			this.SuspensionDrawbackInvoiceGroupBox.Controls.Add(this.SuspensionDrawbackInvoiceGrid);
			this.SuspensionDrawbackInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SuspensionDrawbackInvoiceGroupBox.Name = "SuspensionDrawbackInvoiceGroupBox";
			this.SuspensionDrawbackInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 107, true);
			this.SuspensionDrawbackInvoiceGroupBox.TabIndex = 7;
			this.SuspensionDrawbackInvoiceGroupBox.TabStop = false;
			// 
			// SuspensionDrawbackInvoiceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SuspensionDrawbackInvoiceGroupBox);
			this.Name = "SuspensionDrawbackInvoiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 107, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackInvoiceGrid)).EndInit();
			this.SuspensionDrawbackInvoiceGrid.ResumeLayout(false);
			this.SuspensionDrawbackInvoiceGrid.PerformLayout();
			this.SuspensionDrawbackInvoiceGroupBox.ResumeLayout(false);
			this.SuspensionDrawbackInvoiceGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SuspensionDrawbackInvoiceGrid;
		private ZArchitecture.GUI.ZGroupBox SuspensionDrawbackInvoiceGroupBox;
	}
}
