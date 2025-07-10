using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class EntryLineGroup : EU.Business.Documents.DocDataObjects.EntryLineGroup
	{
		public EntryLineGroup(IEnumerable<CusEntryLine> entryLines, int groupNumber) : base(entryLines, groupNumber)
		{
			EntryLines = entryLines;
		}

		public new IEnumerable<CusEntryLine> EntryLines { get; set; }

		protected override EU.Business.Documents.DocDataObjects.EntryLineDataObject Item1Core => item1 ?? (item1 = EntryLines.Any() ? new EntryLineDataObject(EntryLines.FirstOrDefault()) : null);

		protected override EU.Business.Documents.DocDataObjects.EntryLineDataObject Item2Core => item2 ?? (item2 = EntryLines.Count() > 1 ? new EntryLineDataObject(EntryLines.Skip(1).FirstOrDefault()) : null);

		protected override EU.Business.Documents.DocDataObjects.EntryLineDataObject Item3Core => item3 ?? (item3 = EntryLines.Count() > 2 ? new EntryLineDataObject(EntryLines.Skip(2).FirstOrDefault()) : null);

		EntryLineDataObject item1;
		EntryLineDataObject item2;
		EntryLineDataObject item3;
	}
}
