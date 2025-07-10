using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class UAInvoiceForm : InvoiceForm, IButtonDeleteTextOverride
	{
		public UAInvoiceForm(InvoicingBase businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get
			{
				string result = Res.GetString("InvoiceForm|7413D01B-B661-4282-B3DD-CEB84A4CDA6F", "Unapproved AP Invoice");
				return result;
			}
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return false; }
		}

		void InvoiceDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
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
			// CashInvoiceOnCheckbox
			// 
			this.CashInvoiceOnCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 23, true);
			// 
			// PeriodApportionmentTabPage
			// 
			this.PeriodApportionmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 99, true);
			// 
			// ReceiptPaymentOuterPanel
			// 
			this.ReceiptPaymentOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 459, true);
			this.ReceiptPaymentOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 163, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 459, true);
			// 
			// InvoiceDetails
			// 
			this.InvoiceDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 333, true);
			// 
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 333, true);
			this.ChargeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 126, true);
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

		#region IButtonDeleteTextOverride Members

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("ebb192dd-35f8-4fe5-9682-288ee9c9ae06", "Reject"); }
		}

		#endregion
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

