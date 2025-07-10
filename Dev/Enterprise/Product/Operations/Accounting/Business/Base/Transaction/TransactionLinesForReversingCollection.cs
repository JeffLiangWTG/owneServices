using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLinesForReversingCollection : NonPersistentBusinessObjectCollection<TransactionLineForReversing>
	{
		public TransactionLinesForReversingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject AddNewCore() => throw new NotSupportedException("Allow new is false so shouldn't get called");

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("Allow new is false so shouldn't get called");
	}
}
