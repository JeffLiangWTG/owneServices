using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class AdditionalTaxSelectionCriteria : IZZRateSelectionCriteria
{
	public AdditionalTaxSelectionCriteria(CusLineTariffDetail tariffDetail)
	{
		this.tariffDetail = Argument.NotNull(tariffDetail, nameof(tariffDetail));
	}
	readonly CusLineTariffDetail tariffDetail;

	JobComInvoiceLine invoiceLine => tariffDetail.Parent as JobComInvoiceLine;

	public ZDateTime EffectiveDate => invoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Today;

	public ZString TradeGroupCountry => invoiceLine?.EffectiveCountryOfOrigin ?? ZString.Empty;

	public ISet<ZString> SecondTradeGroups => new HashSet<ZString>();

	public ZString DataGrouping => invoiceLine?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;

	public ZString PrimaryPreference => invoiceLine?.JI_PrimaryPreference ?? ZString.Empty;

	public ISet<ZString> AdditionalCodes => new HashSet<ZString> { invoiceLine?.TariffWithoutCustomsFavourCode ?? ZString.Empty };

	public ZString ConcessionOrder => invoiceLine?.JI_ConcessionOrder ?? ZString.Empty;

	public ZString RateType => UniversalReferenceConstants.RateTypes.AdditionalTaxes;

	public ZString RateCode => tariffDetail.BZ_TaxType;

	public RateDirection Direction => RateDirection.Both;
}
