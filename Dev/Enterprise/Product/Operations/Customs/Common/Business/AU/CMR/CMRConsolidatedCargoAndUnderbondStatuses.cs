using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public static class CMRConsolidatedCargoAndUnderbondStatuses
	{
		public static CodeDescriptionPairList GetStatuses(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CMRConsolidatedCargoAndUnderbondStatuses", () => new CMRConsolidatedCargoStatuses() + new CMRUnderbondStatuses());
		}
	}
}
