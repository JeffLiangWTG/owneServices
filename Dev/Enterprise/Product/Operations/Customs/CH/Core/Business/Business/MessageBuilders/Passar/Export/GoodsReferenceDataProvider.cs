using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class GoodsReferenceDataProvider : IGoodsReference
{
	public static IEnumerable<GoodsReferenceDataProvider> NewCollection(BaseCusContainer container) =>
		container?.InvoiceLinePivotCollection.Cast<CusContainerInvoiceLinePivot>().Select(x => x.InvoiceLine?.CusEntryLine)
			.WhereNotNull().Distinct().OrderBy(x => x.CL_LineNumber).Select((line, index) => new GoodsReferenceDataProvider(index + 1, line))
		?? Enumerable.Empty<GoodsReferenceDataProvider>();

	GoodsReferenceDataProvider(int sequenceNumber, Customs.Business.CusEntryLine entryLine)
	{
		SequenceNumber = sequenceNumber;
		this.entryLine = entryLine;
	}
	readonly Customs.Business.CusEntryLine entryLine;

	public int SequenceNumber { get; }

	public int DeclarationGoodsItemNumber => entryLine.CL_LineNumber;
}
