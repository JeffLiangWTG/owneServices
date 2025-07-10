using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class AdjustmentNoteForm
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
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 549, true);
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AdjustmentNoteForm|e594bf95-ae92-467d-9a81-891a6255a78b", "Adjustment Note Details");
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 522, true);
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 549, true);
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 37, true);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 4, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ARAdjustmentNote);
			// 
			// AdjustmentNoteForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 586, true);
			this.DataSourceType = typeof(ARAdjustmentNote);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.ARAdjustmentNote";
			this.DoubleBuffered = true;
			this.Name = "AdjustmentNoteForm";
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
			this.ChargesAndApportionmentsTabControl.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
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
			// PeriodApportionmentTabPage
			// 
			this.PeriodApportionmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 99, true);
			// 
			// ReceiptPaymentOuterPanel
			// 
			this.ReceiptPaymentOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 522, true);
			this.ReceiptPaymentOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 0, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 522, true);
			// 
			// InvoiceDetails
			// 
			this.InvoiceDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 396, true);
			// 
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 396, true);
			this.ChargeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 126, true);
			// 
			// ExtraTaxGroupBox
			// 
			this.ChargesAndApportionmentsTabControl.ResumeLayout(false);
			this.ChargesAndApportionmentsTabControl.PerformLayout();
			this.LineChargesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LineChargesTabPage_InitializeTab));
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
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
		#endregion

	}
}