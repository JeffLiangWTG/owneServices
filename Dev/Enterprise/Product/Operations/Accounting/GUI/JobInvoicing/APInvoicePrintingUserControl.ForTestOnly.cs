#if DEBUG

using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APInvoicePrintingUserControl
	{
		public ZForm AmendingForm_ForTestOnly;
		public ZForm EditForm_ForTestOnly;

		public static MultilingualString PrintTransactionMenuText_ForTestOnly => PrintTransactionMenuText;

		public static MultilingualString PrintMatchDocMenuText_ForTestOnly => PrintMatchDocMenuText;

		public static MultilingualString PrintSelfBillingInvoiceMenuText_ForTestOnly => PrintSelfBillingInvoiceMenuText;

		public JobAPInvoicePrintingFilter InvoiceFilterObject_ForTestOnly
		{
			get { return InvoiceFilterObject; }
			set { InvoiceFilterObject = value; }
		}

		public void AmendWithCreditNote_ForTestOnly(object sender, EventArgs e)
		{
			AmendWithCreditNote(sender, e);
		}

		public TransactionHeader SelectedTransactionInInvoicesGrid_ForTestOnly => SelectedTransactionInInvoicesGrid;

		public void FindButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			FindButton_Click(sender, e);
		}

		public void ClearButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ClearButton_Click(sender, e);
		}

		public void UpdateComplianceNumber_ForTestOnly(object sender, EventArgs e)
		{
			UpdateComplianceNumber(sender, e);
		}
	}
}

#endif
