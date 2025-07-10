using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryLineConfirmedFeeWrapperCollection : NonPersistentBusinessObjectCollection<CusEntryLineConfirmedFeeWrapper>
	{
		public CusEntryLineConfirmedFeeWrapperCollection(CusEntryLine entryLine) : base(entryLine?.Factory)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			BuildCollectionCore(entryLine);
		}

		protected virtual void BuildCollectionCore(CusEntryLine entryLine)
		{
			AddRange(entryLine.ConfirmedFees
				.Cast<CusEntryLineFee>()
				.Select(fee => new CusEntryLineConfirmedFeeWrapper(fee)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("It is not possible to create a new object from this collection");

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
