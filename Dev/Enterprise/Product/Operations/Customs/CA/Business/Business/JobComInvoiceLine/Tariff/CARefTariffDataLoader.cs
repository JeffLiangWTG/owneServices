using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public static class CARefTariffDataLoader
	{
		public static bool DoesTariffHasPGA(BusinessObjectFactory factory, ZString tariffCode, ZDateTime? valuationDate)
		{
			return !tariffCode.IsEmpty && valuationDate.HasValue && valuationDate.Value.IsValid &&
				IsTariffMatchCondition(factory, tariffCode, RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.Codes.PGA, ZString.Empty, ZString.Empty, ZString.Empty, valuationDate.Value);
		}

		public static bool DoesTariffHasPGAType(BusinessObjectFactory factory, ZString tariffCode, ZString pgaTypeCode, ZDateTime? valuationDate)
		{
			return !tariffCode.IsEmpty && valuationDate.HasValue && valuationDate.Value.IsValid &&
				IsTariffMatchCondition(factory, tariffCode, RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.Codes.PGA, pgaTypeCode, ZString.Empty, ZString.Empty, valuationDate.Value, true);
		}

		public static bool DoesTariffPGATypeHasProgram(BusinessObjectFactory factory, ZString tariffCode, ZString pgaTypeCode, ZString programCode, ZDateTime? valuationDate)
		{
			return !tariffCode.IsEmpty && valuationDate.HasValue && valuationDate.Value.IsValid &&
				IsTariffMatchCondition(factory, tariffCode, RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.Codes.PGA, pgaTypeCode, programCode, ZString.Empty, valuationDate.Value);
		}

		public static HashSet<ZString> LoadAllProgramsOfPGAType(BusinessObjectFactory factory, ZString tariffCode, ZString pgaTypeCode, ZDateTime valuationDate)
		{
			HashSet<ZString> result = null;
			if (!tariffCode.IsEmpty && !pgaTypeCode.IsEmpty)
			{
				var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
				result = factory.GetCachedValue(string.Join("_", "AllProgramsOfPGAType", tariffCode, pgaTypeCode, GetCacheKeyTimePart(effectiveValuationDate)), () =>
				{
					var matchedTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
					if (matchedTariff != null)
					{
						var criteria = new ZZConditionSelectionCriteria(effectiveValuationDate, ZString.Empty, ZString.Empty, null, ZString.Empty, Core.Constants.CountryCodes.Canada, ConditionChecker.ConditionDirection.Either, RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.Codes.PGA);
						return ConditionChecker.GetApplicableConditions(factory, matchedTariff, criteria).SelectMany(condition => condition.ConditionValues).Where(conditionValue => conditionValue.ValueType.Equals(pgaTypeCode)).Select(conditionValue => conditionValue.ZX3_Value).ToHashSet();
					}
					return null;
				});
			}
			return result ?? new HashSet<ZString>();
		}

		public static ZBool IsTariffMatchCondition(BusinessObjectFactory factory, ZString tariffCode, ZString conditionClass, ZString conditionType, ZString conditionValueType, ZString conditionValue, ZString tradeGroupCountry, ZDateTime valuationDate, bool conditionValueTypeMatchOnly = false)
		{
			var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
			return factory.GetCachedValue(string.Join("_", "isTariffMatchCondition", tariffCode, conditionClass, conditionType, conditionValueType, conditionValue, tradeGroupCountry, GetCacheKeyTimePart(effectiveValuationDate)), () =>
			{
				var result = false;
				if (!tariffCode.IsEmpty && !conditionClass.IsEmpty && !conditionType.IsEmpty)
				{
					var matchedTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
					if (matchedTariff != null)
					{
						var criteria = new ZZConditionSelectionCriteria(effectiveValuationDate, tradeGroupCountry, ZString.Empty, null, ZString.Empty, Core.Constants.CountryCodes.Canada, ConditionChecker.ConditionDirection.Either, conditionClass, conditionType);

						foreach (var condition in ConditionChecker.GetApplicableConditions(factory, matchedTariff, criteria))
						{
							if ((conditionValueType.IsEmpty && conditionValue.IsEmpty) || condition.ConditionValues.Any(x => x.ValueType == conditionValueType && (conditionValueTypeMatchOnly || x.ZX3_Value == conditionValue)))
							{
								result = true;
								break;
							}
						}
					}
				}

				return result;
			});
		}

		static string GetCacheKeyTimePart(ZDateTime dateTime)
		{
			return dateTime.ToString("yyyyMMdd");
		}
	}
}
