using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class ComplianceJobEndDateLimitDateType : IntRegistryDataType
	{
		public ComplianceJobEndDateLimitDateType() : base(lowerBound: 1, upperBound: 14)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue < LowerBound)
			{
				throw new RegistryValidationException(Res.GetString("D57FEE02-D64F-4618-A093-BDD80CDE4F96", "Minimum value is {0} day.", LowerBound));
			}

			if (proposedValue > UpperBound)
			{
				throw new RegistryValidationException(Res.GetString("4754DE75-86DB-4D78-9F5A-44B2436C0887", "Maximum value is {0} days.", UpperBound));
			}
		}
	}
}
