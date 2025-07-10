using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	static class CMRStatusRecalculationSuspender
	{
		const string CMRStatusRecalculationSuspendedFlagName = "CMRStatusRecalculationSuspended";

		internal static bool SuspendStatusRecalculation(BusinessObjectFactory factory)
		{
			factory.ClearCachedValue<bool>(CMRStatusRecalculationSuspendedFlagName);
			return factory.GetCachedValue(CMRStatusRecalculationSuspendedFlagName, () => true, CacheStalenessPolicy.NeverStale);
		}

		internal static void ResumeStatusRecalculation(BusinessObjectFactory factory)
		{
			factory.ClearCachedValue<bool>(CMRStatusRecalculationSuspendedFlagName);
		}

		/// <summary>
		/// This flag is used to prevent status recalculation during message pre-processing.
		/// During message pre-processing current branch is not yet setup to the correct branch.
		/// </summary>
		internal static bool IsStatusRecalculationSuspended(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(CMRStatusRecalculationSuspendedFlagName, () => false, CacheStalenessPolicy.NeverStale);
		}
	}
}
