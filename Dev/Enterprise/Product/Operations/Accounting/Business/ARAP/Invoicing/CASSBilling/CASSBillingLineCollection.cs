
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSBillingLineCollection : NonPersistentBusinessObjectCollection<CASSBillingLine>, IObsoleteValidation
	{
		public CASSBillingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CASSBillingLine(Factory);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}
	}
}
