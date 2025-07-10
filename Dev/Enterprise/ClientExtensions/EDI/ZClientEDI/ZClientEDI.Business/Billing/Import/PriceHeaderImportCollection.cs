using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceHeaderImportCollection : NonPersistentBusinessObjectCollection<PriceHeaderImport>
	{
		public PriceHeaderImportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PriceHeaderImport();
		}
	}
}

