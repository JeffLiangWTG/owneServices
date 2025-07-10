using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSAdjustmentLineCollection : NonPersistentBusinessObjectCollection<CASSAdjustmentLine>
	{
		public CASSAdjustmentLineCollection()
			: base()
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CASSAdjustmentLine();
		}
	}
}
