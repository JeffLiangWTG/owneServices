#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicePrintingControl
	{
		public void FindButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			FindButton_Click(sender, e);
		}

		public void FetchButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			FetchButton_Click(sender, e);
		}

		public void ClearButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ClearButton_Click(sender, e);
		}

		public void UpdateGovtTaxInvoices_ForTestOnly(BusinessObject[] invoicesToUpdate)
		{
			UpdateGovtTaxInvoices(invoicesToUpdate);
		}

		public void PrintInvoices_ForTestOnly(object sender, EventArgs e)
		{
			PrintInvoices(sender, e);
		}

		public void PrintClassAInvoices_ForTestOnly(object sender, EventArgs e)
		{
			PrintClassAInvoices(sender, e);
		}

		public void UpdateClassAInvoices_ForTestOnly(object sender, EventArgs e)
		{
			UpdateClassAInvoices(sender, e);
		}

		public Business.JobInvoicing.JobARInvoicePrintingFilter InvoiceFilterObject_ForTestOnly
		{
			get { return InvoiceFilterObject; }
			set { InvoiceFilterObject = value; }
		}

		public void AmendWithCreditNote_ForTestOnly(object sender, EventArgs e)
		{
			AmendWithCreditNote(sender, e);
		}

		public void AmendWithInvoice_ForTestOnly(object sender, EventArgs e)
		{
			AmendWithInvoice(sender, e);
		}

		public Business.ARAP.Invoicing.Printing.InvoicePrintTask GetInvoicePrintTask_ForTestOnly(Business.Base.Transaction.TransactionHeader[] headers = null)
		{
			return GetInvoicePrintTask(headers);
		}

		public ZButton FetchButton_ForTestOnly
		{
			get { return FetchButton; }
			set { FetchButton = value; }
		}

		public void HandleOverrideTransactionDescription_ForTestOnly(object sender, EventArgs e)
		{
			HandleOverrideTransactionDescription(sender, e);
		}

		public void RefreshAfterAmending_ForTestOnly(Business.ARAP.Invoicing.InvoicingBase transaction)
		{
			RefreshAfterAmending(transaction);
		}

		public void UpdateComplianceNumber_ForTestOnly(object sender, EventArgs e)
		{
			UpdateComplianceNumber(sender, e);
		}

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;
	}
}

#endif
