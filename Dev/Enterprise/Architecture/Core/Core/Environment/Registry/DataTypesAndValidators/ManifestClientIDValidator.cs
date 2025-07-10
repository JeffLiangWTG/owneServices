using System;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class ManifestClientIDDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue.Length != 10)
			{
				throw new RegistryValidationException(Res.GetString("f50f5f73-1be7-4d4e-9551-d3d0185ad276", "The Manifest Client ID must have a length of 10."));
			}
			else
			{
				if (!Char.IsLetter(proposedValue[0]))
				{
					throw new RegistryValidationException(Res.GetString("29bfc96a-8bf2-4179-bec3-8479acb9d987", "The Manifest Client ID must start with a character."));
				}
				for (int i = 1; i <= 9; i++)
				{
					if (!Char.IsNumber(proposedValue[i]))
					{
						throw new RegistryValidationException(Res.GetString("3d779e54-7dba-4c98-a8e3-649215351149", "The last 9 characters must be a valid ACN."));
					}
				}
			}
		}
	}
}
