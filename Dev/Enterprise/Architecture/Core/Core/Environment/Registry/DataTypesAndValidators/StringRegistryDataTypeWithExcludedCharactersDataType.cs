using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class StringRegistryDataTypeWithExcludedCharactersDataType : StringRegistryDataType
	{
		readonly char[] excludedChars;

		public StringRegistryDataTypeWithExcludedCharactersDataType(int minLength, int maxLength, char[] excludedChars)
			: base(minLength, maxLength)
		{
			this.excludedChars = excludedChars;
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (excludedChars != null && excludedChars.Length > 0 && !string.IsNullOrEmpty(proposedValue) && proposedValue.Any(c => excludedChars.Contains(c)))
			{
				throw new RegistryValidationException(Res.GetString("6579FBA3-913E-4B26-8741-7A0525794A78", "The value cannot contain following characters: {0}", string.Join(", ", excludedChars)));
			}
		}
	}
}
