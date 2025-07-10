using System.Collections;
using System.ComponentModel;

namespace Enterprise.Customs.CA.Business
{
	public class AllCusEntryLineCollection : Customs.Business.AllCusEntryLineCollection<CusEntryLine>
	{
		public AllCusEntryLineCollection(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == CusEntryLine.Schema.SequenceNumber)
			{
				comparer = new CAEntryLineComparer(direction == ListSortDirection.Descending);
			}
			else
			{
				comparer = base.GetComparerForSort(property, direction);
			}
			return comparer;
		}
	}
}
