
namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class InvoicingBaseImportApportionmentForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ConsolCostsToImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolCostsToImportGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolCostsToImportGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostsToImportGrid)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 366, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter);
			// 
			// ConsolCostsToImportGroupBox
			// 
			this.ConsolCostsToImportGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolCostsToImportGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|e98f5da1-b7f5-4e1e-a97a-34df27274ce7", "Consol Costs");
			this.ConsolCostsToImportGroupBox.Controls.Add(this.ConsolCostsToImportGrid);
			this.ConsolCostsToImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolCostsToImportGroupBox.Name = "ConsolCostsToImportGroupBox";
			this.ConsolCostsToImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 319, true);
			this.ConsolCostsToImportGroupBox.TabIndex = 0;
			this.ConsolCostsToImportGroupBox.TabStop = false;
			// 
			// ConsolCostsToImportGrid
			// 
			this.ConsolCostsToImportGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsolCostsToImportGrid, "CostsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).Lookups.ChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_AT_TaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_A9_VATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).Lookups.TaxRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_OSCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_OSGSTAmount_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_Calc_OSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_ApportionmentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_InvoiceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).Lookups.Creditors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_ParentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).Lookups.Consols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostForImporting)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter)(null)).CostsCollection)).SyncRoot)).E6_GB_CostTaxBranch)));
			this.ConsolCostsToImportGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "E6_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "E6_AT_TaxRate";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "E6_A9_VATClass";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|5a18414e-bc1e-4bb4-9a72-993c8283b55e", "O S Amt");
			zCalcEditColumnStyleInfo1.ColumnName = "E6_OSCostAmount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|365339bc-c15a-4746-8d29-ddd9e3bf22e9", "OS Tax Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "E6_OSGSTAmount_Calc";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|462e4c30-204d-44bf-875e-f5a4f56c7c19", "OS Total Amt");
			zCalcEditColumnStyleInfo3.ColumnName = "E6_Calc_OSTotalAmount";
			zTextBoxColumnStyleInfo1.ColumnName = "E6_ApportionmentMethod";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|93d5628e-7673-43b4-b3af-973bf85d4390", "Inv. Date", "Invoice Date.");
			zDateEditColumnStyleInfo1.ColumnName = "E6_InvoiceDate";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|707617f6-bf21-4707-8560-3b43dc8c0ff2", "Invoice Num.", "Invoice Number");
			zTextBoxColumnStyleInfo2.ColumnName = "E6_InvoiceNum";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "E6_OH_Creditor";
			zTextBoxColumnStyleInfo3.ColumnName = "E6_RX_NKCurrency";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "E6_ExchangeRate";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "E6_ParentID";
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|7A1CE9DA-9362-45B5-97E3-C787B5706A76", "Cost Tax Branch");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "E6_GB_CostTaxBranch";
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ConsolCostsToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.ConsolCostsToImportGrid.GridId = "88d19c9e-f55b-496a-81e8-26a4c6bdf947";
			this.ConsolCostsToImportGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostsToImportGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolCostsToImportGrid.LayoutKey = "ConsolCostsToImportGrid";
			this.ConsolCostsToImportGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConsolCostsToImportGrid.Name = "ConsolCostsToImportGrid";
			this.ConsolCostsToImportGrid.ReadOnly = true;
			this.ConsolCostsToImportGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ConsolCostsToImportGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 300, true);
			this.ConsolCostsToImportGrid.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.CancelImportButton);
			this.zPanel1.Controls.Add(this.ImportButton);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 322, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 38, true);
			this.zPanel1.TabIndex = 1;
			// 
			// CancelImportButton
			// 
			this.CancelImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelImportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|c144ccee-62e6-4fcc-9e2f-b56ba2c02f61", "&Cancel");
			this.CancelImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 8, true);
			this.CancelImportButton.Name = "CancelImportButton";
			this.CancelImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.CancelImportButton.TabIndex = 0;
			this.CancelImportButton.UseVisualStyleBackColor = true;
			this.CancelImportButton.Click += new System.EventHandler(this.CancelImportButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|41a8413e-2912-4212-be5e-2879db2c35dd", "&Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 8, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.ImportButton.TabIndex = 0;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// APInvoiceImportApportionmentForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 390, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicingBaseImportApportionmentForm|7efb5439-556f-4daf-9d35-983353f59e64", "Import Consol Costs");
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.ConsolCostsToImportGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBaseConsolCostImporter";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 410, true);
			this.Name = "APInvoiceImportApportionmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConsolCostsToImportGroupBox, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolCostsToImportGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostsToImportGrid)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolCostsToImportGroupBox;
		public Enterprise.ZArchitecture.ZGrid ConsolCostsToImportGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZButton CancelImportButton;
		private Enterprise.ZArchitecture.GUI.ZButton ImportButton;
	}
}
