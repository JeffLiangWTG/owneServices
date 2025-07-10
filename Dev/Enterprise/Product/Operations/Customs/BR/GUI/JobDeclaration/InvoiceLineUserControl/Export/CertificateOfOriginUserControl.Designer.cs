
using CargoWise.Types;

namespace Enterprise.Customs.BR.GUI
{
	partial class CertificateOfOriginUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertificateOfOriginUserControl));
			this.CertificateOfOriginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateOfOriginGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateOfOriginGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificateOfOriginGrid)).BeginInit();
			this.CertificateOfOriginGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// CertificateOfOriginGroupBox
			//
			this.CertificateOfOriginGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("457F95CC-7B7F-4071-9FF2-8CBF7DD37397", "CCPTC/CCROM");
			this.CertificateOfOriginGroupBox.Controls.Add(this.CertificateOfOriginGrid);
			this.CertificateOfOriginGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificateOfOriginGroupBox.Name = "CertificateOfOriginGroupBox";
			this.CertificateOfOriginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			this.CertificateOfOriginGroupBox.TabIndex = 5;
			this.CertificateOfOriginGroupBox.TabStop = false;
			this.CertificateOfOriginGroupBox.Text = ZString.Empty;
			// 
			// CertificateOfOriginGrid
			// 
			this.CertificateOfOriginGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CertificateOfOriginGrid, "FilteredInvoiceLines.CertificateOfOriginCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_Quantity3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CertificateOfOrigin)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCollection)).SyncRoot)).CSI_ItemNumber)));
			this.CertificateOfOriginGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "TariffCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "UQ";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 5;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.CertificateOfOriginGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CertificateOfOriginGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CertificateOfOriginGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CertificateOfOriginGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CertificateOfOriginGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CertificateOfOriginGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateOfOriginGrid.GridId = "87B00C58-3DBA-4C25-B382-7B3F1427D06E";
			this.CertificateOfOriginGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificateOfOriginGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CertificateOfOriginGrid.Name = "CertificateOfOriginGrid";
			this.CertificateOfOriginGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 209, true);
			this.CertificateOfOriginGrid.TabIndex = 2;
			// 
			// CertificateOfOriginUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CertificateOfOriginGroupBox);
			this.Name = "CertificateOfOriginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateOfOriginGroupBox.ResumeLayout(false);
			this.CertificateOfOriginGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificateOfOriginGrid)).EndInit();
			this.CertificateOfOriginGrid.ResumeLayout(false);
			this.CertificateOfOriginGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CertificateOfOriginGroupBox;
		public ZArchitecture.ZGrid CertificateOfOriginGrid;
	}
}
