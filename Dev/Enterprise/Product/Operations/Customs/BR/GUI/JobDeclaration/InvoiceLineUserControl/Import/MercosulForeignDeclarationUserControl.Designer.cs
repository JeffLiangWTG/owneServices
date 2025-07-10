
namespace Enterprise.Customs.BR.GUI
{
	partial class MercosulForeignDeclarationUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MercosulForeignDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MercosulForeignDeclarationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TypeCertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MercosulForeignDeclarationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).BeginInit();
			this.MercosulForeignDeclarationGrid.SuspendLayout();
			this.TypeCertificateGroupBox.SuspendLayout();
			this.CertificateTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// MercosulForeignDeclarationGroupBox
			// 
			this.MercosulForeignDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("3505867f-42aa-4cb7-9b32-d714797b56aa", "MERCOSUR – Foreign Declaration");
			this.MercosulForeignDeclarationGroupBox.Controls.Add(this.MercosulForeignDeclarationGrid);
			this.MercosulForeignDeclarationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MercosulForeignDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.MercosulForeignDeclarationGroupBox.Name = "MercosulForeignDeclarationGroupBox";
			this.MercosulForeignDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 169, true);
			this.MercosulForeignDeclarationGroupBox.TabIndex = 5;
			this.MercosulForeignDeclarationGroupBox.TabStop = false;
			// 
			// MercosulForeignDeclarationGrid
			// 
			this.MercosulForeignDeclarationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MercosulForeignDeclarationGrid, "FilteredInvoiceLines.MercosulForeignDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_Quantity3)));
			this.MercosulForeignDeclarationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_RN_NKCountryCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ItemNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Quantity3";
			zCalcEditColumnStyleInfo3.Decimals = 5;
			zCalcEditColumnStyleInfo3.MaxValue = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MercosulForeignDeclarationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MercosulForeignDeclarationGrid.GridId = "9AB14A68-7E10-4D36-800C-81797A66E935";
			this.MercosulForeignDeclarationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MercosulForeignDeclarationGrid.LayoutKey = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MercosulForeignDeclarationGrid.Name = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 150, true);
			this.MercosulForeignDeclarationGrid.TabIndex = 2;
			// 
			// TypeCertificateGroupBox
			// 
			this.TypeCertificateGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("62c6b5b2-a092-4e4b-a055-4abd63664ef2", "Certificate");
			this.TypeCertificateGroupBox.Controls.Add(this.CertificateTypeDropEdit);
			this.TypeCertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TypeCertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TypeCertificateGroupBox.Name = "TypeCertificateGroupBox";
			this.TypeCertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 59, true);
			this.TypeCertificateGroupBox.TabIndex = 6;
			this.TypeCertificateGroupBox.TabStop = false;
			// 
			// CertificateTypeDropEdit
			// 
			this.CertificateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateTypeDropEdit, "FilteredInvoiceLines.MercosulForeignDeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MercosulForeignDeclarationType)));
			this.CertificateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 19, true);
			this.CertificateTypeDropEdit.Name = "CertificateTypeDropEdit";
			this.CertificateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 20, true);
			this.CertificateTypeDropEdit.TabIndex = 0;
			// 
			// MercosulForeignDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MercosulForeignDeclarationGroupBox);
			this.Controls.Add(this.TypeCertificateGroupBox);
			this.Name = "MercosulForeignDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MercosulForeignDeclarationGroupBox.ResumeLayout(false);
			this.MercosulForeignDeclarationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).EndInit();
			this.MercosulForeignDeclarationGrid.ResumeLayout(false);
			this.MercosulForeignDeclarationGrid.PerformLayout();
			this.TypeCertificateGroupBox.ResumeLayout(false);
			this.TypeCertificateGroupBox.PerformLayout();
			this.CertificateTypeDropEdit.ResumeLayout(true);
			this.CertificateTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.ZGrid MercosulForeignDeclarationGrid;
		private ZArchitecture.GUI.ZGroupBox TypeCertificateGroupBox;
		private ZArchitecture.GUI.ZDropEdit CertificateTypeDropEdit;
		internal ZArchitecture.GUI.ZGroupBox MercosulForeignDeclarationGroupBox;
	}
}
