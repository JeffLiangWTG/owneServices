using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public static class SupplementaryCodeHelper
	{
		public static void SupplementaryCodeSetter<T>(T parent, ZPropertyInfo info, ZString value, SupplementaryCode supplementaryCode, short order)
				where T : BusinessObject, ISupplementaryCodeSupporter
		{
			var supplementaryCodeHandler = new BaseSupplementaryCodeHandler<SupplementaryCode>();
			supplementaryCodeHandler.LoadOrCreate(value, parent, order, info);
		}

		public static CodeDescriptionPairList GetCodeList(ISupplementaryCodeSupporter supporter)
		{
			return UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(supporter.Tariff, supporter.CachedListOfAdditionalCodeDescriptions, supporter.RateSelectionCriteria);
		}
	}
}
