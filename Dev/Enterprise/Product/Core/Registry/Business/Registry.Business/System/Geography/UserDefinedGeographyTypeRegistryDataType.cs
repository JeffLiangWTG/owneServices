using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class UserDefinedGeographyTypeRegistryDataType : CodeDescriptionPairListRegistryDataType
	{
		public UserDefinedGeographyTypeRegistryDataType() : base(4) { }

		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			foreach (CodeDescriptionPair pair in proposedValue)
			{
				ValidateCode(pair.Code);
			}
		}

		Regex RegexOnlyNumbersAndLetters => regexOnlyNumbersAndAlphabets ?? (regexOnlyNumbersAndAlphabets = new Regex("^[a-zA-Z0-9]*$"));

		Regex regexOnlyNumbersAndAlphabets;

		void ValidateCode(string code)
		{
			if (code != null && code.Length == 4)
			{
				if (!code.StartsWith("U", StringComparison.OrdinalIgnoreCase))
				{
					throw new RegistryValidationException(Res.GetString("ECC4595C-A634-4A9E-B0C7-BF1D33116B66", "Code should start with 'U'."));
				}

				if (!RegexOnlyNumbersAndLetters.IsMatch(code))
				{
					throw new RegistryValidationException(Res.GetString("EE53B888-5FA3-4064-9AEB-65D8F8F8335D", "Code can only contain numbers and letters."));
				}
			}
			else
			{
				throw new RegistryValidationException(Res.GetString("DA36C35A-E0AE-47A1-8C21-21F7214B9588", "Code should consist of 4 characters."));
			}
		}
	}
}
