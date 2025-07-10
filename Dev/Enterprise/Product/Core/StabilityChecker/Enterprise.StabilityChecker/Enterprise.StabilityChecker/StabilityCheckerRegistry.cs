using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.StabilityChecker
{
	sealed class StabilityCheckerRegistry : RegistryItemSet
	{
		StabilityCheckerRegistry()
		{
		}

		internal static StabilityCheckerRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new StabilityCheckerRegistry();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static StabilityCheckerRegistry instance;

		public override bool IsForProductivityWise => false;

		internal StabilityResultsRegistryItem StabilityCheckerResults
		{
			get
			{
				return GetItem("StabilityCheckerResults", delegate
				{
					return new StabilityResultsRegistryItem("StabilityCheckerResults");
				});
			}
		}
	}
}
