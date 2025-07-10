using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecIMPFeeDataProvider : IEdecFee
{
	public static IEnumerable<EdecIMPFeeDataProvider> NewCollection(CusEntryLine entryLine)
	{
		if (entryLine != null)
		{
			foreach (var additionalFee in entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalFees.Cast<CusLineTariffDetail>()))
			{
				yield return New(additionalFee);
			}
		}
	}

	public static EdecIMPFeeDataProvider New(CusLineTariffDetail additionalFee)
	{
		return additionalFee == null ? null : new EdecIMPFeeDataProvider(additionalFee);
	}

	EdecIMPFeeDataProvider(CusLineTariffDetail additionalFee)
	{
		this.additionalFee = additionalFee;
	}
	readonly CusLineTariffDetail additionalFee;

	public string Type => additionalFee.BZ_Tariff;

	public decimal? Quantity => additionalFee.BZ_Qty1;

	public decimal? Rate => additionalFee.BZ_ManualRate;
}
