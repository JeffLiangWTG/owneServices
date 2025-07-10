using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceHeaderLinkImportCollection : NonPersistentBusinessObjectCollection<PriceHeaderLinkImport>
	{
		public PriceHeaderLinkImportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PriceHeaderLinkImport();
		}
	}
}

