using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 707, true);
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|e594bf95-ae92-467d-9a81-891a6255a78b", "Invoice Details");
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 680, true);
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 707, true);
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 37, true);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 4, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 685, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ARInvoice);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ARInvoice)(null)).IsInvoiceReceiptPayment)));
			// 
			// InvoiceForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|99f5fa03-e202-43b6-9048-96195f31957f", "Base Invoicing Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 744, true);
			this.DataSourceType = typeof(ARInvoice);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.ARInvoice";
			this.DoubleBuffered = true;
			this.Name = "InvoiceForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void InvoiceDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CashInvoiceOnCheckbox = new ZCheckBox();
			this.ReceiptPaymentPanel = new ZPanel();
			this.ChargesAndApportionmentsTabControl.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.InvoiceDetailsTabPage.SuspendLayout();
			this.ReceiptPaymentOuterPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.InvoiceDetails.SuspendLayout();
			this.AH_LocalTotalAmountCalcEdit.SuspendLayout();
			this.AH_LocalTaxAmountCalcEdit.SuspendLayout();
			this.AH_OSTotalAmountCalcEdit.SuspendLayout();
			this.AH_OSTaxAmountCalcEdit.SuspendLayout();
			this.AH_LocalWHTAmountCalcEdit.SuspendLayout();
			this.AH_OSWHTAmountCalcEdit.SuspendLayout();
			this.ChargeDetailsPanel.SuspendLayout();
			this.AH_LocalExtraTaxAmountCalcEdit.SuspendLayout();
			// 
			// PeriodApportionmentTabPage
			// 
			this.PeriodApportionmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 99, true);
			// 
			// ReceiptPaymentOuterPanel
			// 
			this.ReceiptPaymentOuterPanel.Controls.Add(this.ReceiptPaymentPanel);
			this.ReceiptPaymentOuterPanel.Controls.Add(this.CashInvoiceOnCheckbox);
			this.ReceiptPaymentOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 517, true);
			this.ReceiptPaymentOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 163, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 517, true);
			// 
			// InvoiceDetails
			// 
			this.InvoiceDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 391, true);
			// 
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 391, true);
			this.ChargeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 126, true);
			// 
			// CashInvoiceOnCheckbox
			// 
			this.BindingSource.SetBindingMember(this.CashInvoiceOnCheckbox, "IsInvoiceReceiptPayment");
			this.CashInvoiceOnCheckbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|07f65acc-15a4-41f1-a938-2de5c44ef863", "Cash Invoice - Please tick the box if you want to enter the receipt / payment details immediately.");
			this.CashInvoiceOnCheckbox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CashInvoiceOnCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CashInvoiceOnCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashInvoiceOnCheckbox.Name = "CashInvoiceOnCheckbox";
			this.CashInvoiceOnCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 23, true);
			this.CashInvoiceOnCheckbox.TabIndex = 13;
			// 
			// ReceiptPaymentPanel
			// 
			this.ReceiptPaymentPanel.AutoScroll = true;
			this.ReceiptPaymentPanel.AutoSize = true;
			this.ReceiptPaymentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ReceiptPaymentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptPaymentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ReceiptPaymentPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 140, true);
			this.ReceiptPaymentPanel.Name = "ReceiptPaymentPanel";
			this.ReceiptPaymentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 140, true);
			this.ReceiptPaymentPanel.TabIndex = 14;
			this.ChargesAndApportionmentsTabControl.ResumeLayout(false);
			this.ChargesAndApportionmentsTabControl.PerformLayout();
			this.LineChargesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LineChargesTabPage_InitializeTab));
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
			this.InvoiceDetailsTabPage.PerformLayout();
			this.ReceiptPaymentOuterPanel.ResumeLayout(false);
			this.ReceiptPaymentOuterPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.InvoiceDetails.ResumeLayout(true);
			this.InvoiceDetails.PerformLayout();
			this.AH_LocalTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalTotalAmountCalcEdit.PerformLayout();
			this.AH_LocalTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalTaxAmountCalcEdit.PerformLayout();
			this.AH_OSTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_OSTotalAmountCalcEdit.PerformLayout();
			this.AH_OSTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_OSTaxAmountCalcEdit.PerformLayout();
			this.AH_LocalWHTAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalWHTAmountCalcEdit.PerformLayout();
			this.AH_OSWHTAmountCalcEdit.ResumeLayout(true);
			this.AH_OSWHTAmountCalcEdit.PerformLayout();
			this.ChargeDetailsPanel.ResumeLayout(false);
			this.ChargeDetailsPanel.PerformLayout();
			this.AH_LocalExtraTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalExtraTaxAmountCalcEdit.PerformLayout();
			this.InvoiceDetailsTabPage.ResumeLayout(true);
		}
		#endregion

	}
}