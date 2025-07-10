using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ABCCategoryCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var abcCategories = new CodeDescriptionPairList();
			WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value.CopyToList(abcCategories);
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), abcCategories);
		}

		#endregion

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ABCCategoryCodeDescriptionPairProvider();
		}

		#endregion
	}
}
