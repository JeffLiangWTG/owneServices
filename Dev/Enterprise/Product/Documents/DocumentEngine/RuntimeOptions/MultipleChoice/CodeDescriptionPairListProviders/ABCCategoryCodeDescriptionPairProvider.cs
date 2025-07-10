using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ABCCategoryCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var abcCategories = new CodeDescriptionPairList();
			foreach (ABCAnalysisCategory item in WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value)
			{
				abcCategories.AddPairIfNotExist(item.Code, item.Description);
			}
			return abcCategories;
		}
	}
}
