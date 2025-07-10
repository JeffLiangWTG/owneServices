namespace Enterprise.Accounting.Module
{
	partial class ChequeTransactionFilterStripControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBox_APB_BatchNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBox_APB_PaymentType = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEdit_APB_PaymentDate = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEdit_OSAmount = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBox_APB_RX_NKBatchCurrency = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox_APB_OH_DebtorOrCreditor = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox_APB_AB = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox_APB_GB = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBox_DebtorOrCreditorFullName = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBox_APB_AB_FundingBankAccount = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBox_APB_Description = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEdit_APB_ExchangeRate = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEdit_LocalAmount = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_BatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_PaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_PaymentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).AmountTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_RX_NKBatchCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_OH_DebtorOrCreditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).DebtorOrCreditorFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_AB_FundingBankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).APB_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader)(null)).LocalAmountTotal)));
			zTextBox_APB_BatchNumber.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|TransactionNumber", "Transaction Number", "Cheque Transaction Number", "");
			zTextBox_APB_BatchNumber.ColumnName = "APB_BatchNumber";
			zTextBox_APB_BatchNumber.IsCustomColumn = false;
			zTextBox_APB_BatchNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBox_APB_PaymentType.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|TransactionType", "Transaction Type");
			zTextBox_APB_PaymentType.ColumnName = "APB_PaymentType";
			zTextBox_APB_PaymentType.IsCustomColumn = false;
			zTextBox_APB_PaymentType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEdit_APB_PaymentDate.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|TransactionDate", "Transaction Date");
			zDateEdit_APB_PaymentDate.ColumnName = "APB_PaymentDate";
			zDateEdit_APB_PaymentDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEdit_APB_PaymentDate.IsCustomColumn = false;
			zDateEdit_APB_PaymentDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEdit_OSAmount.BindToDecimalPlaces = null;
			zCalcEdit_OSAmount.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|TotalAmount", "Total Amount");
			zCalcEdit_OSAmount.ColumnName = "AmountTotal";
			zCalcEdit_OSAmount.IsCustomColumn = false;
			zCalcEdit_OSAmount.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBox_APB_RX_NKBatchCurrency.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|Currency", "Currency");
			zTextBox_APB_RX_NKBatchCurrency.ColumnName = "APB_RX_NKBatchCurrency";
			zTextBox_APB_RX_NKBatchCurrency.IsCustomColumn = false;
			zTextBox_APB_RX_NKBatchCurrency.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBox_APB_OH_DebtorOrCreditor.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|DebtorCreditor", "Debtor/Creditor");
			zGuidFindBox_APB_OH_DebtorOrCreditor.ColumnName = "APB_OH_DebtorOrCreditor";
			zGuidFindBox_APB_OH_DebtorOrCreditor.IsCustomColumn = false;
			zGuidFindBox_APB_OH_DebtorOrCreditor.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBox_APB_AB.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|BankAccount", "Bank Account");
			zGuidFindBox_APB_AB.ColumnName = "APB_AB";
			zGuidFindBox_APB_AB.IsCustomColumn = false;
			zGuidFindBox_APB_AB.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBox_APB_GB.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|CreatingBranch", "Creating Branch");
			zGuidFindBox_APB_GB.ColumnName = "APB_GB";
			zGuidFindBox_APB_GB.IsCustomColumn = false;
			zGuidFindBox_APB_GB.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBox_APB_GB.IsVisible = false;
			zTextBox_DebtorOrCreditorFullName.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|DebtorCreditorFullName", "Debtor/Creditor Full Name");
			zTextBox_DebtorOrCreditorFullName.ColumnName = "DebtorOrCreditorFullName";
			zTextBox_DebtorOrCreditorFullName.IsCustomColumn = false;
			zTextBox_DebtorOrCreditorFullName.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBox_DebtorOrCreditorFullName.IsVisible = false;
			zGuidFindBox_APB_AB_FundingBankAccount.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|CashAccount", "Cash Account");
			zGuidFindBox_APB_AB_FundingBankAccount.ColumnName = "APB_AB_FundingBankAccount";
			zGuidFindBox_APB_AB_FundingBankAccount.IsCustomColumn = false;
			zGuidFindBox_APB_AB_FundingBankAccount.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBox_APB_AB_FundingBankAccount.IsVisible = false;
			zTextBox_APB_Description.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|Description", "Description");
			zTextBox_APB_Description.ColumnName = "APB_Description";
			zTextBox_APB_Description.IsCustomColumn = false;
			zTextBox_APB_Description.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBox_APB_Description.IsVisible = false;
			zCalcEdit_APB_ExchangeRate.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|ExchangeRate", "Exchange Rate");
			zCalcEdit_APB_ExchangeRate.ColumnName = "APB_ExchangeRate";
			zCalcEdit_APB_ExchangeRate.IsCustomColumn = false;
			zCalcEdit_APB_ExchangeRate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEdit_APB_ExchangeRate.IsVisible = false;
			zCalcEdit_LocalAmount.BindToDecimalPlaces = null;
			zCalcEdit_LocalAmount.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ChequeTransactionHeader|LocalTotalAmount", "Total in Local Currency");
			zCalcEdit_LocalAmount.ColumnName = "LocalAmountTotal";
			zCalcEdit_LocalAmount.IsCustomColumn = false;
			zCalcEdit_LocalAmount.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEdit_LocalAmount.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBox_APB_BatchNumber);
			this.grid.ColumnStyles.Add(zTextBox_APB_PaymentType);
			this.grid.ColumnStyles.Add(zDateEdit_APB_PaymentDate);
			this.grid.ColumnStyles.Add(zCalcEdit_OSAmount);
			this.grid.ColumnStyles.Add(zTextBox_APB_RX_NKBatchCurrency);
			this.grid.ColumnStyles.Add(zGuidFindBox_APB_OH_DebtorOrCreditor);
			this.grid.ColumnStyles.Add(zGuidFindBox_APB_AB);
			this.grid.ColumnStyles.Add(zGuidFindBox_APB_GB);
			this.grid.ColumnStyles.Add(zTextBox_DebtorOrCreditorFullName);
			this.grid.ColumnStyles.Add(zGuidFindBox_APB_AB_FundingBankAccount);
			this.grid.ColumnStyles.Add(zTextBox_APB_Description);
			this.grid.ColumnStyles.Add(zCalcEdit_APB_ExchangeRate);
			this.grid.ColumnStyles.Add(zCalcEdit_LocalAmount);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ChequeTransaction.ChequeTransactionHeader);
			// 
			// ChequeTransactionsFilterStripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ChequeTransactionsFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
