using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Accounting.CountryCompliance.Interfaces.DataObjects
{
	public class SupportedFeatures
	{
		public SupportedFeatures(params Features[] features)
		{
			Features = features.ToHashSet();
		}

		public HashSet<Features> Features { get; }
	}

	public enum Features
	{
		IncludeGovernmentBatchReferenceInXUT,
	}
}
