using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseCollectionForModule : TransactionHeaderCollection
	{
		public InvoicingBaseCollectionForModule(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new InvoicingBase this[int index]
		{
			get
			{
				return (InvoicingBase)(Elements[index]);
			}
		}
	}
}
