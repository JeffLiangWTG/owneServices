using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business.Declaration;

public class LineNumberAssigner : Customs.Business.LineNumberAssigner
{
	public LineNumberAssigner(Customs.Business.CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering(Customs.Business.CusEntryHeader entry)
	{
		return new EntryLineComparerAccordingToPackageAndMark();
	}
}
