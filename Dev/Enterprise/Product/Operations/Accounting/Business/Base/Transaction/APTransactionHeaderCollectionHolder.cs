using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class APTransactionHeaderCollectionHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public APTransactionHeaderCollectionHolder(BusinessObjectFactory factory, APTransactionHeaderCollection collection)
			: base(factory)
		{
			this.collection = collection;
		}

		readonly APTransactionHeaderCollection collection;

		public APTransactionHeaderCollection Collection
		{
			get { return collection; }
		}
	}
}