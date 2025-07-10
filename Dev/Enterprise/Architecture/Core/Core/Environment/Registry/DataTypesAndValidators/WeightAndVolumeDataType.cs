using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class WeightAndVolumeDataType : CodePairRegistryDataType
	{
		public WeightAndVolumeDataType()
			: base(OLookUpEditType.WeightAndVolumeDisplayTypes)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new ComboBoxRegistryEditorInfo(LookUpListProvider);
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!LookUpList.ContainsCode(proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("5e9c9651-157b-4da6-a515-07151c934701", "This is not a valid selection - {0}. Please select a value from the list.", proposedValue));
			}
		}
	}
}
