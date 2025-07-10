using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseConsolCostCollectionForImporting : BusinessObjectCollection<InvoicingBaseConsolCostForImporting>
	{
		public InvoicingBaseConsolCostCollectionForImporting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal InvoicingBaseBulkConsolCostImporter Importer
		{
			get { return importer_cached; }
			set
			{
				importer_cached = value;
				foreach (InvoicingBaseConsolCostForImporting bizo in this)
				{
					bizo.Importer = importer_cached;
				}
			}
		}
		InvoicingBaseBulkConsolCostImporter importer_cached;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((InvoicingBaseConsolCostForImporting)bizOAdded).Importer = Importer;
		}
	}
}
