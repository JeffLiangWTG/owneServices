using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Module;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.GUI;

namespace Enterprise.Client.JAS.Module
{
	public class JASARCreditNoteController : ARCreditNoteController
	{
		protected override CreditNoteForm GetNewCreditNoteForm(CreditNote creditNote)
		{
			return new JASARCreditNoteForm(creditNote as JASARCreditNote);
		}

		protected override InvoiceForm GetNewInvoiceForm(Invoice invoice)
		{
			return new JASARInvoiceForm(invoice as JASARInvoice);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JASARCreditNote); }
		}
			}
}
