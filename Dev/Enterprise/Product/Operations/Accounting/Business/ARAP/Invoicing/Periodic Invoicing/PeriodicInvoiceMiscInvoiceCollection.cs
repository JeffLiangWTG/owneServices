using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceMiscInvoiceCollection : InvoicingBaseCollection
	{
		public PeriodicInvoiceMiscInvoiceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Events

		public event EventHandler<ChangedBizoEventArgs> OnIncludingInThePeriodicInvoiceChanged;

		public void RaiseOnIncludingInThePeriodicInvoiceChanged(InvoicingBase sender)
		{
			if (OnIncludingInThePeriodicInvoiceChanged != null)
			{
				OnIncludingInThePeriodicInvoiceChanged(this, new ChangedBizoEventArgs(sender));
			}
		}

		#endregion

		#region Base Override

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((InvoicingBase)bizOAdded).IncludeInThePeriodicInvoice = true;
		}

		#endregion

		public int SelectedCount
		{
			get
			{
				int total = 0;
				foreach (InvoicingBase invoice in this)
				{
					if (invoice.IncludeInThePeriodicInvoice)
					{
						total++;
					}
				}
				return total;
			}
		}
	}
}
