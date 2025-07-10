using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingBasicValueSetStrategy : IValueSetStrategy
	{
		public GuidedDecisionMakingBasicValueSetStrategy(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			this.guidedDecisionMakingBasic = guidedDecisionMakingBasic;
		}

		protected readonly GuidedDecisionMakingBasic guidedDecisionMakingBasic;

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case GuidedDecisionMakingBasic.Schema.TariffCode:
				case GuidedDecisionMakingBasic.Schema.DataGrouping:
				case GuidedDecisionMakingBasic.Schema.DutyRateTypeCode:
				case GuidedDecisionMakingBasic.Schema.CountryOfOrigin:
				case GuidedDecisionMakingBasic.Schema.CountryOfDestination:
				case nameof(GuidedDecisionMakingBasic.EffectiveDate):
					ClearPreferenceWhenEmptyList();
					ClearQuotaOrderNumberWhenEmptyList();
					ResetCustomsSecondQuantity();
					guidedDecisionMakingBasic.ClearAdditionalCodesCache();
					break;
				case GuidedDecisionMakingBasic.Schema.Preference:
					ClearQuotaOrderNumberWhenEmptyList();
					guidedDecisionMakingBasic.ClearAdditionalCodesCache();
					break;
				case GuidedDecisionMakingBasic.Schema.QuotaOrderNumber:
					guidedDecisionMakingBasic.ClearAdditionalCodesCache();
					break;
				case GuidedDecisionMakingBasic.Schema.CustomsFirstQuantity:
				case GuidedDecisionMakingBasic.Schema.CustomsSecondQuantity:
				case GuidedDecisionMakingBasic.Schema.CustomsSecondUnitQty:
				case GuidedDecisionMakingBasic.Schema.CustomsThirdQuantity:
				case GuidedDecisionMakingBasic.Schema.CustomsThirdUnitQty:
					guidedDecisionMakingBasic.ClearDocumentConditionsCache();
					break;
			}
		}

		void ClearPreferenceWhenEmptyList()
		{
			if (guidedDecisionMakingBasic.Lookups.PreferenceList.Count == 0)
			{
				guidedDecisionMakingBasic.PreferenceInfo.ClearValue();
			}
		}

		void ClearQuotaOrderNumberWhenEmptyList()
		{
			if (guidedDecisionMakingBasic.Lookups.QuotaOrderNumberList.Count == 0)
			{
				guidedDecisionMakingBasic.QuotaOrderNumberInfo.ClearValue();
			}
		}

		void ResetCustomsSecondQuantity()
		{
			var supplementaryQuantity = (NoResString)"Supplementary Qty.";
			if (CU2UOM == null)
			{
				guidedDecisionMakingBasic.CustomsSecondQuantityInfo.ClearValue();
				guidedDecisionMakingBasic.CustomsSecondUnitQtyInfo.ClearValue();
				guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription = supplementaryQuantity;
			}
			else if (guidedDecisionMakingBasic.CustomsSecondUnitQty.IsEmpty)
			{
				guidedDecisionMakingBasic.CustomsSecondUnitQty = CU2UOM.ZZ8_UOM.SubstringSafe(0, GuidedDecisionMakingBasic.Schema.CustomsSecondUnitQtyMaxLength);
				guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription = supplementaryQuantity + (NoResString)" in " + CU2UOM.ZZ8_UOM;
			}
		}

		TariffUOMView CU2UOM => guidedDecisionMakingBasic.Tariff?.UnitsOfMeasure?.FirstOrDefault(a =>
			a.ZZ8_Type == UOMTypeList.Codes.CU2 && (a.CusTradeGroup == null || a.CusTradeGroup
				.GetApplicableTradeGroupCountries(guidedDecisionMakingBasic.EffectiveDate).Any(b =>
					b.ZZB_RN_NKTradeGroupCountryCode == guidedDecisionMakingBasic.EffectiveTradeGroupCountry)));
	}
}
