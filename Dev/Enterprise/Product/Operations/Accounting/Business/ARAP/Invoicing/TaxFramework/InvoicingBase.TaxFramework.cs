using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase
	{
		[ChildEditable(true)]
		public AccTaxTransactionCollection TaxTransactionCollection
		{
			get
			{
				if (taxTransactionCollection == null)
				{
					taxTransactionCollection = new AccTaxTransactionCollection(Factory, this, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(this));
					RegisterEditableChildObject(taxTransactionCollection);
				}

				return taxTransactionCollection;
			}
		}
		AccTaxTransactionCollection taxTransactionCollection;

		public void RegisterTaxTransactionCollectionAsEditableChild()
		{
			_ = TaxTransactionCollection;
		}
	}
}
