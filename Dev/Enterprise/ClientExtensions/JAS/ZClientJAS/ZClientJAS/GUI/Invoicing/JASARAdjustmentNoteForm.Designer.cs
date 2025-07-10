using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JASARAdjustmentNoteForm : AdjustmentNoteForm, IJXCExportForm
	{
		ZPanel zPanel1;

		new void InitializeComponent()
		{
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainTabControl.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 548, true);
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 521, true);
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.zPanel1);
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 548, true);
			this.ButtonsPanel.Controls.SetChildIndex(this.zPanel1, 0);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 526, true);
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.PostingButtonsUserControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(694, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 37, true);
			this.zPanel1.TabIndex = 7;
			// 
			// JASARAdjustmentNoteForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 585, true);
			this.Name = "JASARAdjustmentNoteForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void InvoiceDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ChargesAndApportionmentsTabControl.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.PeriodApportionmentTabPage.SuspendLayout();
			this.InvoiceDetailsTabPage.SuspendLayout();
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
			// ReceiptPaymentOuterPanel
			// 
			this.ReceiptPaymentOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 521, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 521, true);
			// 
			// InvoiceDetails
			// 
			this.InvoiceDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 395, true);
			// 
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 395, true);
			this.ChargesAndApportionmentsTabControl.ResumeLayout(false);
			this.ChargesAndApportionmentsTabControl.PerformLayout();
			this.LineChargesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.LineChargesTabPage_InitializeTab));
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
			this.PeriodApportionmentTabPage.ResumeLayout(false);
			this.PeriodApportionmentTabPage.PerformLayout();
			this.InvoiceDetailsTabPage.PerformLayout();
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

		void LineChargesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.LineChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.LineChargesGrid.ResumeLayout(false);
			this.LineChargesGrid.PerformLayout();
		}
	}
}
