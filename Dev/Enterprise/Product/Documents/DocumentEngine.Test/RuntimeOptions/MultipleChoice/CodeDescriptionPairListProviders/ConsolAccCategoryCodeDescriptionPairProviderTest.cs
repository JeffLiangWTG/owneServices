using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ConsolAccCategoryCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ConsolAccCategoryCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList(), CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList());
		}
	}
}
