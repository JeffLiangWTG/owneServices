using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	class B3LineNumberAssigner : LineNumberAssigner
	{
		public B3LineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected override IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering(Customs.Business.CusEntryHeader entry)
		{
			return new B3EntryLineComparer();
		}

		public IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerForLineReordering()
		{
			var comparer = new B3EntryLineComparer();
			comparer.CompareGoodsShipmentSequenceAndCommoditySequence = false;
			return comparer;
		}
	}
}
