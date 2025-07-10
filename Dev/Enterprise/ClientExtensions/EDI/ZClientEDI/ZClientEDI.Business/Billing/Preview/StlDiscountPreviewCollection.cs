using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlDiscountPreviewCollection : NonPersistentBusinessObjectCollection<StlDiscountPreview>
	{
		public StlDiscountPreviewCollection()
			: base()
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
			return new StlDiscountPreview();
		}

		#endregion
	}
}

