namespace Enterprise.Customs.BR.GUI
{
	partial class SuspensionDrawbackUserControl
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
		/// the contents of this method with the code editor.suspensionDrawbackInvoiceUserControl1 
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SuspensionDrawbackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SuspensionDrawbackGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.suspensionDrawbackInvoiceUserControl1 = new Enterprise.Customs.BR.GUI.SuspensionDrawbackInvoiceUserControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.suspensionDrawbackImportEntryDocumentUserControl3 = new Enterprise.Customs.BR.GUI.SuspensionDrawbackImportEntryDocumentUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspensionDrawbackGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackGrid)).BeginInit();
			this.SuspensionDrawbackGrid.SuspendLayout();
			this.suspensionDrawbackInvoiceUserControl1.SuspendLayout();
			this.suspensionDrawbackImportEntryDocumentUserControl3.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// SuspensionDrawbackGroupBox
			// 
			this.SuspensionDrawbackGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("A3B3C3F1-D7EE-4169-BC0B-4801E0507E1C", "Suspension Drawback");
			this.SuspensionDrawbackGroupBox.Controls.Add(this.SuspensionDrawbackGrid);
			this.SuspensionDrawbackGroupBox.Controls.Add(this.splitter2);
			this.SuspensionDrawbackGroupBox.Controls.Add(this.suspensionDrawbackInvoiceUserControl1);
			this.SuspensionDrawbackGroupBox.Controls.Add(this.splitter1);
			this.SuspensionDrawbackGroupBox.Controls.Add(this.suspensionDrawbackImportEntryDocumentUserControl3);
			this.SuspensionDrawbackGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SuspensionDrawbackGroupBox.Name = "SuspensionDrawbackGroupBox";
			this.SuspensionDrawbackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 338, true);
			this.SuspensionDrawbackGroupBox.TabIndex = 5;
			this.SuspensionDrawbackGroupBox.TabStop = false;
			// 
			// SuspensionDrawbackGrid
			// 
			this.SuspensionDrawbackGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SuspensionDrawbackGrid, "SuspensionDrawbackCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_IsSupplierBeneficiary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)).CSI_Value)));
			this.SuspensionDrawbackGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.ColumnName = "CSI_IsSupplierBeneficiary";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			tariffColumnStyleInfo1.ColumnName = "CSI_Tariff";
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SuspensionDrawbackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SuspensionDrawbackGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SuspensionDrawbackGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.SuspensionDrawbackGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SuspensionDrawbackGrid.LayoutKey = null;
			this.SuspensionDrawbackGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SuspensionDrawbackGrid.Name = "SuspensionDrawbackGrid";
			this.SuspensionDrawbackGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 94, true);
			this.SuspensionDrawbackGrid.TabIndex = 2;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.DoNotSaveSplitterLayout = false;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 110, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 7, true);
			this.splitter2.TabIndex = 2;
			this.splitter2.TabStop = false;
			// 
			// suspensionDrawbackInvoiceUserControl1
			//
			this.suspensionDrawbackInvoiceUserControl1.CaptionRenderingEnabled = true;
			this.suspensionDrawbackInvoiceUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.suspensionDrawbackInvoiceUserControl1, "SuspensionDrawbackCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)))));
			this.suspensionDrawbackInvoiceUserControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.suspensionDrawbackInvoiceUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 117, true);
			this.suspensionDrawbackInvoiceUserControl1.Name = "suspensionDrawbackInvoiceUserControl1";
			this.suspensionDrawbackInvoiceUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 103, true);
			this.suspensionDrawbackInvoiceUserControl1.TabIndex = 3;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 220, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 3, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// suspensionDrawbackImportEntryDocumentUserControl3
			//
			this.suspensionDrawbackImportEntryDocumentUserControl3.CaptionRenderingEnabled = true;
			this.suspensionDrawbackImportEntryDocumentUserControl3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.suspensionDrawbackImportEntryDocumentUserControl3, "SuspensionDrawbackCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((Enterprise.Customs.BR.Business.SuspensionDrawback)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SuspensionDrawbackCollection)).SyncRoot)))));
			this.suspensionDrawbackImportEntryDocumentUserControl3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.suspensionDrawbackImportEntryDocumentUserControl3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 223, true);
			this.suspensionDrawbackImportEntryDocumentUserControl3.Name = "suspensionDrawbackImportEntryDocumentUserControl3";
			this.suspensionDrawbackImportEntryDocumentUserControl3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 112, true);
			this.suspensionDrawbackImportEntryDocumentUserControl3.TabIndex = 5;
			// 
			// SuspensionDrawbackUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SuspensionDrawbackGroupBox);
			this.Name = "SuspensionDrawbackUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 338, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SuspensionDrawbackGroupBox.ResumeLayout(false);
			this.SuspensionDrawbackGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SuspensionDrawbackGrid)).EndInit();
			this.SuspensionDrawbackGrid.ResumeLayout(false);
			this.SuspensionDrawbackGrid.PerformLayout();
			this.suspensionDrawbackInvoiceUserControl1.ResumeLayout(true);
			this.suspensionDrawbackInvoiceUserControl1.PerformLayout();
			this.suspensionDrawbackImportEntryDocumentUserControl3.ResumeLayout(true);
			this.suspensionDrawbackImportEntryDocumentUserControl3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SuspensionDrawbackGroupBox;
		public ZArchitecture.ZGrid SuspensionDrawbackGrid;
		private SuspensionDrawbackInvoiceUserControl suspensionDrawbackInvoiceUserControl1;
		private SuspensionDrawbackImportEntryDocumentUserControl suspensionDrawbackImportEntryDocumentUserControl3;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private CargoWise.Windows.UI.KSplitter splitter2;
	}
}
