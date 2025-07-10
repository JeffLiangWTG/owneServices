using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public static class CMRBaseAndUnderbondStatuses
	{
		public static CodeDescriptionPairList GetStatuses(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CMRBaseAndUnderbondStatuses", () => new CMRBaseStatuses() + new CMRUnderbondStatuses());
		}
	}
}
