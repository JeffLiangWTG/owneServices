namespace Enterprise.Customs.BR.GUI
{
	partial class ElectronicLogisticInvoiceUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElectronicLogisticInvoiceUserControl));
			this.ElectronicLogisticInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ElectronicLogisticInvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ElectronicLogisticInvoiceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicLogisticInvoiceGrid)).BeginInit();
			this.ElectronicLogisticInvoiceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ElectronicLogisticInvoiceGroupBox
			// 
			this.ElectronicLogisticInvoiceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("FAD7A5CD-4D36-432A-A069-A04E8FC0EEDA", "Electronic Logistic Invoices");
			this.ElectronicLogisticInvoiceGroupBox.Controls.Add(this.ElectronicLogisticInvoiceGrid);
			this.ElectronicLogisticInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElectronicLogisticInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ElectronicLogisticInvoiceGroupBox.Name = "ElectronicLogisticInvoiceGroupBox";
			this.ElectronicLogisticInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			this.ElectronicLogisticInvoiceGroupBox.TabIndex = 5;
			this.ElectronicLogisticInvoiceGroupBox.TabStop = false;
			// 
			// ElectronicLogisticInvoiceGrid
			// 
			this.ElectronicLogisticInvoiceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ElectronicLogisticInvoiceGrid, "FilteredInvoiceLines.ElectronicLogisticInvoiceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ElectronicLogisticInvoiceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ElectronicLogisticInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ElectronicLogisticInvoiceCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ElectronicLogisticInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ElectronicLogisticInvoiceCollection)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ElectronicLogisticInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ElectronicLogisticInvoiceCollection)).SyncRoot)).CSI_Quantity)));
			this.ElectronicLogisticInvoiceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ElectronicLogisticInvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ElectronicLogisticInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ElectronicLogisticInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ElectronicLogisticInvoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElectronicLogisticInvoiceGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.ElectronicLogisticInvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ElectronicLogisticInvoiceGrid.LayoutKey = resources.GetString("ElectronicLogisticInvoiceGrid.LayoutKey");
			this.ElectronicLogisticInvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ElectronicLogisticInvoiceGrid.Name = "ElectronicLogisticInvoiceGrid";
			this.ElectronicLogisticInvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 209, true);
			this.ElectronicLogisticInvoiceGrid.TabIndex = 2;
			// 
			// ElectronicLogisticInvoiceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ElectronicLogisticInvoiceGroupBox);
			this.Name = "ElectronicLogisticInvoiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ElectronicLogisticInvoiceGroupBox.ResumeLayout(false);
			this.ElectronicLogisticInvoiceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicLogisticInvoiceGrid)).EndInit();
			this.ElectronicLogisticInvoiceGrid.ResumeLayout(false);
			this.ElectronicLogisticInvoiceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox ElectronicLogisticInvoiceGroupBox;
		public ZArchitecture.ZGrid ElectronicLogisticInvoiceGrid;
	}
}
