using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceCollection : NonPersistentBusinessObjectCollection<PeriodicInvoice>	{
		public PeriodicInvoiceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public event EventHandler IncludeInThePeriodicInvoiceChanged;

		public event EventHandler TaxBranchChanged;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			PeriodicInvoice periodicInvoice = (PeriodicInvoice)bizOAdded;
			var writableProperties = new string[]
			{
				periodicInvoice.IncludeInThePeriodicInvoiceInfo.Name,
				periodicInvoice.SellReferenceInfo.Name,
				periodicInvoice.TaxBranchInfo.Name
			};
			periodicInvoice.AddWritableProperties(writableProperties);
			periodicInvoice.Jobs.IncludeInThePeriodicInvoiceChanged += IncludeInThePeriodicInvoiceInfo_ValueChanged;
			periodicInvoice.IncludeInThePeriodicInvoice = true;
			periodicInvoice.TaxBranchAndJobsChanged += TaxBranchChanged;
		}

		void IncludeInThePeriodicInvoiceInfo_ValueChanged(object sender, EventArgs e)
		{
			if (IncludeInThePeriodicInvoiceChanged != null)
			{
				IncludeInThePeriodicInvoiceChanged(sender, e);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PeriodicInvoice(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
