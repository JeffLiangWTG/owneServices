
using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineConfirmedFeeWrapperCollection : NonPersistentBusinessObjectCollection<CusEntryLineConfirmedFeeWrapper>
	{
		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public CusEntryLineConfirmedFeeWrapperCollection(CusEntryLine entryLine)
			: base(entryLine?.Factory)
		{
			Argument.NotNull(entryLine, "entryLine");
			BuildCollectionCore(entryLine);
		}

		protected virtual void BuildCollectionCore(CusEntryLine entryLine)
		{
			AddRange(from CusEntryLineFee fee in entryLine.ConfirmedFees
					 select new CusEntryLineConfirmedFeeWrapper(fee));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("It is not possible to create a new object from this collection");
		}
	}
}
