using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFinalizerLineItemCollection : NonPersistentBusinessObjectCollection<CommissionFinalizerLineItem>
	{
		public CommissionFinalizerLineItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommissionFinalizerLineItemCollection(BusinessObjectFactory factory, IEnumerable<CommissionFinalizerLineItem> lineItems)
			: base(factory)
		{
			AddRange(lineItems);
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

		public CommissionFinalizerLineItem AddNew(ViewCommissionLine line)
		{
			var result = new CommissionFinalizerLineItem(line);
			Add(result);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionFinalizerLineItem(null);
		}

		#endregion
	}
}
