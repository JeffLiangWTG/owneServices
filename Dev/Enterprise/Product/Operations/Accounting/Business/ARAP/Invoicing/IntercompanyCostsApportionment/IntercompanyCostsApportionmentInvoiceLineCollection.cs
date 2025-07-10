using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IntercompanyCostsApportionmentInvoiceLineCollection : NonPersistentBusinessObjectCollection<IntercompanyCostsApportionmentInvoiceLine>	{
		readonly IntercompanyCostsApportionmentInvoice invoice;

		public IntercompanyCostsApportionmentInvoiceLineCollection(BusinessObjectFactory factory, IntercompanyCostsApportionmentInvoice invoice)
			: base(factory)
		{
			this.invoice = invoice;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IntercompanyCostsApportionmentInvoiceLine(Factory, invoice);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var line = (IntercompanyCostsApportionmentInvoiceLine)child;
			line.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
			line.TaxBranch = invoice.TaxBranch;
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		public void CalculateGSTForAllLines()
		{
			foreach (IntercompanyCostsApportionmentInvoiceLine line in this)
			{
				line.CalculateGST(line.IsGSTMandatory);
			}
		}

		public void updateApportionments()
		{
			foreach (IntercompanyCostsApportionmentInvoiceLine line in this)
			{
				line.updateApportionments();
			}
		}

		public void updateLineTaxes()
		{
			foreach (IntercompanyCostsApportionmentInvoiceLine line in this)
			{
				line.updateTax();
			}
		}

		public void updateLineTotals()
		{
			foreach (IntercompanyCostsApportionmentInvoiceLine line in this)
			{
				line.updateTotal();
			}
		}

		public void updateApportionmentsExchangeRate()
		{
			foreach (IntercompanyCostsApportionmentInvoiceLine line in this)
			{
				line.updateApportionmentsExchangeRate();
			}
		}
	}
}
