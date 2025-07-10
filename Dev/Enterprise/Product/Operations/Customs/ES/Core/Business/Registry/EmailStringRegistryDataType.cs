using System;
using Enterprise.Integration;

namespace Enterprise.Customs.ES.Registry
{
	public class EmailStringRegistryDataType : ZArchitecture.Environment.EmailStringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!string.IsNullOrWhiteSpace(proposedValue))
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			}
		}
	}
}
