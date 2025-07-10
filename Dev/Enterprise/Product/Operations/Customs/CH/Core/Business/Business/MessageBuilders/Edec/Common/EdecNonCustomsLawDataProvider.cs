using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecNonCustomsLawDataProvider : IEdecNonCustomsLaw
{
	public string NonCustomsLawType { get; private set; }

	public static IEnumerable<EdecNonCustomsLawDataProvider> NewCollection(CusEntryLine entryLine)
	{
		return entryLine?.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(il => il.NonCustomsLaws.Cast<NonCustomsLaw>())
			.Select(ncl => ncl.CSI_Code).Where(c => !c.IsEmpty).Distinct()
			.Select(c => new EdecNonCustomsLawDataProvider { NonCustomsLawType = c }) ?? Enumerable.Empty<EdecNonCustomsLawDataProvider>();
	}
}
