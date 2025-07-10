using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CancelCommissionLineActionCollection : NonPersistentBusinessObjectCollection<CancelCommissionLineAction>
	{
		public CancelCommissionLineActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CancelCommissionLineAction(Factory);
		}
	}
}
