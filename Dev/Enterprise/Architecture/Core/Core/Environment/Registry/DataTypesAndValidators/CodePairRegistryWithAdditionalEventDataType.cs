using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodePairRegistryWithAdditionalEventDataType : CodePairRegistryDataType
	{
		public CodePairRegistryWithAdditionalEventDataType(ICodeDescriptionPairListProvider lookUpListProvider, bool allowBlank, bool validateCode, Func<IRegistryItem, bool> additionalRegistryItemEvent)
			: base(lookUpListProvider, allowBlank, validateCode)
		{
			AdditionalRegistryItemEvent = additionalRegistryItemEvent;
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (ValidateCode)
			{
				if (AdditionalRegistryItemEvent != null && !isWarned)
				{
					isWarned = AdditionalRegistryItemEvent(registryItem);
				}
			}
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		readonly Func<IRegistryItem, bool> AdditionalRegistryItemEvent;
		bool isWarned;
	}
}
