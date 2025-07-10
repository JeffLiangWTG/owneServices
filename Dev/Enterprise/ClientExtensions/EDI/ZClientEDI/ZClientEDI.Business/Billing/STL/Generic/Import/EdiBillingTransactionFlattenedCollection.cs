using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBillingTransactionFlattenedCollection : NonPersistentBusinessObjectCollection<EdiBillingTransactionFlattened>
	{
		public EdiBillingTransactionFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EdiBillingTransactionFlattened(Factory);
		}
	}
}


