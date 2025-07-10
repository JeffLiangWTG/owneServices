using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SystemUsageCollection : NonPersistentBusinessObjectCollection<SystemUsage>
	{
		public SystemUsageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

