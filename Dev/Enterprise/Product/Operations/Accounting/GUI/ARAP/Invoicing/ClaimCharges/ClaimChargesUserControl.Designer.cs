using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class ClaimChargesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoiceUserControl = new Enterprise.Accounting.GUI.ARAP.Invoicing.InvoiceUserControl();
			this.ChargeDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExtraTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AH_LocalExtraTaxAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AH_OSExtraTaxAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.LineTotalsGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AH_LocalTotalAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AH_LocalTaxAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AH_OSTotalAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AH_OSTaxAmountCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ChargesAndSubAccountsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.LineChargesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SubAccountsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SubAccountsControl = new SubAccountsControl();
			this.RestrictedLineChargesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LineChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChargeDetailsPanel.SuspendLayout();
			this.ExtraTaxGroupBox.SuspendLayout();
			this.LineTotalsGroupbox.SuspendLayout();
			this.ChargesAndSubAccountsTabControl.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.SubAccountsTabPage.SuspendLayout();
			this.SubAccountsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase);
			// 
			// InvoiceUserControl
			//
			this.InvoiceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceUserControl, ".");
			this.InvoiceUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceUserControl.Name = "InvoiceUserControl";
			this.InvoiceUserControl.ReadOnly = false;
			this.InvoiceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 678, true);
			this.InvoiceUserControl.TabIndex = 0;
			//
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.Controls.Add(this.ExtraTaxGroupBox);
			this.ChargeDetailsPanel.Controls.Add(this.LineTotalsGroupbox);
			this.ChargeDetailsPanel.Controls.Add(this.ChargesAndSubAccountsTabControl);
			this.ChargeDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 678, true);
			this.ChargeDetailsPanel.Name = "ChargeDetailsPanel";
			this.ChargeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 122, true);
			this.ChargeDetailsPanel.TabIndex = 19;
			// 
			// ExtraTaxGroupBox
			// 
			this.ExtraTaxGroupBox.Controls.Add(this.AH_LocalExtraTaxAmountCalcEdit);
			this.ExtraTaxGroupBox.Controls.Add(this.AH_OSExtraTaxAmountCalcEdit);
			this.ExtraTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ExtraTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 56, true);
			this.ExtraTaxGroupBox.Name = "ExtraTaxGroupBox";
			this.ExtraTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 32, true);
			this.ExtraTaxGroupBox.TabIndex = 21;
			this.ExtraTaxGroupBox.TabStop = false;
			this.ExtraTaxGroupBox.Visible = false;
			// 
			// AH_LocalExtraTaxAmountCalcEdit
			// 
			this.AH_LocalExtraTaxAmountCalcEdit.AllowDrop = true;
			this.AH_LocalExtraTaxAmountCalcEdit.BindToAmount = "AH_LocalExtraTaxAmount";
			this.AH_LocalExtraTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalExtraTaxAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AH_LocalExtraTaxAmountCalcEdit, false);
			this.AH_LocalExtraTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 9, true);
			this.AH_LocalExtraTaxAmountCalcEdit.Name = "AH_LocalExtraTaxAmountCalcEdit";
			this.AH_LocalExtraTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_LocalExtraTaxAmountCalcEdit.TabIndex = 34;
			// 
			// AH_OSExtraTaxAmountCalcEdit
			// 
			this.AH_OSExtraTaxAmountCalcEdit.AllowDrop = true;
			this.AH_OSExtraTaxAmountCalcEdit.BindToAmount = "AH_OSExtraTaxAmount";
			this.AH_OSExtraTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSExtraTaxAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSExtraTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|87132041-ae43-4c69-b1ad-d26e85578887", "QST Amount");
			this.AH_OSExtraTaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSExtraTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 9, true);
			this.AH_OSExtraTaxAmountCalcEdit.Name = "AH_OSExtraTaxAmountCalcEdit";
			this.AH_OSExtraTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_OSExtraTaxAmountCalcEdit.TabIndex = 33;
			// 
			// LineTotalsGroupbox
			// 
			this.LineTotalsGroupbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|30fb4f12-d303-4280-91f1-9025ed18c1ff", "Totals");
			this.LineTotalsGroupbox.Controls.Add(this.AH_LocalTotalAmountCalcEdit);
			this.LineTotalsGroupbox.Controls.Add(this.AH_LocalTaxAmountCalcEdit);
			this.LineTotalsGroupbox.Controls.Add(this.AH_OSTotalAmountCalcEdit);
			this.LineTotalsGroupbox.Controls.Add(this.AH_OSTaxAmountCalcEdit);
			this.LineTotalsGroupbox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LineTotalsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 0, true);
			this.LineTotalsGroupbox.Name = "LineTotalsGroupbox";
			this.LineTotalsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 56, true);
			this.LineTotalsGroupbox.TabIndex = 18;
			this.LineTotalsGroupbox.TabStop = false;
			// 
			// AH_LocalTotalAmountCalcEdit
			// 
			this.AH_LocalTotalAmountCalcEdit.AllowDrop = true;
			this.AH_LocalTotalAmountCalcEdit.BindToAmount = "AH_LocalTotalAmount";
			this.AH_LocalTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalTotalAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AH_LocalTotalAmountCalcEdit, false);
			this.AH_LocalTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 33, true);
			this.AH_LocalTotalAmountCalcEdit.Name = "AH_LocalTotalAmountCalcEdit";
			this.AH_LocalTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_LocalTotalAmountCalcEdit.TabIndex = 25;
			// 
			// AH_LocalTaxAmountCalcEdit
			// 
			this.AH_LocalTaxAmountCalcEdit.AllowDrop = true;
			this.AH_LocalTaxAmountCalcEdit.BindToAmount = "AH_LocalTaxAmount";
			this.AH_LocalTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalTaxAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AH_LocalTaxAmountCalcEdit, false);
			this.AH_LocalTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 11, true);
			this.AH_LocalTaxAmountCalcEdit.Name = "AH_LocalTaxAmountCalcEdit";
			this.AH_LocalTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_LocalTaxAmountCalcEdit.TabIndex = 24;
			// 
			// AH_OSTotalAmountCalcEdit
			// 
			this.AH_OSTotalAmountCalcEdit.AllowDrop = true;
			this.AH_OSTotalAmountCalcEdit.BindToAmount = "AH_OSTotalAmount";
			this.AH_OSTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSTotalAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2739bd4-53ad-4e72-87fe-1338e21b5eb3", "Invoice Amount");
			this.AH_OSTotalAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 33, true);
			this.AH_OSTotalAmountCalcEdit.Name = "AH_OSTotalAmountCalcEdit";
			this.AH_OSTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_OSTotalAmountCalcEdit.TabIndex = 21;
			// 
			// AH_OSTaxAmountCalcEdit
			// 
			this.AH_OSTaxAmountCalcEdit.AllowDrop = true;
			this.AH_OSTaxAmountCalcEdit.BindToAmount = "AH_OSTaxAmount";
			this.AH_OSTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSTaxAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6797e11f-c73c-45c7-b04a-e2edc6db1da7", "Tax Amount");
			this.AH_OSTaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 11, true);
			this.AH_OSTaxAmountCalcEdit.Name = "AH_OSTaxAmountCalcEdit";
			this.AH_OSTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_OSTaxAmountCalcEdit.TabIndex = 20;
			// 
			// ChargesAndSubAccountsTabControl
			// 
			this.ChargesAndSubAccountsTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.ChargesAndSubAccountsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ChargesAndSubAccountsTabControl.Controls.Add(this.LineChargesTabPage);
			this.ChargesAndSubAccountsTabControl.Controls.Add(this.SubAccountsTabPage);
			this.ChargesAndSubAccountsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargesAndSubAccountsTabControl.Name = "ChargesAndSubAccountsTabControl";
			this.ChargesAndSubAccountsTabControl.SelectedIndex = 0;
			this.ChargesAndSubAccountsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 126, true);
			this.ChargesAndSubAccountsTabControl.TabIndex = 17;
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|42e69979-7fc6-4ddf-be9d-25716058bcf8", "Line Charges");
			this.LineChargesTabPage.Controls.Add(this.LineChargesGrid);
			this.LineChargesTabPage.Controls.Add(this.RestrictedLineChargesLabel);
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineChargesTabPage.Name = "LineChargesTabPage";
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 122, true);
			this.LineChargesTabPage.TabIndex = 0;
			// 
			// RestrictedLineChargesLabel
			// 
			this.RestrictedLineChargesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RestrictedLineChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RestrictedLineChargesLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 93, true);
			this.RestrictedLineChargesLabel.Name = "RestrictedLineChargesLabel";
			this.RestrictedLineChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 93, true);
			this.RestrictedLineChargesLabel.TabIndex = 2;
			this.RestrictedLineChargesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.RestrictedLineChargesLabel.Visible = false;
			// 
			// LineChargesGrid
			// 
			this.LineChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LineChargesGrid, "Lines.LineCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).AL_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).AL_LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccTransactionLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(null)).Lines)).SyncRoot)).LineCharges)).SyncRoot)).AL_LineAmount)));
			this.LineChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|a0c0807d-b4d6-4243-8148-603df8f9f722", "Job");
			zTextBoxColumnStyleInfo3.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo4.Caption = "";
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|8c15263a-c160-40cb-bef1-dd318c128e46", "Charge");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_AC";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|fbad634e-be02-4434-844f-8605636212f4", "Type");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "AL_LineType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo5.Caption = "";
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("07c2ffa0-7e6e-4652-a269-81cf1bb786df", "Branch");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo6.Caption = "";
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|6f74515e-9c5e-415d-be4a-2520ac4158c6", "Dept");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|45816bf0-d076-4fc1-a3e4-642ae26786a5", "Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_LineAmount";
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LineChargesGrid.CopySelectedRowsAllowed = true;
			this.LineChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineChargesGrid.GridId = "f6a18787-ea2a-4919-b474-e6daf8c90892";
			this.LineChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineChargesGrid.LayoutKey = "LineChargesGrid";
			this.LineChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LineChargesGrid.Name = "LineChargesGrid";
			this.LineChargesGrid.ReadOnly = true;
			this.LineChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 103, true);
			this.LineChargesGrid.TabIndex = 0;
			// 
			// SubAccountsTabPage
			// 
			this.SubAccountsTabPage.Controls.Add(SubAccountsControl);
			this.SubAccountsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClaimChargesUserControl|fceae150-0f35-4bfd-a3e0-cbd12fa3856c", "Sub Accounts");
			this.SubAccountsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.SubAccountsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SubAccountsTabPage.Name = "SubAccountsTabPage";
			this.SubAccountsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SubAccountsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 80, true);
			this.SubAccountsTabPage.TabIndex = 1;
			// 
			// SubAccountsControl
			// 
			this.SubAccountsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubAccountsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubAccountsControl.Name = "SubAccountsControl";
			// 
			// ClaimChargesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceUserControl);
			this.Controls.Add(this.ChargeDetailsPanel);
			this.Name = "ClaimChargesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 800, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargeDetailsPanel.ResumeLayout(false);
			this.ExtraTaxGroupBox.ResumeLayout(false);
			this.LineTotalsGroupbox.ResumeLayout(false);
			this.ChargesAndSubAccountsTabControl.ResumeLayout(false);
			this.LineChargesTabPage.ResumeLayout(false);
			this.SubAccountsTabPage.ResumeLayout(false);
			this.SubAccountsControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
#if DEBUG 
		public 
#endif 
		InvoiceUserControl InvoiceUserControl;
		public Enterprise.ZArchitecture.GUI.ZPanel ChargeDetailsPanel;
		public Enterprise.ZArchitecture.GUI.ZGroupBox ExtraTaxGroupBox;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_LocalExtraTaxAmountCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_OSExtraTaxAmountCalcEdit;
		public Enterprise.ZArchitecture.ZGrid LineChargesGrid;
		public Enterprise.ZArchitecture.GUI.ZGroupBox LineTotalsGroupbox;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_LocalTotalAmountCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_LocalTaxAmountCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_OSTotalAmountCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox AH_OSTaxAmountCalcEdit;
		private ZArchitecture.ZLabel RestrictedLineChargesLabel;
		private ZTabControl ChargesAndSubAccountsTabControl;
		private ZTabPage LineChargesTabPage;
		private ZTabPage SubAccountsTabPage;
		private SubAccountsControl SubAccountsControl;
	}
}
