using System;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	class IPAddressRangesDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(proposedValue))
				{
					IPAddressRanges.Parse(proposedValue).ToArray();
				}
			}
			catch (FormatException ex)
			{
				throw new RegistryValidationException(ex.Message);
			}
		}
	}
}
