using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceBillCollection : NonPersistentBusinessObjectCollection<MaintenanceBill>
	{
		public MaintenanceBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MaintenanceBill(Factory, null, new MaintenanceBillRecipient(Factory, ZGuid.Empty, ZGuid.Empty, "", ZDateTime.Now), null);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}


