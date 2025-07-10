using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration;

public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
{
	public AddInfoJobComInvoiceLineLookups(EU.Business.Declaration.AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();

	protected override System.Collections.ICollection RegionOfDestinationListCore => InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty ? LookupsHelper.RegionOfDestinationDropEditList(Parent.Factory) : LookupsHelper.RegionOfDestinationCodeFindBoxList(Parent.Factory);

	public CodeDescriptionPairList ExciseExemptionList => Factory.GetCachedValue<ExciseExemptionList>();

	public CodeDescriptionPairList REAProductCodeList
		=> UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(Parent.Parent.UniversalTariff, CachedListOfREACodeDescriptions, InvoiceLine.REARateSelectionCriteria);

	public CodeDescriptionPairList AIEMTypeCodeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (InvoiceLine.UniversalTariff != null && InvoiceLine.DestinationStateIsCanaryIsland)
			{
				result = Factory.GetCachedValue(FormattableString.Invariant($"ES.AIEMTypeCodeList_{InvoiceLine.JI_Tariff}"), () =>
				{
					var list = new CodeDescriptionPairList();
					var childTariffs = InvoiceLine.GetChildTariffsGivenTariffTypeCode(UniversalReferenceConstants.RefCusTariffType.AIEM).Select(x => x.RelatedTariffFrom);

					foreach (var tariff in childTariffs)
					{
						var aiemCode = tariff.ZZ1_TariffCode.Split('_').ElementAtOrDefault(1);
						if (!aiemCode.IsEmpty && tariff.ZZ1_EndDate.IsInTheFuture())
						{
							list.AddPair(aiemCode, tariff.ZZ1_Description);
						}
					}

					return list;
				});
			}

			return result;
		}
	}

	public CodeDescriptionPairList ExciseCodeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (InvoiceLine.UniversalTariff != null)
			{
				var exciseCode = InvoiceLine.DestinationStateIsCanaryIsland ? UniversalReferenceConstants.RefCusTariffType.CANEX : UniversalReferenceConstants.RefCusTariffType.ESEXC;
				result = Factory.GetCachedValue(FormattableString.Invariant($"ES.ExciseCodeList_{InvoiceLine.JI_Tariff}_{exciseCode}"), () =>
				{
					var list = new CodeDescriptionPairList();
					var childTariffs = InvoiceLine.GetChildTariffsGivenTariffTypeCode(exciseCode).Select(x => x.RelatedTariffFrom);

					foreach (var tariff in childTariffs)
					{
						if (tariff.ZZ1_TariffCode != UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee && tariff.ZZ1_EndDate.IsInTheFuture())
						{
							list.AddPair(tariff.ZZ1_TariffCode, tariff.ZZ1_Description);
						}
					}

					return list;
				});
			}

			return result;
		}
	}

	public CodeDescriptionPairList CachedListOfREACodeDescriptions
										=> RefCusCodeListTypes.GetCachedList(Factory,
																			Parent.Parent.GetDefaultDataGroupingCode(),
																			UniversalReferenceConstants.RefCusCodeListTypes.REACodeDescriptions,
																			Parent.Parent.EffectiveAssessmentDate);

	JobComInvoiceLine InvoiceLine => Parent.InvoiceLine;
}
