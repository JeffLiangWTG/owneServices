using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceSummaryLineCollection : NonPersistentBusinessObjectCollection<PriceSummaryLine>
	{
		public PriceSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new PriceSummaryLine(Factory);

		protected override bool AllowNewCore
			=> false;
	}
}
