using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class SelectNDRPathRegistryDataType : CodePairRegistryDataType
	{
		public SelectNDRPathRegistryDataType(OLookUpEditType lookUpEditType)
			: base(lookUpEditType)
		{
		}

		public SelectNDRPathRegistryDataType(ICodeDescriptionPairListProvider lookUpListProvider)
			: base(lookUpListProvider, false, true)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!DataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress)
			{
				throw new RegistryValidationException((NoResString)"The default value cannot be overridden as the Allow Emails To Be Sent From User's Address Registry setting is set to No.");
			}
		}
	}
}
