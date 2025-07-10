using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkAddDiscountCollection : NonPersistentBusinessObjectCollection<BulkAddDiscount>
	{
		public BulkAddDiscountCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BulkAddDiscount();
		}
	}
}

