using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AmbiguousCommissionResolveItemCollection : NonPersistentBusinessObjectCollection<AmbiguousCommissionResolveItem>
	{
		public AmbiguousCommissionResolveItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region New

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AmbiguousCommissionResolveItem();
		}

		#endregion
	}
}
