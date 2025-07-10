namespace Enterprise.Customs.BR.GUI
{
	partial class ComplementaryLogisticInvoiceUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComplementaryLogisticInvoiceUserControl));
			this.ComplementaryLogisticInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComplementaryLogisticInvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComplementaryLogisticInvoiceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComplementaryLogisticInvoiceGrid)).BeginInit();
			this.ComplementaryLogisticInvoiceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ComplementaryLogisticInvoiceGroupBox
			//
			this.ComplementaryLogisticInvoiceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("90a65599-cc9b-430f-9d80-506e026c4588", "Complementary Logistic Invoices");
			this.ComplementaryLogisticInvoiceGroupBox.Controls.Add(this.ComplementaryLogisticInvoiceGrid);
			this.ComplementaryLogisticInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplementaryLogisticInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplementaryLogisticInvoiceGroupBox.Name = "ComplementaryLogisticInvoiceGroupBox";
			this.ComplementaryLogisticInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			this.ComplementaryLogisticInvoiceGroupBox.TabIndex = 5;
			this.ComplementaryLogisticInvoiceGroupBox.TabStop = false;
			// 
			// ElectronicLogisticInvoiceGrid
			// 
			this.ComplementaryLogisticInvoiceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplementaryLogisticInvoiceGrid, "FilteredInvoiceLines.ComplementaryLogisticInvoiceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ComplementaryLogisticInvoiceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ComplementaryLogisticInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ComplementaryLogisticInvoiceCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ComplementaryLogisticInvoice)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ComplementaryLogisticInvoiceCollection)).SyncRoot)).CSI_LineNo)));
			this.ComplementaryLogisticInvoiceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ComplementaryLogisticInvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComplementaryLogisticInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComplementaryLogisticInvoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplementaryLogisticInvoiceGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.ComplementaryLogisticInvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplementaryLogisticInvoiceGrid.LayoutKey = resources.GetString("ComplementaryLogisticInvoiceGrid.LayoutKey");
			this.ComplementaryLogisticInvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ComplementaryLogisticInvoiceGrid.Name = "ComplementaryLogisticInvoiceGrid";
			this.ComplementaryLogisticInvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 209, true);
			this.ComplementaryLogisticInvoiceGrid.TabIndex = 2;
			// 
			// ComplementaryLogisticInvoiceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplementaryLogisticInvoiceGroupBox);
			this.Name = "ElectronicLogisticInvoiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComplementaryLogisticInvoiceGroupBox.ResumeLayout(false);
			this.ComplementaryLogisticInvoiceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComplementaryLogisticInvoiceGrid)).EndInit();
			this.ComplementaryLogisticInvoiceGrid.ResumeLayout(false);
			this.ComplementaryLogisticInvoiceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox ComplementaryLogisticInvoiceGroupBox;
		public ZArchitecture.ZGrid ComplementaryLogisticInvoiceGrid;
	}
}
