using System;
using CargoWise.Types;
using Enterprise.Integration;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class NativeXMLSupportTillDateDataType : DateTimeRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, DateTime proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue > ZDateTime.UtcNow.Date.AddMonths(3).ToDateTime())
			{
				throw new RegistryValidationException(ResString.GetMultilingualString("06B73373-45AB-42D8-B572-A28E00D8BA2B", "Date cannot be greater than 3 months in the future"));
			}
		}
	}
}
