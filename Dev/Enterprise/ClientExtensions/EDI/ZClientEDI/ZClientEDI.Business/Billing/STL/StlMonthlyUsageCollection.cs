using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlMonthlyUsageCollection : NonPersistentBusinessObjectCollection<StlMonthlyUsage>
	{
		public StlMonthlyUsageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlMonthlyUsage(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

