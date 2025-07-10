using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec;

namespace Enterprise.Customs.CH.Business;

public class EdecAdditionalTaxDataProvider : IEdecAdditionalTax
{
	public static IEnumerable<EdecAdditionalTaxDataProvider> NewCollection(CusEntryLine entryLine)
	{
		return entryLine?.RandomLine?.AdditionalTaxes.Cast<CusLineTariffDetail>()
					.Where(x => x.IsAdditionalTaxApplied).Select(x => New(x)) ?? Enumerable.Empty<EdecAdditionalTaxDataProvider>();
	}

	public static EdecAdditionalTaxDataProvider New(CusLineTariffDetail tariffDetail) => tariffDetail == null ? null : new EdecAdditionalTaxDataProvider(tariffDetail);

	EdecAdditionalTaxDataProvider(CusLineTariffDetail tariffDetail)
	{
		this.tariffDetail = Argument.NotNull(tariffDetail, nameof(tariffDetail));
	}

	readonly CusLineTariffDetail tariffDetail;

	public string Type => tariffDetail.BZ_Tariff.SubstringSafe(0, 3);

	public string Key => tariffDetail.BZ_Tariff.SubstringSafe(4, 3);

	public decimal Quantity => UniversalReferenceConstants.AdditionalTaxesTypes.IsAdditionalTaxTypeForSpirits(tariffDetail.BZ_TaxType) ? tariffDetail.InvoiceLine.JI_CustomsThirdQuantity : tariffDetail.BZ_BaseValue.FallbackIfEmpty(tariffDetail.BZ_Qty1);

	public decimal? Rate => tariffDetail.BZ_ManualRate.ReturnNullIfEmpty();

	public bool RateConfirmation => false;

	public decimal? AlcoholLevel => tariffDetail.BZ_AlcoholPercentage.ReturnNullIfEmpty();
}
