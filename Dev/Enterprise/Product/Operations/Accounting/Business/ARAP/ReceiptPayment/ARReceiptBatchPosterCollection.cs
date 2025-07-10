using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ARReceiptBatchPosterCollection : NonPersistentBusinessObjectCollection<ARReceiptBatchPoster>	{
		public ARReceiptBatchPosterCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARReceiptBatchPoster(Factory);
		}
	}
}