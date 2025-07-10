namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class OverrideTransactionBranchAndDepartmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo organisationFindBoxColumnStyleInfo = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo orgFullNameTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo transactionTypeTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo transactionCategoryTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo transactionNumberTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo jobFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo currencyCodeFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo osAmountCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo taxAmountCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo osExTaxAmountCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo branchGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo departmentGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit(); 
			this.SuspendLayout();
			// 
			// InvoicesGrid
			// 
			organisationFindBoxColumnStyleInfo.ColumnName = "AH_OH";
			orgFullNameTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceAddressContactForm|e3b44aa6-1f38-4996-b283-f1dca8f7e9a0", "Account Full Name");
			orgFullNameTextBoxColumnStyleInfo.ColumnName = "HeaderFullName";
			orgFullNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			transactionTypeTextBoxColumnStyleInfo.ColumnName = "AH_TransactionType";
			transactionTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			transactionCategoryTextBoxColumnStyleInfo.ColumnName = "AH_TransactionCategory";
			transactionCategoryTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			transactionNumberTextBoxColumnStyleInfo.ColumnName = "AH_TransactionNum";
			jobFindBoxColumnStyleInfo.ColumnName = "AH_JH";
			currencyCodeFindBoxColumnStyleInfo.ColumnName = "AH_RX_NKTransactionCurrency";
			currencyCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);			
			osAmountCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			osAmountCalcEditColumnStyleInfo.ColumnName = "AH_OSTotalAmount";
			descriptionTextBoxColumnStyleInfo.ColumnName = "AH_Desc";
			descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			taxAmountCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			taxAmountCalcEditColumnStyleInfo.ColumnName = "AH_OSTaxAmount";
			taxAmountCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			osExTaxAmountCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			osExTaxAmountCalcEditColumnStyleInfo.ColumnName = "AH_OSExTaxAmount";
			osExTaxAmountCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			branchGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a10f1d7c-7856-4ae2-a0b1-73005c5f8d7e", "Branch");
			branchGuidFindBoxColumnStyleInfo.ColumnName = "AH_GB";
			branchGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			departmentGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ba62cd9-c817-42cb-894f-b2cf77c8a7d7", "Dept.");
			departmentGuidFindBoxColumnStyleInfo.ColumnName = "AH_GE";
			departmentGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InvoicesGrid.ColumnStyles.Add(organisationFindBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(orgFullNameTextBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(transactionTypeTextBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(transactionCategoryTextBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(transactionNumberTextBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(jobFindBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(currencyCodeFindBoxColumnStyleInfo);			
			this.InvoicesGrid.ColumnStyles.Add(osAmountCalcEditColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(descriptionTextBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(taxAmountCalcEditColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(osExTaxAmountCalcEditColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(branchGuidFindBoxColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(departmentGuidFindBoxColumnStyleInfo);
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 185, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 24, true);
			// 
			// OverrideInvoiceAddressContactForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cdac8754-7a55-4133-bf90-55c1915ebc70", "Override Transaction Branch / Department");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 245, true);
			this.Name = "OverrideTransactionBranchAndDepartmentForm";
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
