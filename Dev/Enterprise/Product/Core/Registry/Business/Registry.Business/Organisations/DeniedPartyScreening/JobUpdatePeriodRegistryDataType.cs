using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class JobUpdatePeriodRegistryDataType : IntRegistryDataType
	{
		public JobUpdatePeriodRegistryDataType() : base(1, 24)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue < LowerBound)
			{
				throw new RegistryValidationException(Res.GetString("0BA2C1EC-BBA9-4EEE-B18B-437DB841A8BB", "The minimum fallback period is {0} month.", LowerBound));
			}

			if (proposedValue > UpperBound)
			{
				throw new RegistryValidationException(Res.GetString("8793147D-AD65-4BE8-9895-6978A7BB2524", "The maximum fallback period is {0} months.", UpperBound));
			}
		}
	}
}
