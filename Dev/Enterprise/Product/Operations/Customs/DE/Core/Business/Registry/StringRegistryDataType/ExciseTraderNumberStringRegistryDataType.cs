using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.DE.Business.Res;

namespace Enterprise.Customs.DE.Registry
{
	public class ExciseTraderNumberStringRegistryDataType : StringRegistryDataType
	{
		public ExciseTraderNumberStringRegistryDataType()
		{
			CharacterCase = CharacterCase.Upper;
			MinLength = 13;
			MaxLength = 13;
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var match = new Regex(@"^DE[0-9]{11}$").Match(proposedValue);

			if (!match.Success)
			{
				throw new RegistryValidationException(Res.GetString("DF9650B8-781F-4EC4-92C2-EFEA8F479906", "Excise Trader Number must start with 'DE' and then be followed by 11 digits."));
			}
		}
	}
}
