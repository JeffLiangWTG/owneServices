using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodePairRegistryDataTypeWithAdditionalValidation : CodePairRegistryDataType
	{
		public CodePairRegistryDataTypeWithAdditionalValidation(ICodeDescriptionPairListProvider lookUpList, bool allowBlank, bool validateCode, AdditionalValidation additionalValidationHandler = null)
			: base(lookUpList, allowBlank, validateCode)
		{
			AdditionalValidationHandler = additionalValidationHandler;
		}

		public CodePairRegistryDataTypeWithAdditionalValidation(OLookUpEditType lookUpEditType, bool allowBlank, bool validateCode, AdditionalValidation additionalValidationHandler = null)
			: base(lookUpEditType, allowBlank, validateCode)
		{
			AdditionalValidationHandler = additionalValidationHandler;
		}

		public delegate string AdditionalValidation(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK);

		readonly AdditionalValidation AdditionalValidationHandler;

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var errorMessage = AdditionalValidationHandler?.Invoke(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new RegistryValidationException(errorMessage);
			}
		}
	}
}
