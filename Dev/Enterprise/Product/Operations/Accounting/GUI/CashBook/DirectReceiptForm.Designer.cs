using System;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectReceiptForm
	{


		#region Windows Form Designer generated code
		new void InitializeComponent()
		{
			this.TopPanel.SuspendLayout();
			this.CashBookTransactionTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// AH_OSTotalAmountCalcFindBox
			//
			this.AH_OSTotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|2a8aa5e7-675a-43f9-8ffb-e0deebf59c25", "Receipt Amount");
			//
			// AH_DrawerBranchTextBox
			//
			this.AH_DrawerBranchTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|3f4cd977-29e4-46d9-b645-48d8e4a30618", "Branch");
			//
			// AH_DrawerBankTextBox
			//
			this.AH_DrawerBankTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|b8f5fac7-1b75-4975-940d-d565b6602f9f", "Bank");
			//
			// AH_ChequeDrawerTextBox
			//
			this.AH_ChequeDrawerTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|9342164b-2d56-47db-b6d2-172ad9ed110d", "Drawer");
			//
			// AH_TransactionNumTextBox
			//
			this.AH_TransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|c2304704-f553-4fd7-a52a-d9d76fcb3379", "Direct Receipt No.");
			//
			// AH_InvoiceDateDateEdit
			//
			this.AH_InvoiceDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|e7f9f046-b7e5-4321-b8de-794ac5b14c87", "Receipt Date");
			//
			// ChequeOrReferenceLabel
			//
			this.ChequeOrReferenceLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|b2168acc-19db-497a-b76f-75d1ec0077ef", "Check No.:");
			// 
			// CashBookTransactionTabPage1
			// 
			this.CashBookTransactionTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|5764aafa-cb04-45a4-b326-a24f3a724390", "Receipt");
			this.CashBookTransactionTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 490, true);
			// 
			// CashBookLineBoundGrid
			// 
			this.CashBookLineBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 209, true);
			//
			// CashBookLineGroupBox
			//
			this.CashBookLineGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|0c1b2665-bf58-4ad2-916e-de3a0fe8196d", "Receipt Details");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 549, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DirectReceipt);
			// 
			// DirectReceiptForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectReceiptForm|f6278681-7104-46e8-8d73-ed116585d5c8", "Direct Receipt");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 573, true);
			this.DataSourceType = typeof(DirectReceipt);
			this.Name = "DirectReceiptForm";
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.CashBookTransactionTabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}