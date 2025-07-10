using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class BranchManagementCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (branchManagementCodes == null)
			{
				branchManagementCodes = AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
			}
			return branchManagementCodes;
		}

		CodeDescriptionPairList branchManagementCodes;
	}
}
