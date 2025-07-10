using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceBillRecipientCollection : NonPersistentBusinessObjectCollection<MaintenanceBillRecipient>
	{
		public MaintenanceBillRecipientCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MaintenanceBillRecipient(Factory, ZGuid.Empty, ZGuid.Empty, "", ZDateTime.Today);
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


