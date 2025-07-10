using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoiceBulkOperation : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected InvoiceBulkOperation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected InvoiceBulkOperation()
		{
		}

		public InvoiceBulkOperationFilters Filters
		{
			get { return FiltersCore; }
		}

		InvoiceBulkOperationFilters fFiltersCore;
		[ChildEditable]
		public InvoiceBulkOperationFilters FiltersCore
		{
			get
			{
				if (fFiltersCore == null)
				{
					fFiltersCore = GetNewFilters();
					RegisterEditableChildObject(fFiltersCore);
				}
				return fFiltersCore;
			}
		}

		protected abstract InvoiceBulkOperationFilters GetNewFilters();
	}
}
