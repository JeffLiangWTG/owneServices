using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class FilteredInvoicingBaseConsolCollectionForImportingView : BusinessObjectCollectionView<InvoicingBaseConsolForImporting>
	{
		public FilteredInvoicingBaseConsolCollectionForImportingView(InvoicingBaseConsolCollectionForImporting collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;

			var consol = element as InvoicingBaseConsolForImporting;
			if (consol != null)
			{
				result = consol.ConsolCostsFilteredByViewingPermission.Any();
			}

			return result;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return collectionToFilter.AllowNew; }
		}

		protected override bool AllowRemoveCore
		{
			get { return collectionToFilter.AllowRemove; }
		}

		#endregion
	}
}
