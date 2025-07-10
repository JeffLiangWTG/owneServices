using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseConsolCollectionForImporting : APInvoiceConsolCollection
	{
		public InvoicingBaseConsolCollectionForImporting(InvoicingBaseBulkConsolCostImporter master, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Master = master;
		}

		internal readonly InvoicingBaseBulkConsolCostImporter Master;

		public new InvoicingBaseConsolForImporting this[int index]
		{
			get { return (InvoicingBaseConsolForImporting)Elements[index]; }
		}

		public new InvoicingBaseConsolForImporting AddNew()
		{
			return (InvoicingBaseConsolForImporting)base.AddNew();
		}

		public new InvoicingBaseConsolForImporting AddNew(Type bizObjType)
		{
			return (InvoicingBaseConsolForImporting)base.AddNew(bizObjType);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((InvoicingBaseConsolForImporting)bizOAdded).Importer = Master;
			((InvoicingBaseConsolForImporting)bizOAdded).ConsolCostsAdditionalFilter = Master.Filters.GetChildQuery();
			((InvoicingBaseConsolForImporting)bizOAdded).IsSelectedForImport = true;
		}
	}
}
