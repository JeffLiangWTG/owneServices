using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLinkCollectionForUnmatching : TransactionMatchLinkCollection
	{
		public TransactionMatchLinkCollectionForUnmatching(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionMatchLinkCollectionForUnmatching(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public override void Load()
		{
			throw new NotSupportedException("You cannot call Load on this collection since you are using it from an UnmatchingRow.");
		}
	}
}
