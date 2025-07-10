using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class UndoCancelCommissionLineActionCollection : NonPersistentBusinessObjectCollection<UndoCancelCommissionLineAction>
	{
		public UndoCancelCommissionLineActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UndoCancelCommissionLineAction(Factory);
		}
	}
}
