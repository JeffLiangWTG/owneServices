using System;
using CargoWise.Types;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class EnglishOnlyStringRegistryDataType : StringRegistryDataType
	{
		public EnglishOnlyStringRegistryDataType() : base() { }
		public EnglishOnlyStringRegistryDataType(int minLength, int maxLength) : base(minLength, maxLength) { }
		public EnglishOnlyStringRegistryDataType(CharacterCase characterCase) : base(characterCase) { }

		protected override void ValidateCore(Enterprise.Integration.IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!new ZString(proposedValue).IsWesternEuropeanOrEmpty)
			{
				throw new RegistryValidationException(
					Res.GetString("4679de5a-5c02-457d-914f-00c81e57ef5a", "Registry item {0} only accepts Western European languages characters.", registryItem.Caption));
			}

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
