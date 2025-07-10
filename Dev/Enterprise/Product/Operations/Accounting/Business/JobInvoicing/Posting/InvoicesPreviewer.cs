using System;
using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class InvoicesPreviewer : NonPersistentBusinessObject, IDisposable
	{
		public InvoicesPreviewer(TransactionCreatorHashtable transactions, JobInvoicingPostingOption postingOption, JobInvoicingSecurityHelper securityHelper = null)
		{
			this.PostingOption = postingOption;

			factory = new BusinessObjectFactory();
			fPreviewInvoices = new InvoicingBaseCollection(factory);
			if (transactions != null)
			{
				foreach (InvoicingBase transaction in transactions.Values)
				{
					fPreviewInvoices.Add(transaction);
				}
			}

			this.SecurityHelper = securityHelper;
		}

		public InvoicesPreviewer(ArrayList invoices, JobInvoicingPostingOption postingOption, JobInvoicingSecurityHelper securityHelper = null)
		{
			this.PostingOption = postingOption;

			factory = new BusinessObjectFactory();
			fPreviewInvoices = new InvoicingBaseCollection(factory);
			if (invoices != null)
			{
				foreach (InvoicingBase invoice in invoices)
				{
					fPreviewInvoices.Add(invoice);
				}
			}

			this.SecurityHelper = securityHelper;
		}

		readonly BusinessObjectFactory factory;

		public JobInvoicingPostingOption PostingOption;

		public JobInvoicingSecurityHelper SecurityHelper { get; }

		public InvoicingBaseCollection PreviewInvoices
		{
			get
			{
				return fPreviewInvoices;
			}
		}
		readonly InvoicingBaseCollection fPreviewInvoices;

		public void Dispose()
		{
			if (fPreviewInvoices != null)
			{
				fPreviewInvoices.RemoveAll();
			}
		}
	}
}