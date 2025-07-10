using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.CountryCompliance.Interfaces.DataObjects;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Brazil
{
	class BrazilFeatureConstants : IFeatureConstants
	{
		T IFeatureConstants.GetFeatureContants<T>()
		{
			return typeof(T).Name switch
			{
				"SupportedFeatures" => new SupportedFeatures
				(
					Features.IncludeGovernmentBatchReferenceInXUT
				) as T,
				_ => null
			};
		}
	}
}
