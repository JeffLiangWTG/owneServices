using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DuimpLinesProvider : Collection<DuimpLineProvider>, IEnumerable<IDuimpLine>
	{
		DuimpLinesProvider(IEnumerable<CusEntryLine> entryLines)
		{
			foreach (var entryLine in entryLines)
			{
				Add(DuimpLineProvider.New(entryLine));
			}
		}

		public static DuimpLinesProvider New(IEnumerable<CusEntryLine> entryLines) => entryLines == null ? null : new DuimpLinesProvider(entryLines);

		IEnumerator<IDuimpLine> IEnumerable<IDuimpLine>.GetEnumerator() => GetEnumerator();
	}
}
