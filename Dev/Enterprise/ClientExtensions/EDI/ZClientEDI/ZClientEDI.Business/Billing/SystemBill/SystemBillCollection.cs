using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SystemBillCollection : NonPersistentBusinessObjectCollection<SystemBill>
	{
		public SystemBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SystemBill(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

