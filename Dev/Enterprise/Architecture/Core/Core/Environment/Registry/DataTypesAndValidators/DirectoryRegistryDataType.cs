using System;
using System.IO;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class DirectoryRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!Directory.Exists(proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("de3e8adf-29bf-4968-b39a-a0eefdcae518", "The directory that you have entered is not valid."));
			}
		}
	}
}
