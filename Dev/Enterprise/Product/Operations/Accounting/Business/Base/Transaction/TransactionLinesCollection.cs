using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLinesCollection : BusinessObjectCollection<TransactionLine>
	{
		public TransactionLinesCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public TransactionLinesCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
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