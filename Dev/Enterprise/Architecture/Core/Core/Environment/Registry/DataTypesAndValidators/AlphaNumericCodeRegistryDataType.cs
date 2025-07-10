using System;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class AlphaNumericCodeRegistryDataType : StringRegistryDataType
	{
		public AlphaNumericCodeRegistryDataType(int minLength, int maxLength)
			: base(minLength, maxLength)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var isAlphaNumericCodeOnly = true;

			if (proposedValue != null)
			{
				foreach (char character in proposedValue)
				{
					if (!(Char.IsLetter(character) || Char.IsNumber(character)))
					{
						isAlphaNumericCodeOnly = false;
						break;
					}
				}
			}
			else
			{
				isAlphaNumericCodeOnly = false;
			}

			if (!isAlphaNumericCodeOnly)
			{
				throw new RegistryValidationException(Res.GetString("ea5a4e51-2e8b-4ade-8ff7-6bf5087b85ab", "The entered value must be letters and numbers only."));
			}
		}
	}
}
