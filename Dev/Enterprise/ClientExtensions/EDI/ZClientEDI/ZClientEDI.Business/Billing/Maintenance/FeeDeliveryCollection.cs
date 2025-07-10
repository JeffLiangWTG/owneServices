using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class FeeDeliveryCollection : NonPersistentBusinessObjectCollection<FeeDelivery>
	{
		public FeeDeliveryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FeeDelivery(Factory, null, null, ZDateTime.Empty, null, null);
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


