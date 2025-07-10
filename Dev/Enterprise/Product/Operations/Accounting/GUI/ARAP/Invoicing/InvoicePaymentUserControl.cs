using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Summary description for InvoicePaymentUserControl.
	/// </summary>
	public partial class InvoicePaymentUserControl : ZUserControl
	{
		public InvoicePaymentUserControl()
		{
			fReadOnly = false;
			InitializeComponent();
			SetDataSourceBinding(ReceiptPaymentAH_ChequeOrReferenceTextbox.GetExtension<LabelCaptionRenderer>(), "Caption", "ReceiptPaymentAH_ChequeReferenceLabel", true);
			this.BindingContextChanged += InvoicePaymentUserControl_BindingContextChanged;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			APInvoice invoice = BindingSource.DataSource as APInvoice;
			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				invoice.UpdatePaymentAddress(invoice.AH_OH);
			}
		}

		void InvoicePaymentUserControl_BindingContextChanged(object sender, EventArgs e)
		{
			if (oldInvoice != null)
			{
				oldInvoice.ReceiptPaymentAH_ReceiptTypeInfo.ValueChanged -= ReceiptPaymentAH_ReceiptTypeInfo_ValueChanged;
			}
			APInvoice invoice = BindingSource.DataSource as APInvoice;
			if (invoice != null)
			{
				oldInvoice = invoice;
				invoice.ReceiptPaymentAH_ReceiptTypeInfo.ValueChanged += ReceiptPaymentAH_ReceiptTypeInfo_ValueChanged;
				CardSecurityCodeTextBox.Visible = invoice.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard;
			}
		}

		public ZDateEdit ReceiptPaymentAH_InvoiceDateEdit;
		public ZDateEdit ReceiptPaymentAH_PostDateEdit;
		public ARAP.PaymentAddressWithContactControl AddressWithContactControl;

		APInvoice oldInvoice;

		protected override void Dispose(bool disposing)
		{
			if (disposing && oldInvoice != null)
			{
				oldInvoice.ReceiptPaymentAH_ReceiptTypeInfo.ValueChanged -= ReceiptPaymentAH_ReceiptTypeInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		void ReceiptPaymentAH_ReceiptTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			APInvoice invoice = BindingSource.DataSource as APInvoice;
			if (invoice != null)
			{
				CardSecurityCodeTextBox.Visible = invoice.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard;
			}
		}

		bool fReadOnly;
		public ZGroupBox ReceiptPaymentDetailsGroupbox;
		public ZTextBox ReceiptPaymentAH_ChequeOrReferenceTextbox;
		public ZGuidFindBox ReceiptPaymentAH_ABFindbox;
		public ZGuidFindBox ChequeBookGuidFindBox;
		public ZDropEdit ReceiptPaymentAH_ReceiptTypeDropEdit;
		public ZCalcEdit ReceiptPaymentAH_OSTotalAmountCalcEdit;
		public ZButton ClearHotChequeDetailsButton;
		public ZTextBox ReceiptPaymentAH_DescTextbox;
		ZLabel AutoAllocateZLabel;
		ZLabel AutoPrintZLabel;
		ZTextBox CardSecurityCodeTextBox;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;
				}
			}
		}

		void ClearImportedHotChequeButton_Click(object sender, EventArgs e)
		{
			((APInvoice)((InvoiceForm)ParentForm).BusinessEntity).ClearImportedHotCheque();
		}
	}
}

