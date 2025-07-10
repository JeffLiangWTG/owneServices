using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryLineComparerAccordingToPackageAndMark : IComparer<Customs.Business.CusEntryLine>
{
	public int Compare(Customs.Business.CusEntryLine x, Customs.Business.CusEntryLine y)
	{
		var invoiceLineX = x.InvoiceLines.Count > 0 ? x.InvoiceLines[0] : x.RandomLine;
		var invoiceLineY = y.InvoiceLines.Count > 0 ? y.InvoiceLines[0] : y.RandomLine;

		return new ByPackageAndMarkLineComparer().Compare(invoiceLineX, invoiceLineY);
	}
}
