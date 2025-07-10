using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ReceiptPaymentBaseCollection : BusinessObjectCollection<ReceiptPaymentBase>
	{
		public ReceiptPaymentBaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ReceiptPaymentBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type element.");
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}
	}
}
