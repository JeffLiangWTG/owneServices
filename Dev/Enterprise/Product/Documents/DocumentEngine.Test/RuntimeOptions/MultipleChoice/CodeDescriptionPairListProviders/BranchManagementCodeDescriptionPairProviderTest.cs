using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class BranchManagementCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new BranchManagementCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var testPairProvider = new BranchManagementCodeDescriptionPairProvider();
			var list = testPairProvider.GetCodeDescriptionPairList();
			var actualList = AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
			AssertContainsExactElementsInAnyOrder(list, actualList);
		}
	}
}
