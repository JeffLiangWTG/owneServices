using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingBasicLookups : ZLookups
	{
		public GuidedDecisionMakingBasicLookups(GuidedDecisionMakingBasic parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CountryOfOriginList => CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, Parent.DataGrouping);

		public TariffViewCollection Tariffs => TariffViewCollection.GetCachedCollection(Factory, Parent.DataGrouping, Parent.TariffType, Parent.EffectiveDate);

		public CodeDescriptionPairList PreferenceList => PreferenceListCore();

		protected virtual CodeDescriptionPairList PreferenceListCore() => Factory.GetCachedValue($"GuidedDecisionMaking_PreferenceList_{Parent.DataGrouping}_{Parent.TariffCode}_{Parent.DutyRateTypeCode}_{Parent.EffectiveTradeGroupCountry}_{Parent.EffectiveDate}",
			() =>
			{
				var effectiveTradeGroupCountry = Parent.EffectiveTradeGroupCountry;
				var result = new CodeDescriptionPairList();
				if (!Parent.DataGrouping.IsEmpty && !Parent.DutyRateTypeCode.IsEmpty && !effectiveTradeGroupCountry.IsEmpty && Parent.EffectiveDate.IsValid && Parent.Tariff is TariffView tariff)
				{
					var criteria = new SpecificRateSelectionCriteria(effectiveTradeGroupCountry, Parent.DataGrouping, ZString.Empty, ZString.Empty, null, Parent.EffectiveDate, Parent.DutyRateTypeCode, ZString.Empty);
					result = UniversalReferenceDataHelper.GetDynamicPreferenceList(tariff, criteria);
				}
				return result;
			});

		public CodeDescriptionPairList QuotaOrderNumberList => Factory.GetCachedValue($"GuidedDecisionMaking_QuotaOrderNumberList_{Parent.DataGrouping}_{Parent.TariffCode}_{Parent.DutyRateTypeCode}_{Parent.EffectiveTradeGroupCountry}_{Parent.Preference}_{Parent.EffectiveDate}",
			() =>
				{
					var effectiveTradeGroupCountry = Parent.EffectiveTradeGroupCountry;
					var result = new CodeDescriptionPairList();
					if (!Parent.DataGrouping.IsEmpty && !Parent.DutyRateTypeCode.IsEmpty && !effectiveTradeGroupCountry.IsEmpty && Parent.EffectiveDate.IsValid && Parent.Tariff is TariffView tariff)
					{
						var criteria = new SpecificRateSelectionCriteria(effectiveTradeGroupCountry, Parent.DataGrouping, Parent.Preference, ZString.Empty, null, Parent.EffectiveDate, Parent.DutyRateTypeCode, ZString.Empty);
						result = UniversalReferenceDataHelper.GetDynamicOrderNumberList(tariff, criteria);
					}
					return result;
				});

		public CodeDescriptionPairList CustomsUQList
			=> RefCusCodeListTypes.GetCachedList(Factory, Parent.DataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Parent.EffectiveDate);

		public CodeDescriptionPairList CustomsSecondUQList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var tariffCU2 = Parent.Tariff?.UnitsOfMeasure.FirstOrDefault(u => u.ZZ8_Type == UOMTypeList.Codes.CU2)?.ZZ8_UOM ?? ZString.Empty;
				if (!tariffCU2.IsEmpty && CustomsUQList.ContainsCode(tariffCU2))
				{
					result.AddPair(tariffCU2, CustomsUQList.GetDescriptionFromCode(tariffCU2));
				}
				return result;
			}
		}

		public CodeDescriptionPairList MeursingResultList => new MeursingList();

		protected new GuidedDecisionMakingBasic Parent => (GuidedDecisionMakingBasic)base.Parent;
	}
}
