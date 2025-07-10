#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument  = CargoWise.Common.Argument;
#endif
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeCollection : NonPersistentBusinessObjectCollection<GuidedDecisionMakingAdditionalCode>
	{
		public GuidedDecisionMakingAdditionalCodeCollection(GuidedDecisionMakingBasic gDMBasic) : base(gDMBasic.Factory)
		{
			this.gDMBasic = Argument.NotNull(gDMBasic, nameof(gDMBasic));
			Load();
		}

		readonly GuidedDecisionMakingBasic gDMBasic;

		public override void Load()
		{
			RemoveAndDeleteAll();

			if (!gDMBasic.DataGrouping.IsEmpty && gDMBasic.EffectiveTradeGroupCountry is ZString effectiveTradeGroupCountry && !effectiveTradeGroupCountry.IsEmpty && gDMBasic.Tariff is TariffView tariff)
			{
				var rateSelectionCriteriaInfos = UniversalReferenceDataHelper
					.GetRateSelectionCriteriaInfo(tariff, gDMBasic.RateSelectionCriteriaWithoutAdditionalCodes)
					.Where(x => !x.ZZT_AdditionalCode.IsEmpty && x.MatchExcludingAdditionalCodes(gDMBasic.RateSelectionCriteriaWithoutAdditionalCodes))
					.OrderBy(x => x.RateType)
					.ThenBy(x => x.ZZT_AdditionalCode)
					.GroupBy(x => x.RateType);

				foreach (var additionalCodeGroups in rateSelectionCriteriaInfos)
				{
					foreach (var additionalCodes in additionalCodeGroups.DistinctBy(x => x.ZZT_AdditionalCode))
					{
						var guidedDecisionMakingAdditionalCode = new GuidedDecisionMakingAdditionalCodeFromRate(gDMBasic);
						guidedDecisionMakingAdditionalCode.ApplicableToType = additionalCodes.RateType;
						guidedDecisionMakingAdditionalCode.AdditionalCode = additionalCodes.ZZT_AdditionalCode;
						guidedDecisionMakingAdditionalCode.IsTicked = gDMBasic.CapturedSupplementaryCodes.Contains(additionalCodes.ZZT_AdditionalCode);
						Add(guidedDecisionMakingAdditionalCode);
					}
				}

				tariff.Wrapper.RatesApplyToCountry = effectiveTradeGroupCountry;

				var conditionSelectionCriteriaInfos = UniversalReferenceDataHelper
					.GetConditionApplicabilitiesByCriteria(tariff, new List<IZZConditionSelectionCriteria> { gDMBasic.ConditionSelectionCriteriaWithoutAdditionalCodes })
					.Where(x => !x.ZZT_AdditionalCode.IsEmpty && (x.ZX2_ConditionClass == RefCusConditionTypes.ConditionClass.Control))
					.GroupBy(x => x.ZX2_ConditionType);

				foreach (var conditionGroups in conditionSelectionCriteriaInfos)
				{
					foreach (var condition in conditionGroups.DistinctBy(x => x.ZZT_AdditionalCode))
					{
						var guidedDecisionMakingAdditionalCode = new GuidedDecisionMakingAdditionalCodeFromCondition(gDMBasic);
						guidedDecisionMakingAdditionalCode.ApplicableToType = condition.ZX2_ConditionType;
						guidedDecisionMakingAdditionalCode.AdditionalCode = condition.ZZT_AdditionalCode;
						guidedDecisionMakingAdditionalCode.IsTicked = gDMBasic.CapturedSupplementaryCodes.Contains(condition.ZZT_AdditionalCode);
						Add(guidedDecisionMakingAdditionalCode);
					}
				}

				var tariffAdditionalCodeSelectionCriteriaInfos = UniversalReferenceDataHelper
					.GetTariffAdditionalCodeApplicabilities(tariff, gDMBasic.TariffAdditionalCodeSelectionCriteria)
					.Where(x => !x.ZY2_AdditionalCode.IsEmpty)
					.GroupBy(x => x.ZY2_ZY3_NKCategory);

				foreach (var tariffAdditionalCodeGroups in tariffAdditionalCodeSelectionCriteriaInfos)
				{
					foreach (var condition in tariffAdditionalCodeGroups.DistinctBy(x => x.ZY2_AdditionalCode))
					{
						var guidedDecisionMakingAdditionalCode = new GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCode(gDMBasic);
						guidedDecisionMakingAdditionalCode.ApplicableToType = condition.ZY2_ZY3_NKCategory;
						guidedDecisionMakingAdditionalCode.AdditionalCode = condition.ZY2_AdditionalCode;
						guidedDecisionMakingAdditionalCode.IsTicked = gDMBasic.CapturedSupplementaryCodes.Contains(condition.ZY2_AdditionalCode);
						Add(guidedDecisionMakingAdditionalCode);
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GuidedDecisionMakingAdditionalCode(gDMBasic);

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
