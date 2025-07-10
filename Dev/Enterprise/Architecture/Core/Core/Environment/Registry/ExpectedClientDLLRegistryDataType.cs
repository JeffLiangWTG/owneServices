using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	class ExpectedClientDLLRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, string proposedValue, System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			if (!string.IsNullOrEmpty((string)registryItem.Value))
			{
				throw new RegistryValidationException(Res.GetString("87659367-aea3-4d1f-8409-99a3b6fd7828", "For safety reasons, changing the value of 'Expected Client-Specific DLL' is not allowed if its current value is not empty"));
			}

			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
