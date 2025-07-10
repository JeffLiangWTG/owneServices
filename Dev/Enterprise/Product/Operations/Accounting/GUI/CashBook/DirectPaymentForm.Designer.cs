using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectPaymentForm
	{


		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.AutoAllocateZLabel = new ZLabel();
			this.AutoPrintZLabel = new ZLabel();
			this.AH_LocalExTaxAmountCalcFindBox.SuspendLayout();
			this.AH_OSTotalAmountCalcFindBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.AH_ReceiptTypeDropEdit.SuspendLayout();
			this.AH_PostDateDateEdit.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.ChequeBookFindBox.SuspendLayout();
			this.BankAccountsFindBox.SuspendLayout();
			this.AH_InvoiceDateDateEdit.SuspendLayout();
			this.zDropEditPlaceOfSupply.SuspendLayout();
			this.CashBookTransactionTabPage1.SuspendLayout();
			this.CashBookLineGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).BeginInit();
			this.CashBookLineBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AH_OSTotalAmountCalcFindBox
			// 
			this.AH_OSTotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|2a8aa5e7-675a-43f9-8ffb-e0deebf59c25", "Payment Amount");
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.AutoPrintZLabel);
			this.TopPanel.Controls.Add(this.AutoAllocateZLabel);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 128, true);
			this.TopPanel.Controls.SetChildIndex(this.AH_NumberOfSupportingDocumentsCalcEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.zDropEditPlaceOfSupply, 0);
			this.TopPanel.Controls.SetChildIndex(this.ChequeOrReferenceLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_InvoiceDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.BankAccountsFindBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.ChequeBookFindBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.ExchangeRateControl, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_DescTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_TransactionNumTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_ChequeOrReferenceTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_PostDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.AH_ReceiptTypeDropEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.AutoAllocateZLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.AutoPrintZLabel, 0);
			// 
			// AH_ReceiptTypeDropEdit
			// 
			this.AH_ReceiptTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|ce1700d5-87ec-474b-baad-eda4fa55bedf", "Payment Type");
			// 
			// AH_PostDateDateEdit
			// 
			this.AH_PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 9, true);
			// 
			// AH_TransactionNumTextBox
			// 
			this.AH_TransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|c2304704-f553-4fd7-a52a-d9d76fcb3379", "Direct Payment No.");
			// 
			// AH_InvoiceDateDateEdit
			// 
			this.AH_InvoiceDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|e7f9f046-b7e5-4321-b8de-794ac5b14c87", "Payment Date");
			// 
			// CashBookTransactionTabPage1
			// 
			this.CashBookTransactionTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|031c0efa-883c-4c00-8372-0120d2c7817d", "Payment");
			this.CashBookTransactionTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.CashBookTransactionTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 514, true);
			// 
			// CashBookLineGroupBox
			// 
			this.CashBookLineGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|0c1b2665-bf58-4ad2-916e-de3a0fe8196d", "Payment Details");
			this.CashBookLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 278, true);
			// 
			// CashBookLineBoundGrid
			// 
			this.CashBookLineBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 228, true);
			// 
			// AH_DrawerBranchTextBox
			// 
			this.AH_DrawerBranchTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|3f4cd977-29e4-46d9-b645-48d8e4a30618", "BSB Number");
			// 
			// AH_DrawerBankTextBox
			// 
			this.AH_DrawerBankTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|b8f5fac7-1b75-4975-940d-d565b6602f9f", "Account Number");
			// 
			// AH_ChequeDrawerTextBox
			// 
			this.AH_ChequeDrawerTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|9342164b-2d56-47db-b6d2-172ad9ed110d", "Payee Name");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 570, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 24, true);
			// 
			// AutoAllocateZLabel
			// 
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((DirectPayment)(null)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AutoAllocateZLabel, false);
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 54, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 66, true);
			this.AutoAllocateZLabel.TabIndex = 15;
			this.AutoAllocateZLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((DirectPayment)(null)).Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AutoPrintZLabel, false);
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 55, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.AutoPrintZLabel.TabIndex = 14;
			// 
			// DirectPaymentForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectPaymentForm|f6278681-7104-46e8-8d73-ed116585d5c8", "Direct Payment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 594, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 621, true);
			this.Name = "DirectPaymentForm";
			this.AH_LocalExTaxAmountCalcFindBox.ResumeLayout(true);
			this.AH_LocalExTaxAmountCalcFindBox.PerformLayout();
			this.AH_OSTotalAmountCalcFindBox.ResumeLayout(true);
			this.AH_OSTotalAmountCalcFindBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.AH_ReceiptTypeDropEdit.ResumeLayout(true);
			this.AH_ReceiptTypeDropEdit.PerformLayout();
			this.AH_PostDateDateEdit.ResumeLayout(true);
			this.AH_PostDateDateEdit.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.ChequeBookFindBox.ResumeLayout(true);
			this.ChequeBookFindBox.PerformLayout();
			this.BankAccountsFindBox.ResumeLayout(true);
			this.BankAccountsFindBox.PerformLayout();
			this.AH_InvoiceDateDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateDateEdit.PerformLayout();
			this.zDropEditPlaceOfSupply.ResumeLayout(true);
			this.zDropEditPlaceOfSupply.PerformLayout();
			this.CashBookTransactionTabPage1.ResumeLayout(false);
			this.CashBookTransactionTabPage1.PerformLayout();
			this.CashBookLineGroupBox.ResumeLayout(false);
			this.CashBookLineGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).EndInit();
			this.CashBookLineBoundGrid.ResumeLayout(false);
			this.CashBookLineBoundGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}