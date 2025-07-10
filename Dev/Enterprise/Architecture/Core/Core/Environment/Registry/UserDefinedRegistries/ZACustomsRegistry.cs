using System;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Core.Environment.ZA
{
	public class Registry
	{
		public Registry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public bool GetIsTestMode(IBranch branch)
		{
			return (bool)RawRegistry.ZAIsTestMode.GetFallBackValueAtAllLevels(branch.CompanyPK, branch.PK, Guid.Empty);
		}

#if DEBUG
		public void SetIsTestMode(IBranch branch, bool value)
		{
			RawRegistry.ZAIsTestMode.SetValue(Guid.Empty, branch.PK, Guid.Empty, value);
		}
#endif

		readonly RawDataRegistry RawRegistry;
	}
}
