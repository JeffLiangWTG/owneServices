using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class UsageLineCollection : NonPersistentBusinessObjectCollection<UsageLine>
	{
		public UsageLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsageLine(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

