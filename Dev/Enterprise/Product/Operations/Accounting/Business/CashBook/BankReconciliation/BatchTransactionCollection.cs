
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BatchTransactionCollection : BusinessObjectCollection<BatchTransaction>
	{
		public BatchTransactionCollection(BusinessObjectFactory factory) : base(factory) { }

		public BatchTransactionCollection(BusinessObjectFactory factory, ZQuery additionalQuery)
			: base(factory, additionalQuery)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}